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
public class ContratosController : ControllerBase
{
    private readonly AppDbContext _context;

    public ContratosController(AppDbContext context)
    {
        _context = context;
    }

    private Guid? GetUsuarioLogadoId()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return userId != null && Guid.TryParse(userId, out var id) ? id : null;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ContratoResumoDto>>> GetContratos()
    {
        var usuarioId = GetUsuarioLogadoId();
        if (usuarioId == null) return Unauthorized();

        // Buscar contratos próprios
        var contratosProprios = await _context.Contratos
            .Where(c => c.UsuarioId == usuarioId.Value)
            .Include(c => c.Usuario)
            .Select(c => new ContratoResumoDto(
                c.Id,
                c.Nome,
                c.Descricao,
                c.NumeroMeses,
                c.MesInicial,
                c.AnoInicial,
                c.DataCriacao,
                c.UsuarioId,
                c.Usuario.Nome,
                true,
                true,
                c.PercentualReajuste,
                c.MesInicioReajuste
            ))
            .ToListAsync();

        // Buscar contratos compartilhados comigo
        var contratosCompartilhados = await _context.ContratosCompartilhados
            .Where(cc => cc.UsuarioId == usuarioId.Value)
            .Include(cc => cc.Contrato)
                .ThenInclude(c => c.Usuario)
            .Select(cc => new ContratoResumoDto(
                cc.Contrato.Id,
                cc.Contrato.Nome,
                cc.Contrato.Descricao,
                cc.Contrato.NumeroMeses,
                cc.Contrato.MesInicial,
                cc.Contrato.AnoInicial,
                cc.Contrato.DataCriacao,
                cc.Contrato.UsuarioId,
                cc.Contrato.Usuario.Nome,
                false,
                cc.PodeEditar,
                cc.Contrato.PercentualReajuste,
                cc.Contrato.MesInicioReajuste
            ))
            .ToListAsync();

        var todos = contratosProprios.Concat(contratosCompartilhados)
            .OrderByDescending(c => c.DataCriacao)
            .ToList();

        return Ok(todos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ContratoDto>> GetContrato(Guid id)
    {
        var usuarioId = GetUsuarioLogadoId();
        if (usuarioId == null) return Unauthorized();

        var contrato = await _context.Contratos
            .Include(c => c.Usuario)
            .Include(c => c.Servicos)
                .ThenInclude(s => s.Medicoes)
            .Include(c => c.PagamentosMensais)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (contrato == null)
        {
            return NotFound();
        }

        // Verificar se o usuário tem acesso
        var isProprietario = contrato.UsuarioId == usuarioId.Value;
        var compartilhamento = await _context.ContratosCompartilhados
            .FirstOrDefaultAsync(cc => cc.ContratoId == id && cc.UsuarioId == usuarioId.Value);

        if (!isProprietario && compartilhamento == null)
        {
            return Forbid();
        }

        var podeEditar = isProprietario || (compartilhamento?.PodeEditar ?? false);

        var dto = new ContratoDto(
            contrato.Id,
            contrato.Nome,
            contrato.Descricao,
            contrato.NumeroMeses,
            contrato.MesInicial,
            contrato.AnoInicial,
            contrato.DataCriacao,
            contrato.UsuarioId,
            contrato.Usuario.Nome,
            isProprietario,
            podeEditar,
            contrato.PercentualReajuste,
            contrato.MesInicioReajuste,
            contrato.Servicos.Select(s => new ServicoDto(
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
            )).ToList(),
            contrato.PagamentosMensais.OrderBy(p => p.Ordem).Select(p => new PagamentoMensalDto(
                p.Id,
                p.Ordem,
                p.Mes,
                p.Valor
            )).ToList()
        );

        return Ok(dto);
    }

    [HttpPost]
    public async Task<ActionResult<ContratoDto>> CreateContrato(CriarContratoDto dto)
    {
        var usuarioId = GetUsuarioLogadoId();
        if (usuarioId == null) return Unauthorized();

        // Forçar o usuário logado como proprietário
        var contrato = new Contrato
        {
            Id = Guid.NewGuid(),
            Nome = dto.Nome,
            Descricao = dto.Descricao,
            NumeroMeses = dto.NumeroMeses,
            MesInicial = dto.MesInicial,
            AnoInicial = dto.AnoInicial,
            UsuarioId = usuarioId.Value,
            DataCriacao = DateTime.UtcNow,
            PercentualReajuste = dto.PercentualReajuste,
            MesInicioReajuste = dto.MesInicioReajuste
        };

        _context.Contratos.Add(contrato);
        await _context.SaveChangesAsync();

        var usuario = await _context.Usuarios.FindAsync(usuarioId.Value);

        var result = new ContratoDto(
            contrato.Id,
            contrato.Nome,
            contrato.Descricao,
            contrato.NumeroMeses,
            contrato.MesInicial,
            contrato.AnoInicial,
            contrato.DataCriacao,
            contrato.UsuarioId,
            usuario?.Nome,
            true,
            true,
            contrato.PercentualReajuste,
            contrato.MesInicioReajuste,
            new List<ServicoDto>(),
            new List<PagamentoMensalDto>()
        );

        return CreatedAtAction(nameof(GetContrato), new { id = contrato.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateContrato(Guid id, AtualizarContratoDto dto)
    {
        var usuarioId = GetUsuarioLogadoId();
        if (usuarioId == null) return Unauthorized();

        var contrato = await _context.Contratos.FindAsync(id);

        if (contrato == null)
        {
            return NotFound();
        }

        // Verificar permissão de edição
        var isProprietario = contrato.UsuarioId == usuarioId.Value;
        var compartilhamento = await _context.ContratosCompartilhados
            .FirstOrDefaultAsync(cc => cc.ContratoId == id && cc.UsuarioId == usuarioId.Value);

        if (!isProprietario && (compartilhamento == null || !compartilhamento.PodeEditar))
        {
            return Forbid();
        }

        contrato.Nome = dto.Nome;
        contrato.Descricao = dto.Descricao;
        contrato.NumeroMeses = dto.NumeroMeses;
        contrato.MesInicial = dto.MesInicial;
        contrato.AnoInicial = dto.AnoInicial;
        contrato.PercentualReajuste = dto.PercentualReajuste;
        contrato.MesInicioReajuste = dto.MesInicioReajuste;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteContrato(Guid id)
    {
        var usuarioId = GetUsuarioLogadoId();
        if (usuarioId == null) return Unauthorized();

        var contrato = await _context.Contratos.FindAsync(id);

        if (contrato == null)
        {
            return NotFound();
        }

        // Somente o proprietário pode deletar
        if (contrato.UsuarioId != usuarioId.Value)
        {
            return Forbid();
        }

        _context.Contratos.Remove(contrato);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // === Endpoints de Compartilhamento ===

    [HttpGet("{id}/compartilhamentos")]
    public async Task<ActionResult<List<ContratoCompartilhadoDto>>> GetCompartilhamentos(Guid id)
    {
        var usuarioId = GetUsuarioLogadoId();
        if (usuarioId == null) return Unauthorized();

        var contrato = await _context.Contratos.FindAsync(id);
        if (contrato == null) return NotFound();

        // Somente o proprietário pode ver compartilhamentos
        if (contrato.UsuarioId != usuarioId.Value)
        {
            return Forbid();
        }

        var compartilhamentos = await _context.ContratosCompartilhados
            .Where(cc => cc.ContratoId == id)
            .Include(cc => cc.Usuario)
            .Select(cc => new ContratoCompartilhadoDto(
                cc.Id,
                cc.UsuarioId,
                cc.Usuario.Nome,
                cc.Usuario.Email,
                cc.PodeEditar,
                cc.DataCompartilhamento
            ))
            .ToListAsync();

        return Ok(compartilhamentos);
    }

    [HttpPost("{id}/compartilhar")]
    public async Task<ActionResult<ContratoCompartilhadoDto>> CompartilharContrato(Guid id, CompartilharContratoDto dto)
    {
        var usuarioId = GetUsuarioLogadoId();
        if (usuarioId == null) return Unauthorized();

        var contrato = await _context.Contratos.FindAsync(id);
        if (contrato == null) return NotFound();

        // Somente o proprietário pode compartilhar
        if (contrato.UsuarioId != usuarioId.Value)
        {
            return Forbid();
        }

        // Não pode compartilhar consigo mesmo
        if (dto.UsuarioId == usuarioId.Value)
        {
            return BadRequest(new { message = "Você não pode compartilhar um contrato consigo mesmo" });
        }

        // Verificar se usuário destino existe
        var usuarioDestino = await _context.Usuarios.FindAsync(dto.UsuarioId);
        if (usuarioDestino == null)
        {
            return BadRequest(new { message = "Usuário não encontrado" });
        }

        // Verificar se já está compartilhado
        var existente = await _context.ContratosCompartilhados
            .FirstOrDefaultAsync(cc => cc.ContratoId == id && cc.UsuarioId == dto.UsuarioId);

        if (existente != null)
        {
            // Atualizar permissão existente
            existente.PodeEditar = dto.PodeEditar;
            await _context.SaveChangesAsync();

            return Ok(new ContratoCompartilhadoDto(
                existente.Id,
                existente.UsuarioId,
                usuarioDestino.Nome,
                usuarioDestino.Email,
                existente.PodeEditar,
                existente.DataCompartilhamento
            ));
        }

        var compartilhamento = new ContratoCompartilhado
        {
            Id = Guid.NewGuid(),
            ContratoId = id,
            UsuarioId = dto.UsuarioId,
            PodeEditar = dto.PodeEditar,
            DataCompartilhamento = DateTime.UtcNow
        };

        _context.ContratosCompartilhados.Add(compartilhamento);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCompartilhamentos), new { id }, new ContratoCompartilhadoDto(
            compartilhamento.Id,
            compartilhamento.UsuarioId,
            usuarioDestino.Nome,
            usuarioDestino.Email,
            compartilhamento.PodeEditar,
            compartilhamento.DataCompartilhamento
        ));
    }

    [HttpDelete("{id}/compartilhar/{usuarioDestinoId}")]
    public async Task<IActionResult> RemoverCompartilhamento(Guid id, Guid usuarioDestinoId)
    {
        var usuarioId = GetUsuarioLogadoId();
        if (usuarioId == null) return Unauthorized();

        var contrato = await _context.Contratos.FindAsync(id);
        if (contrato == null) return NotFound();

        // Somente o proprietário pode remover compartilhamentos
        if (contrato.UsuarioId != usuarioId.Value)
        {
            return Forbid();
        }

        var compartilhamento = await _context.ContratosCompartilhados
            .FirstOrDefaultAsync(cc => cc.ContratoId == id && cc.UsuarioId == usuarioDestinoId);

        if (compartilhamento == null)
        {
            return NotFound();
        }

        _context.ContratosCompartilhados.Remove(compartilhamento);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // ===== ENDPOINTS PARA PAGAMENTOS MENSAIS =====

    [HttpGet("{id}/pagamentos")]
    public async Task<ActionResult<List<PagamentoMensalDto>>> GetPagamentos(Guid id)
    {
        var usuarioId = GetUsuarioLogadoId();
        if (usuarioId == null) return Unauthorized();

        // Verificar acesso ao contrato
        var contrato = await _context.Contratos.FindAsync(id);
        if (contrato == null) return NotFound();

        var temAcesso = contrato.UsuarioId == usuarioId.Value ||
            await _context.ContratosCompartilhados.AnyAsync(cc => cc.ContratoId == id && cc.UsuarioId == usuarioId.Value);

        if (!temAcesso) return Forbid();

        var pagamentos = await _context.PagamentosMensais
            .Where(p => p.ContratoId == id)
            .OrderBy(p => p.Ordem)
            .Select(p => new PagamentoMensalDto(p.Id, p.Ordem, p.Mes, p.Valor))
            .ToListAsync();

        return Ok(pagamentos);
    }

    [HttpPut("{id}/pagamentos/{ordem}")]
    public async Task<IActionResult> AtualizarPagamento(Guid id, int ordem, AtualizarPagamentoMensalDto dto)
    {
        var usuarioId = GetUsuarioLogadoId();
        if (usuarioId == null) return Unauthorized();

        // Verificar acesso ao contrato
        var contrato = await _context.Contratos.FindAsync(id);
        if (contrato == null) return NotFound();

        var isProprietario = contrato.UsuarioId == usuarioId.Value;
        var compartilhamento = await _context.ContratosCompartilhados
            .FirstOrDefaultAsync(cc => cc.ContratoId == id && cc.UsuarioId == usuarioId.Value);

        if (!isProprietario && (compartilhamento == null || !compartilhamento.PodeEditar))
        {
            return Forbid();
        }

        var pagamento = await _context.PagamentosMensais
            .FirstOrDefaultAsync(p => p.ContratoId == id && p.Ordem == ordem);

        if (pagamento == null)
        {
            // Criar novo pagamento
            pagamento = new PagamentoMensal
            {
                Id = Guid.NewGuid(),
                ContratoId = id,
                Ordem = ordem,
                Mes = dto.Mes,
                Valor = dto.Valor
            };
            _context.PagamentosMensais.Add(pagamento);
        }
        else
        {
            // Atualizar existente
            pagamento.Mes = dto.Mes;
            pagamento.Valor = dto.Valor;
        }

        await _context.SaveChangesAsync();

        return NoContent();
    }
}
