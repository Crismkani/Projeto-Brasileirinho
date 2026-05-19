# Brasileirinho PDV — Documentação Técnica

ERP para restaurante self-service em **Blazor Server (.NET 10)** com **PostgreSQL 18** via **EF Core 10**.
Suporta venda por unidade e por peso, controle de estoque, cadastros base e (futuramente) financeiro.

---

## 1. Stack

| Camada | Tecnologia |
|---|---|
| Front-end | Blazor Server (Razor Components, `@rendermode InteractiveServer`) |
| Back-end | .NET 10, C# 13 |
| ORM | Entity Framework Core 10 + Npgsql 10 |
| Banco | PostgreSQL 18 (banco `BRASILEIRINHO`) |
| UI | Bootstrap 5 + CSS isolado por componente + paleta Blue Grey custom |
| Upload | `<InputFile>` (SignalR, limite 20 MB) → disco em `wwwroot/uploads/` |

---

## 2. Estrutura de pastas

```
Brasileirinho-Restourant/                       ← raiz da solução
├─ Brasileirinho-Restourant.sln
├─ DOCUMENTACAO.md                              ← este arquivo
├─ banco-brasileirinho-atual.sql                ← dump real (pg_dump)
├─ banco-brasileirinho-apresentacao.sql         ← script limpo p/ apresentar
│
└─ Brasileirinho-Restourant/                    ← projeto Web
    ├─ Brasileirinho-Restourant.csproj
    ├─ Program.cs                               ← bootstrap + DI
    ├─ appsettings.json
    ├─ appsettings.Development.json             ← connection string
    │
    ├─ Domain/                                  ← regras de negócio puras
    │   ├─ Common/
    │   │   ├─ EntityBase.cs                    ← Id + Ativo
    │   │   └─ IAuditavel.cs                    ← DataCadastro + UltimaModificacao
    │   └─ Entities/                            ← 14 classes mapeadas em tabelas
    │
    ├─ Application/                             ← camada de aplicação
    │   ├─ Services/
    │   │   ├─ IServicoBase.cs / ServicoBase.cs
    │   │   ├─ IServicoVenda.cs / ServicoVenda.cs
    │   │   └─ IArmazenamentoFotos.cs / ArmazenamentoFotos.cs
    │   └─ Helpers/
    │       ├─ Documento.cs                     ← CPF/CNPJ
    │       ├─ Telefone.cs                      ← telefone
    │       └─ MensagensErro.cs                 ← traduz exceções PG
    │
    ├─ Infrastructure/                          ← acesso a dados
    │   ├─ Data/
    │   │   └─ AppDbContext.cs                  ← DbContext + Fluent API
    │   └─ Repositories/
    │       ├─ IRepository.cs / Repository.cs
    │
    ├─ Components/                              ← UI Blazor
    │   ├─ App.razor                            ← root HTML
    │   ├─ Routes.razor                         ← router
    │   ├─ _Imports.razor                       ← @using globais
    │   │
    │   ├─ Layout/
    │   │   ├─ MainLayout.razor + .css          ← shell
    │   │   └─ NavMenu.razor + .css             ← sidebar
    │   │
    │   ├─ Shared/                              ← componentes reutilizáveis
    │   │   ├─ TabelaListagem.razor             ← grid genérica
    │   │   ├─ ModalCrud.razor                  ← modal de cadastro
    │   │   ├─ ConfirmDialog.razor              ← diálogo de confirmação
    │   │   ├─ DialogoDuplicado.razor           ← detecção de duplicados
    │   │   ├─ BotoesAcaoCadastro.razor         ← Editar/Inativar/Excluir
    │   │   ├─ Icone.razor                      ← SVG icons inline
    │   │   └─ NavLinkComMais.razor             ← link com botão "+"
    │   │
    │   └─ Pages/                               ← rotas
    │       ├─ Home.razor                       ← "/"
    │       ├─ Error.razor                      ← "/Error"
    │       ├─ Cadastros/                       ← 10 telas
    │       ├─ Operacao/Estoque.razor
    │       ├─ Vendas/InserirVenda.razor
    │       └─ Financeiro/                      ← placeholders
    │
    └─ wwwroot/
        ├─ app.css                              ← tema (Blue Grey palette)
        ├─ img/logo.png                         ← logo do restaurante
        ├─ uploads/produtos/                    ← fotos de produto
        └─ lib/                                 ← Bootstrap
```

---

## 3. Camadas e responsabilidades

### 3.1 Domain (`Domain/`)

