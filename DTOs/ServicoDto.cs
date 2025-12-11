namespace cronograma_atividades_backend.DTOs;

public record ServicoDto(
    Guid Id,
    string Item,
    string Servico,
    decimal ValorTotal,
    Guid ContratoId,
    List<MedicaoDto> Medicoes
);

public record CriarServicoDto(
    string Item,
    string Servico,
    Guid ContratoId,
    List<CriarMedicaoDto> Medicoes
);

public record AtualizarServicoDto(
    string Item,
    string Servico,
    List<CriarMedicaoDto> Medicoes
);
