namespace cronograma_atividades_backend.Entities;

public class ContratoCompartilhado
{
    public Guid Id { get; set; }
    public Guid ContratoId { get; set; }
    public Contrato Contrato { get; set; } = null!;
    public Guid UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;
    public DateTime DataCompartilhamento { get; set; }
    public bool PodeEditar { get; set; } // Se true, pode editar; se false, só visualiza
}
