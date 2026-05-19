using System.Linq.Expressions;
using BrasileirinhoRestourant.Domain.Common;

namespace BrasileirinhoRestourant.Infrastructure.Repositories;

public interface IRepository<T> where T : EntityBase
{
    Task<T?> ObterPorIdAsync(long id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<T>> ListarAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<T>> BuscarAsync(
        Expression<Func<T, bool>> filtro,
        CancellationToken cancellationToken = default);

    Task<T> AdicionarAsync(T entidade, CancellationToken cancellationToken = default);

    Task AtualizarAsync(T entidade, CancellationToken cancellationToken = default);

    Task RemoverAsync(long id, CancellationToken cancellationToken = default);

    Task<bool> ExisteAsync(
        Expression<Func<T, bool>> filtro,
        CancellationToken cancellationToken = default);
}
