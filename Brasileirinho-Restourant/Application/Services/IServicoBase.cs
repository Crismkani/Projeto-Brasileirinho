using BrasileirinhoRestourant.Domain.Common;

namespace BrasileirinhoRestourant.Application.Services;

public interface IServicoBase<T> where T : EntityBase
{
    public Task<IReadOnlyList<T>> ListarAsync(
        bool somenteAtivos = false,
        string? filtroTexto = null,
        CancellationToken cancellationToken = default);

    public Task<T?> ObterPorIdAsync(long id, CancellationToken cancellationToken = default);

    public Task<T> SalvarAsync(T entidade, CancellationToken cancellationToken = default);

    public Task InativarAsync(long id, CancellationToken cancellationToken = default);

    public Task ReativarAsync(long id, CancellationToken cancellationToken = default);

    public Task ExcluirDefinitivoAsync(long id, CancellationToken cancellationToken = default);

    public Task<IReadOnlyList<T>> BuscarDuplicadosPorNomeAsync(
        T entidade,
        CancellationToken cancellationToken = default);
}
