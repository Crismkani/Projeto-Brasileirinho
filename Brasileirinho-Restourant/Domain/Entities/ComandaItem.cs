using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BrasileirinhoRestourant.Domain.Common;

namespace BrasileirinhoRestourant.Domain.Entities;

[Table("comanda_item")]
public class ComandaItem : EntityBase
{
    [Column("comanda_id")]
    public long ComandaId { get; set; }

    [ForeignKey(nameof(ComandaId))]
    public Comanda? Comanda { get; set; }

    [Column("produto_id")]
    public long ProdutoId { get; set; }

    [ForeignKey(nameof(ProdutoId))]
    public Produto? Produto { get; set; }

    [Column("quantidade_comanda_item", TypeName = "numeric(14,3)")]
    public decimal Quantidade { get; set; } = 1m;

    [Column("peso_bruto_comanda_item", TypeName = "numeric(10,3)")]
    public decimal PesoBruto { get; set; }

    [Column("tara_comanda_item", TypeName = "numeric(10,3)")]
    public decimal Tara { get; set; }

    [Column("preco_unitario_comanda_item", TypeName = "numeric(12,2)")]
    public decimal PrecoUnitario { get; set; }

    [Column("fracionado_comanda_item")]
    public bool Fracionado { get; set; }

    [StringLength(255), Column("observacao_comanda_item")]
    public string? Observacao { get; set; }

    public decimal Subtotal { get; private set; }

    public void RecalcularSubtotal()
    {
        if (Fracionado)
        {
            var pesoLiquido = PesoBruto - Tara;
            if (pesoLiquido < 0m) pesoLiquido = 0m;
            Subtotal = decimal.Round(pesoLiquido * PrecoUnitario, 2, MidpointRounding.AwayFromZero);
        }
        else
        {
            Subtotal = decimal.Round(Quantidade * PrecoUnitario, 2, MidpointRounding.AwayFromZero);
        }
    }
}
