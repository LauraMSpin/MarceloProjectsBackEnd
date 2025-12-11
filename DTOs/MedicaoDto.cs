namespace cronograma_atividades_backend.DTOs;

public record MedicaoDto(
    Guid Id,
    string Mes,
    decimal Previsto,
    decimal Realizado,
    decimal Pago
);

public record CriarMedicaoDto(
    string Mes,
    decimal Previsto,
    decimal Realizado,
    decimal Pago
);

public record AtualizarMedicaoDto(
    string Mes,
    decimal Previsto,
    decimal Realizado,
    decimal Pago
);
