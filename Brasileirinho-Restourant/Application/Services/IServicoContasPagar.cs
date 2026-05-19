using BrasileirinhoRestourant.Domain.Entities;

namespace BrasileirinhoRestourant.Application.Services;

public interface IServicoContasPagar
{
    Task<IReadOnlyList<ContaPagar>> ListarAsync(
        StatusContaPagar? status = null,
        DateOnly? vencimentoDe = null,
        DateOnly? vencimentoAte = null,
        long? fornecedorId = null,
        string? texto = null,
        CancellationToken ct = default);

    Task<ContaPagar> SalvarAsync(ContaPagar conta, CancellationToken ct = default);

    Task RegistrarPagamentoAsync(
        long id,
        DateOnly dataPagamento,
        decimal valorPago,
        long? formaPagamentoId,
        CancellationToken ct = default);

    Task CancelarAsync(long id, CancellationToken ct = default);

    Task ExcluirAsync(long id, CancellationToken ct = default);
}
