-- =============================================================
-- Módulo Financeiro — executar no pgAdmin (banco BRASILEIRINHO)
-- Fazer pg_dump antes de rodar!
-- =============================================================

-- 1. Contas a Pagar
CREATE TABLE conta_pagar (
    id_conta_pagar               BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    fornecedor_id                BIGINT,
    descricao_conta_pagar        VARCHAR(255)    NOT NULL,
    valor_conta_pagar            NUMERIC(12,2)   NOT NULL,
    data_emissao_conta_pagar     DATE            NOT NULL,
    data_vencimento_conta_pagar  DATE            NOT NULL,
    data_pagamento_conta_pagar   DATE,
    valor_pago_conta_pagar       NUMERIC(12,2),
    status_conta_pagar           VARCHAR(20)     NOT NULL DEFAULT 'Pendente',
    forma_pagamento_id           BIGINT,
    condicao_pagamento_id        BIGINT,
    observacao_conta_pagar       VARCHAR(500),
    ativo_conta_pagar            BOOLEAN         NOT NULL DEFAULT TRUE,
    data_cadastro_conta_pagar          TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    ultima_modificacao_conta_pagar     TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    CONSTRAINT fk_cp_fornecedor   FOREIGN KEY (fornecedor_id)       REFERENCES fornecedor(id_fornecedor)               ON DELETE RESTRICT,
    CONSTRAINT fk_cp_forma_pag    FOREIGN KEY (forma_pagamento_id)  REFERENCES forma_pagamento(id_forma_pagamento)     ON DELETE RESTRICT,
    CONSTRAINT fk_cp_cond_pag     FOREIGN KEY (condicao_pagamento_id) REFERENCES condicao_pagamento(id_condicao_pagamento) ON DELETE RESTRICT
);

-- 2. Contas a Receber
CREATE TABLE conta_receber (
    id_conta_receber               BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    cliente_id                     BIGINT,
    venda_id                       BIGINT,
    descricao_conta_receber        VARCHAR(255)    NOT NULL,
    valor_conta_receber            NUMERIC(12,2)   NOT NULL,
    data_emissao_conta_receber     DATE            NOT NULL,
    data_vencimento_conta_receber  DATE            NOT NULL,
    data_recebimento_conta_receber DATE,
    valor_recebido_conta_receber   NUMERIC(12,2),
    status_conta_receber           VARCHAR(20)     NOT NULL DEFAULT 'Pendente',
    forma_pagamento_id             BIGINT,
    observacao_conta_receber       VARCHAR(500),
    ativo_conta_receber            BOOLEAN         NOT NULL DEFAULT TRUE,
    data_cadastro_conta_receber          TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    ultima_modificacao_conta_receber     TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    CONSTRAINT fk_cr_cliente    FOREIGN KEY (cliente_id)          REFERENCES cliente(id_cliente)                 ON DELETE RESTRICT,
    CONSTRAINT fk_cr_venda      FOREIGN KEY (venda_id)            REFERENCES venda(id_venda)                     ON DELETE RESTRICT,
    CONSTRAINT fk_cr_forma_pag  FOREIGN KEY (forma_pagamento_id)  REFERENCES forma_pagamento(id_forma_pagamento) ON DELETE RESTRICT
);

-- Índices úteis para filtros por status/vencimento
CREATE INDEX idx_conta_pagar_status_venc  ON conta_pagar  (status_conta_pagar, data_vencimento_conta_pagar);
CREATE INDEX idx_conta_receber_status_venc ON conta_receber (status_conta_receber, data_vencimento_conta_receber);

-- ADICIONADO DEPOIS: número do documento e arquivo da nota
-- (só executar se as tabelas já existem sem essas colunas)
-- ALTER TABLE conta_pagar
--     ADD COLUMN nro_documento_conta_pagar VARCHAR(50),
--     ADD COLUMN arquivo_nota_conta_pagar  VARCHAR(500);
