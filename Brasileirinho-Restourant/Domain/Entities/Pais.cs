using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BrasileirinhoRestourant.Domain.Common;

namespace BrasileirinhoRestourant.Domain.Entities;

[Table("pais")]
public class Pais : EntityBase, IAuditavel
{
    [Required, StringLength(100), Column("nome_pais")]
    public string Nome { get; set; } = string.Empty;

    [StringLength(5), Column("codigo_pais")]
    public string? Codigo { get; set; }

    [StringLength(5), Column("sigla_pais")]
    public string? Sigla { get; set; }

    [StringLength(100), Column("nacionalidade_pais")]
    public string? Nacionalidade { get; set; }

    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;
    public DateTime UltimaModificacao { get; set; } = DateTime.UtcNow;

    public ICollection<Estado> Estados { get; set; } = new List<Estado>();
}
