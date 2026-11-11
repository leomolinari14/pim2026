# Guia para entender a API do PIMBURGUER

Esta versão mantém a API existente e aproxima a escrita e a organização dos exemplos do professor. O site e o banco foram preservados. As classes usam construtores explícitos; as consultas e regras ficam nos serviços; os controllers recebem os pedidos HTTP e devolvem as respostas.

## Tecnologias mantidas

- ASP.NET Core e C# no .NET 9.
- Entity Framework Core com Npgsql para acessar o PostgreSQL.
- JWT HS256 para autenticação, com validade de 30 minutos.
- PasswordHasher para senhas e sessões no banco para revogação de acesso.
- Swagger para consultar e testar as rotas.
- DTOs, validações, migrations e transações.

## Como as partes se comunicam

Site ou aplicativo → Controller → Service → Entity Framework Core → PostgreSQL.

A resposta percorre o caminho de volta e chega à interface em JSON.

## Comece por estes arquivos

1. `API/Models/Produto.cs`: representa um produto salvo no banco.
2. `API/Dtos/ProdutoResposta.cs`: define os campos que a API devolve no cardápio.
3. `API/Data/PimBurguerDbContext.cs`: liga as classes às tabelas do PostgreSQL.
4. `API/Services/ProdutoService.cs`: consulta o cardápio e executa o cadastro administrativo.
5. `API/Controllers/ProdutosController.cs`: recebe `GET /api/produtos` e chama o serviço.
6. `API/Program.cs`: configura a conexão, os serviços, o JWT, o Swagger e as rotas.

No exemplo do professor, `LivrosController` recebe `LivroService` pelo construtor. Aqui, `ProdutosController` recebe `ProdutoService` do mesmo modo. A injeção de dependência do ASP.NET cria essas instâncias para a requisição.

## Controller e Service

O controller interpreta os parâmetros e devolve códigos HTTP, como 200 para sucesso, 404 para recurso não encontrado e 401 para falta de autenticação. As regras e o acesso aos dados ficam no serviço.

Os módulos de administração e conta seguem essa divisão. O endpoint de saúde faz uma consulta curta para verificar se o banco está disponível.

## Entity Framework Core

`PimBurguerDbContext` representa a conexão e o mapeamento das entidades. `Produtos`, `Pedidos` e `Usuarios` são conjuntos de registros.

Uma consulta com `Where` seleciona os registros; `ToListAsync` executa a consulta; `Add` prepara uma inclusão; `SaveChangesAsync` grava as alterações. `AsNoTracking` é usado nas leituras que não precisam editar os objetos. As migrations registram a evolução da estrutura do banco.

As tabelas desta versão são as oito tabelas de negócio mais `__EFMigrationsHistory`. Os arquivos antigos de integração permanecem somente no histórico das migrations; a configuração ativa desta API usa um banco do canal web.

## Login com JWT

1. A interface envia usuário/e-mail e senha para `POST /api/conta/login`.
2. `AcessoService` consulta o usuário e verifica o hash da senha.
3. `TokenService` gera o JWT com identificação, perfil e prazo de validade.
4. O retorno contém `token` e o mesmo valor em `access_token`, além de `token_type` e `expires_in`.
5. A interface envia `Authorization: Bearer TOKEN` nas operações protegidas.
6. O ASP.NET valida assinatura, emissor, público e validade do JWT.
7. `AutenticacaoToken` confere se a sessão ainda existe e se o usuário está ativo.

Logout, troca de senha e bloqueio administrativo revogam sessões. O banco guarda hashes de senha e dos tokens de sessão.

## Pedido e cupom

O cliente envia identificadores e quantidades. `PedidoService` busca os preços no banco, valida os itens e calcula o total. `CupomService` confere validade, compra mínima e limites de utilização. O pedido e os itens são gravados em uma transação.

O orçamento tem um hash que permite detectar mudança de valores antes da confirmação. O identificador do pedido permite reconhecer envios repetidos. O token de consulta protege a leitura do pedido. Esses controles foram mantidos porque fazem parte do checkout que o site já utiliza.

## Classes e records

As entidades e as entradas são classes com propriedades. Vários DTOs de resposta são `record`, recurso também presente no exemplo de imutabilidade do professor. Eles agrupam os dados devolvidos pela API. Cada tipo está em seu próprio arquivo.

## Rotas para o protótipo mobile

O protótipo poderá consumir os mesmos contratos HTTP da API. O código do protótipo ainda precisa ser integrado e testado; ele não acompanha este pacote.

- `GET /api/produtos`: cardápio público.
- `POST /api/conta/cadastro`: cria uma conta de cliente.
- `POST /api/conta/login`: autentica e retorna o JWT.
- `POST /api/pedidos/orcamento`: calcula os valores do carrinho.
- `POST /api/pedidos`: registra o pedido com autenticação.
- `GET /api/cliente/pedidos`: histórico do usuário autenticado.
- `/api/admin/...`: operações do administrador.

Os campos e modelos completos estão no Swagger. A API atende `/api/...` e `/mobile/api/...` com bancos separados para pedidos e produtos. Cupons e seus limites de utilização são compartilhados. Contas, senhas e sessões são compartilhadas usando a conexão web `PimBurguer`; cada canal emite seu próprio JWT. O vínculo local do usuário preserva o histórico de pedidos de cada banco.

## Executar

Restaure o backup conforme `BANCO/LEIA-ME.md`. No PowerShell, dentro de `API`:

```powershell
$env:ConnectionStrings__PimBurguer = 'Host=127.0.0.1;Port=5432;Database=pimburguer_v2;Username=postgres;Password=SUA_SENHA'
dotnet run --launch-profile http
```

Sistema: http://localhost:5181. Swagger: http://localhost:5181/swagger.

## Referências das aulas consultadas

- [API de livros e serviços](https://github.com/teodorolucaas/ProgramacaoAplicadaDotNet/tree/main/ApiLivros).
- [DTOs](https://github.com/teodorolucaas/ProgramacaoAplicadaDotNet/tree/main/DTO).
- [Records e imutabilidade](https://github.com/teodorolucaas/ProgramacaoAplicadaDotNet/tree/main/Imutabilidade).

Foi usado o estilo desses exemplos. O armazenamento permanece no PostgreSQL, conforme as necessidades do PIMBURGUER.
