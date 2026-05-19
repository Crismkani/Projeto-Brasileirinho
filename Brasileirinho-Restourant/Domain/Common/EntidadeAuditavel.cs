namespace BrasileirinhoRestourant.Domain.Common;

public abstract class EntidadeAuditavel : EntityBase, IAuditavel
{
    public DateTime DataCadastro { get; private set; } = DateTime.UtcNow;

    public DateTime UltimaModificacao { get; private set; } = DateTime.UtcNow;

    public void MarcarComoNovo(DateTime quando)
    {
        DataCadastro = quando;
        UltimaModificacao = quando;
    }

    public void RegistrarModificacao(DateTime quando)
        => UltimaModificacao = quando;
}
