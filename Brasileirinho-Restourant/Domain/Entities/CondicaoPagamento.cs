using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BrasileirinhoRestourant.Domain.Common;

namespace BrasileirinhoRestourant.Domain.Entities;

[Table("condicao_pagamento")]
public class CondicaoPagamento : EntityBase, IAuditavel
{
    [Required, StringLength(100), Column("nome_condicao_pagamento")]
    public string Nome { get; set; } = string.Empty;

    [StringLength(200), Column("descricao_condicao_pagamento")]
    public string? Descricao { get; set; }

    [Column("num_parcelas_condicao_pagamento")]
    public int NumeroParcelas { get; set; } = 1;

    [Column("prazo_dias_condicao_pagamento")]
    public int PrazoDias { get; set; } = 0;

    [Column("taxa_juros_condicao_pagamento", TypeName = "numeric(5,2)")]
    public decimal TaxaJuros { get; set; } = 0m;

    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;
    public DateTime UltimaModificacao { get; set; } = DateTime.UtcNow;
}
