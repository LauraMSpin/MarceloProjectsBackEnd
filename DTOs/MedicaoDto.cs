namespace cronograma_atividades_backend.DTOs;

public record MedicaoDto(
    Guid Id,
    int Ordem,
    string Mes,
    decimal Previsto,
    decimal Realizado,
    decimal Pago
);

public record CriarMedicaoDto(
    int Ordem,
    string Mes,
    decimal Previsto,
    decimal Realizado,
    decimal Pago
);

public record AtualizarMedicaoDto(
    int Ordem,
    string Mes,
    decimal Previsto,
    decimal Realizado,
    decimal Pago
);
