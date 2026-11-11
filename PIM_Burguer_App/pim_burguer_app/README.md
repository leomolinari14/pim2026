# PIM Burguer · App do cliente

Projeto Flutter para Android Studio. Use uma conta cadastrada no aplicativo para montar um pedido, conferir o total e acompanhar suas compras. Esta cópia mantém as telas e funções existentes e integra com um banco PostgreSQL exclusivo do app.

## Executar no Android Studio

1. Instale o Flutter com Dart 3.13.2 ou superior e habilite os plugins Flutter e Dart no Android Studio. Esta integração foi compilada com Flutter 3.47.9, Dart 3.13.7 e o JDK do Android Studio.
2. Inicie o PostgreSQL preparado na porta 65434 e execute a API existente em <code>D:\PIMBURGUER_PARA_APRESENTAR\PIMBURGUER_API_SIMPLES</code>, abrindo <code>PimBurguer.sln</code> no Visual Studio. A API continua nessa pasta separada. No computador, abra <code>http://localhost:5181/mobile/api/saude</code> e confirme <code>nomeBanco: pimburguer_mobile</code>. A conexão já está nos Segredos do Usuário deste Windows; em outro computador, consulte <code>INTEGRACAO_APP.md</code> na pasta da API.
3. Extraia o ZIP e abra a pasta <code>pim_burguer_app</code>, que contém <code>pubspec.yaml</code>, pelo menu **File → Open**.
4. Execute no terminal do Android Studio e selecione um emulador Android no seletor de dispositivos.

~~~powershell
flutter pub get
flutter run
~~~

O endereço padrão em debug é <code>http://10.0.2.2:5181/mobile/</code>. No emulador padrão do Android Studio, <code>10.0.2.2</code> aponta para o computador que roda a API. Você também pode configurar **Run → Edit Configurations → Additional run args** com <code>--dart-define=API_BASE_URL=...</code>.

### Usar um celular físico

Conecte o celular por USB com depuração ativada e encaminhe a porta para a API no computador. Se <code>adb</code> não estiver no PATH, use o executável em <code>platform-tools</code> da instalação do Android SDK.

~~~powershell
adb reverse tcp:5181 tcp:5181
flutter run --dart-define=API_BASE_URL=http://127.0.0.1:5181/mobile
~~~

Para testar pela rede local, faça a API escutar no IP de rede do computador e use esse endereço no <code>API_BASE_URL</code>. Use uma rede de testes e dados fictícios para HTTP. Em profile e release, informe uma URL HTTPS válida. Uma publicação também exige assinatura própria do APK ou do App Bundle.

## O que fica em cada banco

| Local | Dados |
| --- | --- |
| SQLite no Android | Cópia dos produtos públicos e carrinho deste aparelho. Cada endereço de API tem seu próprio arquivo <code>pim_burguer_*.db</code>. O cardápio já baixado e o carrinho podem ser consultados offline. |
| PostgreSQL da API | Contas, produtos oficiais, preços, disponibilidade, cupons, pedidos, status e pagamento. O app usa <code>pimburguer_mobile</code>, separado de <code>pimburguer_apresentacao</code>, que atende o site. Os sete produtos iniciais foram copiados uma vez. Contas e sessões são compartilhadas pelo banco web; os pedidos permanecem separados e os cupons são compartilhados. |
| Armazenamento privado do Android | Tentativa de checkout sem confirmação, cifrada com o Android Keystore e separada por conta e endereço da API. |
| Memória do app | JWT de cliente com o prazo definido pela API. Ao fechar o processo, entre novamente. |

O SQLite é o cache local do aparelho. O banco principal do app é outro PostgreSQL: <code>pimburguer_mobile</code>. Use a mesma conta do site no aplicativo. Contas antigas exclusivas do app são incorporadas após login válido; para e-mails duplicados, prevalece a senha do site. Após o cliente revisar o orçamento, a API recalcula os valores e registra o pedido. Se a confirmação se perder, a próxima tentativa reaproveita o mesmo UUID, o token de consulta e os dados originais. O carrinho deste aparelho permanece acessível antes do login.

