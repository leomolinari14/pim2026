# Recriar os bancos do PIM Burguer no notebook

Este pacote contém uma cópia atualizada dos dois bancos, com a estrutura de oito migrations e os dados existentes no momento da exportação. Os dois formatos de cada banco foram restaurados e conferidos em bancos temporários. A senha de conexão deste computador não está no pacote.

Para o relatório acadêmico, use os arquivos de estrutura e a base inicial sem clientes em `BANCO/MOBILE/DOCUMENTACAO_ATUALIZADA`. Os backups completos deste diretório contêm cadastros e pedidos e servem para transferir o ambiente ao notebook.

## 1 Instalar e conferir o PostgreSQL

Use PostgreSQL 18 no notebook. Guarde a senha escolhida para o usuário `postgres` e confira a porta da instalação. Os comandos a seguir usam a porta 5432 e a pasta padrão do PostgreSQL 18.

## 2 Criar dois bancos novos

Extraia o ZIP. Abra o PowerShell dentro da pasta `PARA_RECRIAR_NO_NOTEBOOK`, onde estão os arquivos `.backup`. Execute:

```powershell
$pgPim = 'C:\Program Files\PostgreSQL\18\bin'
& "$pgPim\createdb.exe" -h 127.0.0.1 -p 5432 -U postgres -W -T template0 -E UTF8 pimburguer_web_notebook
& "$pgPim\createdb.exe" -h 127.0.0.1 -p 5432 -U postgres -W -T template0 -E UTF8 pimburguer_mobile_notebook
```

Cada comando solicita a senha do PostgreSQL desse notebook. Se um nome já existir, escolha outro banco novo e ajuste os comandos seguintes e as conexões. Não é necessário apagar bancos existentes.

## 3 Restaurar os backups

Na mesma janela e pasta:

```powershell
& "$pgPim\pg_restore.exe" -h 127.0.0.1 -p 5432 -U postgres -W --single-transaction --no-owner --no-privileges -d pimburguer_web_notebook '.\PIM_BURGUER_WEB_ATUAL.backup'
& "$pgPim\pg_restore.exe" -h 127.0.0.1 -p 5432 -U postgres -W --single-transaction --no-owner --no-privileges -d pimburguer_mobile_notebook '.\PIM_BURGUER_MOBILE_ATUAL.backup'
```

Se os comandos terminarem sem erro, a restauração foi concluída. O `.sql` é uma alternativa ao `.backup`: use apenas um formato por banco vazio. Para a alternativa em SQL, troque a restauração por `psql.exe -h 127.0.0.1 -p 5432 -U postgres -W -v ON_ERROR_STOP=1 -d NOME_DO_BANCO -f CAMINHO_DO_SQL`.

## 4 Configurar a API

Abra `PIMBURGUER_API_SIMPLES/PimBurguer.sln` no Visual Studio. Clique com o botão direito no projeto `PimBurguer.Api` e escolha **Gerenciar Segredos do Usuário**. Configure:

```json
{
  "ConnectionStrings:PimBurguer": "Host=127.0.0.1;Port=5432;Database=pimburguer_web_notebook;Username=postgres;Password=SENHA_DO_NOTEBOOK;Timeout=5;Command Timeout=10",
  "ConnectionStrings:PimBurguerApp": "Host=127.0.0.1;Port=5432;Database=pimburguer_mobile_notebook;Username=postgres;Password=SENHA_DO_NOTEBOOK;Timeout=5;Command Timeout=10"
}
```

Substitua o marcador pela senha local e ajuste a porta, se necessário. Salve esse arquivo local de segredos. Não coloque a senha em `Program.cs`, no relatório ou no arquivo de configuração distribuído.

## 5 Executar e conferir

1. No Visual Studio, selecione o perfil `http` e pressione F5.
2. Abra `http://localhost:5181/api/saude`: deve informar o banco `pimburguer_web_notebook`, conectado.
3. Abra `http://localhost:5181/mobile/api/saude`: deve informar `pimburguer_mobile_notebook`, conectado.
4. Abra o Swagger em `http://localhost:5181/swagger` e o site em `http://localhost:5181`.
5. No Android Studio, abra a pasta `pim_burguer_app`, execute `flutter pub get`, selecione um dispositivo Android e clique em Run.
6. Use a mesma conta de cliente no site e no app. Os cupons são compartilhados; os pedidos permanecem separados.

No emulador, o app já usa `http://10.0.2.2:5181/mobile/`. A API precisa estar rodando no mesmo notebook. O SQLite do aparelho é criado pelo próprio aplicativo; ele não é restaurado pelo PostgreSQL.

## Arquivos e estado da cópia

- `PIM_BURGUER_WEB_ATUAL.backup` e `.sql`: mesmo snapshot do banco web.
- `PIM_BURGUER_MOBILE_ATUAL.backup` e `.sql`: mesmo snapshot do banco mobile.
- `MANIFESTO_NOTEBOOK_ATUAL.json`: contagens e resultados das quatro restaurações de verificação.

Depois da transferência, alterações feitas em outro computador não aparecem automaticamente no notebook. Para levar alterações posteriores, gere um novo backup.
