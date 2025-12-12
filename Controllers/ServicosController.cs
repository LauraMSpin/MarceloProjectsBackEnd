using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using cronograma_atividades_backend.Data;
using cronograma_atividades_backend.Entities;
using cronograma_atividades_backend.DTOs;

namespace cronograma_atividades_backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ServicosController : ControllerBase
{
    private readonly AppDbContext _context;

    public ServicosController(AppDbContext context)
    {
        _context = context;
    }

    private Guid? GetUsuarioLogadoId()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return userId != null && Guid.TryParse(userId, out var id) ? id : null;
    }

    private async Task<bool> TemAcessoContrato(Guid contratoId, bool precisaEditar = false)
    {
        var usuarioId = GetUsuarioLogadoId();
        if (usuarioId == null) return false;

        var contrato = await _context.Contratos.FindAsync(contratoId);
        if (contrato == null) return false;

        if (contrato.UsuarioId == usuarioId.Value) return true;

        var compartilhamento = await _context.ContratosCompartilhados
            .FirstOrDefaultAsync(cc => cc.ContratoId == contratoId && cc.UsuarioId == usuarioId.Value);

        if (compartilhamento == null) return false;
        if (precisaEditar && !compartilhamento.PodeEditar) return false;

        return true;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ServicoDto>>> GetServicos([FromQuery] Guid? contratoId)
    {
        var usuarioId = GetUsuarioLogadoId();
        if (usuarioId == null) return Unauthorized();

        if (contratoId.HasValue && !await TemAcessoContrato(contratoId.Value))
        {
            return Forbid();
        }

        var query = _context.Servicos
            .Include(s => s.Medicoes)
            .AsQueryable();

        if (contratoId.HasValue)
        {
            query = query.Where(s => s.ContratoId == contratoId.Value);
        }

        var servicos = await query
            .Select(s => new ServicoDto(
                s.Id,
                s.Item,
                s.ServicoNome,
                s.ValorTotal,
                s.ContratoId,
                s.Medicoes.OrderBy(m => m.Ordem).Select(m => new MedicaoDto(
                    m.Id,
                    m.Ordem,
                    m.Mes,
                    m.Previsto,
                    m.Realizado
                )).ToList()
            ))
            .ToListAsync();

        return Ok(servicos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ServicoDto>> GetServico(Guid id)
    {
        var usuarioId = GetUsuarioLogadoId();
        if (usuarioId == null) return Unauthorized();

        var servico = await _context.Servicos
            .Include(s => s.Medicoes)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (servico == null)
        {
            return NotFound();
        }

        if (!await TemAcessoContrato(servico.ContratoId))
        {
            return Forbid();
        }

        var dto = new ServicoDto(
            servico.Id,
            servico.Item,
            servico.ServicoNome,
            servico.ValorTotal,
            servico.ContratoId,
            servico.Medicoes.OrderBy(m => m.Ordem).Select(m => new MedicaoDto(
                m.Id,
                m.Ordem,
                m.Mes,
                m.Previsto,
                m.Realizado
            )).ToList()
        );

        return Ok(dto);
    }

    [HttpPost]
    public async Task<ActionResult<ServicoDto>> CreateServico(CriarServicoDto dto)
    {
        var usuarioId = GetUsuarioLogadoId();
        if (usuarioId == null) return Unauthorized();

        if (!await TemAcessoContrato(dto.ContratoId, precisaEditar: true))
        {
            return Forbid();
        }

        var servico = new Servico
        {
            Id = Guid.NewGuid(),
            Item = dto.Item,
            ServicoNome = dto.Servico,
            ContratoId = dto.ContratoId,
            ValorTotal = dto.Medicoes.Sum(m => m.Previsto),
            Medicoes = dto.Medicoes.Select(m => new Medicao
            {
                Id = Guid.NewGuid(),
                Ordem = m.Ordem,
                Mes = m.Mes,
                Previsto = m.Previsto,
                Realizado = m.Realizado
            }).ToList()
        };

        _context.Servicos.Add(servico);
        await _context.SaveChangesAsync();

        var result = new ServicoDto(
            servico.Id,
            servico.Item,
            servico.ServicoNome,
            servico.ValorTotal,
            servico.ContratoId,
            servico.Medicoes.OrderBy(m => m.Ordem).Select(m => new MedicaoDto(
                m.Id,
                m.Ordem,
                m.Mes,
                m.Previsto,
                m.Realizado
            )).ToList()
        );

        return CreatedAtAction(nameof(GetServico), new { id = servico.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateServico(Guid id, AtualizarServicoDto dto)
    {
        var usuarioId = GetUsuarioLogadoId();
        if (usuarioId == null) return Unauthorized();

        var servico = await _context.Servicos
            .Include(s => s.Medicoes)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (servico == null)
        {
            return NotFound();
        }

        if (!await TemAcessoContrato(servico.ContratoId, precisaEditar: true))
        {
            return Forbid();
        }

        servico.Item = dto.Item;
        servico.ServicoNome = dto.Servico;

        // Remover medições antigas de forma segura
        var medicoesParaRemover = servico.Medicoes.ToList();
        foreach (var medicao in medicoesParaRemover)
        {
            _context.Medicoes.Remove(medicao);
        }
        
        // Salvar a remoção primeiro
        await _context.SaveChangesAsync();

        // Adicionar novas medições
        var novasMedicoes = dto.Medicoes.Select(m => new Medicao
        {
            Id = Guid.NewGuid(),
            Ordem = m.Ordem,
            Mes = m.Mes,
            Previsto = m.Previsto,
            Realizado = m.Realizado,
            ServicoId = servico.Id
        }).ToList();

        foreach (var medicao in novasMedicoes)
        {
            _context.Medicoes.Add(medicao);
        }

        servico.ValorTotal = dto.Medicoes.Sum(m => m.Previsto);

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPut("{id}/medicoes/{medicaoIndex}")]
    public async Task<IActionResult> UpdateMedicao(Guid id, int medicaoIndex, AtualizarMedicaoDto dto)
    {
        var usuarioId = GetUsuarioLogadoId();
        if (usuarioId == null) return Unauthorized();

        var servico = await _context.Servicos
            .Include(s => s.Medicoes)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (servico == null)
        {
            return NotFound();
        }

        if (!await TemAcessoContrato(servico.ContratoId, precisaEditar: true))
        {
            return Forbid();
        }

        // Ordenar por Ordem para garantir que o índice correto seja atualizado
        var medicoes = servico.Medicoes.OrderBy(m => m.Ordem).ToList();
        if (medicaoIndex < 0 || medicaoIndex >= medicoes.Count)
        {
            return BadRequest("Índice de medição inválido");
        }

        var medicao = medicoes[medicaoIndex];
        medicao.Ordem = dto.Ordem;
        medicao.Mes = dto.Mes;
        medicao.Previsto = dto.Previsto;
        medicao.Realizado = dto.Realizado;

        servico.ValorTotal = medicoes.Sum(m => m.Previsto);

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteServico(Guid id)
    {
        var usuarioId = GetUsuarioLogadoId();
        if (usuarioId == null) return Unauthorized();

        var servico = await _context.Servicos.FindAsync(id);

        if (servico == null)
        {
            return NotFound();
        }

        if (!await TemAcessoContrato(servico.ContratoId, precisaEditar: true))
        {
            return Forbid();
        }

        _context.Servicos.Remove(servico);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
