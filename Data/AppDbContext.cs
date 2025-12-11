using Microsoft.EntityFrameworkCore;
using cronograma_atividades_backend.Entities;

namespace cronograma_atividades_backend.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Contrato> Contratos => Set<Contrato>();
    public DbSet<Servico> Servicos => Set<Servico>();
    public DbSet<Medicao> Medicoes => Set<Medicao>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuração Usuario
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Login).IsRequired().HasMaxLength(100);
            entity.Property(e => e.SenhaHash).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Role).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Empresa).HasMaxLength(200);
            entity.HasIndex(e => e.Login).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // Configuração Contrato
        modelBuilder.Entity<Contrato>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Descricao).HasMaxLength(500);
            
            entity.HasOne(e => e.Usuario)
                .WithMany(u => u.Contratos)
                .HasForeignKey(e => e.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configuração Servico
        modelBuilder.Entity<Servico>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Item).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ServicoNome).IsRequired().HasMaxLength(300);
            entity.Property(e => e.ValorTotal).HasPrecision(18, 2);
            
            entity.HasOne(e => e.Contrato)
                .WithMany(c => c.Servicos)
                .HasForeignKey(e => e.ContratoId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configuração Medicao
        modelBuilder.Entity<Medicao>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Mes).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Previsto).HasPrecision(18, 2);
            entity.Property(e => e.Realizado).HasPrecision(18, 2);
            entity.Property(e => e.Pago).HasPrecision(18, 2);
            
            entity.HasOne(e => e.Servico)
                .WithMany(s => s.Medicoes)
                .HasForeignKey(e => e.ServicoId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