Camada **pura** — sem dependência de EF, ASP.NET ou framework.

- **`Common/EntityBase.cs`** — todas as entidades herdam. Tem `Id` (long, PK) e `Ativo` (bool, soft delete).
- **`Common/IAuditavel.cs`** — opcional. Entidades que implementam ganham `DataCadastro` + `UltimaModificacao` preenchidos automaticamente pelo DbContext.
- **`Entities/*.cs`** — 14 entidades de domínio (detalhadas adiante).

> Métodos de negócio (ex: `Venda.RecalcularTotal()`, `ComandaItem.RecalcularSubtotal()`) ficam na própria entidade.

### 3.2 Application (`Application/`)

Camada que **orquestra** o domínio + a infra.

- **`Services/IServicoBase<T>` + `ServicoBase<T>`** — CRUD genérico com:
  - `ListarAsync(somenteAtivos, filtroTexto)` — listagem com filtro
  - `SalvarAsync(entidade)` — cria ou atualiza + roda `Validator.ValidateObject` (DataAnnotations)
  - `InativarAsync(id)` / `ReativarAsync(id)` — soft delete via `Ativo`
  - `ExcluirDefinitivoAsync(id)` — hard delete
  - `BuscarDuplicadosPorNomeAsync(entidade)` — usa reflection na propriedade `Nome` pra encontrar duplicados case-insensitive

- **`Services/IServicoVenda` + `ServicoVenda`** — fluxo transacional de fechamento de venda:
  1. Recalcula totais via `Venda.RecalcularTotal()`
  2. Decrementa `Produto.Quantidade` de cada item (no mesmo DbContext)
  3. Marca status como Concluída
  4. `SaveChanges` único = atomicidade garantida

- **`Services/IArmazenamentoFotos` + `ArmazenamentoFotos`** — upload/remoção de fotos de produto:
  - Aceita só `.jpg/.jpeg/.png/.webp` até 5 MB
  - Nome do arquivo = `Guid.NewGuid()` (impede path traversal e conflito)
  - Salva em `wwwroot/uploads/produtos/`
  - Retorna URL relativa `/uploads/produtos/<guid>.<ext>`

- **`Helpers/Documento.cs`** — utilitário estático pra CPF/CNPJ:
  - `SomenteDigitos(valor)` — limpa máscara
  - `TamanhoEsperado(tipo)` — 11 (CPF) ou 14 (CNPJ)
  - `Valido(valor, tipo)` — confere comprimento
  - `Formatar(valor, tipo)` — aplica máscara `000.000.000-00` ou `00.000.000/0000-00`

- **`Helpers/Telefone.cs`** — telefone brasileiro vs internacional:
  - `Valido(valor, estrangeiro)` — 10/11 dígitos (br) ou 6–20 (internacional)
  - `Formatar(valor, estrangeiro)` — `(XX) XXXXX-XXXX` ou `+...`

- **`Helpers/MensagensErro.cs`** — traduz `PostgresException` pra texto pro usuário:
  - `23503` (FK violation) → "Este registro está vinculado a outros cadastros…"
  - `23505` (unique violation) → identifica a constraint e diz qual campo está duplicado
  - `23502` (NOT NULL) → "Há campos obrigatórios não preenchidos"

### 3.3 Infrastructure (`Infrastructure/`)

Camada de **acesso ao banco**.

- **`Data/AppDbContext.cs`** — `DbContext` do EF Core:
  - 14 `DbSet<T>`
  - `OnModelCreating`: mapeia colunas com sufixo da tabela via loop (`id_<tabela>`, `ativo_<tabela>`, `data_cadastro_<tabela>`)
  - Configurações Fluent: relacionamentos (FK + `DeleteBehavior`), `Ignore()` em colunas derivadas (`Subtotal`, `ValorTotal`), índices únicos (`uq_produto_codigo_barras`)
  - Seed inicial: 3 Categorias, 2 Marcas, 3 Unidades de Medida (via `HasData`)
  - Override de `SaveChanges`/`SaveChangesAsync` que preenche `IAuditavel.DataCadastro`/`UltimaModificacao`

- **`Repositories/IRepository<T>` + `Repository<T>`** — wrapper genérico sobre EF:
  - Recebe **`IDbContextFactory<AppDbContext>`** (não DbContext direto) — cria contexto novo por operação. Essencial em Blazor Server pra evitar tracking conflicts entre requests.
  - Métodos: `ObterPorIdAsync`, `ListarAsync`, `BuscarAsync(filtro)`, `AdicionarAsync`, `AtualizarAsync`, `RemoverAsync`, `ExisteAsync`
  - Leituras usam `AsNoTracking()` pra performance.

