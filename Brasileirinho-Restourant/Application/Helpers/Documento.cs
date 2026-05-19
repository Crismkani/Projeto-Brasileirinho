using BrasileirinhoRestourant.Domain.Entities;

namespace BrasileirinhoRestourant.Application.Helpers;

public static class Documento
{
    public static string SomenteDigitos(string? valor)
        => string.IsNullOrEmpty(valor) ? string.Empty : new string(valor.Where(char.IsDigit).ToArray());

    public static int TamanhoEsperado(TipoPessoa tipo)
        => tipo == TipoPessoa.Fisica ? 11 : 14;

    public static bool Valido(string? valor, TipoPessoa tipo)
    {
        var digitos = SomenteDigitos(valor);
        return digitos.Length == TamanhoEsperado(tipo);
    }

    public static string Formatar(string? valor, TipoPessoa tipo)
    {
        var d = SomenteDigitos(valor);
        if (d.Length == 0) return string.Empty;

        return tipo == TipoPessoa.Fisica && d.Length == 11
            ? $"{d[..3]}.{d.Substring(3, 3)}.{d.Substring(6, 3)}-{d.Substring(9, 2)}"
            : tipo == TipoPessoa.Juridica && d.Length == 14
                ? $"{d[..2]}.{d.Substring(2, 3)}.{d.Substring(5, 3)}/{d.Substring(8, 4)}-{d.Substring(12, 2)}"
                : d;
    }

    public static TipoPessoa InferirTipo(string? valor)
        => SomenteDigitos(valor).Length > 11 ? TipoPessoa.Juridica : TipoPessoa.Fisica;
}
