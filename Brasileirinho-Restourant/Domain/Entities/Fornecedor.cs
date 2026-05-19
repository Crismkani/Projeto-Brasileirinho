using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BrasileirinhoRestourant.Domain.Common;

namespace BrasileirinhoRestourant.Domain.Entities;

[Table("fornecedor")]
public class Fornecedor : EntityBase
{
    [Required, StringLength(255), Column("fornecedor")]
    public string Nome { get; set; } = string.Empty;

    [Column("tipo_pessoa_fornecedor")]
    public TipoPessoa TipoPessoa { get; set; } = TipoPessoa.Juridica;

    [Column("estrangeiro_fornecedor")]
    public bool Estrangeiro { get; set; }

    [StringLength(14), Column("cpf_cnpj_fornecedor")]
    public string? CpfCnpj { get; set; }

    [Required, StringLength(255), Column("email_fornecedor")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(255), Column("telefone_fornecedor")]
    public string Telefone { get; set; } = string.Empty;

    [Column("cidade_id")]
    public long? CidadeId { get; set; }

    [ForeignKey(nameof(CidadeId))]
    public Cidade? Cidade { get; set; }

    [Column("condicao_pagamento_id")]
    public long? CondicaoPagamentoId { get; set; }

    [ForeignKey(nameof(CondicaoPagamentoId))]
    public CondicaoPagamento? CondicaoPagamento { get; set; }
}
