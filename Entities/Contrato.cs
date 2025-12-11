namespace cronograma_atividades_backend.Entities;

public class Contrato
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public int NumeroMeses { get; set; }
    public int MesInicial { get; set; }
    public int AnoInicial { get; set; }
    public DateTime DataCriacao { get; set; }

    // Chave estrangeira
    public Guid UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;

    // Navegação
    public ICollection<Servico> Servicos { get; set; } = new List<Servico>();
}
