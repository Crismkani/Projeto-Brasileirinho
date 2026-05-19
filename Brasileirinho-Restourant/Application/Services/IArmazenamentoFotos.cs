using Microsoft.AspNetCore.Components.Forms;

namespace BrasileirinhoRestourant.Application.Services;

public interface IArmazenamentoFotos
{
    public Task<string> SalvarAsync(IBrowserFile arquivo, CancellationToken cancellationToken = default);
    public void Remover(string? urlRelativa);
}
