# Brasileirinho PDV

ERP para restaurante self-service. Blazor Server + .NET 10 + PostgreSQL 18 (EF Core 10 via Npgsql).

Documentação completa em [DOCUMENTACAO.md](DOCUMENTACAO.md). Esse arquivo é o briefing curto para o Claude Code.

---

## Stack

- **Frontend:** Blazor Server, Razor Components com `@rendermode InteractiveServer`
- **Backend:** .NET 10 (`net10.0`), C# 13
- **ORM:** Entity Framework Core 10 + Npgsql 10
- **Banco:** PostgreSQL 18, banco `BRASILEIRINHO`, usuário `postgres`, senha `__SUA_SENHA__` (dev)
- **Estilo:** Bootstrap 5 + CSS isolado por componente + paleta Blue Grey custom

## Estrutura

```
Brasileirinho-Restourant/                   ← raiz da solução
├─ Brasileirinho-Restourant.sln
├─ DOCUMENTACAO.md                          ← doc completa
├─ banco-brasileirinho-atual.sql            ← dump real (pg_dump)
├─ banco-brasileirinho-apresentacao.sql     ← script limpo
└─ Brasileirinho-Restourant/                ← projeto
   ├─ Domain/{Common,Entities}/             ← entidades puras
   ├─ Application/{Services,Helpers}/       ← regras + utilitários
   ├─ Infrastructure/{Data,Repositories}/   ← EF Core
   ├─ Components/{Layout,Shared,Pages}/     ← UI Blazor
   ├─ wwwroot/                              ← Bootstrap, app.css, uploads
   ├─ Program.cs
   └─ appsettings.Development.json          ← connection string
```

## Comandos comuns

```powershell
# Restaurar pacotes
dotnet restore

# Compilar
dotnet build --nologo

# Rodar (porta http://localhost:5166)
dotnet run --project Brasileirinho-Restourant

# Parar app rodando (Windows)
Get-Process Brasileirinho-Restourant -ErrorAction SilentlyContinue | Stop-Process -Force

# Backup do banco
$env:PGPASSWORD = "__SUA_SENHA__"
& "C:\Program Files\PostgreSQL\18\bin\pg_dump.exe" -U postgres -d BRASILEIRINHO -F c `
  -f "$HOME\Downloads\brasileirinho-$((Get-Date).ToString('yyyyMMdd-HHmm')).backup"

