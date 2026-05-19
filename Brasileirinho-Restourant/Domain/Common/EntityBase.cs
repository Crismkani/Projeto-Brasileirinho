using System.ComponentModel.DataAnnotations;

namespace BrasileirinhoRestourant.Domain.Common;

public abstract class EntityBase
{
    [Key]
    public long Id { get; set; }

    public bool Ativo { get; set; } = true;
}
