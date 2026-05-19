using Microsoft.AspNetCore.Components.Forms;

namespace BrasileirinhoRestourant.Application.Services;

public class ArmazenamentoNotas : IArmazenamentoNotas
{
    public const long MaxBytes = 10 * 1024 * 1024; // 10 MB
    private const string SubPasta = "uploads/notas";
    private static readonly string[] ExtensoesPermitidas = { ".pdf", ".jpg", ".jpeg", ".png", ".webp" };

    private readonly IWebHostEnvironment _env;

    public ArmazenamentoNotas(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<string> SalvarAsync(IBrowserFile arquivo, CancellationToken cancellationToken = default)
    {
        if (arquivo.Size > MaxBytes)
        {
            throw new InvalidOperationException($"Arquivo maior que {MaxBytes / (1024 * 1024)} MB.");
        }

        var ext = Path.GetExtension(arquivo.Name).ToLowerInvariant();
        if (!ExtensoesPermitidas.Contains(ext))
        {
            throw new InvalidOperationException("Use PDF, JPG, PNG ou WebP.");
        }

        var pastaAbs = Path.Combine(_env.WebRootPath, SubPasta);
        Directory.CreateDirectory(pastaAbs);

        var nomeArquivo = $"{Guid.NewGuid():N}{ext}";
        var caminhoAbs = Path.Combine(pastaAbs, nomeArquivo);

#pragma warning disable S5693 // Limite intencional: 10 MB já validado em arquivo.Size acima.
        await using var origem = arquivo.OpenReadStream(MaxBytes, cancellationToken);
#pragma warning restore S5693
        await using var destino = File.Create(caminhoAbs);
        await origem.CopyToAsync(destino, cancellationToken);

        return $"/{SubPasta}/{nomeArquivo}";
    }

    public void Remover(string? urlRelativa)
    {
        if (string.IsNullOrWhiteSpace(urlRelativa))
        {
            return;
        }

        var caminhoAbs = Path.Combine(_env.WebRootPath, urlRelativa.TrimStart('/'));
        if (!File.Exists(caminhoAbs))
        {
            return;
        }

        try { File.Delete(caminhoAbs); }
        catch { /* arquivo órfão — não crítico */ }
    }
}
