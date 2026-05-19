using System.Linq.Expressions;
using BrasileirinhoRestourant.Domain.Common;
using BrasileirinhoRestourant.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BrasileirinhoRestourant.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : EntityBase
{
    protected readonly IDbContextFactory<AppDbContext> Factory;

    public Repository(IDbContextFactory<AppDbContext> factory)
    {
        Factory = factory;
    }

    public virtual async Task<T?> ObterPorIdAsync(long id, CancellationToken cancellationToken = default)
    {
        await using var contexto = await Factory.CreateDbContextAsync(cancellationToken);
        return await contexto.Set<T>().AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public virtual async Task<IReadOnlyList<T>> ListarAsync(CancellationToken cancellationToken = default)
    {
        await using var contexto = await Factory.CreateDbContextAsync(cancellationToken);
        return await contexto.Set<T>().AsNoTracking().ToListAsync(cancellationToken);
    }

    public virtual async Task<IReadOnlyList<T>> BuscarAsync(
        Expression<Func<T, bool>> filtro,
        CancellationToken cancellationToken = default)
    {
        await using var contexto = await Factory.CreateDbContextAsync(cancellationToken);
        return await contexto.Set<T>().AsNoTracking().Where(filtro).ToListAsync(cancellationToken);
    }

    public virtual async Task<T> AdicionarAsync(T entidade, CancellationToken cancellationToken = default)
    {
        await using var contexto = await Factory.CreateDbContextAsync(cancellationToken);
        await contexto.Set<T>().AddAsync(entidade, cancellationToken);
        await contexto.SaveChangesAsync(cancellationToken);
        return entidade;
    }

    public virtual async Task AtualizarAsync(T entidade, CancellationToken cancellationToken = default)
    {
        await using var contexto = await Factory.CreateDbContextAsync(cancellationToken);
        contexto.Set<T>().Update(entidade);
        await contexto.SaveChangesAsync(cancellationToken);
    }

    public virtual async Task RemoverAsync(long id, CancellationToken cancellationToken = default)
    {
        await using var contexto = await Factory.CreateDbContextAsync(cancellationToken);
        var entidade = await contexto.Set<T>().FindAsync(new object[] { id }, cancellationToken);
        if (entidade is null) return;

        contexto.Set<T>().Remove(entidade);
        await contexto.SaveChangesAsync(cancellationToken);
    }

    public virtual async Task<bool> ExisteAsync(
        Expression<Func<T, bool>> filtro,
        CancellationToken cancellationToken = default)
    {
        await using var contexto = await Factory.CreateDbContextAsync(cancellationToken);
        return await contexto.Set<T>().AsNoTracking().AnyAsync(filtro, cancellationToken);
    }
}
