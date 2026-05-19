using BrasileirinhoRestourant.Domain.Entities;
using BrasileirinhoRestourant.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BrasileirinhoRestourant.Application.Services;

public class ServicoContasReceber : IServicoContasReceber
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public ServicoContasReceber(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<IReadOnlyList<ContaReceber>> ListarAsync(
        StatusContaReceber? status = null,
        DateOnly? vencimentoDe = null,
        DateOnly? vencimentoAte = null,
        long? clienteId = null,
        CancellationToken ct = default)
    {
        await using var ctx = await _factory.CreateDbContextAsync(ct);

        var query = ctx.ContasReceber
            .AsNoTracking()
            .Include(c => c.Cliente)
            .Include(c => c.FormaPagamento)
            .Where(c => c.Ativo)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(c => c.Status == status.Value);

        if (vencimentoDe.HasValue)
            query = query.Where(c => c.DataVencimento >= vencimentoDe.Value);

        if (vencimentoAte.HasValue)
            query = query.Where(c => c.DataVencimento <= vencimentoAte.Value);

        if (clienteId.HasValue)
            query = query.Where(c => c.ClienteId == clienteId.Value);

        return await query.OrderBy(c => c.DataVencimento).ToListAsync(ct);
    }

    public async Task<ContaReceber> SalvarAsync(ContaReceber conta, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(conta.Descricao))
            throw new InvalidOperationException("Descrição é obrigatória.");
        if (conta.Valor <= 0)
            throw new InvalidOperationException("Valor deve ser maior que zero.");
        if (conta.DataVencimento == default)
            throw new InvalidOperationException("Data de vencimento é obrigatória.");
        if (conta.DataVencimento < conta.DataEmissao)
            throw new InvalidOperationException("Data de vencimento não pode ser anterior à data de emissão.");

        await using var ctx = await _factory.CreateDbContextAsync(ct);

        conta.Descricao = conta.Descricao.Trim();
        conta.Ativo = true;

        if (conta.Id == 0)
        {
            await ctx.ContasReceber.AddAsync(conta, ct);
        }
        else
        {
            ctx.ContasReceber.Update(conta);
        }

        await ctx.SaveChangesAsync(ct);
        return conta;
    }

    public async Task RegistrarRecebimentoAsync(
        long id,
        DateOnly dataRecebimento,
        decimal valorRecebido,
        long? formaPagamentoId,
        CancellationToken ct = default)
    {
        if (valorRecebido <= 0)
            throw new InvalidOperationException("Valor recebido deve ser maior que zero.");

        await using var ctx = await _factory.CreateDbContextAsync(ct);

        var conta = await ctx.ContasReceber.FindAsync(new object[] { id }, ct)
            ?? throw new InvalidOperationException($"Conta #{id} não encontrada.");

        if (conta.Status != StatusContaReceber.Pendente)
            throw new InvalidOperationException("Apenas contas pendentes podem ser recebidas.");

        conta.Status = StatusContaReceber.Recebido;
        conta.DataRecebimento = dataRecebimento;
        conta.ValorRecebido = valorRecebido;
        conta.FormaPagamentoId = formaPagamentoId;

        await ctx.SaveChangesAsync(ct);
    }

    public async Task CancelarAsync(long id, CancellationToken ct = default)
    {
        await using var ctx = await _factory.CreateDbContextAsync(ct);

        var conta = await ctx.ContasReceber.FindAsync(new object[] { id }, ct)
            ?? throw new InvalidOperationException($"Conta #{id} não encontrada.");

        if (conta.Status == StatusContaReceber.Recebido)
            throw new InvalidOperationException("Conta já recebida não pode ser cancelada.");

        conta.Status = StatusContaReceber.Cancelado;
        await ctx.SaveChangesAsync(ct);
    }

    public async Task ExcluirAsync(long id, CancellationToken ct = default)
    {
        await using var ctx = await _factory.CreateDbContextAsync(ct);

        var conta = await ctx.ContasReceber.FindAsync(new object[] { id }, ct);
        if (conta is null) return;

        if (conta.Status == StatusContaReceber.Recebido)
            throw new InvalidOperationException("Conta já recebida não pode ser excluída. Cancele primeiro.");

        ctx.ContasReceber.Remove(conta);
        await ctx.SaveChangesAsync(ct);
    }
}
