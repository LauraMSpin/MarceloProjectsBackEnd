namespace cronograma_atividades_backend.DTOs;

public record LoginDto(
    string Login,
    string Senha
);

public record RegistroUsuarioDto(
    string Login,
    string Senha,
    string Nome,
    string Email,
    string? Empresa
);

public record TokenDto(
    string Token,
    DateTime Expiracao,
    UsuarioLogadoDto Usuario
);

public record UsuarioLogadoDto(
    Guid Id,
    string Login,
    string Nome,
    string Email,
    string? Empresa,
    string Role,
    bool Ativo
);

public record CriarUsuarioAdminDto(
    string Login,
    string Senha,
    string Nome,
    string Email,
    string? Empresa,
    string Role
);

public record AtualizarUsuarioAdminDto(
    string Nome,
    string Email,
    string? Empresa,
    string Role,
    bool Ativo,
    string? NovaSenha
);