### 3.4 Components (UI)

Camada de apresentação Blazor.

#### 3.4.1 Bootstrap / shell

- **`App.razor`** — HTML root, importa Bootstrap, fonts, ícone.
- **`Routes.razor`** — `<Router>` do Blazor com `DefaultLayout=MainLayout`.
- **`_Imports.razor`** — `@using` globais pra todas as páginas (Domain, Services, Helpers, etc).

#### 3.4.2 Layout

- **`Layout/MainLayout.razor`** — `<div class="page">` com sidebar + main:
  - `<div class="sidebar"><NavMenu /></div>`
  - `<main><article>@Body</article></main>`
- **`Layout/MainLayout.razor.css`** — sidebar com background cinza claro `#e9ecef` + box-shadow.
- **`Layout/NavMenu.razor`** — sidebar com logo + itens:
  - Início
  - **OPERAÇÃO**: Inserir Venda, Estoque
  - **FINANCEIRO**: Contas a Pagar, Contas a Receber, Fluxo de Caixa
  - **Cadastros** (`<details>` colapsável — abre quando URL é `/cadastros/*`):
    - Cardápio: Produtos, Categorias, Marcas, Unidades de Medida
    - Financeiro: Formas de Pagamento
    - Pessoas: Clientes, Fornecedores
    - Localização: Países, Estados, Cidades
  - Cada item de cadastro tem botão "**+**" (`NavLinkComMais`) que abre o modal de "Novo" via query string.
- **`Layout/NavMenu.razor.css`** — estilos.

#### 3.4.3 Shared (componentes reusáveis)

| Componente | Função |
|---|---|
| **`TabelaListagem<TItem>`** | Tabela genérica. Recebe `Itens`, `Cabecalhos`, `LinhaTemplate`, `AcoesTemplate`. Usada em todas as listagens. |
| **`ModalCrud`** | Modal genérico para formulários. CSS-only, sem JS. Suporta camada secundária (z-index maior), confirmação de edição opcional (botão "Confirmar alteração"). |
| **`ConfirmDialog`** | Diálogo de Sim/Não. Camadas (Primaria/Secundaria), cor (Perigo/Aviso/Padrao). |
| **`DialogoDuplicado<TItem>`** | Mostra lista de duplicados encontrados + opções "Salvar mesmo assim" / "Revisar". |
| **`BotoesAcaoCadastro`** | Trio padrão de botões Editar/Inativar/Excluir. Recebe 4 `EventCallback`. |
| **`Icone`** | Renderiza SVG inline. Catálogo: `conta`, `editar`, `compartilhar`, `excluir`, `grafico`, `bloquear`, `reativar`, `caixa`, `cadastros`, `carrinho`, `mais`, `dinheiro`. |
| **`NavLinkComMais`** | `NavLink` + botão "+" do lado. Usado nos itens dos cadastros. |

#### 3.4.4 Pages (`Components/Pages/`)

##### Home (`/`)
Landing page com 6 cards de atalho.

##### Cadastros (`/cadastros/...`)
**10 telas com padrão idêntico**, cada uma `@rendermode InteractiveServer`:

| Rota | Entidade | Particularidades |
|---|---|---|
| `/cadastros/produtos` | Produto | Foto, código de barras, regra de pesagem (Tara/PrecoKg) |
| `/cadastros/categorias` | Categoria | Simples (Nome + Ativo) |
| `/cadastros/marcas` | Marca | Simples |
| `/cadastros/unidades-medida` | UnidadeMedida | Sigla, flag Fracionavel |
| `/cadastros/formas-pagamento` | FormaPagamento | Nome + Descrição |
| `/cadastros/paises` | Pais | Nome, Sigla, Código, Nacionalidade |
| `/cadastros/estados` | Estado | FK Pais + quick-add inline |
| `/cadastros/cidades` | Cidade | FK Estado + quick-add inline |
| `/cadastros/clientes` | Cliente | Tipo Pessoa (PF/PJ), Estrangeiro, CPF/CNPJ, FK Cidade |
| `/cadastros/fornecedores` | Fornecedor | igual Cliente + email/telefone obrigatórios |

