namespace cronograma_atividades_backend.Entities;

public class PagamentoMensal
{
    public Guid Id { get; set; }
    public int Ordem { get; set; } // Índice do mês (0, 1, 2, ...)
    public string Mes { get; set; } = string.Empty;
    public decimal Valor { get; set; }

    // Chave estrangeira
    public Guid ContratoId { get; set; }
    public Contrato Contrato { get; set; } = null!;
}
