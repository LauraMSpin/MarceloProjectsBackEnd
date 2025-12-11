using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using cronograma_atividades_backend.Data;
using cronograma_atividades_backend.Entities;
using cronograma_atividades_backend.DTOs;

namespace cronograma_atividades_backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServicosController : ControllerBase
{
    private readonly AppDbContext _context;

    public ServicosController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ServicoDto>>> GetServicos([FromQuery] Guid? contratoId)
    {
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
                    m.Realizado,
                    m.Pago
                )).ToList()
            ))
            .ToListAsync();

        return Ok(servicos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ServicoDto>> GetServico(Guid id)
    {
        var servico = await _context.Servicos
            .Include(s => s.Medicoes)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (servico == null)
        {
            return NotFound();
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
                m.Realizado,
                m.Pago
            )).ToList()
        );

        return Ok(dto);
    }

    [HttpPost]
    public async Task<ActionResult<ServicoDto>> CreateServico(CriarServicoDto dto)
    {
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
                Realizado = m.Realizado,
                Pago = m.Pago
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
                m.Realizado,
                m.Pago
            )).ToList()
        );

        return CreatedAtAction(nameof(GetServico), new { id = servico.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateServico(Guid id, AtualizarServicoDto dto)
    {
        var servico = await _context.Servicos
            .Include(s => s.Medicoes)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (servico == null)
        {
            return NotFound();
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
            Pago = m.Pago,
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
        var servico = await _context.Servicos
            .Include(s => s.Medicoes)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (servico == null)
        {
            return NotFound();
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
        medicao.Pago = dto.Pago;

        servico.ValorTotal = medicoes.Sum(m => m.Previsto);

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteServico(Guid id)
    {
        var servico = await _context.Servicos.FindAsync(id);

        if (servico == null)
        {
            return NotFound();
        }

        _context.Servicos.Remove(servico);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
