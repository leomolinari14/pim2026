using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using PimBurguer.Api.Data;
using PimBurguer.Api.Dtos;
using PimBurguer.Api.Models;

namespace PimBurguer.Api.Services
{
    public class CupomService
    {
        private readonly PimBurguerDbContext _local;
        private readonly PimBurguerDbContext _catalogo;
        private readonly ContasCompartilhadas _contas;
        private readonly CanalAcesso _canal;
        private readonly IConfiguration _config;
        private record Uso(int Total, bool PeloCliente);

        public CupomService(PimBurguerDbContext local, ContasDbContext central,
            ContasCompartilhadas contas, CanalAcesso canal, IConfiguration config)
        {
            _local = local;
            _catalogo = canal.Nome == "web" ? local : central;
            _contas = contas;
            _canal = canal;
            _config = config;
        }

        private PimBurguerDbContext? AbrirMobile()
        {
            var conexao = _config.GetConnectionString("PimBurguerApp");
            return string.IsNullOrWhiteSpace(conexao) ? null : new PimBurguerDbContext(
                new DbContextOptionsBuilder<PimBurguerDbContext>().UseNpgsql(conexao).Options);
        }

        public async Task PrepararCompartilhamentoAsync(CancellationToken ct = default)
        {
            await using var mobile = AbrirMobile();
            if (mobile == null) return;
            var antigos = await mobile.Cupons.Where(c => c.CupomCompartilhadoId == null).ToListAsync(ct);
            if (antigos.Count == 0) return;
            await using var transacao = await _catalogo.Database.BeginTransactionAsync(ct);
            await _catalogo.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(7842052)", ct);
            foreach (var antigo in antigos)
            {
                var oficial = await _catalogo.Cupons.SingleOrDefaultAsync(c => c.Codigo == antigo.Codigo, ct);
                if (oficial == null)
                {
                    oficial = new Cupom();
                    Copiar(antigo, oficial);
                    _catalogo.Cupons.Add(oficial);
                    await _catalogo.SaveChangesAsync(ct);
                }
                antigo.CupomCompartilhadoId = oficial.Id;
            }
            await transacao.CommitAsync(ct);
            await mobile.SaveChangesAsync(ct);
        }

        private async Task<Dictionary<int, Uso>> UsosAsync(List<Cupom> cupons, Guid? usuarioLocal, CancellationToken ct)
        {
            var ids = cupons.Select(c => c.Id).ToArray();
            var conta = usuarioLocal is Guid id ? await _contas.ContaPorUsuarioLocalAsync(id, ct) : null;
            var contaId = conta?.Id;
            var email = conta?.EmailNormalizado;
            var web = await _catalogo.UsosCupom.AsNoTracking().Where(u => ids.Contains(u.CupomId))
                .GroupBy(u => u.CupomId)
                .Select(g => new { Id = g.Key, Total = g.Count(), Usado = g.Any(u => u.UsuarioId == contaId) })
                .ToListAsync(ct);
            var resultado = ids.ToDictionary(id => id, _ => new Uso(0, false));
            foreach (var uso in web) resultado[uso.Id] = new Uso(uso.Total, uso.Usado);
            await using var externo = _canal.Nome == "mobile" ? null : AbrirMobile();
            var mobile = _canal.Nome == "mobile" ? _local : externo;
            if (mobile != null)
            {
                var usosMobile = await mobile.UsosCupom.AsNoTracking()
                    .Where(u => u.Cupom.CupomCompartilhadoId != null && ids.Contains(u.Cupom.CupomCompartilhadoId.Value))
                    .GroupBy(u => u.Cupom.CupomCompartilhadoId!.Value)
                    .Select(g => new { Id = g.Key, Total = g.Count(), Usado = g.Any(u => email != null && u.Usuario.EmailNormalizado == email) })
                    .ToListAsync(ct);
                foreach (var uso in usosMobile)
                {
                    var anterior = resultado[uso.Id];
                    resultado[uso.Id] = new Uso(anterior.Total + uso.Total, anterior.PeloCliente || uso.Usado);
                }
            }
            return resultado;
        }

