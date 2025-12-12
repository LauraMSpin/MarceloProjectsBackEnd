namespace cronograma_atividades_backend.DTOs;

public record MedicaoDto(
    Guid Id,
    int Ordem,
    string Mes,
    decimal Previsto,
    decimal Realizado
);

public record CriarMedicaoDto(
    int Ordem,
    string Mes,
    decimal Previsto,
    decimal Realizado
);

public record AtualizarMedicaoDto(
    int Ordem,
    string Mes,
    decimal Previsto,
    decimal Realizado
);

public record PagamentoMensalDto(
    Guid Id,
    int Ordem,
    string Mes,
    decimal Valor
);

public record CriarPagamentoMensalDto(
    int Ordem,
    string Mes,
    decimal Valor
);

public record AtualizarPagamentoMensalDto(
    int Ordem,
    string Mes,
    decimal Valor
);
