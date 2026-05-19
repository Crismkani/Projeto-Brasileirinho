using BrasileirinhoRestourant.Domain.Entities;

namespace BrasileirinhoRestourant.Application.Services;

public interface IServicoContasReceber
{
    Task<IReadOnlyList<ContaReceber>> ListarAsync(
        StatusContaReceber? status = null,
        DateOnly? vencimentoDe = null,
        DateOnly? vencimentoAte = null,
        long? clienteId = null,
        CancellationToken ct = default);

    Task<ContaReceber> SalvarAsync(ContaReceber conta, CancellationToken ct = default);

    Task RegistrarRecebimentoAsync(
        long id,
        DateOnly dataRecebimento,
        decimal valorRecebido,
        long? formaPagamentoId,
        CancellationToken ct = default);

    Task CancelarAsync(long id, CancellationToken ct = default);

    Task ExcluirAsync(long id, CancellationToken ct = default);
}
