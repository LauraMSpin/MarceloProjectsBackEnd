namespace cronograma_atividades_backend.Entities;

public class Usuario
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Empresa { get; set; }
    public DateTime DataCriacao { get; set; }

    // Navegação
    public ICollection<Contrato> Contratos { get; set; } = new List<Contrato>();
}
