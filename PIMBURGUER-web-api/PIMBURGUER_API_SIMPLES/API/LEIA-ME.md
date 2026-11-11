# API

Para abrir no Visual Studio, dê dois cliques em `PimBurguer.sln` na pasta principal. Também é possível abrir este `PimBurguer.Api.csproj` diretamente ou executar os comandos abaixo dentro desta pasta:

```powershell
$env:ConnectionStrings__PimBurguer = 'Host=127.0.0.1;Port=5432;Database=pimburguer_v2;Username=postgres;Password=SUA_SENHA'
dotnet run --launch-profile http
```

Restaure o banco primeiro, conforme `../BANCO/LEIA-ME.md`. Sistema: http://localhost:5181. Swagger: http://localhost:5181/swagger.

- `Controllers/`: endpoints HTTP.
- `Services/`: regras do sistema, autenticação e imagens.
- `Data/`, `Models/` e `Migrations/`: mapeamento e evolução do PostgreSQL.
- `Dtos/` e `Validacao/`: dados de entrada/saída e validações.
- `CONFIGURACAO_LOJA.json`: nome da loja, WhatsApp e taxa de entrega.

As imagens e o site ficam em `../WEB`. A API não inicia nem encerra o servidor PostgreSQL. A conexão é configurada pelos arquivos appsettings ou pela variável `ConnectionStrings__PimBurguer`; a senha real não foi copiada para os fontes.

O comando padrão usa o perfil Development de `Properties/launchSettings.json`. Para publicar, configure a conexão, uma chave `Jwt__Key` com pelo menos 32 bytes e HTTPS no ambiente de destino. A pasta WEB deve acompanhar a API como pasta irmã, ou o site deve ser hospedado separadamente.

O canal do app usa as rotas `/mobile/api/...` e a conexão `ConnectionStrings:PimBurguerApp`, com outro banco PostgreSQL. Configure as duas conexões nos Segredos do Usuário ou no ambiente. A rota escolhe o banco no servidor e o JWT é validado para o respectivo canal. Consulte `../INTEGRACAO_APP.md`.
