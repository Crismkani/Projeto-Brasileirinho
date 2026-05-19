# Brasileirinho PDV

Sistema ERP de ponto de venda (PDV) para restaurante self-service. Projeto desenvolvido para a matéria de **Projetos de Sistemas**.

Cobre o fluxo operacional completo: cadastros básicos, venda no balcão com cálculo por peso (self-service), e módulo financeiro com contas a pagar/receber, fluxo de caixa e dashboard analítico.

---

## Stack

- **.NET 10** (`net10.0`) + **C# 13**
- **Blazor Server** com `@rendermode InteractiveServer`
- **Entity Framework Core 10** + **Npgsql 10** (sem migrations — schema gerenciado por SQL manual)
- **PostgreSQL 18**
- **Bootstrap 5** + CSS isolado por componente
- **Syncfusion Blazor 33.2.6** (Layouts, Charts, Grid) — para a dashboard de visão geral

---

## Funcionalidades

### Operação
- **Inserir venda** no PDV com cálculo automático por peso (`Subtotal = (PesoBruto - Tara) × PrecoUnitario`) ou por unidade
- **Comandas** (mesas/clientes) com itens
- **Estoque** básico

### Financeiro
- **Contas a pagar** — CRUD + ação Pagar com data, valor e forma de pagamento
- **Contas a receber** — CRUD + geração automática a partir de vendas com prazo
- **Fluxo de caixa** — resumo do período + projeção (a receber/pagar)
- **Dashboard de visão geral** (Home) — KPIs do mês, gráfico de entradas vs saídas (6 meses), despesas por fornecedor e últimas movimentações

### Cadastros (10 telas com padrão idêntico)
- Cardápio: Produtos, Categorias, Marcas, Unidades de Medida
- Financeiro: Formas de Pagamento, Condições de Pagamento
- Pessoas: Clientes, Fornecedores
- Localização: Países, Estados, Cidades

---

## Pré-requisitos

- [.NET SDK 10](https://dotnet.microsoft.com/download)
- [PostgreSQL 18](https://www.postgresql.org/download/)
- Visual Studio 2022/2026 ou VS Code (opcional)

---

## Setup

### 1. Clonar o repositório

```powershell
git clone https://github.com/Crismkani/Projeto-Brasileirinho.git
cd Projeto-Brasileirinho
```

### 2. Criar o banco

No pgAdmin ou psql, crie um banco chamado `BRASILEIRINHO` e rode os scripts SQL **na ordem**:

```powershell
psql -U postgres -d BRASILEIRINHO -f banco-brasileirinho-apresentacao.sql
psql -U postgres -d BRASILEIRINHO -f condicao-pagamento.sql
psql -U postgres -d BRASILEIRINHO -f financeiro.sql
```

### 3. Configurar credenciais

Copie o arquivo de exemplo e substitua a senha pela sua:

```powershell
Copy-Item Brasileirinho-Restourant/appsettings.Development.example.json Brasileirinho-Restourant/appsettings.Development.json
```

Edite `Brasileirinho-Restourant/appsettings.Development.json` e troque `__TROQUE_PELA_SUA_SENHA__` pela senha real do PostgreSQL. Esse arquivo está no `.gitignore` e nunca vai pro repositório.

### 4. (Opcional) Chave Syncfusion

A dashboard usa componentes Syncfusion que pedem licença. A [Community License](https://www.syncfusion.com/products/communitylicense) é gratuita para empresas com receita menor que US$ 1M/ano. Sem chave, os componentes mostram um aviso mas continuam funcionando em desenvolvimento.

Configure via User Secrets (recomendado) ou no `appsettings.Development.json`:

```powershell
cd Brasileirinho-Restourant
dotnet user-secrets init
dotnet user-secrets set "Syncfusion:LicenseKey" "SUA_CHAVE_AQUI"
```

### 5. Restaurar e rodar

```powershell
dotnet restore
dotnet run --project Brasileirinho-Restourant
```

App disponível em `http://localhost:5166`.

---

## Estrutura

```
Projeto-Brasileirinho/
├─ Brasileirinho-Restourant.sln
├─ DOCUMENTACAO.md                       ← documentação técnica completa
├─ banco-brasileirinho-apresentacao.sql  ← schema inicial
├─ condicao-pagamento.sql                ← script incremental
├─ financeiro.sql                        ← script incremental
└─ Brasileirinho-Restourant/
   ├─ Domain/{Common,Entities}/          ← entidades de domínio
   ├─ Application/{Services,Helpers}/    ← regras de negócio
   ├─ Infrastructure/{Data,Repositories}/ ← EF Core, repositórios
   ├─ Components/{Layout,Shared,Pages}/  ← componentes Blazor
   ├─ wwwroot/                           ← Bootstrap, app.css, uploads
   ├─ Program.cs
   └─ appsettings.Development.example.json
```

---

## Convenções

### Banco
- **snake_case** em tudo
- Sufixo da tabela em cada coluna: `id_cliente`, `nome_pais`, `codigo_barras_produto`
- FKs no formato `entidade_id`
- Soft delete via `ativo_<tabela>` (boolean)
- Audit via `data_cadastro_<tabela>` + `ultima_modificacao_<tabela>`

### Código
- C# em **PascalCase**
- Métodos async com sufixo `Async`
- Toda entidade herda de `EntityBase` (Id + Ativo)
- Acesso a dados sempre via `IDbContextFactory<AppDbContext>` (Blazor Server exige para evitar conflitos de tracking)
- Comentários só quando explicam *por quê*, não *o quê*

Detalhes em [DOCUMENTACAO.md](DOCUMENTACAO.md) e diretrizes para IA em [CLAUDE.md](CLAUDE.md).

---

## Status

Em desenvolvimento ativo. Funcionalidades implementadas e funcionando:

- Todos os 10 cadastros básicos
- Operação de venda no PDV
- Módulo financeiro completo (pagar, receber, fluxo de caixa)
- Dashboard de visão geral com gráficos

### Próximos passos

- Autenticação (ASP.NET Core Identity) — hoje sem login
- Migrar credenciais do banco para User Secrets na configuração padrão
- Criar usuário PostgreSQL dedicado (não usar `postgres` superuser)
- Parcelamento automático em condições de pagamento

---

## Autor

[@Crismkani](https://github.com/Crismkani) — Cristian M. K.
