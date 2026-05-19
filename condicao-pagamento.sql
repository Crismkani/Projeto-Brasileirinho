-- =============================================================
-- Condição de Pagamento — executar no pgAdmin (banco BRASILEIRINHO)
-- Fazer pg_dump antes de rodar!
-- =============================================================

-- 1. Nova tabela
CREATE TABLE condicao_pagamento (
    id_condicao_pagamento           BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    nome_condicao_pagamento         VARCHAR(100)   NOT NULL,
    descricao_condicao_pagamento    VARCHAR(200),
    num_parcelas_condicao_pagamento INTEGER        NOT NULL DEFAULT 1,
    prazo_dias_condicao_pagamento   INTEGER        NOT NULL DEFAULT 0,
    taxa_juros_condicao_pagamento   NUMERIC(5,2)   NOT NULL DEFAULT 0.00,
    ativo_condicao_pagamento        BOOLEAN        NOT NULL DEFAULT TRUE,
    data_cadastro_condicao_pagamento      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    ultima_modificacao_condicao_pagamento TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- 2. FK em venda (nullable — vendas existentes ficam sem condição)
ALTER TABLE venda
    ADD COLUMN condicao_pagamento_id BIGINT,
    ADD CONSTRAINT fk_venda_condicao_pagamento
        FOREIGN KEY (condicao_pagamento_id)
        REFERENCES condicao_pagamento(id_condicao_pagamento)
        ON DELETE RESTRICT;

-- 3. FK em fornecedor (nullable — fornecedores existentes ficam sem condição)
ALTER TABLE fornecedor
    ADD COLUMN condicao_pagamento_id BIGINT,
    ADD CONSTRAINT fk_fornecedor_condicao_pagamento
        FOREIGN KEY (condicao_pagamento_id)
        REFERENCES condicao_pagamento(id_condicao_pagamento)
        ON DELETE RESTRICT;
