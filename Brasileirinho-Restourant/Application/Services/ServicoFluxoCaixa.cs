using BrasileirinhoRestourant.Domain.Entities;
using BrasileirinhoRestourant.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BrasileirinhoRestourant.Application.Services;

public class ServicoFluxoCaixa : IServicoFluxoCaixa
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public ServicoFluxoCaixa(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<ResumoFluxoCaixa> ObterResumoAsync(
        DateOnly de,
        DateOnly ate,
        CancellationToken ct = default)
    {
        await using var ctx = await _factory.CreateDbContextAsync(ct);

        var deUtc = de.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var ateUtc = ate.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);

        var totalVendas = await ctx.Vendas
            .Where(v => v.Status == StatusVenda.Concluida
                     && v.DataVenda >= deUtc
                     && v.DataVenda <= ateUtc)
            .SumAsync(v => (decimal?)v.ValorPago, ct) ?? 0m;

        var totalRecebimentos = await ctx.ContasReceber
            .Where(c => c.Ativo
                     && c.Status == StatusContaReceber.Recebido
                     && c.DataRecebimento >= de
                     && c.DataRecebimento <= ate)
            .SumAsync(c => c.ValorRecebido ?? 0m, ct);

        var totalPagamentos = await ctx.ContasPagar
            .Where(c => c.Ativo
                     && c.Status == StatusContaPagar.Pago
                     && c.DataPagamento >= de
                     && c.DataPagamento <= ate)
            .SumAsync(c => c.ValorPago ?? 0m, ct);

        var saldoPeriodo = totalVendas + totalRecebimentos - totalPagamentos;

        var hoje = DateOnly.FromDateTime(DateTime.Today);
        var projetadoReceber = await ctx.ContasReceber
            .Where(c => c.Ativo
                     && c.Status == StatusContaReceber.Pendente
                     && c.DataVencimento >= hoje)
            .SumAsync(c => c.Valor, ct);

        var projetadoPagar = await ctx.ContasPagar
            .Where(c => c.Ativo
                     && c.Status == StatusContaPagar.Pendente
                     && c.DataVencimento >= hoje)
            .SumAsync(c => c.Valor, ct);

        return new ResumoFluxoCaixa(
            TotalVendas: totalVendas,
            TotalRecebimentos: totalRecebimentos,
            TotalPagamentos: totalPagamentos,
            SaldoPeriodo: saldoPeriodo,
            ProjetadoReceber: projetadoReceber,
            ProjetadoPagar: projetadoPagar,
            SaldoProjetado: projetadoReceber - projetadoPagar
        );
    }

    public async Task<IReadOnlyList<PontoSerieFluxo>> ObterSerieMensalAsync(
        int meses,
        CancellationToken ct = default)
    {
        if (meses < 1)
        {
            meses = 1;
        }

        await using var ctx = await _factory.CreateDbContextAsync(ct);

        var hoje = DateOnly.FromDateTime(DateTime.Today);
        var inicio = new DateOnly(hoje.Year, hoje.Month, 1).AddMonths(-(meses - 1));
        var inicioUtc = inicio.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        var vendas = await ctx.Vendas
            .Where(v => v.Status == StatusVenda.Concluida && v.DataVenda >= inicioUtc)
            .GroupBy(v => new { v.DataVenda.Year, v.DataVenda.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(x => x.ValorPago) })
            .ToListAsync(ct);

        var recebimentos = await ctx.ContasReceber
            .Where(c => c.Ativo
                     && c.Status == StatusContaReceber.Recebido
                     && c.DataRecebimento >= inicio)
            .GroupBy(c => new { c.DataRecebimento!.Value.Year, c.DataRecebimento!.Value.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(x => x.ValorRecebido ?? 0m) })
            .ToListAsync(ct);

        var pagamentos = await ctx.ContasPagar
            .Where(c => c.Ativo
                     && c.Status == StatusContaPagar.Pago
                     && c.DataPagamento >= inicio)
            .GroupBy(c => new { c.DataPagamento!.Value.Year, c.DataPagamento!.Value.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(x => x.ValorPago ?? 0m) })
            .ToListAsync(ct);

        var pontos = new List<PontoSerieFluxo>(meses);
        for (var i = 0; i < meses; i++)
        {
            var dt = inicio.AddMonths(i);
            var ano = dt.Year;
            var mes = dt.Month;

            var entradaVendas = vendas.FirstOrDefault(x => x.Year == ano && x.Month == mes)?.Total ?? 0m;
            var entradaReceb = recebimentos.FirstOrDefault(x => x.Year == ano && x.Month == mes)?.Total ?? 0m;
            var saida = pagamentos.FirstOrDefault(x => x.Year == ano && x.Month == mes)?.Total ?? 0m;

            pontos.Add(new PontoSerieFluxo(
                Periodo: new DateTime(ano, mes, 1, 0, 0, 0, DateTimeKind.Unspecified),
                Entradas: entradaVendas + entradaReceb,
                Saidas: saida
            ));
        }

        return pontos;
    }

    public async Task<IReadOnlyList<DespesaFornecedor>> ObterDespesasPorFornecedorAsync(
        DateOnly de,
        DateOnly ate,
        int top,
        CancellationToken ct = default)
    {
        if (top < 1)
        {
            top = 10;
        }

        await using var ctx = await _factory.CreateDbContextAsync(ct);

        var agrupado = await ctx.ContasPagar
            .Where(c => c.Ativo
                     && c.Status == StatusContaPagar.Pago
                     && c.DataPagamento >= de
                     && c.DataPagamento <= ate)
            .GroupBy(c => c.FornecedorId)
            .Select(g => new { Id = g.Key, Total = g.Sum(x => x.ValorPago ?? 0m) })
            .OrderByDescending(x => x.Total)
            .Take(top)
            .ToListAsync(ct);

        var ids = agrupado
            .Where(x => x.Id != null)
            .Select(x => x.Id!.Value)
            .ToList();

        var nomes = ids.Count == 0
            ? new Dictionary<long, string>()
            : await ctx.Fornecedores
                .Where(f => ids.Contains(f.Id))
                .ToDictionaryAsync(f => f.Id, f => f.Nome, ct);

        return agrupado
            .Select(x => new DespesaFornecedor(
                Fornecedor: x.Id.HasValue && nomes.TryGetValue(x.Id.Value, out var n) ? n : "(sem fornecedor)",
                Valor: x.Total))
            .ToList();
    }

    public async Task<IReadOnlyList<MovimentacaoRecente>> ObterUltimasMovimentacoesAsync(
        int qtd,
        CancellationToken ct = default)
    {
        if (qtd < 1)
        {
            qtd = 10;
        }

        await using var ctx = await _factory.CreateDbContextAsync(ct);

        var vendas = await ctx.Vendas
            .Where(v => v.Status == StatusVenda.Concluida)
            .OrderByDescending(v => v.DataVenda)
            .Take(qtd)
            .Select(v => new MovimentacaoRecente(
                v.DataVenda,
                "Venda",
                v.Cliente != null ? v.Cliente.Nome : "Venda no PDV",
                v.FormaPagamento != null ? v.FormaPagamento.Nome : null,
                v.ValorPago,
                true))
            .ToListAsync(ct);

        var recebimentos = await ctx.ContasReceber
            .Where(c => c.Ativo
                     && c.Status == StatusContaReceber.Recebido
                     && c.DataRecebimento != null)
            .OrderByDescending(c => c.DataRecebimento)
            .Take(qtd)
            .Select(c => new MovimentacaoRecente(
                c.DataRecebimento!.Value.ToDateTime(TimeOnly.MinValue),
                "Recebimento",
                c.Descricao,
                c.FormaPagamento != null ? c.FormaPagamento.Nome : null,
                c.ValorRecebido ?? 0m,
                true))
            .ToListAsync(ct);

        var pagamentos = await ctx.ContasPagar
            .Where(c => c.Ativo
                     && c.Status == StatusContaPagar.Pago
                     && c.DataPagamento != null)
            .OrderByDescending(c => c.DataPagamento)
            .Take(qtd)
            .Select(c => new MovimentacaoRecente(
                c.DataPagamento!.Value.ToDateTime(TimeOnly.MinValue),
                "Pagamento",
                c.Descricao,
                c.FormaPagamento != null ? c.FormaPagamento.Nome : null,
                c.ValorPago ?? 0m,
                false))
            .ToListAsync(ct);

        return vendas
            .Concat(recebimentos)
            .Concat(pagamentos)
            .OrderByDescending(m => m.Data)
            .Take(qtd)
            .ToList();
    }

    public async Task<int> ContarVendasAsync(
        DateOnly de,
        DateOnly ate,
        CancellationToken ct = default)
    {
        await using var ctx = await _factory.CreateDbContextAsync(ct);

        var deUtc = de.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var ateUtc = ate.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);

        return await ctx.Vendas
            .Where(v => v.Status == StatusVenda.Concluida
                     && v.DataVenda >= deUtc
                     && v.DataVenda <= ateUtc)
            .CountAsync(ct);
    }
}
