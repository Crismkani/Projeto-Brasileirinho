using BrasileirinhoRestourant.Domain.Entities;

namespace BrasileirinhoRestourant.Application.Services;

public interface IServicoContasReceber
{
    public Task<IReadOnlyList<ContaReceber>> ListarAsync(
        StatusContaReceber? status = null,
        DateOnly? vencimentoDe = null,
        DateOnly? vencimentoAte = null,
        long? clienteId = null,
        CancellationToken ct = default);

    public Task<ContaReceber> SalvarAsync(ContaReceber conta, CancellationToken ct = default);

    public Task RegistrarRecebimentoAsync(
        long id,
        DateOnly dataRecebimento,
        decimal valorRecebido,
        long? formaPagamentoId,
        CancellationToken ct = default);

    public Task CancelarAsync(long id, CancellationToken ct = default);

    public Task ExcluirAsync(long id, CancellationToken ct = default);
}
