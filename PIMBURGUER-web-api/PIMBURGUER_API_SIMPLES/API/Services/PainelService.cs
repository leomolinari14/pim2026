using Microsoft.EntityFrameworkCore;
using PimBurguer.Api.Data;
using PimBurguer.Api.Dtos;
using PimBurguer.Api.Models;

namespace PimBurguer.Api.Services
{
    public class PainelService
    {
        private readonly PimBurguerDbContext _banco;

        public PainelService(PimBurguerDbContext db)
        {
            _banco = db;
        }

        public static readonly string[] StatusPedido = new string[]
        {
            "Aguardando confirmação",
            "Confirmado",
            "Em preparo",
            "Saiu para entrega",
            "Pronto para retirada",
            "Concluído",
            "Cancelado"
        };
        public async Task<PaginaPedidosResposta> Pedidos(Guid? usuarioId, int pagina, string? status, CancellationToken ct)
        {
            pagina = Math.Max(1, pagina);
            var query = _banco.Pedidos
                .AsNoTracking();
            if (usuarioId is not null)
            {
                query = query
                    .Where(p => p.UsuarioId == usuarioId);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query
                    .Where(p => p.Status == status);
            }

            var total = await query.CountAsync(ct);
            const int tamanho = 12;
            pagina = Math.Min(pagina, Math.Max(1, (int)Math.Ceiling((double)total / tamanho)));
            var pedidos = await query
                .OrderByDescending(p => p.CriadoEm)
                .ThenBy(p => p.Id)
                .Skip((pagina - 1) * tamanho)
                .Take(tamanho)
                .Select(p => new PedidoResumoResposta(
                p.Id,
                p.CriadoEm,
                p.NomeCliente,
                p.Status,
                p.Recebimento,
                p.Pagamento,
                p.Pago,
                p.Total,
                p.Desconto,
                p.CodigoCupom,
                p.Itens.Sum(i => i.Quantidade)))
                .ToListAsync(ct);
            return new PaginaPedidosResposta(pedidos, pagina, total, tamanho);
        }

        public async Task AtualizarPedido(Guid id, Guid adminId, AtualizarPedidoEntrada entrada, CancellationToken ct)
        {
            await using var t = await _banco.Database.BeginTransactionAsync(ct);
            var p = await _banco.Pedidos.FromSqlInterpolated($"SELECT * FROM pedidos WHERE id={id} FOR UPDATE")
                .SingleOrDefaultAsync(ct) ?? throw new RegraException("Pedido não encontrado.", 404);
            if (!StatusPedido.Contains(entrada.Status))
            {
                throw new RegraException("Escolha um status válido.");
            }

            if ((entrada.Status == "Saiu para entrega" && p.Recebimento != "Entrega") || (entrada.Status == "Pronto para retirada" && p.Recebimento != "Retirada"))
            {
                throw new RegraException("Este andamento não corresponde à forma de recebimento do pedido.");
            }

            if (p.Status != entrada.Status || p.Pago != entrada.Pago)
            {
                _banco.MovimentacoesPedido.Add(new MovimentacaoPedido
                {
                    PedidoId = id,
                    AdministradorId = adminId,
                    StatusAnterior = p.Status,
                    StatusNovo = entrada.Status,
                    PagoAnterior = p.Pago,
                    PagoNovo = entrada.Pago
                });
                p.Status = entrada.Status;
                if (p.Pago != entrada.Pago)
                {
                    p.PagoEm = entrada.Pago ? DateTimeOffset.UtcNow : null;
                }

                p.Pago = entrada.Pago;
                await _banco.SaveChangesAsync(ct);
            }

            await t.CommitAsync(ct);
        }

