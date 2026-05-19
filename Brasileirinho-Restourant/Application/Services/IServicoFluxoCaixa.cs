namespace BrasileirinhoRestourant.Application.Services;

public record ResumoFluxoCaixa(
    decimal TotalVendas,
    decimal TotalRecebimentos,
    decimal TotalPagamentos,
    decimal SaldoPeriodo,
    decimal ProjetadoReceber,
    decimal ProjetadoPagar,
    decimal SaldoProjetado
);

public record PontoSerieFluxo(DateTime Periodo, decimal Entradas, decimal Saidas);

public record DespesaFornecedor(string Fornecedor, decimal Valor);

public record MovimentacaoRecente(
    DateTime Data,
    string Tipo,
    string Descricao,
    string? FormaPagamento,
    decimal Valor,
    bool Entrada
);

public interface IServicoFluxoCaixa
{
    Task<ResumoFluxoCaixa> ObterResumoAsync(
        DateOnly de,
        DateOnly ate,
        CancellationToken ct = default);

    Task<IReadOnlyList<PontoSerieFluxo>> ObterSerieMensalAsync(
        int meses,
        CancellationToken ct = default);

    Task<IReadOnlyList<DespesaFornecedor>> ObterDespesasPorFornecedorAsync(
        DateOnly de,
        DateOnly ate,
        int top,
        CancellationToken ct = default);

    Task<IReadOnlyList<MovimentacaoRecente>> ObterUltimasMovimentacoesAsync(
        int qtd,
        CancellationToken ct = default);

    Task<int> ContarVendasAsync(
        DateOnly de,
        DateOnly ate,
        CancellationToken ct = default);
}
