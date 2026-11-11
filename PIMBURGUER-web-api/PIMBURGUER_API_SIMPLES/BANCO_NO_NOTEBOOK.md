# Banco de dados neste computador e no notebook

## Recomendação

Use um servidor PostgreSQL instalado normalmente no notebook, na versão 18, para restaurar o backup preparado nesta entrega. O banco do pacote já usa PostgreSQL 18. Um dump gerado por uma versão mais nova não tem compatibilidade garantida com um servidor de versão anterior.

Referências: [portabilidade entre versões](https://www.postgresql.org/docs/18/app-pgdump.html) e [restauração de backups](https://www.postgresql.org/docs/18/app-pgrestore.html).

## Conexão já ajustada neste computador

A API foi verificada utilizando a configuração local dos Segredos do Usuário:

- Servidor: `127.0.0.1`.
- Porta: `65434`.
- Banco: `pimburguer_apresentacao`.
- Usuário: `postgres`.

A senha foi reaproveitada da configuração já existente do banco preparado. Ela fica fora do projeto, nos Segredos do Usuário do Windows atual. O PostgreSQL 17 instalado na porta 5432 e sua senha não foram alterados.

Se o Visual Studio estava parado no erro, pressione **Shift+F5** e execute novamente com **F5**, para carregar a conexão ajustada. O Swagger deve abrir em http://localhost:5181/swagger. A rota http://localhost:5181/api/saude deve indicar `banco: conectado`.

O servidor PostgreSQL 18 preparado foi iniciado para esta sessão. Seus dados físicos permanecem em `D:\PIMBURGUER_PARA_APRESENTAR\PIMBURGUER\BANCO\.dados`. Preserve essa pasta enquanto este computador utilizar esse banco.

Se o computador for reiniciado, esse servidor do pacote precisa ser iniciado novamente. No PowerShell:

```powershell
& 'D:\PIMBURGUER_PARA_APRESENTAR\PIMBURGUER\BANCO\PostgreSQL\bin\pg_ctl.exe' start -D 'D:\PIMBURGUER_PARA_APRESENTAR\PIMBURGUER\BANCO\.dados' -l 'D:\PIMBURGUER_PARA_APRESENTAR\PIMBURGUER\API\.local\postgresql.log' -o '-h 127.0.0.1 -p 65434' -w
```

## O que levar para o notebook

Copie a pasta inteira `PIMBURGUER_API_SIMPLES`, com API, WEB, BANCO e a solução `PimBurguer.sln`. O novo backup está em `BANCO/Backup/PIM_BURGUER_PARA_NOTEBOOK.backup`. O SQL equivalente está no mesmo diretório, junto com `MANIFESTO_NOTEBOOK.json`.

O backup atualizado contém 7 produtos, 7 usuários, 5 pedidos, 7 itens de pedido e os demais registros detalhados no manifesto. Backup e SQL foram restaurados em bancos isolados; as contagens das nove tabelas coincidiram. As imagens referenciadas acompanham `WEB/imagens` e `WEB/Uploads`.

Esta é uma cópia dos dados no momento da exportação. Alterações futuras exigem gerar outro backup. O banco do notebook será independente do banco deste computador.

## Restaurar no notebook

1. Instale PostgreSQL 18 e guarde a senha que escolher para o usuário `postgres`. Os comandos abaixo pressupõem o caminho padrão da instalação e a porta 5432.
2. Extraia o projeto completo e abra o PowerShell dentro de `PIMBURGUER_API_SIMPLES`.
3. Crie um banco novo e vazio. O comando solicitará a senha escolhida no notebook:

```powershell
& 'C:\Program Files\PostgreSQL\18\bin\createdb.exe' -h 127.0.0.1 -p 5432 -U postgres -W -T template0 -E UTF8 pimburguer_v2
```

4. Restaure o backup nesse banco vazio:

```powershell
& 'C:\Program Files\PostgreSQL\18\bin\pg_restore.exe' -h 127.0.0.1 -p 5432 -U postgres -W --no-owner --no-privileges --single-transaction -d pimburguer_v2 '.\BANCO\Backup\PIM_BURGUER_PARA_NOTEBOOK.backup'
```

Se já houver um banco chamado `pimburguer_v2`, use outro nome vazio e ajuste a conexão. A restauração acima não apaga um banco existente. Use o backup ou o SQL equivalente, uma única vez no banco vazio.

## Configurar a API no notebook

1. Abra `PimBurguer.sln` no Visual Studio.
2. Clique com o botão direito no projeto `PimBurguer.Api` e escolha **Gerenciar Segredos do Usuário**.
3. Configure a conexão com a senha escolhida no PostgreSQL daquele notebook:

```json
{
  "ConnectionStrings:PimBurguer": "Host=127.0.0.1;Port=5432;Database=pimburguer_v2;Username=postgres;Password=SUA_SENHA;Timeout=5;Command Timeout=10"
}
```

4. Salve e execute com **F5**. Verifique http://localhost:5181/api/saude e abra o site em http://localhost:5181.

O arquivo de Segredos do Usuário é local de cada Windows e não acompanha o ZIP. Não é necessário alterar os arquivos C# nem copiar a senha deste computador para o notebook.

## Banco do aplicativo

O app possui outro banco PostgreSQL, com backup em `BANCO/MOBILE`. Para levar os dois canais ao notebook, siga também `INTEGRACAO_APP.md` e preserve as duas entradas de conexão nos Segredos do Usuário. O exemplo de JSON acima configura apenas o site.