        private static string? Indisponibilidade(Cupom? cupom, Uso uso)
        {
            var hoje = Identidade.Hoje;
            if (cupom == null || !cupom.Ativo || hoje < cupom.ValidoDe || hoje > cupom.ValidoAte)
                return "Cupom inválido, inativo ou fora da validade.";
            if (cupom.LimiteUsos is int limite && uso.Total >= limite)
                return "O limite de utilizações deste cupom foi atingido.";
            if (cupom.UmaVezPorCliente && uso.PeloCliente) return "Você já utilizou este cupom.";
            return null;
        }

        private static CupomResposta Resposta(Cupom c, Uso uso) => new(
            c.Id, c.Codigo, c.Tipo, c.Valor, c.CompraMinima, c.ValidoDe, c.ValidoAte,
            c.LimiteUsos, c.UmaVezPorCliente, c.Divulgar, c.Ativo, uso.Total, uso.PeloCliente);

        public async Task<List<CupomResposta>> Listar(bool admin, Guid? usuarioId, CancellationToken ct)
        {
            var hoje = Identidade.Hoje;
            var conta = usuarioId is Guid id ? await _contas.ContaPorUsuarioLocalAsync(id, ct) : null;
            var contaId = conta?.Id;
            var query = _catalogo.Cupons.AsNoTracking();
            if (!admin)
                query = query.Where(c => c.Ativo && c.ValidoDe <= hoje && c.ValidoAte >= hoje
                    && (c.Divulgar || _catalogo.CuponsClientes.Any(s => s.CupomId == c.Id && s.UsuarioId == contaId)));
            var cupons = await query.OrderByDescending(c => c.Id).ToListAsync(ct);
            var usos = await UsosAsync(cupons, usuarioId, ct);
            return cupons.Select(c => Resposta(c, usos[c.Id])).ToList();
        }

        public async Task<CupomResposta> AdicionarAsync(string codigo, Guid usuarioId, CancellationToken ct)
        {
            codigo = codigo.Trim().ToUpperInvariant();
            var conta = await _contas.ContaPorUsuarioLocalAsync(usuarioId, ct)
                ?? throw new RegraException("Entre novamente para adicionar um cupom.", 401);
            var cupom = await _catalogo.Cupons.AsNoTracking().SingleOrDefaultAsync(c => c.Codigo == codigo, ct);
            var uso = cupom == null ? new Uso(0, false) : (await UsosAsync([cupom], usuarioId, ct))[cupom.Id];
            var erro = Indisponibilidade(cupom, uso);
            if (erro != null) throw new RegraException(erro);
            await _catalogo.Database.ExecuteSqlInterpolatedAsync(
                $"INSERT INTO cupons_cliente (usuario_id, cupom_id) VALUES ({conta.Id}, {cupom!.Id}) ON CONFLICT DO NOTHING", ct);
            return Resposta(cupom!, uso);
        }

        // Protege o limite compartilhado até a transação do pedido local terminar.
        public async Task<IDbContextTransaction?> BloquearAsync(string? codigo, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(codigo)) return null;
            var transacao = _canal.Nome == "mobile" ? await _catalogo.Database.BeginTransactionAsync(ct) : null;
            try
            {
                await _catalogo.Database.ExecuteSqlInterpolatedAsync($"SELECT id FROM cupons WHERE codigo={codigo} FOR UPDATE", ct);
                return transacao;
            }
            catch
            {
                if (transacao != null) await transacao.DisposeAsync();
                throw;
            }
        }

