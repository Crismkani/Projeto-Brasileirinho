using System.ComponentModel.DataAnnotations;
using BrasileirinhoRestourant.Domain.Common;
using BrasileirinhoRestourant.Infrastructure.Repositories;

namespace BrasileirinhoRestourant.Application.Services;

public class ServicoBase<T> : IServicoBase<T> where T : EntityBase
{
    protected IRepository<T> Repositorio { get; }

    public ServicoBase(IRepository<T> repositorio)
    {
        Repositorio = repositorio;
    }

    public virtual async Task<IReadOnlyList<T>> ListarAsync(
        bool somenteAtivos = false,
        string? filtroTexto = null,
        CancellationToken cancellationToken = default)
    {
        var lista = await Repositorio.ListarAsync(cancellationToken);

        if (somenteAtivos)
        {
            lista = lista.Where(e => e.Ativo).ToList();
        }

        if (!string.IsNullOrWhiteSpace(filtroTexto))
        {
            var termo = filtroTexto.Trim();
            lista = lista.Where(e => CorresponderFiltro(e, termo)).ToList();
        }

        return lista;
    }

    public virtual Task<T?> ObterPorIdAsync(long id, CancellationToken cancellationToken = default)
        => Repositorio.ObterPorIdAsync(id, cancellationToken);

    public virtual async Task<T> SalvarAsync(T entidade, CancellationToken cancellationToken = default)
    {
        await ValidarAsync(entidade, cancellationToken);

        if (entidade.Id == 0)
        {
            return await Repositorio.AdicionarAsync(entidade, cancellationToken);
        }

        await Repositorio.AtualizarAsync(entidade, cancellationToken);
        return entidade;
    }

    public virtual async Task InativarAsync(long id, CancellationToken cancellationToken = default)
    {
        var entidade = await Repositorio.ObterPorIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException($"Entidade #{id} não encontrada.");

        entidade.Inativar();
        await Repositorio.AtualizarAsync(entidade, cancellationToken);
    }

    public virtual async Task ReativarAsync(long id, CancellationToken cancellationToken = default)
    {
        var entidade = await Repositorio.ObterPorIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException($"Entidade #{id} não encontrada.");

        entidade.Ativar();
        await Repositorio.AtualizarAsync(entidade, cancellationToken);
    }

    public virtual Task ExcluirDefinitivoAsync(long id, CancellationToken cancellationToken = default)
        => Repositorio.RemoverAsync(id, cancellationToken);

    public virtual async Task<IReadOnlyList<T>> BuscarDuplicadosPorNomeAsync(
        T entidade, CancellationToken cancellationToken = default)
    {
        var nomeProp = typeof(T).GetProperty("Nome");
        if (nomeProp is null)
        {
            return Array.Empty<T>();
        }

        var nome = (nomeProp.GetValue(entidade) as string)?.Trim();
        if (string.IsNullOrWhiteSpace(nome))
        {
            return Array.Empty<T>();
        }

        var todos = await Repositorio.ListarAsync(cancellationToken);
        return todos
            .Where(e => e.Id != entidade.Id)
            .Where(e => string.Equals(
                (nomeProp.GetValue(e) as string)?.Trim(),
                nome,
                StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    protected virtual Task ValidarAsync(T entidade, CancellationToken cancellationToken)
    {
        var contexto = new ValidationContext(entidade);
        Validator.ValidateObject(entidade, contexto, validateAllProperties: true);
        return Task.CompletedTask;
    }

    protected virtual bool CorresponderFiltro(T entidade, string termo)
    {
        var props = typeof(T).GetProperties()
            .Where(p => p.PropertyType == typeof(string));

        foreach (var p in props)
        {
            var valor = p.GetValue(entidade) as string;
            if (!string.IsNullOrEmpty(valor) &&
                valor.Contains(termo, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }
}
