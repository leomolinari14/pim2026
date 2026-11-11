using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;
using PimBurguer.Api.Data;
using PimBurguer.Api.Dtos;
using PimBurguer.Api.Models;
using PimBurguer.Validacao;

namespace PimBurguer.Api.Services
{
    public class PedidoService
    {
        private readonly PimBurguerDbContext _banco;
        private readonly IOptionsMonitor<LojaOptions> _configuracao;
        private readonly CupomService _cupons;

        public PedidoService(PimBurguerDbContext banco, IOptionsMonitor<LojaOptions> configuracao, CupomService cupons)
        {
            _banco = banco;
            _configuracao = configuracao;
            _cupons = cupons;
        }

        private static string Hash(object valor)
        {
            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(valor))));
        }

        private static string? Limpar(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                return null;
            }
            else
            {
                return valor.Trim();
            }
        }

        public async Task<OrcamentoResposta> OrcarAsync(OrcamentoRequisicao entrada, CancellationToken ct, Guid? usuarioId = null)
        {
            if (entrada.Itens.Count is < 1 or > 30 || entrada.Itens.Sum(i => i.Quantidade) > 100 || entrada.Itens.Any(i => i.Quantidade is < 1 or > 20) || entrada.Itens
                .Select(i => i.ProdutoId).Distinct().Count() != entrada.Itens.Count)
            {
                throw new PedidoInvalidoException("Revise as quantidades do carrinho. O limite é 20 por produto e 100 unidades por pedido.");
            }

            var taxa = entrada.Recebimento == "Retirada" ? 0m : _configuracao.CurrentValue.TaxaEntrega;
            if (entrada.Recebimento is not ("Retirada" or "Entrega") || taxa is null or < 0 || decimal.Round(taxa.Value, 2) != taxa.Value)
            {
                throw new PedidoInvalidoException("A entrega não está disponível neste momento. Escolha retirada ou fale com a loja.");
            }

            var ids = entrada.Itens
                .Select(i => i.ProdutoId).ToArray();
            var produtos = await _banco.Produtos
                .AsNoTracking()
                .Where(p => ids.Contains(p.Id) && p.Ativo && !p.Removido)
                .ToDictionaryAsync(p => p.Id, ct);
            if (produtos.Count != ids.Length)
            {
                throw new PedidoInvalidoException("Um produto ficou indisponível. Volte ao carrinho e remova o item indicado.", 409);
            }

            // Preços vêm do banco. O cliente envia somente IDs e quantidades.
            var itens = entrada.Itens
                .OrderBy(i => i.ProdutoId)
                .Select(i =>
            {
                var p = produtos[i.ProdutoId];
                return new ItemOrcamento(
                    p.Id,
                    p.Nome,
                    p.Imagem,
                    i.Quantidade,
                    p.Preco,
                    p.Preco * i.Quantidade);
            }).ToList();
            var subtotal = itens.Sum(i => i.Subtotal);
            var (cupom, desconto) = await _cupons.Calcular(entrada.CodigoCupom, subtotal, usuarioId, ct);
            var total = subtotal + taxa.Value - desconto;
            var hash = Hash(new
            {
                entrada.Recebimento,
                Itens = itens,
                Subtotal = subtotal,
                Taxa = taxa.Value,
                Total = total,
                Cupom = cupom?.Codigo,
                Desconto = desconto
            });
            return new OrcamentoResposta(
                itens,
                entrada.Recebimento,
                subtotal,
                taxa.Value,
                total,
                hash,
                desconto,
                cupom?.Codigo,
                cupom?.Id);
        }

        public async Task<PedidoResposta> CriarAsync(PedidoRequisicao entrada, CancellationToken ct, Guid? usuarioId = null)
        {
            if (entrada.Id == Guid.Empty)
            {
                throw new PedidoInvalidoException("Identificação do pedido inválida.");
            }

            if (!Cpf.Valido(entrada.CpfCliente))
            {
                throw new PedidoInvalidoException("Informe um CPF válido.");
            }

            entrada.CpfCliente = Cpf.Normalizar(entrada.CpfCliente);
            entrada.NomeCliente = entrada.NomeCliente.Trim();
            entrada.Telefone = Regex.Replace(entrada.Telefone, "[^0-9]", "");
            if (entrada.NomeCliente.Length < 2 || !Regex.IsMatch(entrada.Telefone, @"^\d{10,11}$"))
            {
                throw new PedidoInvalidoException("Informe seu nome e um telefone brasileiro com DDD.");
            }

            entrada.Cep = Limpar(entrada.Cep)?.Replace("-", "");
            entrada.Logradouro = Limpar(entrada.Logradouro);
            entrada.Numero = Limpar(entrada.Numero);
            entrada.Complemento = Limpar(entrada.Complemento);
            entrada.Bairro = Limpar(entrada.Bairro);
            entrada.Cidade = Limpar(entrada.Cidade);
            entrada.Uf = Limpar(entrada.Uf)?.ToUpperInvariant();
            entrada.Observacoes = Limpar(entrada.Observacoes);
            if (entrada.Recebimento == "Entrega")
            {
                if (!Regex.IsMatch(entrada.Cep ?? "", @"^\d{8}$") || entrada.Logradouro is null || entrada.Numero is null || entrada.Bairro is null || entrada.Cidade is null || !Regex.IsMatch(entrada.Uf ?? "", "^(AC|AL|AP|AM|BA|CE|DF|ES|GO|MA|MT|MS|MG|PA|PB|PR|PE|PI|RJ|RN|RS|RO|RR|SC|SP|SE|TO)$"))
                {
                    throw new PedidoInvalidoException("Complete o endereço da entrega, incluindo CEP, número, cidade e estado.");
                }
            }
            else
            {
                entrada.Cep = entrada.Logradouro = entrada.Numero = entrada.Complemento = null;
                entrada.Bairro = entrada.Cidade = entrada.Uf = null;
            }

            if (entrada.Pagamento != "Dinheiro")
            {
                entrada.TrocoPara = null;
            }

            entrada.CodigoCupom = Limpar(entrada.CodigoCupom)?.ToUpperInvariant();
            var tokenHash = Hash(entrada.TokenConsulta);
            var requisicaoHash = Hash(new
            {
                entrada.NomeCliente,
                entrada.Telefone,
                entrada.CpfCliente,
                entrada.Recebimento,
                entrada.Cep,
                entrada.Logradouro,
                entrada.Numero,
                entrada.Complemento,
                entrada.Bairro,
                entrada.Cidade,
                entrada.Uf,
                entrada.Pagamento,
                entrada.TrocoPara,
                entrada.Observacoes,
                entrada.CodigoCupom,
                UsuarioId = usuarioId,
                Itens = entrada.Itens
                .OrderBy(i => i.ProdutoId).ToArray()
            });
            var existente = await _banco.Pedidos
                .Include(p => p.Itens)
                .SingleOrDefaultAsync(p => p.Id == entrada.Id, ct);
            if (existente is not null)
            {
                return Repeticao(existente, tokenHash, requisicaoHash);
            }

            var loja = _configuracao.CurrentValue;
            if (!Regex.IsMatch(loja.WhatsApp, @"^55\d{10,11}$"))
            {
                throw new PedidoInvalidoException("O atendimento da loja está indisponível. Tente novamente em instantes.", 503);
            }

            await using var transacao = await _banco.Database.BeginTransactionAsync(ct);
            await using var bloqueioCupom = await _cupons.BloquearAsync(entrada.CodigoCupom, ct);
            if (entrada.CodigoCupom is not null)
            {
                // Outro envio idêntico pode ter terminado enquanto aguardávamos o cupom.
                var concluido = await _banco.Pedidos
                    .AsNoTracking()
                    .Include(p => p.Itens)
                    .SingleOrDefaultAsync(p => p.Id == entrada.Id, ct);
                if (concluido is not null)
                {
                    return Repeticao(concluido, tokenHash, requisicaoHash);
                }
            }

            var orcamento = await OrcarAsync(entrada, ct, usuarioId);
            if (orcamento.Hash != entrada.OrcamentoHash)
            {
                throw new PedidoInvalidoException("Os valores do pedido mudaram. Confira o resumo atualizado e confirme novamente.", 409);
            }

            if (entrada.TrocoPara is decimal troco && (troco < orcamento.Total || decimal.Round(troco, 2) != troco))
            {
                throw new PedidoInvalidoException("O valor para troco deve ser igual ou maior que o total e ter até duas casas decimais.");
            }

            var pedido = new Pedido
            {
                Id = entrada.Id,
                CriadoEm = DateTimeOffset.UtcNow,
                NomeCliente = entrada.NomeCliente,
                Telefone = entrada.Telefone,
                CpfCliente = entrada.CpfCliente,
                Recebimento = entrada.Recebimento,
                Cep = entrada.Cep,
                Logradouro = entrada.Logradouro,
                Numero = entrada.Numero,
                Complemento = entrada.Complemento,
                Bairro = entrada.Bairro,
                Cidade = entrada.Cidade,
                Uf = entrada.Uf,
                Pagamento = entrada.Pagamento,
                TrocoPara = entrada.TrocoPara,
                Observacoes = entrada.Observacoes,
                Subtotal = orcamento.Subtotal,
                TaxaEntrega = orcamento.TaxaEntrega,
                Total = orcamento.Total,
                WhatsAppLoja = loja.WhatsApp,
                TokenHash = tokenHash,
                RequisicaoHash = requisicaoHash,
                UsuarioId = usuarioId,
                CupomId = orcamento.CupomId,
                CodigoCupom = orcamento.CodigoCupom,
                Desconto = orcamento.Desconto,
                Itens = orcamento.Itens
                    .Select(i => new ItemPedido
                    {
                        ProdutoId = i.ProdutoId,
                        NomeProduto = i.Nome,
                        Quantidade = i.Quantidade,
                        PrecoUnitario = i.PrecoUnitario,
                        Subtotal = i.Subtotal
                    }).ToList()

            };
            _banco.Pedidos.Add(pedido);
            if (pedido.CupomId is int cupomId)
            {
                _banco.UsosCupom.Add(new UsoCupom
                {
                    CupomId = cupomId,
                    UsuarioId = usuarioId!.Value,
                    Pedido = pedido
                });
            }

            try
            {
                // Pedido e itens são gravados na mesma transação pelo EF Core.
                await _banco.SaveChangesAsync(ct);
                await transacao.CommitAsync(ct);
            }
            catch (DbUpdateException erro) when (erro.InnerException is PostgresException { SqlState: "23505" })
            {
                // Dois cliques simultâneos no mesmo envio geram somente um pedido.
                await transacao.RollbackAsync(ct);
                _banco.ChangeTracker.Clear();
                var repetido = await _banco.Pedidos
                    .Include(p => p.Itens)
                    .SingleOrDefaultAsync(p => p.Id == entrada.Id, ct);
                if (repetido is null)
                {
                    throw;
                }

                return Repeticao(repetido, tokenHash, requisicaoHash);
            }

            return Resposta(pedido);
        }

        public async Task<PedidoResposta?> ConsultarAsync(Guid id, string? token, CancellationToken ct)
        {
            if (token is null || token.Length is < 32 or > 128)
            {
                return null;
            }

            var pedido = await _banco.Pedidos
                .AsNoTracking()
                .Include(p => p.Itens)
                .SingleOrDefaultAsync(p => p.Id == id, ct);
            if (pedido is not null && TokenIgual(pedido.TokenHash, Hash(token)))
            {
                return Resposta(pedido);
            }
            else
            {
                return null;
            }
        }

        private static bool TokenIgual(string a, string b)
        {
            return CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(a), Encoding.UTF8.GetBytes(b));
        }

        private static PedidoResposta Repeticao(Pedido pedido, string tokenHash, string requisicaoHash)
        {
            if (!TokenIgual(pedido.TokenHash, tokenHash) || pedido.RequisicaoHash != requisicaoHash)
            {
                throw new PedidoInvalidoException("Este envio já foi utilizado. Confira a confirmação do pedido ou reinicie o checkout.", 409);
            }

            return Resposta(pedido);
        }

        public static PedidoResposta Resposta(Pedido pedido)
        {
            var br = CultureInfo.GetCultureInfo("pt-BR");
            var itens = pedido.Itens
                .OrderBy(i => i.ProdutoId)
                .Select(i => new ItemOrcamento(
                i.ProdutoId,
                i.NomeProduto,
                "",
                i.Quantidade,
                i.PrecoUnitario,
                i.Subtotal)).ToList();
            var endereco = pedido.Recebimento == "Entrega" ? $"{pedido.Logradouro}, {pedido.Numero}{(pedido.Complemento is null ? "" : " — " + pedido.Complemento)}, {pedido.Bairro}, {pedido.Cidade}/{pedido.Uf}, CEP {pedido.Cep}" : null;
            var pagamento = pedido.Pagamento switch
            {
                "CartaoDebito" => "Cartão de débito",
                "CartaoCredito" => "Cartão de crédito",
                _ => pedido.Pagamento
            };
            var texto = new StringBuilder($"Olá! Meu pedido no PimBurguer: {pedido.Id}\n\n");
            foreach (var item in itens)
            {
                texto.AppendLine($"{item.Quantidade}x {item.Nome} — {item.PrecoUnitario.ToString("C", br)} cada — {item.Subtotal.ToString("C", br)}");
            }

            texto.AppendLine($"\nSubtotal: {pedido.Subtotal.ToString("C", br)}");
            if (pedido.Desconto > 0)
            {
                texto.AppendLine($"Cupom {pedido.CodigoCupom}: -{pedido.Desconto.ToString("C", br)}");
            }

            texto.AppendLine($"Taxa de entrega: {pedido.TaxaEntrega.ToString("C", br)}");
            texto.AppendLine($"Total: {pedido.Total.ToString("C", br)}");
            texto.AppendLine($"\nCliente: {pedido.NomeCliente}\nTelefone: {pedido.Telefone}\nRecebimento: {pedido.Recebimento}");
            if (endereco is not null)
            {
                texto.AppendLine($"Endereço: {endereco}");
            }

            texto.AppendLine($"Pagamento: {pagamento} (a combinar com a loja)");
            if (pedido.TrocoPara is decimal troco)
            {
                texto.AppendLine($"Troco para: {troco.ToString("C", br)}");
            }

            if (pedido.Observacoes is not null)
            {
                texto.AppendLine($"Observações: {pedido.Observacoes}");
            }

            texto.AppendLine("\nAguardo a confirmação da loja e a previsão de preparo.");
            return new PedidoResposta(
                pedido.Id,
                pedido.CriadoEm,
                pedido.Status,
                pedido.NomeCliente,
                pedido.Telefone,
                pedido.Recebimento,
                endereco,
                pagamento,
                pedido.TrocoPara,
                pedido.Observacoes,
                pedido.Subtotal,
                pedido.TaxaEntrega,
                pedido.Total,
                itens,
                $"https://wa.me/{pedido.WhatsAppLoja}?text={Uri.EscapeDataString(texto.ToString())}",
                pedido.Desconto,
                pedido.CodigoCupom,
                pedido.Pago,
                pedido.PagoEm,
                pedido.UsuarioId,
                pedido.CpfCliente);
        }
    }
}
