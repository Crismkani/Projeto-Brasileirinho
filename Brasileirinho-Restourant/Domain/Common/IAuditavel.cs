namespace BrasileirinhoRestourant.Domain.Common;

public interface IAuditavel
{
    DateTime DataCadastro { get; set; }
    DateTime UltimaModificacao { get; set; }
}
