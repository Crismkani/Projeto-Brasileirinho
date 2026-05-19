using BrasileirinhoRestourant.Domain.Common;
using BrasileirinhoRestourant.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BrasileirinhoRestourant.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Pais> Paises => Set<Pais>();
    public DbSet<Estado> Estados => Set<Estado>();
    public DbSet<Cidade> Cidades => Set<Cidade>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Marca> Marcas => Set<Marca>();
    public DbSet<UnidadeMedida> UnidadesMedida => Set<UnidadeMedida>();
    public DbSet<Produto> Produtos => Set<Produto>();
    public DbSet<FormaPagamento> FormasPagamento => Set<FormaPagamento>();
    public DbSet<CondicaoPagamento> CondicoesPagamento => Set<CondicaoPagamento>();
    public DbSet<Comanda> Comandas => Set<Comanda>();
    public DbSet<ComandaItem> ComandaItens => Set<ComandaItem>();
    public DbSet<Venda> Vendas => Set<Venda>();
    public DbSet<VendaItem> VendaItens => Set<VendaItem>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Fornecedor> Fornecedores => Set<Fornecedor>();
    public DbSet<ContaPagar> ContasPagar => Set<ContaPagar>();
    public DbSet<ContaReceber> ContasReceber => Set<ContaReceber>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Mapeia colunas herdadas (id, ativo, data_cadastro, ultima_modificacao) com sufixo da tabela.
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(EntityBase).IsAssignableFrom(entityType.ClrType))
            {
                continue;
            }

            var tabela = entityType.GetTableName();
            if (string.IsNullOrEmpty(tabela))
            {
                continue;
            }

            var eb = modelBuilder.Entity(entityType.ClrType);
            eb.Property(nameof(EntityBase.Id)).HasColumnName($"id_{tabela}");
            eb.Property(nameof(EntityBase.Ativo)).HasColumnName($"ativo_{tabela}");

            if (typeof(IAuditavel).IsAssignableFrom(entityType.ClrType))
            {
                eb.Property(nameof(IAuditavel.DataCadastro))
                  .HasColumnName($"data_cadastro_{tabela}")
                  .HasColumnType("timestamp with time zone");

                eb.Property(nameof(IAuditavel.UltimaModificacao))
                  .HasColumnName($"ultima_modificacao_{tabela}")
                  .HasColumnType("timestamp with time zone");
            }
        }

        modelBuilder.Entity<Produto>(e =>
        {
            e.HasIndex(p => p.Nome)
             .IsUnique()
             .HasDatabaseName("uq_produto_nome");

            e.HasIndex(p => p.CodigoBarras)
             .IsUnique()
             .HasDatabaseName("uq_produto_codigo_barras")
             .HasFilter("codigo_barras_produto IS NOT NULL");
        });

        modelBuilder.Entity<Comanda>(e =>
        {
            e.Ignore(c => c.ValorTotal);
            e.HasOne(c => c.Cliente)
             .WithMany()
             .HasForeignKey(c => c.ClienteId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ComandaItem>(e =>
        {
            e.Ignore(c => c.Subtotal);
            e.HasOne(c => c.Comanda)
             .WithMany(c => c.Itens)
             .HasForeignKey(c => c.ComandaId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(c => c.Produto)
             .WithMany()
             .HasForeignKey(c => c.ProdutoId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Venda>(e =>
        {
            e.Ignore(v => v.SubtotalItens);
            e.Ignore(v => v.ValorTotal);
            e.HasOne(v => v.Cliente)
             .WithMany()
             .HasForeignKey(v => v.ClienteId)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(v => v.Comanda)
             .WithMany()
             .HasForeignKey(v => v.ComandaId)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(v => v.FormaPagamento)
             .WithMany()
             .HasForeignKey(v => v.FormaPagamentoId)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(v => v.CondicaoPagamento)
             .WithMany()
             .HasForeignKey(v => v.CondicaoPagamentoId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Fornecedor>(e =>
        {
            e.HasOne(f => f.CondicaoPagamento)
             .WithMany()
             .HasForeignKey(f => f.CondicaoPagamentoId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ContaPagar>(e =>
        {
            e.Property(c => c.Status).HasConversion<string>();
            e.HasOne(c => c.Fornecedor)
             .WithMany()
             .HasForeignKey(c => c.FornecedorId)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(c => c.FormaPagamento)
             .WithMany()
             .HasForeignKey(c => c.FormaPagamentoId)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(c => c.CondicaoPagamento)
             .WithMany()
             .HasForeignKey(c => c.CondicaoPagamentoId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ContaReceber>(e =>
        {
            e.Property(c => c.Status).HasConversion<string>();
            e.HasOne(c => c.Cliente)
             .WithMany()
             .HasForeignKey(c => c.ClienteId)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(c => c.Venda)
             .WithMany()
             .HasForeignKey(c => c.VendaId)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(c => c.FormaPagamento)
             .WithMany()
             .HasForeignKey(c => c.FormaPagamentoId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<VendaItem>(e =>
        {
            e.Ignore(v => v.Subtotal);
            e.HasOne(v => v.Venda)
             .WithMany(v => v.Itens)
             .HasForeignKey(v => v.VendaId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(v => v.Produto)
             .WithMany()
             .HasForeignKey(v => v.ProdutoId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Categoria>().HasData(
            new Categoria { Id = 1, Nome = "Pratos Quentes", Ativo = true },
            new Categoria { Id = 2, Nome = "Bebidas", Ativo = true },
            new Categoria { Id = 3, Nome = "Sobremesas", Ativo = true });

        modelBuilder.Entity<Marca>().HasData(
            new Marca { Id = 1, Nome = "Casa", Ativo = true },
            new Marca { Id = 2, Nome = "Coca-Cola", Ativo = true });

        modelBuilder.Entity<UnidadeMedida>().HasData(
            new UnidadeMedida { Id = 1, Nome = "Unidade", Sigla = "UN", Fracionavel = false, Ativo = true },
            new UnidadeMedida { Id = 2, Nome = "Quilograma", Sigla = "KG", Fracionavel = true, Ativo = true },
            new UnidadeMedida { Id = 3, Nome = "Litro", Sigla = "LT", Fracionavel = true, Ativo = true });
    }

    public override int SaveChanges()
    {
        AplicarAuditoria();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        AplicarAuditoria();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void AplicarAuditoria()
    {
        var agora = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries<IAuditavel>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.DataCadastro = agora;
                entry.Entity.UltimaModificacao = agora;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UltimaModificacao = agora;
            }
        }
    }
}
