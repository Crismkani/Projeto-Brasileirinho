using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BrasileirinhoRestourant.Domain.Common;

namespace BrasileirinhoRestourant.Domain.Entities;

[Table("conta_pagar")]
public class ContaPagar : EntityBase, IAuditavel
{
    [Column("fornecedor_id")]
    public long? FornecedorId { get; set; }

    [ForeignKey(nameof(FornecedorId))]
    public Fornecedor? Fornecedor { get; set; }

    [StringLength(50), Column("nro_documento_conta_pagar")]
    public string? NumeroDocumento { get; set; }

    [Required, StringLength(255), Column("descricao_conta_pagar")]
    public string Descricao { get; set; } = string.Empty;

    [StringLength(500), Column("arquivo_nota_conta_pagar")]
    public string? ArquivoNota { get; set; }

    [Column("valor_conta_pagar", TypeName = "numeric(12,2)")]
    public decimal Valor { get; set; }

    [Column("data_emissao_conta_pagar")]
    public DateOnly DataEmissao { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Column("data_vencimento_conta_pagar")]
    public DateOnly DataVencimento { get; set; }

    [Column("data_pagamento_conta_pagar")]
    public DateOnly? DataPagamento { get; set; }

    [Column("valor_pago_conta_pagar", TypeName = "numeric(12,2)")]
    public decimal? ValorPago { get; set; }

    [Column("status_conta_pagar")]
    public StatusContaPagar Status { get; set; } = StatusContaPagar.Pendente;

    [Column("forma_pagamento_id")]
    public long? FormaPagamentoId { get; set; }

    [ForeignKey(nameof(FormaPagamentoId))]
    public FormaPagamento? FormaPagamento { get; set; }

    [Column("condicao_pagamento_id")]
    public long? CondicaoPagamentoId { get; set; }

    [ForeignKey(nameof(CondicaoPagamentoId))]
    public CondicaoPagamento? CondicaoPagamento { get; set; }

    [StringLength(500), Column("observacao_conta_pagar")]
    public string? Observacao { get; set; }

    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;
    public DateTime UltimaModificacao { get; set; } = DateTime.UtcNow;

    [NotMapped]
    public bool Atrasado =>
        Status == StatusContaPagar.Pendente &&
        DataVencimento < DateOnly.FromDateTime(DateTime.Today);
}