**Estrutura interna padronizada** de cada tela:
- Header com título + botão "Novo"
- Linha de filtro (texto + toggle "Apenas ativos")
- `<TabelaListagem>` com `<AcoesTemplate>` usando `<BotoesAcaoCadastro>`
- `<ModalCrud>` principal (criar/editar)
- `<ModalCrud Camada="Secundaria">` para quick-add de FK (quando aplicável)
- `<DialogoDuplicado>` (warning se já existe registro com mesmo nome)
- `<ConfirmDialog>` (confirmar exclusão definitiva)
- `@code` com:
  - `[SupplyParameterFromQuery(Name = "novo")] string? NovoQuery` → auto-abre modal quando `?novo=` na URL (vindo do "+" do menu)
  - `OnParametersSet` checa NovoQuery único via Guid
  - `OnInitializedAsync` carrega lookups + lista
  - Métodos: `AbrirNovo`, `AbrirEdicao`, `SalvarAsync`, `InativarAsync`, `ReativarAsync`, `AbrirConfirmExcluir`, `ConfirmarExcluirAsync`, `SalvarMesmoComDuplicado`

##### Operação
- **`/vendas/inserir`** — PDV completo. Adiciona item com cálculo em tempo real `(PesoBruto - Tara) × PrecoKg` ou `Qtd × ValorVenda`. Totaliza descontos/acréscimos, calcula troco. Ao finalizar chama `IServicoVenda.FinalizarAsync` (atômico).
- **`/operacao/estoque`** — listagem com badge Atual vs Mínimo. Modal de "Ajustar" com 3 modos: Entrada (+), Saída (−), Ajuste manual (=).

##### Financeiro
- **`/financeiro/contas-pagar`** — placeholder.
- **`/financeiro/contas-receber`** — placeholder.
- **`/financeiro/fluxo-caixa`** — placeholder.

---

## 4. Entidades de domínio (detalhes)

Todas em `Domain/Entities/`. Herdam de `EntityBase` (Id + Ativo).

| Entidade | Tabela | Campos chave | Relacionamentos |
|---|---|---|---|
| **Pais** | `pais` | Nome, Codigo, Sigla, Nacionalidade | `Estados[]` |
| **Estado** | `estado` | Nome, Uf | FK→`Pais`, `Cidades[]` |
| **Cidade** | `cidade` | Nome, CodigoIbge | FK→`Estado` |
| **Categoria** | `categoria` | Nome | — |
| **Marca** | `marca` | Nome | — |
| **UnidadeMedida** | `unidade_medida` | Nome, Sigla, **Fracionavel** | — |
| **FormaPagamento** | `forma_pagamento` | Nome, Descricao | — |
| **Produto** | `produto` | Nome, ValorCompra, ValorVenda, Quantidade, QuantidadeMinima, **PrecoKg**, **Tara**, **PermiteFracionamento**, FotoUrl, CodigoBarras | FK→`Marca`, `UnidadeMedida`, `Categoria?` |
| **Cliente** | `cliente` | Nome, TipoPessoa, Estrangeiro, CpfCnpj (unique), Email, Telefone, LimiteCredito | FK→`Cidade?` |
| **Fornecedor** | `fornecedor` | Nome, TipoPessoa, Estrangeiro, CpfCnpj, Email*, Telefone* | FK→`Cidade?` |
| **Comanda** | `comanda` | Numero, ClienteAvulso, DataAbertura, DataFechamento, Status | FK→`Cliente?`, `Itens[]` |
| **ComandaItem** | `comanda_item` | Quantidade, PesoBruto, Tara, PrecoUnitario, Fracionado, Observacao | FK→`Comanda`, `Produto` |
| **Venda** | `venda` | DataVenda, Desconto, Acrescimo, ValorPago, Status | FK→`Comanda?`, `Cliente?`, `FormaPagamento?`, `Itens[]` |
| **VendaItem** | `venda_item` | Quantidade, PesoBruto, Tara, PrecoUnitario, Fracionado, Observacao | FK→`Venda`, `Produto` |

**Enums** (`Domain/Entities/*.cs`):
- `StatusComanda` — Aberta / Fechada / Cancelada
- `StatusVenda` — Concluida / Cancelada
- `TipoPessoa` — Fisica / Juridica

**Colunas derivadas (não persistidas)** — calculadas em memória pra 3ª Forma Normal:
- `Comanda.ValorTotal`
- `ComandaItem.Subtotal`
- `Venda.SubtotalItens`, `Venda.ValorTotal`
- `VendaItem.Subtotal`

Marcadas como `Ignore()` no `OnModelCreating`. O método `RecalcularTotal()` na própria entidade preenche em memória.

---

## 5. Convenções

