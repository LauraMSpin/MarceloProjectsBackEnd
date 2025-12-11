namespace cronograma_atividades_backend.Entities;

public class Servico
{
    public Guid Id { get; set; }
    public string Item { get; set; } = string.Empty;
    public string ServicoNome { get; set; } = string.Empty;
    public decimal ValorTotal { get; set; }

    // Chave estrangeira
    public Guid ContratoId { get; set; }
    public Contrato Contrato { get; set; } = null!;

    // Navegação
    public ICollection<Medicao> Medicoes { get; set; } = new List<Medicao>();
}
