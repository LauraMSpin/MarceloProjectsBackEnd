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
    string? NomeProprietario,
    bool IsProprietario,
    bool PodeEditar,
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
    Guid UsuarioId,
    string? NomeProprietario,
    bool IsProprietario,
    bool PodeEditar
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

public record CompartilharContratoDto(
    Guid UsuarioId,
    bool PodeEditar
);

public record ContratoCompartilhadoDto(
    Guid Id,
    Guid UsuarioId,
    string NomeUsuario,
    string EmailUsuario,
    bool PodeEditar,
    DateTime DataCompartilhamento
);
