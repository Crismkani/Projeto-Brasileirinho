using BrasileirinhoRestourant.Domain.Entities;
using BrasileirinhoRestourant.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BrasileirinhoRestourant.Application.Services;

public class ServicoContasPagar : IServicoContasPagar
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public ServicoContasPagar(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<IReadOnlyList<ContaPagar>> ListarAsync(
        StatusContaPagar? status = null,
        DateOnly? vencimentoDe = null,
        DateOnly? vencimentoAte = null,
        long? fornecedorId = null,
        string? texto = null,
        CancellationToken ct = default)
    {
        await using var ctx = await _factory.CreateDbContextAsync(ct);

        var query = ctx.ContasPagar
            .AsNoTracking()
            .Include(c => c.Fornecedor)
            .Include(c => c.FormaPagamento)
            .Include(c => c.CondicaoPagamento)
            .Where(c => c.Ativo)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(c => c.Status == status.Value);
        }

        if (vencimentoDe.HasValue)
        {
            query = query.Where(c => c.DataVencimento >= vencimentoDe.Value);
        }

        if (vencimentoAte.HasValue)
        {
            query = query.Where(c => c.DataVencimento <= vencimentoAte.Value);
        }

        if (fornecedorId.HasValue)
        {
            query = query.Where(c => c.FornecedorId == fornecedorId.Value);
        }

        var lista = await query.OrderBy(c => c.DataVencimento).ToListAsync(ct);

        if (!string.IsNullOrWhiteSpace(texto))
        {
            var termo = texto.Trim();
            lista = lista.Where(c =>
                (c.NumeroDocumento != null && c.NumeroDocumento.Contains(termo, StringComparison.OrdinalIgnoreCase)) ||
                c.Descricao.Contains(termo, StringComparison.OrdinalIgnoreCase) ||
                (c.Observacao != null && c.Observacao.Contains(termo, StringComparison.OrdinalIgnoreCase))
            ).ToList();
        }

        return lista;
    }

    public async Task<ContaPagar> SalvarAsync(ContaPagar conta, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(conta.Descricao))
        {
            throw new InvalidOperationException("Descrição é obrigatória.");
        }

        if (conta.Valor <= 0)
        {
            throw new InvalidOperationException("Valor deve ser maior que zero.");
        }

        if (conta.DataVencimento == default)
        {
            throw new InvalidOperationException("Data de vencimento é obrigatória.");
        }

        if (conta.DataVencimento < conta.DataEmissao)
        {
            throw new InvalidOperationException("Data de vencimento não pode ser anterior à data de emissão.");
        }

        await using var ctx = await _factory.CreateDbContextAsync(ct);

        conta.Descricao = conta.Descricao.Trim();
        conta.NumeroDocumento = conta.NumeroDocumento?.Trim();
        conta.Ativar();

        if (conta.Id == 0)
        {
            await ctx.ContasPagar.AddAsync(conta, ct);
        }
        else
        {
            ctx.ContasPagar.Update(conta);
        }

        await ctx.SaveChangesAsync(ct);
        return conta;
    }

    public async Task RegistrarPagamentoAsync(
        long id,
        DateOnly dataPagamento,
        decimal valorPago,
        long? formaPagamentoId,
        CancellationToken ct = default)
    {
        if (valorPago <= 0)
        {
            throw new InvalidOperationException("Valor pago deve ser maior que zero.");
        }

        await using var ctx = await _factory.CreateDbContextAsync(ct);

        var conta = await ctx.ContasPagar.FindAsync(new object[] { id }, ct)
            ?? throw new InvalidOperationException($"Conta #{id} não encontrada.");

        if (conta.Status != StatusContaPagar.Pendente)
        {
            throw new InvalidOperationException("Apenas contas pendentes podem ser pagas.");
        }

        conta.Status = StatusContaPagar.Pago;
        conta.DataPagamento = dataPagamento;
        conta.ValorPago = valorPago;
        conta.FormaPagamentoId = formaPagamentoId;

        await ctx.SaveChangesAsync(ct);
    }

    public async Task CancelarAsync(long id, CancellationToken ct = default)
    {
        await using var ctx = await _factory.CreateDbContextAsync(ct);

        var conta = await ctx.ContasPagar.FindAsync(new object[] { id }, ct)
            ?? throw new InvalidOperationException($"Conta #{id} não encontrada.");

        if (conta.Status == StatusContaPagar.Pago)
        {
            throw new InvalidOperationException("Conta já paga não pode ser cancelada.");
        }

        conta.Status = StatusContaPagar.Cancelado;
        await ctx.SaveChangesAsync(ct);
    }

    public async Task ExcluirAsync(long id, CancellationToken ct = default)
    {
        await using var ctx = await _factory.CreateDbContextAsync(ct);

        var conta = await ctx.ContasPagar.FindAsync(new object[] { id }, ct);
        if (conta is null)
        {
            return;
        }

        if (conta.Status == StatusContaPagar.Pago)
        {
            throw new InvalidOperationException("Conta já paga não pode ser excluída. Cancele primeiro.");
        }

        ctx.ContasPagar.Remove(conta);
        await ctx.SaveChangesAsync(ct);
    }
}
