using BrasileirinhoRestourant.Domain.Entities;

namespace BrasileirinhoRestourant.Application.Services;

public interface IServicoVenda
{
    Task<Venda> FinalizarAsync(Venda venda, CancellationToken cancellationToken = default);
}
