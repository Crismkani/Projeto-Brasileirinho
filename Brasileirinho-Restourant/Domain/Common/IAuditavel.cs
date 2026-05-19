namespace BrasileirinhoRestourant.Domain.Common;

public interface IAuditavel
{
    DateTime DataCadastro { get; }
    DateTime UltimaModificacao { get; }

    void MarcarComoNovo(DateTime quando);
    void RegistrarModificacao(DateTime quando);
}
