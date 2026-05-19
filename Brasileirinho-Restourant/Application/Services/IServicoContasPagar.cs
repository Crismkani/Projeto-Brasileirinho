using BrasileirinhoRestourant.Domain.Entities;

namespace BrasileirinhoRestourant.Application.Services;

public interface IServicoContasPagar
{
    public Task<IReadOnlyList<ContaPagar>> ListarAsync(
        StatusContaPagar? status = null,
        DateOnly? vencimentoDe = null,
        DateOnly? vencimentoAte = null,
        long? fornecedorId = null,
        string? texto = null,
        CancellationToken ct = default);

    public Task<ContaPagar> SalvarAsync(ContaPagar conta, CancellationToken ct = default);

    public Task RegistrarPagamentoAsync(
        long id,
        DateOnly dataPagamento,
        decimal valorPago,
        long? formaPagamentoId,
        CancellationToken ct = default);

    public Task CancelarAsync(long id, CancellationToken ct = default);

    public Task ExcluirAsync(long id, CancellationToken ct = default);
}
