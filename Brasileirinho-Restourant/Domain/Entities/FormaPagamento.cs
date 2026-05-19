using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BrasileirinhoRestourant.Domain.Common;

namespace BrasileirinhoRestourant.Domain.Entities;

[Table("forma_pagamento")]
public class FormaPagamento : EntityBase, IAuditavel
{
    [Required, StringLength(100), Column("nome_forma_pagamento")]
    public string Nome { get; set; } = string.Empty;

    [Required, StringLength(100), Column("descricao_forma_pagamento")]
    public string Descricao { get; set; } = string.Empty;

    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;
    public DateTime UltimaModificacao { get; set; } = DateTime.UtcNow;
}
