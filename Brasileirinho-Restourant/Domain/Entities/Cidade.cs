using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BrasileirinhoRestourant.Domain.Common;

namespace BrasileirinhoRestourant.Domain.Entities;

[Table("cidade")]
public class Cidade : EntityBase, IAuditavel
{
    [Required, StringLength(100), Column("nome_cidade")]
    public string Nome { get; set; } = string.Empty;

    [StringLength(10), Column("codigo_ibge_cidade")]
    public string? CodigoIbge { get; set; }

    [Column("estado_id")]
    public long EstadoId { get; set; }

    [ForeignKey(nameof(EstadoId))]
    public Estado? Estado { get; set; }

    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;
    public DateTime UltimaModificacao { get; set; } = DateTime.UtcNow;
}
