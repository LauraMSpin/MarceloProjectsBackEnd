using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using cronograma_atividades_backend.Data;
using cronograma_atividades_backend.DTOs;
using cronograma_atividades_backend.Entities;

namespace cronograma_atividades_backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthController(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpPost("login")]
    public async Task<ActionResult<TokenDto>> Login(LoginDto dto)
    {
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Login == dto.Login && u.Ativo);

        if (usuario == null || !VerificarSenha(dto.Senha, usuario.SenhaHash))
        {
            return Unauthorized(new { message = "Login ou senha inválidos" });
        }

        var token = GerarToken(usuario);
        return Ok(token);
    }

    [HttpPost("registro")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<UsuarioLogadoDto>> Registro(RegistroUsuarioDto dto)
    {
        if (await _context.Usuarios.AnyAsync(u => u.Login == dto.Login))
        {
            return BadRequest(new { message = "Login já está em uso" });
        }

        if (await _context.Usuarios.AnyAsync(u => u.Email == dto.Email))
        {
            return BadRequest(new { message = "Email já está em uso" });
        }

        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            Login = dto.Login,
            SenhaHash = GerarHashSenha(dto.Senha),
            Nome = dto.Nome,
            Email = dto.Email,
            Empresa = dto.Empresa,
            Role = "Usuario",
            DataCriacao = DateTime.UtcNow,
            Ativo = true
        };

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(ObterUsuarioAtual), new UsuarioLogadoDto(
            usuario.Id,
            usuario.Login,
            usuario.Nome,
            usuario.Email,
            usuario.Empresa,
            usuario.Role,
            usuario.Ativo
        ));
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UsuarioLogadoDto>> ObterUsuarioAtual()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null || !Guid.TryParse(userId, out var id))
        {
            return Unauthorized();
        }

        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario == null || !usuario.Ativo)
        {
            return Unauthorized();
        }

        return Ok(new UsuarioLogadoDto(
            usuario.Id,
            usuario.Login,
            usuario.Nome,
            usuario.Email,
            usuario.Empresa,
            usuario.Role,
            usuario.Ativo
        ));
    }

    [HttpGet("usuarios")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<UsuarioLogadoDto>>> ListarUsuarios()
    {
        var usuarios = await _context.Usuarios
            .Select(u => new UsuarioLogadoDto(
                u.Id,
                u.Login,
                u.Nome,
                u.Email,
                u.Empresa,
                u.Role,
                u.Ativo
            ))
            .ToListAsync();

        return Ok(usuarios);
    }

    [HttpPost("usuarios")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<UsuarioLogadoDto>> CriarUsuario(CriarUsuarioAdminDto dto)
    {
        if (await _context.Usuarios.AnyAsync(u => u.Login == dto.Login))
        {
            return BadRequest(new { message = "Login já está em uso" });
        }

        if (await _context.Usuarios.AnyAsync(u => u.Email == dto.Email))
        {
            return BadRequest(new { message = "Email já está em uso" });
        }

        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            Login = dto.Login,
            SenhaHash = GerarHashSenha(dto.Senha),
            Nome = dto.Nome,
            Email = dto.Email,
            Empresa = dto.Empresa,
            Role = dto.Role,
            DataCriacao = DateTime.UtcNow,
            Ativo = true
        };

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(ListarUsuarios), new UsuarioLogadoDto(
            usuario.Id,
            usuario.Login,
            usuario.Nome,
            usuario.Email,
            usuario.Empresa,
            usuario.Role,
            usuario.Ativo
        ));
    }

    [HttpPut("usuarios/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<UsuarioLogadoDto>> AtualizarUsuario(Guid id, AtualizarUsuarioAdminDto dto)
    {
        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario == null)
        {
            return NotFound();
        }

        if (await _context.Usuarios.AnyAsync(u => u.Email == dto.Email && u.Id != id))
        {
            return BadRequest(new { message = "Email já está em uso" });
        }

        usuario.Nome = dto.Nome;
        usuario.Email = dto.Email;
        usuario.Empresa = dto.Empresa;
        usuario.Role = dto.Role;
        usuario.Ativo = dto.Ativo;

        if (!string.IsNullOrEmpty(dto.NovaSenha))
        {
            usuario.SenhaHash = GerarHashSenha(dto.NovaSenha);
        }

        await _context.SaveChangesAsync();

        return Ok(new UsuarioLogadoDto(
            usuario.Id,
            usuario.Login,
            usuario.Nome,
            usuario.Email,
            usuario.Empresa,
            usuario.Role,
            usuario.Ativo
        ));
    }

    [HttpDelete("usuarios/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> DeletarUsuario(Guid id)
    {
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (currentUserId != null && Guid.TryParse(currentUserId, out var currentId) && currentId == id)
        {
            return BadRequest(new { message = "Você não pode deletar seu próprio usuário" });
        }

        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario == null)
        {
            return NotFound();
        }

        _context.Usuarios.Remove(usuario);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private TokenDto GerarToken(Usuario usuario)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiracao = DateTime.UtcNow.AddHours(8);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Name, usuario.Login),
            new Claim(ClaimTypes.Email, usuario.Email),
            new Claim(ClaimTypes.Role, usuario.Role)
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expiracao,
            signingCredentials: credentials
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return new TokenDto(
            tokenString,
            expiracao,
            new UsuarioLogadoDto(
                usuario.Id,
                usuario.Login,
                usuario.Nome,
                usuario.Email,
                usuario.Empresa,
                usuario.Role,
                usuario.Ativo
            )
        );
    }

    private static string GerarHashSenha(string senha)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(senha);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    private static bool VerificarSenha(string senha, string hash)
    {
        var senhaHash = GerarHashSenha(senha);
        return senhaHash == hash;
    }
}
