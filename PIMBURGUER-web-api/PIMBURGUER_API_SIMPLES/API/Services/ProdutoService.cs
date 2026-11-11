using Microsoft.EntityFrameworkCore;
using PimBurguer.Api.Data;
using PimBurguer.Api.Dtos;
using PimBurguer.Api.Models;

namespace PimBurguer.Api.Services
{
    public class ProdutoService
    {
        private readonly PimBurguerDbContext _banco;
        private readonly ImagensProdutoService _imagens;

        public ProdutoService(PimBurguerDbContext banco, ImagensProdutoService imagens)
        {
            _banco = banco;
            _imagens = imagens;
        }

        public async Task<List<ProdutoResposta>> ListarAsync(string? busca, CancellationToken cancellationToken)
        {
            // O cardápio público mostra somente produtos disponíveis.
            var consulta = _banco.Produtos
                .AsNoTracking()
                .Where(produto => produto.Ativo && !produto.Removido);
            if (!string.IsNullOrWhiteSpace(busca))
            {
                var termo = busca.Trim().ToLower();
                consulta = consulta
                    .Where(produto => produto.Nome.ToLower().Contains(termo) || produto.Descricao.ToLower().Contains(termo) || produto.Categoria.ToLower().Contains(termo));
            }

            return await consulta
                .OrderBy(produto => produto.Id)
                .Select(produto => new ProdutoResposta(
                produto.Id,
                produto.Nome,
                produto.Descricao,
                produto.Preco,
                produto.Categoria,
                produto.Imagem))
                .ToListAsync(cancellationToken);
        }

        public async Task<ProdutoResposta?> BuscarAsync(int id, CancellationToken cancellationToken)
        {
            var produto = await _banco.Produtos
                .AsNoTracking()
                .SingleOrDefaultAsync(produto => produto.Id == id && produto.Ativo && !produto.Removido, cancellationToken);
            if (produto == null)
            {
                return null;
            }

            return new ProdutoResposta(
                produto.Id,
                produto.Nome,
                produto.Descricao,
                produto.Preco,
                produto.Categoria,
                produto.Imagem);
        }

        public async Task<List<ProdutoAdminResposta>> ListarAdminAsync(bool removidos, CancellationToken ct)
        {
            var produtos = await _banco.Produtos
                .AsNoTracking()
                .Where(produto => removidos || !produto.Removido)
                .OrderBy(produto => produto.Categoria)
                .ThenBy(produto => produto.Nome)
                .ToListAsync(ct);
            return produtos
                .Select(RespostaAdmin).ToList();
        }

        public async Task<ProdutoAdminResposta?> BuscarAdminAsync(int id, CancellationToken ct)
        {
            var produto = await _banco.Produtos
                .AsNoTracking()
                .SingleOrDefaultAsync(produto => produto.Id == id, ct);
            if (produto == null)
            {
                return null;
            }

            return RespostaAdmin(produto);
        }

        public async Task<ProdutoAdminResposta> SalvarAsync(int? id, ProdutoEntrada entrada, CancellationToken ct)
        {
            Produto produto;
            if (id == null)
            {
                produto = new Produto();
            }
            else
            {
                produto = await _banco.Produtos
                    .SingleOrDefaultAsync(produto => produto.Id == id, ct) ?? throw new RegraException("Produto não encontrado.", 404);
            }

            if (produto.Removido)
            {
                throw new RegraException("Restaure o produto antes de editar.");
            }

            if (decimal.Round(entrada.Preco, 2) != entrada.Preco)
            {
                throw new RegraException("O preço deve ter até duas casas decimais.");
            }

            if (string.IsNullOrWhiteSpace(entrada.Nome) || string.IsNullOrWhiteSpace(entrada.Categoria) || string.IsNullOrWhiteSpace(entrada.Descricao))
            {
                throw new RegraException("Preencha nome, categoria e descrição.");
            }

            if (entrada.ArquivoImagem != null)
            {
                produto.Imagem = await _imagens.Salvar(entrada.ArquivoImagem, ct);
            }

            if (string.IsNullOrEmpty(produto.Imagem))
            {
                throw new RegraException("Adicione uma imagem ao novo produto.");
            }

            produto.Nome = entrada.Nome.Trim();
            produto.Descricao = entrada.Descricao.Trim();
            produto.Categoria = entrada.Categoria.Trim();
            produto.Preco = entrada.Preco;
            produto.Ativo = entrada.Ativo;
            if (id == null)
            {
                _banco.Produtos.Add(produto);
            }

            await _banco.SaveChangesAsync(ct);
            return RespostaAdmin(produto);
        }

        public async Task<bool> RemoverAsync(int id, CancellationToken ct)
        {
            var produto = await _banco.Produtos
                .SingleOrDefaultAsync(produto => produto.Id == id, ct);
            if (produto == null)
            {
                return false;
            }

            produto.Removido = true;
            produto.Ativo = false;
            await _banco.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> RestaurarAsync(int id, CancellationToken ct)
        {
            var produto = await _banco.Produtos
                .SingleOrDefaultAsync(produto => produto.Id == id, ct);
            if (produto == null)
            {
                return false;
            }

            produto.Removido = false;
            produto.Ativo = false;
            await _banco.SaveChangesAsync(ct);
            return true;
        }

        private static ProdutoAdminResposta RespostaAdmin(Produto produto)
        {
            return new ProdutoAdminResposta(
                produto.Id,
                produto.Nome,
                produto.Descricao,
                produto.Preco,
                produto.Categoria,
                produto.Imagem,
                produto.Ativo,
                produto.Removido);
        }
    }
}
