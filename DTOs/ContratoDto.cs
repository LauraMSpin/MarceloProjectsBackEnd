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
    decimal PercentualReajuste,
    int? MesInicioReajuste,
    List<ServicoDto> Servicos,
    List<PagamentoMensalDto> PagamentosMensais
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
    bool PodeEditar,
    decimal PercentualReajuste,
    int? MesInicioReajuste
);

public record CriarContratoDto(
    string Nome,
    string Descricao,
    int NumeroMeses,
    int MesInicial,
    int AnoInicial,
    Guid UsuarioId,
    decimal PercentualReajuste = 0,
    int? MesInicioReajuste = null
);

public record AtualizarContratoDto(
    string Nome,
    string Descricao,
    int NumeroMeses,
    int MesInicial,
    int AnoInicial,
    decimal PercentualReajuste = 0,
    int? MesInicioReajuste = null
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
