namespace cronograma_atividades_backend.DTOs;

public record ContratoDto(
    Guid Id,
    string Nome,
    string Descricao,
    int NumeroMeses,
    int MesInicial,
    int AnoInicial,
    DateTime DataCriacao,
    Guid UsuarioId,
    List<ServicoDto> Servicos
);

public record ContratoResumoDto(
    Guid Id,
    string Nome,
    string Descricao,
    int NumeroMeses,
    int MesInicial,
    int AnoInicial,
    DateTime DataCriacao,
    Guid UsuarioId
);

public record CriarContratoDto(
    string Nome,
    string Descricao,
    int NumeroMeses,
    int MesInicial,
    int AnoInicial,
    Guid UsuarioId
);

public record AtualizarContratoDto(
    string Nome,
    string Descricao,
    int NumeroMeses,
    int MesInicial,
    int AnoInicial
);