# Conectar via psql
& "C:\Program Files\PostgreSQL\18\bin\psql.exe" -U postgres -d BRASILEIRINHO
```

## Convenções do banco

- **snake_case** em tudo
- **Sufixo da tabela** em cada coluna: `id_cliente`, `nome_pais`, `codigo_barras_produto`
- FKs mantêm formato `entidade_id`: `cliente_id`, `produto_id`
- Soft delete via `ativo_<tabela>` (boolean)
- Audit via `data_cadastro_<tabela>` + `ultima_modificacao_<tabela>` (timestamptz UTC) em tabelas auditáveis

## Convenções do código

- C# em **PascalCase** (`Nome`, `DataCadastro`)
- Métodos async com sufixo `Async`
- Namespaces seguem pastas: `BrasileirinhoRestourant.Domain.Entities`, etc.
- Toda entidade herda de `EntityBase` (Id + Ativo)
- Acesso a dados SEMPRE via `IDbContextFactory<AppDbContext>` (nunca DbContext direto) — Blazor Server exige isso pra evitar tracking conflicts
- Comentários: só quando explicam *por que*, não *o que*

## Regras críticas

1. **NÃO usar EF Migrations** — mudanças de schema são via SQL manual no pgAdmin. Sempre fazer backup com pg_dump antes.
2. **Senha do banco** está em `appsettings.Development.json` — NÃO commitar produção.
3. **Regra obrigatória de pesagem:** Subtotal = `(PesoBruto - Tara) * PrecoUnitario` para produtos com `PermiteFracionamento=true`. Implementada em `ComandaItem.RecalcularSubtotal()` e `VendaItem.RecalcularSubtotal()`. Reusar — nunca duplicar a fórmula em outro lugar.
4. **Snapshot fiscal:** `preco_unitario` em itens de venda/comanda é uma cópia do preço no momento da venda. Alterar produto.valor_venda NÃO deve afetar vendas históricas.
5. **3ª Forma Normal:** totais (`Subtotal`, `ValorTotal`, `SubtotalItens`) NÃO são persistidos — `Ignore()` no DbContext. Calculados em memória via `RecalcularTotal()`.
6. **Sempre que mexer em layout/CSS**, lembrar que tem CSS isolado por componente (`*.razor.css`). Mudanças aplicam só ao componente.

## Padrão das telas de cadastro

10 telas em `Components/Pages/Cadastros/` seguem padrão idêntico:
- Header com título + botão "Novo"
- Filtro texto + toggle "Apenas ativos"
- `<TabelaListagem>` com `<BotoesAcaoCadastro>` (Editar/Inativar/Excluir)
- `<ModalCrud>` para criar/editar
- `<DialogoDuplicado>` (warning de nome duplicado)
- `<ConfirmDialog>` (confirmar exclusão)
- `[SupplyParameterFromQuery(Name = "novo")] string? NovoQuery` — abre modal automático via URL `?novo=`

Ao adicionar nova tela: copiar `Categorias.razor` como template.

## Banco — 16 tabelas (+2 financeiro)

- **Geográfico:** `pais`, `estado`, `cidade`
- **Apoio:** `categoria`, `marca`, `unidade_medida`, `forma_pagamento`, `condicao_pagamento`
- **Produto:** `produto`
- **Pessoas:** `cliente`, `fornecedor`
- **Operacional:** `comanda`, `comanda_item`, `venda`, `venda_item`
- **Financeiro:** `conta_pagar`, `conta_receber`

Scripts SQL: `condicao-pagamento.sql` e `financeiro.sql` na raiz da solução.

Detalhes de colunas/relacionamentos em [DOCUMENTACAO.md §4](DOCUMENTACAO.md).

## Módulo Financeiro

### Contas a Pagar (`conta_pagar`)
- CRUD manual + filtros por status/vencimento/fornecedor
- Ação **Pagar**: registra data, valor e forma de pagamento
- Status: `Pendente` | `Pago` | `Cancelado` (Atrasado = Pendente com vencimento < hoje, computado)
- Serviço: `IServicoContasPagar` / `ServicoContasPagar`
- Página: `Components/Pages/Financeiro/ContasPagar.razor`

### Contas a Receber (`conta_receber`)
- CRUD manual + geração automática quando `Venda.CondicaoPagamentoId` tem `PrazoDias > 0`
- Ação **Receber**: registra data, valor e forma de pagamento
- Status: `Pendente` | `Recebido` | `Cancelado`
- FK opcional `venda_id` — rastreia origem da venda
- Serviço: `IServicoContasReceber` / `ServicoContasReceber`
- Página: `Components/Pages/Financeiro/ContasReceber.razor`

### Fluxo de Caixa
- Resumo por período: Vendas à vista + Recebimentos + Pagamentos + Saldo
- Projeção: contas pendentes a receber/pagar a partir de hoje
- Atalhos de período: Hoje, 7 dias, 30 dias, Este Mês
- Serviço: `IServicoFluxoCaixa` / `ServicoFluxoCaixa`
- Página: `Components/Pages/Financeiro/FluxoCaixa.razor`

### Regras do Financeiro
- `Atrasado` NÃO é persistido — é computado: `Status == Pendente && DataVencimento < hoje`
- Conta `Pago`/`Recebido` não pode ser excluída — cancelar primeiro
- Status armazenado como string (`HasConversion<string>()`) para legibilidade no banco

## Próximos passos abertos

- Adicionar autenticação (ASP.NET Core Identity) — hoje sem login
- Mover senha do DB para User Secrets
- Criar usuário PG dedicado (não usar `postgres` superuser)
- Parcelamento automático (IntervaloDias em CondicaoPagamento) — aguarda demanda real
