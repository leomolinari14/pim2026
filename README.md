# PIM Burguer — versão final

Sistema completo importado de `PIM FINAL.zip` em 10/11/2026, com API ASP.NET Core, site HTML/CSS/JavaScript, aplicativo Flutter para Android e bancos PostgreSQL.

| Pasta | Conteúdo |
| --- | --- |
| `PIMBURGUER-web-api/PIMBURGUER_API_SIMPLES/API` | API .NET 9, autenticação JWT, regras de negócio e migrations |
| `PIMBURGUER-web-api/PIMBURGUER_API_SIMPLES/WEB` | Site, painel administrativo, imagens e bibliotecas |
| `PIMBURGUER-web-api/PIMBURGUER_API_SIMPLES/BANCO` | Scripts, backups e documentação dos bancos |
| `PIM_Burguer_App/pim_burguer_app` | Aplicativo Flutter, projeto Android e testes |
| `PIM_Burguer_Bancos/PARA_RECRIAR_NO_NOTEBOOK` | Backups mais recentes e instruções de restauração |

## Executar

1. Restaure os bancos conforme o [guia para recriar no notebook](PIM_Burguer_Bancos/PARA_RECRIAR_NO_NOTEBOOK/RECRIAR_BANCOS_NO_NOTEBOOK.md).
2. Configure as conexões e abra a solução seguindo o [guia da API e do site](PIMBURGUER-web-api/PIMBURGUER_API_SIMPLES/LEIA-ME.md) e o [guia de integração](PIMBURGUER-web-api/PIMBURGUER_API_SIMPLES/INTEGRACAO_APP.md).
3. Inicie a API na pasta `API`. O site e o Swagger ficam em `http://localhost:5181` e `http://localhost:5181/swagger` durante o desenvolvimento.
4. Abra a pasta do aplicativo no Android Studio e siga o [README do app](PIM_Burguer_App/pim_burguer_app/README.md).

Os caminhos de outros computadores citados nos documentos originais devem ser adaptados para a pasta onde este repositório foi clonado. Conexões salvas nos Segredos do Usuário do Windows original não são transportadas pelo Git.

## Conteúdo e histórico

Esta importação preserva os fontes, recursos, documentação e backups do ZIP. Caches de IDE, resultados de compilação e configurações locais de SDK ficam fora dos commits. Os arquivos do Gradle Wrapper são versionados para permitir a compilação do Android.

O repositório é privado porque os backups preservam registros da instalação original. Configure suas próprias credenciais antes de usar o sistema em outra instalação.

Os commits desta importação registram a organização da versão final por componente, com suas datas reais; não representam um histórico de desenvolvimento reconstruído.
