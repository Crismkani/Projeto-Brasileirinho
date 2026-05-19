using Microsoft.AspNetCore.Components.Forms;

namespace BrasileirinhoRestourant.Application.Services;

public class ArmazenamentoFotos : IArmazenamentoFotos
{
    public const long MaxBytes = 5 * 1024 * 1024;
    private const string SubPasta = "uploads/produtos";
    private static readonly string[] ExtensoesPermitidas = { ".jpg", ".jpeg", ".png", ".webp" };

    private readonly IWebHostEnvironment _env;

    public ArmazenamentoFotos(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<string> SalvarAsync(IBrowserFile arquivo, CancellationToken cancellationToken = default)
    {
        if (arquivo.Size > MaxBytes)
            throw new InvalidOperationException("Arquivo maior que 2 MB.");

        var ext = Path.GetExtension(arquivo.Name).ToLowerInvariant();
        if (!ExtensoesPermitidas.Contains(ext))
            throw new InvalidOperationException("Use imagens JPG, PNG ou WebP.");

        var pastaAbs = Path.Combine(_env.WebRootPath, SubPasta);
        Directory.CreateDirectory(pastaAbs);

        var nomeArquivo = $"{Guid.NewGuid():N}{ext}";
        var caminhoAbs = Path.Combine(pastaAbs, nomeArquivo);

        await using var origem = arquivo.OpenReadStream(MaxBytes, cancellationToken);
        await using var destino = File.Create(caminhoAbs);
        await origem.CopyToAsync(destino, cancellationToken);

        return $"/{SubPasta}/{nomeArquivo}";
    }

    public void Remover(string? urlRelativa)
    {
        if (string.IsNullOrWhiteSpace(urlRelativa)) return;

        var caminhoAbs = Path.Combine(_env.WebRootPath, urlRelativa.TrimStart('/'));
        if (!File.Exists(caminhoAbs)) return;

        try { File.Delete(caminhoAbs); }
        catch { /* arquivo órfão não é crítico — segue */ }
    }
}