### 5.1 Banco

- **snake_case** em tabelas e colunas
- **Sufixo da tabela** em cada coluna pra disambiguar joins: `id_cliente`, `nome_pais`, `codigo_barras_produto`
- FKs mantêm o nome do alvo: `cliente_id`, `produto_id`, `cidade_id`
- Colunas onde o nome já contém a entidade ficam como estão: `categoria.categoria`, `produto.produto`, `venda.data_venda`
- `ativo_<tabela>` para soft delete
- `data_cadastro_<tabela>` + `ultima_modificacao_<tabela>` em tabelas auditáveis (timestamptz UTC)

### 5.2 C# / nomes

- Propriedades em **PascalCase** (`Nome`, `DataCadastro`, `CpfCnpj`)
- Métodos async com sufixo `Async`
- Namespaces seguem a estrutura de pastas: `BrasileirinhoRestourant.Domain.Entities`, etc.

### 5.3 Tema visual

- **Paleta Blue Grey** (cinza azulado) como cor primária — definida via CSS variables `--bg-50` a `--bg-900` em `wwwroot/app.css`
- **Sidebar cinza claro** `#e9ecef`
- **Tipos especiais**: Bootstrap default (verde=ativo, vermelho=excluir, amarelo=inativar)

---

## 6. Fluxos importantes

### 6.1 Cadastro padrão (criar/editar/excluir)

```
Usuário clica "Novo" ou "Editar"
  → AbrirNovo() ou AbrirEdicao(item)
  → editando = new() ou cópia do item
  → modalAberto = true (ModalCrud aparece)

Usuário preenche e clica "Salvar"
  → SalvarAsync()
    → Valida campos específicos (CPF, telefone, FKs)
    → Servico.BuscarDuplicadosPorNomeAsync()
        Se houver duplicados → DialogoDuplicado abre → usuário decide
    → Servico.SalvarAsync(editando)
        → ServicoBase: ValidarAsync (DataAnnotations) → repo.AdicionarAsync ou AtualizarAsync
    → Recarrega lista
```

### 6.2 Venda completa

```
/vendas/inserir
  → Usuário seleciona produto no dropdown
    → AoMudarProduto()
      → Carrega Tara padrão do produto
      → Define se mostra campo Peso ou Quantidade conforme PermiteFracionamento
  → Usuário digita peso → RecalcularSubtotalEdit() em tempo real
    → Cria VendaItem efêmero → chama item.RecalcularSubtotal()
    → Mostra fórmula e subtotal na UI
  → Clica "Adicionar à venda"
    → Cria VendaItem definitivo, adiciona em venda.Itens
    → RecalcularTotal() do venda (atualiza SubtotalItens e ValorTotal em memória)
  → Repete pra mais itens
  → Preenche desconto, acréscimo, valor pago, forma de pagamento
  → Clica "Finalizar Venda"
    → ServicoVenda.FinalizarAsync(venda)
      1. venda.RecalcularTotal()
      2. Para cada item: decrementa produto.Quantidade
      3. venda.Status = Concluida, DataVenda = now
      4. DbContext.Vendas.AddAsync(venda)
      5. SaveChanges único → tudo atômico
```

### 6.3 Upload de foto

```
Usuário escolhe arquivo no <InputFile>
  → HandleUpload(InputFileChangeEventArgs e)
    → Valida tamanho (< 5 MB no cliente)
    → ArmazenamentoFotos.SalvarAsync(e.File)
      → Valida extensão (.jpg/.jpeg/.png/.webp)
      → Gera Guid.NewGuid() + extensão
      → Cria pasta wwwroot/uploads/produtos/ se não existir
      → Stream do navegador → arquivo em disco
      → Retorna /uploads/produtos/<guid>.<ext>
    → Remove foto antiga via Fotos.Remover(editando.FotoUrl)
    → editando.FotoUrl = nova URL
```

### 6.4 Soft delete vs hard delete

- **Inativar** (`btn-outline-warning`): só seta `Ativo = false`. Registro fica no banco, mas sumido das listagens com filtro "apenas ativos".
- **Reativar** (`btn-outline-success`): `Ativo = true`.
- **Excluir** (`btn-outline-danger` + `ConfirmDialog`): `DELETE FROM ...`. Pode falhar se houver FK referenciando — `MensagensErro` traduz pra "está vinculado a outros cadastros, prefira inativar".

---

## 7. Banco — convenções e administração

### 7.1 Connection string

