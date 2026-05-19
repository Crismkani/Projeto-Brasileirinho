using BrasileirinhoRestourant.Domain.Entities;
using BrasileirinhoRestourant.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BrasileirinhoRestourant.Application.Services;

public class ServicoVenda : IServicoVenda
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public ServicoVenda(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<Venda> FinalizarAsync(Venda venda, CancellationToken cancellationToken = default)
    {
        if (venda.Itens.Count == 0)
            throw new InvalidOperationException("A venda deve ter ao menos um item.");

        venda.RecalcularTotal();

        await using var contexto = await _factory.CreateDbContextAsync(cancellationToken);

        var idsProdutos = venda.Itens.Select(i => i.ProdutoId).Distinct().ToList();
        var produtos = await contexto.Produtos
            .Where(p => idsProdutos.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, cancellationToken);

        foreach (var item in venda.Itens)
        {
            if (!produtos.TryGetValue(item.ProdutoId, out var produto))
                throw new InvalidOperationException($"Produto #{item.ProdutoId} não encontrado.");

            var baixa = item.Fracionado
                ? Math.Max(item.PesoBruto - item.Tara, 0m)
                : item.Quantidade;

            produto.Quantidade -= baixa;
        }

        venda.Status = StatusVenda.Concluida;
        venda.DataVenda = DateTime.UtcNow;

        await contexto.Vendas.AddAsync(venda, cancellationToken);
        await contexto.SaveChangesAsync(cancellationToken);

        // Gera conta a receber se a condição de pagamento tiver prazo > 0
        if (venda.CondicaoPagamentoId.HasValue)
        {
            var condicao = await contexto.CondicoesPagamento
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == venda.CondicaoPagamentoId.Value, cancellationToken);

            if (condicao is not null && condicao.PrazoDias > 0)
            {
                var vencimento = DateOnly.FromDateTime(DateTime.Today).AddDays(condicao.PrazoDias);
                var descricao = $"Venda #{venda.Id}" +
                    (venda.ClienteId.HasValue ? "" : " — cliente avulso");

                var contaReceber = new ContaReceber
                {
                    ClienteId = venda.ClienteId,
                    VendaId = venda.Id,
                    Descricao = descricao,
                    Valor = venda.ValorTotal,
                    DataEmissao = DateOnly.FromDateTime(DateTime.Today),
                    DataVencimento = vencimento,
                    Status = StatusContaReceber.Pendente,
                    Ativo = true
                };

                await contexto.ContasReceber.AddAsync(contaReceber, cancellationToken);
                await contexto.SaveChangesAsync(cancellationToken);
            }
        }

        return venda;
    }
}
