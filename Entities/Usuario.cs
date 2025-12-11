namespace cronograma_atividades_backend.Entities;

public class Usuario
{
    public Guid Id { get; set; }
    public string Login { get; set; } = string.Empty;
    public string SenhaHash { get; set; } = string.Empty;
    public string Role { get; set; } = "Usuario"; // "Admin" ou "Usuario"
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Empresa { get; set; }
    public DateTime DataCriacao { get; set; }
    public bool Ativo { get; set; } = true;

    // Navegação
    public ICollection<Contrato> Contratos { get; set; } = new List<Contrato>();
}