## Organização

~~~text
pim_burguer_app/
├── android/                  Projeto Android e integração com SQLite/Keystore
├── assets/imagens/           Identidade e fotos do pacote PIM Burguer
├── lib/
│   ├── core/                 Rotas, tema, configuração e estado do aplicativo
│   ├── data/
│   │   ├── api/              Requisições HTTP, JSON e autenticação Bearer
│   │   ├── db/               Interface Dart para o armazenamento Android
│   │   └── repositories/     Cache de produtos e persistência do checkout
│   ├── models/               Produtos, usuários, cupons e pedidos
│   ├── ui/
│   │   ├── telas/            Cardápio, carrinho, checkout, pedidos e conta
│   │   └── widgets/          Campos, avisos e resumo de valores
│   └── main.dart
└── test/                    Testes com API e armazenamento simulados
~~~

A estrutura de <code>lib/</code> segue o projeto de estoque das aulas. O banco usa <code>SQLiteOpenHelper</code> do Android, acessado pela interface em <code>lib/data/db/app_database.dart</code>.

## API do PIM Burguer

| Operação | Endpoint |
| --- | --- |
| Cardápio | <code>GET /mobile/api/produtos</code> |
| Criar conta e entrar | <code>POST /mobile/api/conta/cadastro</code> e <code>POST /mobile/api/conta/login</code> |
| Editar perfil, alterar senha e sair | <code>PUT /mobile/api/conta/perfil</code>, <code>PUT /mobile/api/conta/senha</code> e <code>POST /mobile/api/conta/sair</code> |
| Ver cupons | <code>GET /mobile/api/cliente/cupons</code> |
| Revisar e confirmar pedido | <code>POST /mobile/api/pedidos/orcamento</code> e <code>POST /mobile/api/pedidos</code> |
| Acompanhar e repetir pedidos | <code>GET /mobile/api/cliente/pedidos</code>, <code>GET /mobile/api/cliente/pedidos/{id}</code> e <code>POST /mobile/api/cliente/pedidos/{id}/repetir</code> |

## Demonstrar o fluxo

1. Abra o cardápio, busque um produto e adicione ao carrinho. Feche e reabra o app para conferir a persistência.
2. Continue para o checkout e entre com uma conta de cliente. Se for uma conta antiga sem CPF, preencha o CPF no checkout.
3. Escolha retirada ou entrega, a forma de pagamento e um cupom, se houver. Toque em **Revisar valores** e confira o total antes de confirmar.
4. Abra **Pedidos** no app e confira seu pedido no histórico do aplicativo. Ele não aparece no banco do site. Ao pedir novamente, o app traz os itens ainda disponíveis com os preços atuais.
5. Na confirmação, confira o botão para abrir o resumo no WhatsApp. O cliente decide se envia a mensagem na conversa. O app registra a forma de pagamento para atendimento da loja.

Para verificar o código localmente, execute <code>flutter analyze</code> e <code>flutter test</code>.

## Limites desta integração

A API mantém ASP.NET Core, Entity Framework Core e JWT. A rota <code>/api/...</code> atende o site; <code>/mobile/api/...</code> atende o banco do app. O servidor escolhe o banco e valida o canal no JWT. A autenticação usa contas e sessões no banco web; um vínculo de usuário no mobile preserva seus pedidos. Os pedidos continuam separados; o catálogo de cupons e os limites de utilização são compartilhados. O cadastro e o login continuam obrigatórios para concluir pedidos.

O endereço sem caminho informado em <code>API_BASE_URL</code> recebe o prefixo <code>/mobile</code> automaticamente. O APK debug permite HTTP para testes; release exige HTTPS, conforme a configuração original.
