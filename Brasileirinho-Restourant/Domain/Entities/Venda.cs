using System.ComponentModel.DataAnnotations.Schema;
using BrasileirinhoRestourant.Domain.Common;

namespace BrasileirinhoRestourant.Domain.Entities;

[Table("venda")]
public class Venda : EntityBase
{
    [Column("data_venda")]
    public DateTime DataVenda { get; set; } = DateTime.UtcNow;

    [Column("comanda_id")]
    public long? ComandaId { get; set; }

    [ForeignKey(nameof(ComandaId))]
    public Comanda? Comanda { get; set; }

    [Column("cliente_id")]
    public long? ClienteId { get; set; }

    public Cliente? Cliente { get; set; }

    [Column("forma_pagamento_id")]
    public long? FormaPagamentoId { get; set; }

    [ForeignKey(nameof(FormaPagamentoId))]
    public FormaPagamento? FormaPagamento { get; set; }

    [Column("condicao_pagamento_id")]
    public long? CondicaoPagamentoId { get; set; }

    [ForeignKey(nameof(CondicaoPagamentoId))]
    public CondicaoPagamento? CondicaoPagamento { get; set; }

    [Column("desconto_venda", TypeName = "numeric(12,2)")]
    public decimal Desconto { get; set; }

    [Column("acrescimo_venda", TypeName = "numeric(12,2)")]
    public decimal Acrescimo { get; set; }

    [Column("valor_pago_venda", TypeName = "numeric(12,2)")]
    public decimal ValorPago { get; set; }

    [Column("status_venda")]
    public StatusVenda Status { get; set; } = StatusVenda.Concluida;

    public ICollection<VendaItem> Itens { get; set; } = new List<VendaItem>();

    public decimal SubtotalItens { get; private set; }
    public decimal ValorTotal { get; private set; }

    public void RecalcularTotal()
    {
        decimal subtotal = 0m;
        foreach (var item in Itens)
        {
            item.RecalcularSubtotal();
            subtotal += item.Subtotal;
        }
        SubtotalItens = subtotal;

        var bruto = subtotal - Desconto + Acrescimo;
        ValorTotal = bruto < 0m ? 0m : decimal.Round(bruto, 2, MidpointRounding.AwayFromZero);
    }
}
