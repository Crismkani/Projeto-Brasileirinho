using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BrasileirinhoRestourant.Domain.Common;

namespace BrasileirinhoRestourant.Domain.Entities;

[Table("comanda")]
public class Comanda : EntityBase
{
    [Required, StringLength(20), Column("numero_comanda")]
    public string Numero { get; set; } = string.Empty;

    [Column("cliente_id")]
    public long? ClienteId { get; set; }

    public Cliente? Cliente { get; set; }

    [StringLength(80), Column("cliente_avulso_comanda")]
    public string? ClienteAvulso { get; set; }

    [Column("data_abertura_comanda")]
    public DateTime DataAbertura { get; set; } = DateTime.UtcNow;

    [Column("data_fechamento_comanda")]
    public DateTime? DataFechamento { get; set; }

    [Column("status_comanda")]
    public StatusComanda Status { get; set; } = StatusComanda.Aberta;

    public ICollection<ComandaItem> Itens { get; set; } = new List<ComandaItem>();

    public decimal ValorTotal { get; private set; }

    public void RecalcularTotal()
    {
        decimal total = 0m;
        foreach (var item in Itens)
        {
            item.RecalcularSubtotal();
            total += item.Subtotal;
        }
        ValorTotal = total;
    }
}
