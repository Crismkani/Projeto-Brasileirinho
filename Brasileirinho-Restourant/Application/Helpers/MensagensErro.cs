using Npgsql;

namespace BrasileirinhoRestourant.Application.Helpers;

public static class MensagensErro
{
    public static string Para(Exception ex)
    {
        if (ex.GetBaseException() is PostgresException pg)
        {
            if (pg.SqlState == "23505")
                return TraduzirUnico(pg.ConstraintName);

            return pg.SqlState switch
            {
                "23503" => "Este registro está vinculado a outros cadastros (ex.: produtos, vendas) " +
                           "e não pode ser excluído. Inative-o ao invés de excluir.",
                "23502" => "Há campos obrigatórios não preenchidos.",
                _       => $"Erro do banco de dados ({pg.SqlState}): {pg.MessageText}"
            };
        }

        return ex.Message;
    }

    private static string TraduzirUnico(string? constraint) => constraint switch
    {
        not null when constraint.Contains("codigo_barras", StringComparison.OrdinalIgnoreCase)
            => "Já existe um produto cadastrado com esse código de barras.",
        not null when constraint.Contains("produto_nome", StringComparison.OrdinalIgnoreCase)
            => "Já existe um produto cadastrado com esse nome.",
        not null when constraint.Contains("cpf", StringComparison.OrdinalIgnoreCase)
            => "Já existe um cadastro com esse CPF/CNPJ.",
        _ => "Já existe um registro com essas informações. Verifique campos únicos."
    };
}
