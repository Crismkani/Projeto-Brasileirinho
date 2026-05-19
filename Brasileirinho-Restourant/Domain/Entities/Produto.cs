using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BrasileirinhoRestourant.Domain.Common;

namespace BrasileirinhoRestourant.Domain.Entities;

[Table("produto")]
public class Produto : EntityBase
{
    [Required, StringLength(255), Column("produto")]
    public string Nome { get; set; } = string.Empty;

    [Column("marca_id")]
    public long MarcaId { get; set; }

    [ForeignKey(nameof(MarcaId))]
    public Marca? Marca { get; set; }

    [Column("unidade_medida_id")]
    public long UnidadeMedidaId { get; set; }

    [ForeignKey(nameof(UnidadeMedidaId))]
    public UnidadeMedida? UnidadeMedida { get; set; }

    [Column("categoria_id")]
    public long? CategoriaId { get; set; }

    [ForeignKey(nameof(CategoriaId))]
    public Categoria? Categoria { get; set; }

    [Column("valor_compra_produto", TypeName = "numeric(10,2)")]
    public decimal ValorCompra { get; set; }

    [Column("valor_venda_produto", TypeName = "numeric(10,2)")]
    public decimal ValorVenda { get; set; }

    [Column("quantidade_produto", TypeName = "numeric(15,4)")]
    public decimal Quantidade { get; set; }

    [Column("quantidade_minima_produto", TypeName = "numeric(15,4)")]
    public decimal QuantidadeMinima { get; set; } = 1m;

    [Column("preco_kg_produto", TypeName = "numeric(10,2)")]
    public decimal PrecoKg { get; set; }

    [Column("tara_produto", TypeName = "numeric(10,3)")]
    public decimal Tara { get; set; }

    [Column("permite_fracionamento_produto")]
    public bool PermiteFracionamento { get; set; }

    [StringLength(500), Column("foto_url_produto")]
    public string? FotoUrl { get; set; }

    [StringLength(20), Column("codigo_barras_produto")]
    public string? CodigoBarras { get; set; }
}
