using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BrasileirinhoRestourant.Domain.Common;

namespace BrasileirinhoRestourant.Domain.Entities;

[Table("marca")]
public class Marca : EntityBase
{
    [Required, StringLength(60), Column("marca")]
    public string Nome { get; set; } = string.Empty;
}
