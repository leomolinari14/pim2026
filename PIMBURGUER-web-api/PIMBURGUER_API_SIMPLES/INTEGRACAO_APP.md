# Integração do aplicativo Android

O aplicativo fica em `D:\PIM_Burguer_App_Integrado\pim_burguer_app`. A API permanece nesta pasta, sem ser copiada para dentro do aplicativo. Abra `PimBurguer.sln` e execute o perfil **http**, como antes. O servidor PostgreSQL preparado continua na porta 65434.

## Dois bancos, uma API

| Uso | Rotas | Banco neste computador | Conexão |
| --- | --- | --- | --- |
| Sistema web | `/api/...` | `pimburguer_apresentacao` | `ConnectionStrings:PimBurguer` |
| Aplicativo | `/mobile/api/...` | `pimburguer_mobile` | `ConnectionStrings:PimBurguerApp` |

Os dois bancos usam o mesmo modelo do Entity Framework Core e as mesmas regras da API. `Services/CanalAcesso.cs` identifica a rota; `Program.cs` escolhe a conexão correspondente para aquela requisição. Controllers possuem os dois caminhos. O JWT contém o canal e só é aceito no canal em que foi emitido. A sessão também é consultada no respectivo banco.

O login é compartilhado: contas, senhas e sessões usam a conexão `PimBurguer`, tanto no site quanto no app. O mesmo e-mail e senha funcionam nos dois canais. O mobile mantém um vínculo local do usuário para preservar as referências de seus pedidos. Pedidos e produtos continuam no banco de cada canal. O catálogo de cupons e os códigos salvos pelo cliente são compartilhados; os limites de uso somam os dois canais. Contas antigas exclusivas do mobile são incorporadas após a senha ser validada; em caso de e-mail duplicado, a conta web prevalece. Os JWTs continuam restritos ao canal de emissão. As imagens públicas continuam em `WEB/Uploads`.

## Executar neste computador

1. Inicie o PostgreSQL preparado na porta 65434, como já faz para o site.
2. Na pasta desta API, abra `PimBurguer.sln` no Visual Studio e execute **http**. As duas conexões já estão nos Segredos do Usuário do Windows, fora dos fontes.
3. Confira `http://localhost:5181/api/saude`: `nomeBanco` deve ser `pimburguer_apresentacao`.
4. Confira `http://localhost:5181/mobile/api/saude`: `nomeBanco` deve ser `pimburguer_mobile`.
5. No Android Studio, abra somente a pasta do app que contém `pubspec.yaml`. Execute `flutter pub get` e `flutter run` com um emulador selecionado.

O endereço padrão do app em debug é `http://10.0.2.2:5181/mobile/`. A API atende pelo computador, enquanto o app usa o endereço de acesso ao computador pelo emulador. As rotas do site permanecem iguais.

Para celular via USB, execute `adb reverse tcp:5181 tcp:5181` e `flutter run --dart-define=API_BASE_URL=http://127.0.0.1:5181/mobile`. HTTP é usado no APK debug; a configuração original exige HTTPS para release.

## Recriar o banco do app no notebook

Use o PostgreSQL 18 ou ferramentas compatíveis com o backup. O banco web continua sendo restaurado conforme `BANCO_NO_NOTEBOOK.md`. O banco do app tem seu próprio backup em `BANCO/MOBILE/PIM_BURGUER_MOBILE_INICIAL.backup`. Os comandos abaixo assumem as ferramentas PostgreSQL no PATH e um banco ainda inexistente:

```powershell
createdb -h 127.0.0.1 -p 5432 -U postgres -W -T template0 -E UTF8 pimburguer_mobile
pg_restore -h 127.0.0.1 -p 5432 -U postgres -W --single-transaction --no-owner --no-privileges -d pimburguer_mobile .\BANCO\MOBILE\PIM_BURGUER_MOBILE_INICIAL.backup
```

Depois, dentro de `API`, configure a conexão do app para esse notebook:

```powershell
dotnet user-secrets set "ConnectionStrings:PimBurguerApp" "Host=127.0.0.1;Port=5432;Database=pimburguer_mobile;Username=postgres;Password=SUA_SENHA"
```

Informe a porta e a senha do notebook. A porta 65434 é a do PostgreSQL preparado neste computador; uma instalação comum pode usar 5432. Configure também a conexão web e execute a API. Em Development, o administrador local configurado é criado uma vez em cada banco, sem substituir senhas existentes. O app continua voltado ao cliente; não foi adicionada uma tela administrativa.

O arquivo `PIM_BURGUER_MOBILE_INICIAL.sql` é uma alternativa em texto ao backup. Restaure somente uma das duas versões em um banco vazio.

## Alterações no aplicativo

- Endereço padrão e prefixo para acessar o banco exclusivo do app.
- Dois textos que diziam que contas e pedidos eram compartilhados com o site.
- Limite de memória e de processos da compilação Gradle para o computador usado nos testes.
- README ajustado para a instalação separada.

As telas, a navegação, as funções existentes, o cache SQLite, o carrinho e a persistência do checkout foram mantidos. O projeto original em `D:\PIM_Burguer_Android_Studio` foi preservado.