        public async Task<(Cupom? Cupom, decimal Desconto)> Calcular(string? codigo, decimal subtotal, Guid? usuarioId, CancellationToken ct)
        {
            codigo = codigo?.Trim().ToUpperInvariant();
            if (string.IsNullOrEmpty(codigo)) return (null, 0);
            if (usuarioId == null) throw new PedidoInvalidoException("Entre na sua conta para usar um cupom.");
            var cupom = await _catalogo.Cupons.AsNoTracking().SingleOrDefaultAsync(c => c.Codigo == codigo, ct);
            var uso = cupom == null ? new Uso(0, false) : (await UsosAsync([cupom], usuarioId, ct))[cupom.Id];
            var erro = Indisponibilidade(cupom, uso);
            if (erro != null) throw new PedidoInvalidoException(erro);
            if (subtotal < cupom!.CompraMinima)
                throw new PedidoInvalidoException($"Este cupom exige pelo menos {cupom.CompraMinima.ToString("C", System.Globalization.CultureInfo.GetCultureInfo("pt-BR"))} em produtos.");
            var desconto = cupom.Tipo == "Percentual" ? decimal.Round(subtotal * cupom.Valor / 100, 2, MidpointRounding.AwayFromZero) : cupom.Valor;
            var local = cupom;
            if (_canal.Nome == "mobile")
            {
                await _local.Database.ExecuteSqlInterpolatedAsync($"""
                    INSERT INTO cupons (codigo,tipo,valor,compra_minima,valido_de,valido_ate,limite_usos,uma_vez_por_cliente,divulgar,ativo,cupom_compartilhado_id)
                    VALUES ({cupom.Codigo},{cupom.Tipo},{cupom.Valor},{cupom.CompraMinima},{cupom.ValidoDe},{cupom.ValidoAte},{cupom.LimiteUsos},{cupom.UmaVezPorCliente},{cupom.Divulgar},{cupom.Ativo},{cupom.Id})
                    ON CONFLICT (cupom_compartilhado_id) DO UPDATE SET
                    codigo=excluded.codigo,tipo=excluded.tipo,valor=excluded.valor,compra_minima=excluded.compra_minima,
                    valido_de=excluded.valido_de,valido_ate=excluded.valido_ate,limite_usos=excluded.limite_usos,
                    uma_vez_por_cliente=excluded.uma_vez_por_cliente,divulgar=excluded.divulgar,ativo=excluded.ativo
                    """, ct);
                local = await _local.Cupons.AsNoTracking().SingleAsync(c => c.CupomCompartilhadoId == cupom.Id, ct);
            }
            return (local, Math.Min(subtotal, desconto));
        }

        private static void Copiar(Cupom origem, Cupom destino)
        {
            destino.Codigo = origem.Codigo; destino.Tipo = origem.Tipo; destino.Valor = origem.Valor;
            destino.CompraMinima = origem.CompraMinima; destino.ValidoDe = origem.ValidoDe; destino.ValidoAte = origem.ValidoAte;
            destino.LimiteUsos = origem.LimiteUsos; destino.UmaVezPorCliente = origem.UmaVezPorCliente;
            destino.Divulgar = origem.Divulgar; destino.Ativo = origem.Ativo;
        }

        public async Task Salvar(int? id, CupomEntrada e, CancellationToken ct)
        {
            if (e.Tipo == "Percentual" && e.Valor > 100) throw new RegraException("O desconto percentual não pode passar de 100%.");
            if (e.ValidoDe == default || e.ValidoAte < e.ValidoDe) throw new RegraException("Confira as datas de validade do cupom.");
            if (decimal.Round(e.Valor, 2) != e.Valor || decimal.Round(e.CompraMinima, 2) != e.CompraMinima)
                throw new RegraException("Informe valores com até duas casas decimais.");
            var codigo = e.Codigo.Trim().ToUpperInvariant();
            if (await _catalogo.Cupons.AnyAsync(c => c.Codigo == codigo && c.Id != id, ct))
                throw new RegraException("Já existe um cupom com esse código.", 409);
            var c = id == null ? new Cupom() : await _catalogo.Cupons.SingleOrDefaultAsync(c => c.Id == id, ct)
                ?? throw new RegraException("Cupom não encontrado.", 404);
            if (id == null) _catalogo.Cupons.Add(c);
            Copiar(new Cupom { Codigo = codigo, Tipo = e.Tipo, Valor = e.Valor, CompraMinima = e.CompraMinima,
                ValidoDe = e.ValidoDe, ValidoAte = e.ValidoAte, LimiteUsos = e.LimiteUsos,
                UmaVezPorCliente = e.UmaVezPorCliente, Divulgar = e.Divulgar, Ativo = e.Ativo }, c);
            try { await _catalogo.SaveChangesAsync(ct); }
            catch (DbUpdateException ex) when (ex.InnerException is Npgsql.PostgresException { SqlState: "23505" })
            { throw new RegraException("Já existe um cupom com esse código.", 409); }
        }

        public async Task<bool> DesativarAsync(int id, CancellationToken ct)
        {
            var cupom = await _catalogo.Cupons.SingleOrDefaultAsync(c => c.Id == id, ct);
            if (cupom == null) return false;
            cupom.Ativo = false;
            await _catalogo.SaveChangesAsync(ct);
            return true;
        }
    }
}
