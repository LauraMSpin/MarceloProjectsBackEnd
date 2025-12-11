namespace cronograma_atividades_backend.Entities;

public class Medicao
{
    public Guid Id { get; set; }
    public string Mes { get; set; } = string.Empty;
    public decimal Previsto { get; set; }
    public decimal Realizado { get; set; }
    public decimal Pago { get; set; }

    // Chave estrangeira
    public Guid ServicoId { get; set; }
    public Servico Servico { get; set; } = null!;
}
