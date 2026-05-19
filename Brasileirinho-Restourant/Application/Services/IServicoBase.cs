using BrasileirinhoRestourant.Domain.Common;

namespace BrasileirinhoRestourant.Application.Services;

public interface IServicoBase<T> where T : EntityBase
{
    Task<IReadOnlyList<T>> ListarAsync(
        bool somenteAtivos = false,
        string? filtroTexto = null,
        CancellationToken cancellationToken = default);

    Task<T?> ObterPorIdAsync(long id, CancellationToken cancellationToken = default);

    Task<T> SalvarAsync(T entidade, CancellationToken cancellationToken = default);

    Task InativarAsync(long id, CancellationToken cancellationToken = default);

    Task ReativarAsync(long id, CancellationToken cancellationToken = default);

    Task ExcluirDefinitivoAsync(long id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<T>> BuscarDuplicadosPorNomeAsync(
        T entidade,
        CancellationToken cancellationToken = default);
}
