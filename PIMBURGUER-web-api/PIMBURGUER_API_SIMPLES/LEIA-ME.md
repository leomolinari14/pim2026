# PIMBURGUER — API refatorada para estudo

- **WEB/**: sistema web em HTML, CSS e JavaScript, com imagens e bibliotecas usadas pelo site.
- **API/**: fontes ASP.NET Core, projeto C#, configurações e migrations.
- **BANCO/**: backups PostgreSQL, estrutura SQL e consultas de estudo.

A escrita da API foi aproximada dos exemplos do professor, mantendo as tecnologias e os contratos HTTP. Comece por `GUIA_DA_API.md` para entender o fluxo do código.

## Abrir no Visual Studio

Dê dois cliques em `PimBurguer.sln`, nesta pasta. A solução já inclui o projeto `API/PimBurguer.Api.csproj`.

Para usar a conexão já ajustada neste computador ou restaurar os dados no notebook, siga `BANCO_NO_NOTEBOOK.md`.

## Executar

É necessário ter o SDK .NET 9 ou posterior, o runtime ASP.NET Core 9 e um servidor PostgreSQL instalado. Os backups fornecidos foram gerados pelo PostgreSQL 18.6; use ferramentas compatíveis.

1. Restaure o backup em um banco vazio, seguindo `BANCO/LEIA-ME.md`.
2. Abra o PowerShell dentro de `API` e configure a conexão para o seu PostgreSQL:

```powershell
$env:ConnectionStrings__PimBurguer = 'Host=127.0.0.1;Port=5432;Database=pimburguer_v2;Username=postgres;Password=SUA_SENHA'
dotnet run --launch-profile http
```

3. Abra o sistema em http://localhost:5181 e a documentação da API em http://localhost:5181/swagger.

O site continua fisicamente na pasta WEB. A API pode servir esses arquivos pelo mesmo endereço. Para usar Live Server ou outro servidor web local, `WEB/js/config.js` já aponta para a API na porta 5181.

Encerre a API com Ctrl+C. O PostgreSQL é gerenciado separadamente pelo seu servidor instalado.

O administrador local configurado para desenvolvimento é `admin_pim`, com senha `pimburguer2026`. Ele só é criado se ainda não existir; uma senha já alterada no banco continua valendo.

## Dados incluídos

Os backups e os arquivos de estudo foram copiados integralmente do pacote original. Eles representam o snapshot descrito em `BANCO/Backup/MANIFESTO_BACKUP.json`, de 08/11/2026. As imagens enviadas estão em `WEB/Uploads`. Além desses arquivos originais, `PIM_BURGUER_PARA_NOTEBOOK.backup` contém uma exportação atual do banco preparado, de 09/11/2026, verificada conforme `MANIFESTO_NOTEBOOK.json`.

A versão original de apresentação foi preservada. Esta versão usa a inicialização normal da API e não inclui launcher, executáveis publicados, servidor PostgreSQL portátil, logs ou diretórios privados do computador.

## Aplicativo Android e banco independente

A mesma API também atende o aplicativo em `/mobile/api/...`, usando o banco separado `pimburguer_mobile`. O sistema web mantém `/api/...` e seu banco atual. Para executar o app e restaurar seu banco em outro computador, siga `INTEGRACAO_APP.md`. O projeto Android fica em uma pasta separada.
