# Validação da refatoração da API

Conferência concluída em 09/11/2026.

- Compilação Release: zero erros e zero avisos.
- 156 verificações executadas, abrangendo as versões anterior e refatorada.
- OpenAPI integral idêntico: mesmas rotas, parâmetros, schemas e definições de segurança.
- Comparação dos resultados de catálogo, orçamento, taxa de entrega, cupom e pedido.
- Cadastro, login JWT HS256 com validade de 30 minutos, perfis de acesso e recusa de token adulterado.
- Edição de perfil, troca de senha, logout, bloqueio administrativo e revogação de sessões.
- Inclusão, consulta, edição, remoção lógica e restauração de produtos; upload de PNG e recusa de SVG.
- Cadastro, edição e desativação de cupons; regras de utilização e desconto percentual.
- Checkout, reconhecimento de envios repetidos, histórico, repetição de pedido e consulta protegida por token.
- Atualização administrativa de status e pagamento, histórico de movimentações e relatório de vendas.
- Persistência de um único pedido no PostgreSQL e armazenamento de senha com hash.
- Arquivos de WEB e BANCO preservados por comparação de hashes.
- Pacotes e versões do projeto C# preservados.

Os testes usaram dois bancos isolados restaurados do backup. Os bancos reais não foram alterados. Os processos usados pelos testes foram encerrados.

O protótipo mobile não acompanha este pacote e sua integração ainda precisa ser testada com seu código. Esta validação não afirma que o aplicativo móvel ou a sincronização entre dois bancos estejam implementados.
