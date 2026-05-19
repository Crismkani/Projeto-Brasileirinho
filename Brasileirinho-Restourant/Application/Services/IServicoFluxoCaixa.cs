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
    public Task<ResumoFluxoCaixa> ObterResumoAsync(
        DateOnly de,
        DateOnly ate,
        CancellationToken ct = default);

    public Task<IReadOnlyList<PontoSerieFluxo>> ObterSerieMensalAsync(
        int meses,
        CancellationToken ct = default);

    public Task<IReadOnlyList<DespesaFornecedor>> ObterDespesasPorFornecedorAsync(
        DateOnly de,
        DateOnly ate,
        int top,
        CancellationToken ct = default);

    public Task<IReadOnlyList<MovimentacaoRecente>> ObterUltimasMovimentacoesAsync(
        int qtd,
        CancellationToken ct = default);

    public Task<int> ContarVendasAsync(
        DateOnly de,
        DateOnly ate,
        CancellationToken ct = default);
}