Em `appsettings.Development.json`:
```json
"ConnectionStrings": {
  "Postgres": "Host=localhost;Port=5432;Database=BRASILEIRINHO;Username=postgres;Password=__SUA_SENHA__"
}
```

> ⚠️ **Risco:** senha em texto claro. Mover para **User Secrets** em produção:
> ```powershell
> dotnet user-secrets init
> dotnet user-secrets set "ConnectionStrings:Postgres" "Host=..."
> ```

### 7.2 Migrations

**Não usamos EF Core Migrations**. Mudanças de schema são aplicadas via SQL manual no pgAdmin. Workflow:
1. Alterar entidade C#
2. Eu (ou você) gera o SQL `ALTER TABLE …`
3. Backup com `pg_dump`
4. Aplicar via pgAdmin Query Tool

### 7.3 Backup

```powershell
$env:PGPASSWORD = "__SUA_SENHA__"
& "C:\Program Files\PostgreSQL\18\bin\pg_dump.exe" -U postgres -d BRASILEIRINHO -F c `
  -f "$HOME\Downloads\brasileirinho-$((Get-Date).ToString('yyyyMMdd-HHmm')).backup"
```

### 7.4 Recriar do zero

Banco no script `banco-brasileirinho-apresentacao.sql` — roda em qualquer Postgres limpo.

---

## 8. Como rodar

### Pré-requisitos
- .NET 10 SDK
- PostgreSQL 14+ (testado em 18)
- Banco `BRASILEIRINHO` criado (use o `banco-brasileirinho-apresentacao.sql`)

### Comandos
```powershell
cd Brasileirinho-Restourant/Brasileirinho-Restourant
dotnet restore
dotnet run
```

App em `http://localhost:5166`.

### Parar
```powershell
Get-Process Brasileirinho-Restourant -ErrorAction SilentlyContinue | Stop-Process -Force
```

---

## 9. Segurança — estado atual

| Item | Status |
|---|---|
| SQL Injection (LINQ parametrizado) | ✅ Protegido (sem `FromSqlRaw`) |
| XSS via interpolação Razor | ✅ Protegido (sem `MarkupString`) |
| Path traversal no upload | ✅ Protegido (nome = Guid) |
| Tamanho de upload | ✅ 5 MB no service + 20 MB no SignalR |
| CSRF | ✅ `UseAntiforgery` no pipeline |
| Antiforgery em forms | ✅ Bootstrap Blazor inclui |
| Autenticação | ❌ **Não implementada** — qualquer um na rede acessa tudo |
| Autorização (roles) | ❌ Não implementada |
| Senha do DB em config | ⚠️ Texto claro em `appsettings.Development.json` |
| Usuário do DB | ⚠️ Usa `postgres` (superuser) |

**Próximos passos sugeridos:**
1. Criar usuário PG dedicado com permissão só de CRUD
2. Mover connection string pra User Secrets
3. Implementar login (ASP.NET Core Identity)

---

## 10. Pontos de extensão

Adicionar uma nova tela de cadastro:
1. Criar `Domain/Entities/MinhaEntidade.cs` herdando de `EntityBase`
2. Adicionar `DbSet<MinhaEntidade>` em `AppDbContext`
3. Aplicar SQL `CREATE TABLE` no banco
4. Criar `Components/Pages/Cadastros/MinhaEntidade.razor` copiando padrão de Categorias.razor
5. Adicionar link em `NavMenu.razor`
6. `IServicoBase<MinhaEntidade>` já está disponível via DI — sem código adicional

Adicionar um novo ícone:
1. Editar `Components/Shared/Icone.razor`
2. Adicionar entrada no dicionário `_paths`
3. Usar com `<Icone Nome="meu-icone" />`

Adicionar uma nova regra de negócio na venda:
1. Editar `Application/Services/ServicoVenda.cs`
2. Toda a transação é em um único `SaveChangesAsync` — adicionar lógica antes dele

---

## 11. Glossário rápido

- **PDV** — Ponto de Venda
- **Comanda** — pedido aberto (mesa ou ficha) que vira Venda no fechamento
- **Tara** — peso da embalagem subtraído do peso bruto
- **CSOSN** — código fiscal simplificado (Simples Nacional)
- **NFC-e** — Nota Fiscal de Consumidor Eletrônica (modelo 65)
- **CFOP** — Código Fiscal de Operações
- **EF Core** — Entity Framework Core, ORM da Microsoft
- **Blazor Server** — renderização server-side com SignalR para interatividade
- **Soft delete** — marcar como inativo sem apagar do banco
