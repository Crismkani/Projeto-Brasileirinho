using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BrasileirinhoRestourant.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace BrasileirinhoRestourant.Domain.Entities;

[Table("cliente")]
[Index(nameof(CpfCnpj), IsUnique = true)]
public class Cliente : EntityBase
{
    [Required, StringLength(255), Column("cliente")]
    public string Nome { get; set; } = string.Empty;

    [Column("tipo_pessoa_cliente")]
    public TipoPessoa TipoPessoa { get; set; } = TipoPessoa.Fisica;

    [Column("estrangeiro_cliente")]
    public bool Estrangeiro { get; set; }

    [StringLength(14), Column("cpf_cnpj_cliente")]
    public string? CpfCnpj { get; set; }

    [StringLength(100), Column("email_cliente")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    public string? Email { get; set; }

    [StringLength(20), Column("telefone_cliente")]
    public string? Telefone { get; set; }

    [Column("cidade_id")]
    public long? CidadeId { get; set; }

    [ForeignKey(nameof(CidadeId))]
    public Cidade? Cidade { get; set; }

    [Column("limite_credito_cliente", TypeName = "numeric(10,2)")]
    public decimal LimiteCredito { get; set; }
}
