using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using cronograma_atividades_backend.Data;
using cronograma_atividades_backend.Entities;
using cronograma_atividades_backend.DTOs;

namespace cronograma_atividades_backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContratosController : ControllerBase
{
    private readonly AppDbContext _context;

    public ContratosController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ContratoResumoDto>>> GetContratos([FromQuery] Guid? usuarioId)
    {
        var query = _context.Contratos.AsQueryable();

        if (usuarioId.HasValue)
        {
            query = query.Where(c => c.UsuarioId == usuarioId.Value);
        }

        var contratos = await query
            .OrderByDescending(c => c.DataCriacao)
            .Select(c => new ContratoResumoDto(
                c.Id,
                c.Nome,
                c.Descricao,
                c.NumeroMeses,
                c.MesInicial,
                c.AnoInicial,
                c.DataCriacao,
                c.UsuarioId
            ))
            .ToListAsync();

        return Ok(contratos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ContratoDto>> GetContrato(Guid id)
    {
        var contrato = await _context.Contratos
            .Include(c => c.Servicos)
                .ThenInclude(s => s.Medicoes)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (contrato == null)
        {
            return NotFound();
        }

        var dto = new ContratoDto(
            contrato.Id,
            contrato.Nome,
            contrato.Descricao,
            contrato.NumeroMeses,
            contrato.MesInicial,
            contrato.AnoInicial,
            contrato.DataCriacao,
            contrato.UsuarioId,
            contrato.Servicos.Select(s => new ServicoDto(
                s.Id,
                s.Item,
                s.ServicoNome,
                s.ValorTotal,
                s.ContratoId,
                s.Medicoes.Select(m => new MedicaoDto(
                    m.Id,
                    m.Mes,
                    m.Previsto,
                    m.Realizado,
                    m.Pago
                )).ToList()
            )).ToList()
        );

        return Ok(dto);
    }

    [HttpPost]
    public async Task<ActionResult<ContratoDto>> CreateContrato(CriarContratoDto dto)
    {
        var contrato = new Contrato
        {
            Id = Guid.NewGuid(),
            Nome = dto.Nome,
            Descricao = dto.Descricao,
            NumeroMeses = dto.NumeroMeses,
            MesInicial = dto.MesInicial,
            AnoInicial = dto.AnoInicial,
            UsuarioId = dto.UsuarioId,
            DataCriacao = DateTime.UtcNow
        };

        _context.Contratos.Add(contrato);
        await _context.SaveChangesAsync();

        var result = new ContratoDto(
            contrato.Id,
            contrato.Nome,
            contrato.Descricao,
            contrato.NumeroMeses,
            contrato.MesInicial,
            contrato.AnoInicial,
            contrato.DataCriacao,
            contrato.UsuarioId,
            new List<ServicoDto>()
        );

        return CreatedAtAction(nameof(GetContrato), new { id = contrato.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateContrato(Guid id, AtualizarContratoDto dto)
    {
        var contrato = await _context.Contratos.FindAsync(id);

        if (contrato == null)
        {
            return NotFound();
        }

        contrato.Nome = dto.Nome;
        contrato.Descricao = dto.Descricao;
        contrato.NumeroMeses = dto.NumeroMeses;
        contrato.MesInicial = dto.MesInicial;
        contrato.AnoInicial = dto.AnoInicial;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteContrato(Guid id)
    {
        var contrato = await _context.Contratos.FindAsync(id);

        if (contrato == null)
        {
            return NotFound();
        }

        _context.Contratos.Remove(contrato);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
