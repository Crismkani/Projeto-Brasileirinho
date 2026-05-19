using Microsoft.AspNetCore.Components.Forms;

namespace BrasileirinhoRestourant.Application.Services;

public interface IArmazenamentoNotas
{
    Task<string> SalvarAsync(IBrowserFile arquivo, CancellationToken cancellationToken = default);
    void Remover(string? urlRelativa);
}
