using System.Linq.Expressions;
using BrasileirinhoRestourant.Domain.Common;

namespace BrasileirinhoRestourant.Infrastructure.Repositories;

public interface IRepository<T> where T : EntityBase
{
    public Task<T?> ObterPorIdAsync(long id, CancellationToken cancellationToken = default);

    public Task<IReadOnlyList<T>> ListarAsync(CancellationToken cancellationToken = default);

    public Task<IReadOnlyList<T>> BuscarAsync(
        Expression<Func<T, bool>> filtro,
        CancellationToken cancellationToken = default);

    public Task<T> AdicionarAsync(T entidade, CancellationToken cancellationToken = default);

    public Task AtualizarAsync(T entidade, CancellationToken cancellationToken = default);

    public Task RemoverAsync(long id, CancellationToken cancellationToken = default);

    public Task<bool> ExisteAsync(
        Expression<Func<T, bool>> filtro,
        CancellationToken cancellationToken = default);
}
