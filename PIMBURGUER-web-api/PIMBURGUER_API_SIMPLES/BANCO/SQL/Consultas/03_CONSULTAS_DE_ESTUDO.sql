-- CONSULTAS DE ESTUDO - apenas leitura, banco PostgreSQL do PIM
-- A contagem de valores registrados inclui pedidos nao pagos e cancelados.
SELECT id, nome, preco, categoria, ativo, removido FROM produtos ORDER BY id;
SELECT id, criado_em, status, recebimento, pagamento, pago, total
FROM pedidos ORDER BY criado_em DESC;
SELECT p.id AS pedido, p.status, i.nome_produto, i.quantidade,
       i.preco_unitario, i.subtotal
FROM pedidos p JOIN itens_pedido i ON i.pedido_id = p.id
ORDER BY p.criado_em DESC, i.id;
SELECT u.id, u.nome, u.perfil, u.ativo, count(p.id) AS pedidos_registrados
FROM usuarios u LEFT JOIN pedidos p ON p.usuario_id = u.id
GROUP BY u.id, u.nome, u.perfil, u.ativo ORDER BY u.nome;
SELECT status, count(*) AS quantidade, sum(total) AS valor_registrado
FROM pedidos GROUP BY status ORDER BY status;
SELECT count(*) FILTER (WHERE pago) AS pedidos_marcados_pagos,
       coalesce(sum(total) FILTER (WHERE pago), 0) AS valor_marcado_pago
FROM pedidos;
SELECT pedido_id, criado_em, status_anterior, status_novo,
       pago_anterior, pago_novo FROM movimentacoes_pedido ORDER BY criado_em;
SELECT "MigrationId", "ProductVersion" FROM "__EFMigrationsHistory"
ORDER BY "MigrationId";
SELECT table_name, column_name, data_type, is_nullable
FROM information_schema.columns WHERE table_schema='public'
ORDER BY table_name, ordinal_position;

-- Banco de dados utilizado pela API.
SELECT current_database() AS banco_atual;
SELECT datname FROM pg_database WHERE datname = 'pimburguer_v2';
