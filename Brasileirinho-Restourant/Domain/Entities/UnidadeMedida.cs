using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BrasileirinhoRestourant.Domain.Common;

namespace BrasileirinhoRestourant.Domain.Entities;

[Table("unidade_medida")]
public class UnidadeMedida : EntityBase
{
    [Required, StringLength(255), Column("unidade_medida")]
    public string Nome { get; set; } = string.Empty;

    [StringLength(5), Column("sigla_unidade_medida")]
    public string? Sigla { get; set; }

    [Column("fracionavel_unidade_medida")]
    public bool Fracionavel { get; set; }
}
