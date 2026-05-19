using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BrasileirinhoRestourant.Domain.Common;

namespace BrasileirinhoRestourant.Domain.Entities;

[Table("categoria")]
public class Categoria : EntityBase
{
    [Required, StringLength(60), Column("categoria")]
    public string Nome { get; set; } = string.Empty;
}