        public async Task<RelatorioResposta> Relatorio(DateOnly? inicio, DateOnly? fim, CancellationToken ct)
        {
            var ate = fim ?? Identidade.Hoje;
            var desde = inicio ?? ate.AddDays(-29);
            if (ate < desde || ate == DateOnly.MaxValue || ate.DayNumber - desde.DayNumber > 365)
            {
                throw new RegraException("Escolha um período de até 366 dias, com início anterior ao fim.");
            }

            var deUtc = new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(desde.ToDateTime(TimeOnly.MinValue), Identidade.Fuso));
            var ateUtc = new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(ate.AddDays(1).ToDateTime(TimeOnly.MinValue), Identidade.Fuso));
            var query = _banco.Pedidos
                .AsNoTracking()
                .Where(p => p.CriadoEm >= deUtc && p.CriadoEm < ateUtc);
            var valores = await query
                .Select(p => new { p.CriadoEm, p.Status, p.Total, p.Pago, p.Desconto })
                .ToListAsync(ct);
            var totaisProdutos = await query
                .SelectMany(p => p.Itens)
                .GroupBy(i => i.NomeProduto)
                .Select(g => new { Nome = g.Key, Quantidade = g.Sum(i => i.Quantidade), Valor = g.Sum(i => i.Subtotal) })
                .OrderByDescending(p => p.Quantidade)
                .ThenBy(p => p.Nome)
                .Take(20)
                .ToListAsync(ct);
            var produtos = totaisProdutos
                .Select(p => new ProdutoVendidoResposta(p.Nome, p.Quantidade, p.Valor)).ToList();
            var soma = valores.Sum(p => p.Total);
            var dias = valores
                .GroupBy(p => DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(p.CriadoEm, Identidade.Fuso).DateTime))
                .OrderBy(g => g.Key)
                .Select(g => new SerieDiaResposta(g.Key, g.Count(), g.Sum(p => p.Total))).ToList();
            return new RelatorioResposta(
                desde,
                ate,
                valores.Count,
                valores.Count(p => p.Pago),
                valores.Count(p => p.Status == "Cancelado"),
                soma,
                valores
                .Where(p => p.Pago).Sum(p => p.Total),
                valores.Sum(p => p.Desconto),
                valores.Count == 0 ? 0 : decimal.Round(soma / valores.Count, 2),
                dias,
                produtos,
                valores
                .GroupBy(p => p.Status)
                .Select(g => new ContagemResposta(g.Key, g.Count())).ToList());
        }

        public async Task<object> ResumoClienteAsync(Guid? usuarioId, CancellationToken ct)
        {
            var consulta = _banco.Pedidos
                .AsNoTracking()
                .Where(pedido => pedido.UsuarioId == usuarioId);
            var total = await consulta.CountAsync(ct);
            var emAndamento = await consulta.CountAsync(pedido => pedido.Status != "Concluído" && pedido.Status != "Cancelado", ct);
            var concluidos = await consulta.CountAsync(pedido => pedido.Status == "Concluído", ct);
            var valor = await consulta.SumAsync(pedido => (decimal?)pedido.Total, ct) ?? 0;
            return new
            {
                TotalPedidos = total,
                EmAndamento = emAndamento,
                Concluidos = concluidos,
                ValorPedidos = valor
            };
        }

        public async Task<PedidoResposta?> BuscarPedidoClienteAsync(Guid id, Guid? usuarioId, CancellationToken ct)
        {
            var pedido = await _banco.Pedidos
                .AsNoTracking()
                .Include(pedido => pedido.Itens)
                .SingleOrDefaultAsync(pedido => pedido.Id == id && pedido.UsuarioId == usuarioId, ct);
            if (pedido == null)
            {
                return null;
            }

            return PedidoService.Resposta(pedido);
        }

        public async Task<RepetirPedidoResposta?> RepetirPedidoAsync(Guid id, Guid? usuarioId, CancellationToken ct)
        {
            var pedido = await _banco.Pedidos
                .AsNoTracking()
                .Include(pedido => pedido.Itens)
                .ThenInclude(item => item.Produto)
                .SingleOrDefaultAsync(pedido => pedido.Id == id && pedido.UsuarioId == usuarioId, ct);
            if (pedido == null)
            {
                return null;
            }

            var disponiveis = pedido.Itens
                .Where(item => item.Produto.Ativo && !item.Produto.Removido)
                .Select(item => new ItemSolicitado
                {
                    ProdutoId = item.ProdutoId,
                    Quantidade = item.Quantidade
                }).ToList();
            var indisponiveis = pedido.Itens
                .Where(item => !item.Produto.Ativo || item.Produto.Removido)
                .Select(item => item.NomeProduto).ToList();
            return new RepetirPedidoResposta(disponiveis, indisponiveis);
        }

        public async Task<DetalheAdminResposta?> DetalharPedidoAsync(Guid id, CancellationToken ct)
        {
            var pedido = await _banco.Pedidos
                .AsNoTracking()
                .Include(pedido => pedido.Itens)
                .SingleOrDefaultAsync(pedido => pedido.Id == id, ct);
            if (pedido == null)
            {
                return null;
            }

            var movimentos = await _banco.MovimentacoesPedido
                .AsNoTracking()
                .Where(movimento => movimento.PedidoId == id)
                .OrderByDescending(movimento => movimento.CriadoEm)
                .Select(movimento => new MovimentoResposta(
                movimento.CriadoEm,
                movimento.Administrador.Nome,
                movimento.StatusAnterior,
                movimento.StatusNovo,
                movimento.PagoAnterior,
                movimento.PagoNovo))
                .ToListAsync(ct);
            return new DetalheAdminResposta(PedidoService.Resposta(pedido), movimentos);
        }
    }
}
