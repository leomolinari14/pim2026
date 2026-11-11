# Verificação da integração do aplicativo

- API compilada em Release: zero erros e zero avisos.
- 248 verificações de API aprovadas em bancos PostgreSQL temporários, incluindo comparação com a versão anterior do site.
- Contratos das rotas web, schemas e segurança OpenAPI preservados.
- Canais web/mobile com bancos independentes; JWT de um canal rejeitado no outro; alterações de perfil e históricos separados.
- Clientes HTTP e modelos Dart do app testados contra a API real: cadastro, login, perfil, orçamento, criação/reenvio de pedido sem duplicação, histórico, detalhe, repetição, cupons, alteração de senha, revogação e logout.
- Flutter analyze: sem problemas. Flutter test: teste existente de recuperação do checkout aprovado.
- APK debug compilado com Flutter 3.47.9 / Dart 3.13.7.
- MainActivity Kotlin e armazenamento nativo incluídos na compilação.
- API executada da pasta existente em D: e verificada usando as configurações locais: web em pimburguer_apresentacao, app em pimburguer_mobile; 7 produtos em cada banco.
- Fontes originais do aplicativo conferidos por SHA-256: preservados.

O teste visual no Android não pôde ser realizado: o emulador instalado exigiu mais memória e memória virtual do que havia disponível. As tentativas usaram um AVD temporário em D:, sem alterar o AVD original. O APK foi compilado, mas não foi instalado nem validado visualmente em um dispositivo.

A porta 5181 foi liberada ao concluir as verificações, para executar a API normalmente no Visual Studio. Os servidores temporários de API e de PostgreSQL foram encerrados. O PostgreSQL preparado na porta 65434 permanece disponível.
