namespace BrasileirinhoRestourant.Domain.Common;

public interface IAuditavel
{
    public DateTime DataCadastro { get; set; }
    public DateTime UltimaModificacao { get; set; }
}
