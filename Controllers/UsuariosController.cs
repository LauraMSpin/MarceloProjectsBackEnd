using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using cronograma_atividades_backend.Data;
using cronograma_atividades_backend.Entities;
using cronograma_atividades_backend.DTOs;

namespace cronograma_atividades_backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly AppDbContext _context;

    public UsuariosController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UsuarioDto>>> GetUsuarios()
    {
        var usuarios = await _context.Usuarios
            .OrderBy(u => u.Nome)
            .Select(u => new UsuarioDto(
                u.Id,
                u.Nome,
                u.Email,
                u.Empresa,
                u.DataCriacao
            ))
            .ToListAsync();

        return Ok(usuarios);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UsuarioDto>> GetUsuario(Guid id)
    {
        var usuario = await _context.Usuarios.FindAsync(id);

        if (usuario == null)
        {
            return NotFound();
        }

        return Ok(new UsuarioDto(
            usuario.Id,
            usuario.Nome,
            usuario.Email,
            usuario.Empresa,
            usuario.DataCriacao
        ));
    }

    [HttpPost]
    public async Task<ActionResult<UsuarioDto>> CreateUsuario(CriarUsuarioDto dto)
    {
        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            Nome = dto.Nome,
            Email = dto.Email,
            Empresa = dto.Empresa,
            DataCriacao = DateTime.UtcNow
        };

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        var result = new UsuarioDto(
            usuario.Id,
            usuario.Nome,
            usuario.Email,
            usuario.Empresa,
            usuario.DataCriacao
        );

        return CreatedAtAction(nameof(GetUsuario), new { id = usuario.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUsuario(Guid id, AtualizarUsuarioDto dto)
    {
        var usuario = await _context.Usuarios.FindAsync(id);

        if (usuario == null)
        {
            return NotFound();
        }

        usuario.Nome = dto.Nome;
        usuario.Email = dto.Email;
        usuario.Empresa = dto.Empresa;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUsuario(Guid id)
    {
        var usuario = await _context.Usuarios.FindAsync(id);

        if (usuario == null)
        {
            return NotFound();
        }

        _context.Usuarios.Remove(usuario);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
