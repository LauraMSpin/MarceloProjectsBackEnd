namespace cronograma_atividades_backend.DTOs;

public record UsuarioDto(
    Guid Id,
    string Nome,
    string Email,
    string? Empresa,
    DateTime DataCriacao
);

public record CriarUsuarioDto(
    string Nome,
    string Email,
    string? Empresa
);

public record AtualizarUsuarioDto(
    string Nome,
    string Email,
    string? Empresa
);
