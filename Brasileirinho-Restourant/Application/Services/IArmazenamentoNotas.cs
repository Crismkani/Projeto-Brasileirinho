using Microsoft.AspNetCore.Components.Forms;

namespace BrasileirinhoRestourant.Application.Services;

public interface IArmazenamentoNotas
{
    public Task<string> SalvarAsync(IBrowserFile arquivo, CancellationToken cancellationToken = default);
    public void Remover(string? urlRelativa);
}
