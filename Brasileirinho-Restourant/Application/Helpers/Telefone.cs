namespace BrasileirinhoRestourant.Application.Helpers;

public static class Telefone
{
    public static string SomenteDigitos(string? valor)
        => string.IsNullOrEmpty(valor) ? string.Empty : new string(valor.Where(char.IsDigit).ToArray());

    public static bool Valido(string? valor, bool estrangeiro)
    {
        var d = SomenteDigitos(valor);
        if (d.Length == 0)
        {
            return true;
        }

        return estrangeiro
            ? d.Length >= 6 && d.Length <= 20
            : d.Length == 10 || d.Length == 11;
    }

    public static string Formatar(string? valor, bool estrangeiro)
    {
        var d = SomenteDigitos(valor);
        if (d.Length == 0)
        {
            return string.Empty;
        }

        if (estrangeiro)
        {
            return "+" + d;
        }

        return d.Length switch
        {
            11 => $"({d[..2]}) {d.Substring(2, 5)}-{d.Substring(7, 4)}",
            10 => $"({d[..2]}) {d.Substring(2, 4)}-{d.Substring(6, 4)}",
            _ => d
        };
    }
}
