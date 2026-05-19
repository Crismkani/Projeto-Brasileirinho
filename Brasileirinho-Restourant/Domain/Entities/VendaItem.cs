using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BrasileirinhoRestourant.Domain.Common;

namespace BrasileirinhoRestourant.Domain.Entities;

[Table("venda_item")]
public class VendaItem : EntityBase
{
    [Column("venda_id")]
    public long VendaId { get; set; }

    [ForeignKey(nameof(VendaId))]
    public Venda? Venda { get; set; }

    [Column("produto_id")]
    public long ProdutoId { get; set; }

    [ForeignKey(nameof(ProdutoId))]
    public Produto? Produto { get; set; }

    [Column("quantidade_venda_item", TypeName = "numeric(14,3)")]
    public decimal Quantidade { get; set; } = 1m;

    [Column("peso_bruto_venda_item", TypeName = "numeric(10,3)")]
    public decimal PesoBruto { get; set; }

    [Column("tara_venda_item", TypeName = "numeric(10,3)")]
    public decimal Tara { get; set; }

    [Column("preco_unitario_venda_item", TypeName = "numeric(12,2)")]
    public decimal PrecoUnitario { get; set; }

    [Column("fracionado_venda_item")]
    public bool Fracionado { get; set; }

    [StringLength(255), Column("observacao_venda_item")]
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
