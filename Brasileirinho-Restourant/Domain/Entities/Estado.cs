using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BrasileirinhoRestourant.Domain.Common;

namespace BrasileirinhoRestourant.Domain.Entities;

[Table("estado")]
public class Estado : EntityBase, IAuditavel
{
    [Required, StringLength(100), Column("nome_estado")]
    public string Nome { get; set; } = string.Empty;

    [Required, StringLength(2), Column("uf_estado")]
    public string Uf { get; set; } = string.Empty;

    [Column("pais_id")]
    public long? PaisId { get; set; }

    [ForeignKey(nameof(PaisId))]
    public Pais? Pais { get; set; }

    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;
    public DateTime UltimaModificacao { get; set; } = DateTime.UtcNow;

    public ICollection<Cidade> Cidades { get; set; } = new List<Cidade>();
}
