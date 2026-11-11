using Microsoft.EntityFrameworkCore;
using PimBurguer.Api.Models;

namespace PimBurguer.Api.Data
{
    public static class MapeamentoPedido
    {
        public static void Configurar(ModelBuilder modelo)
        {
            var pedido = modelo.Entity<Pedido>();
            pedido.ToTable("pedidos", t =>
            {
                t.HasCheckConstraint("ck_pedidos_valores", "subtotal > 0 AND taxa_entrega >= 0 AND desconto >= 0 AND desconto <= subtotal AND total = subtotal + taxa_entrega - desconto");
                t.HasCheckConstraint("ck_pedidos_recebimento", "recebimento IN ('Retirada', 'Entrega')");
            });
            pedido.HasKey(p => p.Id);
            pedido.Property(p => p.Id).HasColumnName("id").ValueGeneratedNever();
            pedido.Property(p => p.CriadoEm).HasColumnName("criado_em");
            pedido.Property(p => p.Status).HasColumnName("status").HasMaxLength(40);
            pedido.Property(p => p.NomeCliente).HasColumnName("nome_cliente").HasMaxLength(100);
            pedido.Property(p => p.Telefone).HasColumnName("telefone").HasMaxLength(20);
            pedido.Property(p => p.CpfCliente).HasColumnName("cpf_cliente").HasMaxLength(11);
            pedido.Property(p => p.Recebimento).HasColumnName("recebimento").HasMaxLength(10);
            pedido.Property(p => p.Cep).HasColumnName("cep").HasMaxLength(8);
            pedido.Property(p => p.Logradouro).HasColumnName("logradouro").HasMaxLength(160);
            pedido.Property(p => p.Numero).HasColumnName("numero").HasMaxLength(12);
            pedido.Property(p => p.Complemento).HasColumnName("complemento").HasMaxLength(80);
            pedido.Property(p => p.Bairro).HasColumnName("bairro").HasMaxLength(80);
            pedido.Property(p => p.Cidade).HasColumnName("cidade").HasMaxLength(80);
            pedido.Property(p => p.Uf).HasColumnName("uf").HasMaxLength(2);
            pedido.Property(p => p.Pagamento).HasColumnName("pagamento").HasMaxLength(20);
            pedido.Property(p => p.TrocoPara).HasColumnName("troco_para").HasPrecision(10, 2);
            pedido.Property(p => p.Observacoes).HasColumnName("observacoes").HasMaxLength(500);
            pedido.Property(p => p.Subtotal).HasColumnName("subtotal").HasPrecision(10, 2);
            pedido.Property(p => p.TaxaEntrega).HasColumnName("taxa_entrega").HasPrecision(10, 2);
            pedido.Property(p => p.Total).HasColumnName("total").HasPrecision(10, 2);
            pedido.Property(p => p.WhatsAppLoja).HasColumnName("whatsapp_loja").HasMaxLength(13);
            pedido.Property(p => p.TokenHash).HasColumnName("token_hash").HasMaxLength(64);
            pedido.Property(p => p.RequisicaoHash).HasColumnName("requisicao_hash").HasMaxLength(64);
            pedido.HasIndex(p => p.CriadoEm);
            var item = modelo.Entity<ItemPedido>();
            item.ToTable("itens_pedido", t =>
            {
                t.HasCheckConstraint("ck_itens_pedido_quantidade", "quantidade BETWEEN 1 AND 20");
                t.HasCheckConstraint("ck_itens_pedido_valores", "preco_unitario > 0 AND subtotal = quantidade * preco_unitario");
            });
            item.HasKey(i => i.Id);
            item.Property(i => i.Id).HasColumnName("id");
            item.Property(i => i.PedidoId).HasColumnName("pedido_id");
            item.Property(i => i.ProdutoId).HasColumnName("produto_id");
            item.Property(i => i.NomeProduto).HasColumnName("nome_produto").HasMaxLength(120);
            item.Property(i => i.Quantidade).HasColumnName("quantidade");
            item.Property(i => i.PrecoUnitario).HasColumnName("preco_unitario").HasPrecision(10, 2);
            item.Property(i => i.Subtotal).HasColumnName("subtotal").HasPrecision(10, 2);
            item.HasOne(i => i.Pedido).WithMany(p => p.Itens).HasForeignKey(i => i.PedidoId).OnDelete(DeleteBehavior.Cascade);
            item.HasOne(i => i.Produto).WithMany().HasForeignKey(i => i.ProdutoId).OnDelete(DeleteBehavior.Restrict);
            item.HasIndex(i => new { i.PedidoId, i.ProdutoId }).IsUnique();
        }
    }
}
