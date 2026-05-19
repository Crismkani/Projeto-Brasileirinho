using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BrasileirinhoRestourant.Domain.Common;

namespace BrasileirinhoRestourant.Domain.Entities;

[Table("conta_receber")]
public class ContaReceber : EntidadeAuditavel
{
    [Column("cliente_id")]
    public long? ClienteId { get; set; }

    [ForeignKey(nameof(ClienteId))]
    public Cliente? Cliente { get; set; }

    [Column("venda_id")]
    public long? VendaId { get; set; }

    [ForeignKey(nameof(VendaId))]
    public Venda? Venda { get; set; }

    [Required, StringLength(255), Column("descricao_conta_receber")]
    public string Descricao { get; set; } = string.Empty;

    [Column("valor_conta_receber", TypeName = "numeric(12,2)")]
    public decimal Valor { get; set; }

    [Column("data_emissao_conta_receber")]
    public DateOnly DataEmissao { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Column("data_vencimento_conta_receber")]
    public DateOnly DataVencimento { get; set; }

    [Column("data_recebimento_conta_receber")]
    public DateOnly? DataRecebimento { get; set; }

    [Column("valor_recebido_conta_receber", TypeName = "numeric(12,2)")]
    public decimal? ValorRecebido { get; set; }

    [Column("status_conta_receber")]
    public StatusContaReceber Status { get; set; } = StatusContaReceber.Pendente;

    [Column("forma_pagamento_id")]
    public long? FormaPagamentoId { get; set; }

    [ForeignKey(nameof(FormaPagamentoId))]
    public FormaPagamento? FormaPagamento { get; set; }

    [StringLength(500), Column("observacao_conta_receber")]
    public string? Observacao { get; set; }

    [NotMapped]
    public bool Atrasado =>
        Status == StatusContaReceber.Pendente &&
        DataVencimento < DateOnly.FromDateTime(DateTime.Today);
}
