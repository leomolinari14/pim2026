# Banco PostgreSQL

Para restaurar os dados atualizados no notebook, siga `../BANCO_NO_NOTEBOOK.md` e use `Backup/PIM_BURGUER_PARA_NOTEBOOK.backup`. O SQL equivalente e `MANIFESTO_NOTEBOOK.json` acompanham esse backup atualizado.

Esta pasta também preserva os arquivos lógicos originais do banco:

- `Backup/PIM_BURGUER_V2_COMPLETO.backup`: backup completo em formato custom do PostgreSQL.
- `Backup/PIM_BURGUER_V2_COMPLETO.sql`: o mesmo snapshot em SQL, com estrutura e registros.
- `Backup/MANIFESTO_BACKUP.json`: data, contagens e hashes do backup original.
- `SQL/ESTRUTURA_PIMBURGUER.sql`: estrutura baseada nas migrations, com os dados iniciais definidos nelas.
- `SQL/Consultas/`: consultas de leitura, dicionário de dados e diagrama de relacionamentos.

## Restaurar

Use um servidor PostgreSQL e ferramentas compatíveis com o backup gerado pela versão 18.6. Os comandos abaixo usam as ferramentas do seu PostgreSQL instalado; execute em um terminal em que `createdb`, `pg_restore` e `psql` estejam no PATH. Se necessário, use o caminho completo dos executáveis da sua instalação.

Dentro desta pasta, crie um banco novo:

```powershell
createdb -h 127.0.0.1 -p 5432 -U postgres -W pimburguer_v2
pg_restore -h 127.0.0.1 -p 5432 -U postgres -W --no-owner --no-privileges --exit-on-error -d pimburguer_v2 .\Backup\PIM_BURGUER_V2_COMPLETO.backup
```

Se esse nome já estiver em uso, escolha outro banco vazio e ajuste a conexão da API. A senha é solicitada no terminal.

Como alternativa ao `pg_restore`, importe o SQL completo em um banco vazio:

```powershell
psql -h 127.0.0.1 -p 5432 -U postgres -W -v ON_ERROR_STOP=1 -d pimburguer_v2 -f .\Backup\PIM_BURGUER_V2_COMPLETO.sql
```

Escolha uma das duas formas de restauração. O script `SQL/ESTRUTURA_PIMBURGUER.sql` é uma alternativa para montar a estrutura e os dados iniciais das migrations; não o aplique junto com o backup completo.

Configure a API conforme `../API/LEIA-ME.md`. As imagens do backup já acompanham `../WEB/Uploads`.

Os arquivos de backup foram preservados sem alteração e correspondem ao snapshot documentado em 08/11/2026. Os dados físicos `.dados`, configurações privadas, logs, ferramentas do launcher e o PostgreSQL portátil permanecem somente no pacote original.
