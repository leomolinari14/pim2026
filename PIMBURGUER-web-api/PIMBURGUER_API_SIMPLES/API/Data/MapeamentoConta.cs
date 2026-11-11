using Microsoft.EntityFrameworkCore;
using PimBurguer.Api.Models;

namespace PimBurguer.Api.Data
{
    public static class MapeamentoConta
    {
        public static void Configurar(ModelBuilder m)
        {
            var u = m.Entity<Usuario>();
            u.ToTable("usuarios", t => t.HasCheckConstraint("ck_usuarios_perfil", "perfil IN ('Cliente', 'Administrador')"));
            u.HasKey(x => x.Id);
            u.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            u.Property(x => x.Nome).HasColumnName("nome").HasMaxLength(100);
            u.Property(x => x.NomeUsuario).HasColumnName("nome_usuario").HasMaxLength(60);
            u.HasIndex(x => x.NomeUsuario).IsUnique();
            u.Property(x => x.Email).HasColumnName("email").HasMaxLength(180);
            u.Property(x => x.EmailNormalizado).HasColumnName("email_normalizado").HasMaxLength(180);
            u.HasIndex(x => x.EmailNormalizado).IsUnique();
            u.Property(x => x.Telefone).HasColumnName("telefone").HasMaxLength(20);
            u.Property(x => x.Cpf).HasColumnName("cpf").HasMaxLength(11);
            u.Property(x => x.SenhaHash).HasColumnName("senha_hash").HasMaxLength(500);
            u.Property(x => x.Perfil).HasColumnName("perfil").HasMaxLength(20);
            u.Property(x => x.Ativo).HasColumnName("ativo");
            u.Property(x => x.CriadoEm).HasColumnName("criado_em");
            u.Property(x => x.FalhasLogin).HasColumnName("falhas_login");
            u.Property(x => x.BloqueadoAte).HasColumnName("bloqueado_ate");
            var s = m.Entity<SessaoAcesso>();
            s.ToTable("sessoes_acesso");
            s.HasKey(x => x.Id);
            s.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            s.Property(x => x.UsuarioId).HasColumnName("usuario_id");
            s.Property(x => x.TokenHash).HasColumnName("token_hash").HasMaxLength(64);
            s.Property(x => x.ExpiraEm).HasColumnName("expira_em");
            s.HasIndex(x => x.TokenHash).IsUnique();
            s.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.UsuarioId).OnDelete(DeleteBehavior.Cascade);
            var c = m.Entity<Cupom>();
            c.ToTable("cupons", t =>
            {
                t.HasCheckConstraint("ck_cupons_valor", "valor > 0 AND compra_minima >= 0 AND tipo IN ('Percentual','ValorFixo') AND (tipo <> 'Percentual' OR valor <= 100)");
                t.HasCheckConstraint("ck_cupons_validade", "valido_ate >= valido_de AND (limite_usos IS NULL OR limite_usos > 0)");
            });
            c.HasKey(x => x.Id);
            c.Property(x => x.Id).HasColumnName("id");
            c.Property(x => x.CupomCompartilhadoId).HasColumnName("cupom_compartilhado_id");
            c.HasIndex(x => x.CupomCompartilhadoId).IsUnique();
            c.Property(x => x.Codigo).HasColumnName("codigo").HasMaxLength(24);
            c.HasIndex(x => x.Codigo).IsUnique();
            c.Property(x => x.Tipo).HasColumnName("tipo").HasMaxLength(12);
            c.Property(x => x.Valor).HasColumnName("valor").HasPrecision(10, 2);
            c.Property(x => x.CompraMinima).HasColumnName("compra_minima").HasPrecision(10, 2);
            c.Property(x => x.ValidoDe).HasColumnName("valido_de");
            c.Property(x => x.ValidoAte).HasColumnName("valido_ate");
            c.Property(x => x.LimiteUsos).HasColumnName("limite_usos");
            c.Property(x => x.UmaVezPorCliente).HasColumnName("uma_vez_por_cliente");
            c.Property(x => x.Divulgar).HasColumnName("divulgar");
            c.Property(x => x.Ativo).HasColumnName("ativo");
            var salvo = m.Entity<CupomCliente>();
            salvo.ToTable("cupons_cliente");
            salvo.HasKey(x => new { x.UsuarioId, x.CupomId });
            salvo.Property(x => x.UsuarioId).HasColumnName("usuario_id");
            salvo.Property(x => x.CupomId).HasColumnName("cupom_id");
            salvo.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.UsuarioId).OnDelete(DeleteBehavior.Cascade);
            salvo.HasOne(x => x.Cupom).WithMany().HasForeignKey(x => x.CupomId).OnDelete(DeleteBehavior.Cascade);
            var uso = m.Entity<UsoCupom>();
            uso.ToTable("usos_cupom");
            uso.HasKey(x => x.Id);
            uso.Property(x => x.Id).HasColumnName("id");
            uso.Property(x => x.CupomId).HasColumnName("cupom_id");
            uso.Property(x => x.UsuarioId).HasColumnName("usuario_id");
            uso.Property(x => x.PedidoId).HasColumnName("pedido_id");
            uso.HasIndex(x => x.PedidoId).IsUnique();
            uso.HasIndex(x => new { x.CupomId, x.UsuarioId });
            uso.HasOne(x => x.Cupom).WithMany().HasForeignKey(x => x.CupomId).OnDelete(DeleteBehavior.Restrict);
            uso.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.UsuarioId).OnDelete(DeleteBehavior.Restrict);
            uso.HasOne(x => x.Pedido).WithMany().HasForeignKey(x => x.PedidoId).OnDelete(DeleteBehavior.Cascade);
            var p = m.Entity<Pedido>();
            p.Property(x => x.UsuarioId).HasColumnName("usuario_id");
            p.HasOne<Usuario>().WithMany().HasForeignKey(x => x.UsuarioId).OnDelete(DeleteBehavior.Restrict);
            p.Property(x => x.CupomId).HasColumnName("cupom_id");
            p.HasOne<Cupom>().WithMany().HasForeignKey(x => x.CupomId).OnDelete(DeleteBehavior.Restrict);
            p.Property(x => x.CodigoCupom).HasColumnName("codigo_cupom").HasMaxLength(24);
            p.Property(x => x.Desconto).HasColumnName("desconto").HasPrecision(10, 2);
            p.Property(x => x.Pago).HasColumnName("pago");
            p.Property(x => x.PagoEm).HasColumnName("pago_em");
            m.Entity<Produto>().Property(x => x.Removido).HasColumnName("removido");
            var a = m.Entity<MovimentacaoPedido>();
            a.ToTable("movimentacoes_pedido");
            a.HasKey(x => x.Id);
            a.Property(x => x.Id).HasColumnName("id");
            a.Property(x => x.PedidoId).HasColumnName("pedido_id");
            a.Property(x => x.AdministradorId).HasColumnName("administrador_id");
            a.Property(x => x.CriadoEm).HasColumnName("criado_em");
            a.Property(x => x.StatusAnterior).HasColumnName("status_anterior").HasMaxLength(40);
            a.Property(x => x.StatusNovo).HasColumnName("status_novo").HasMaxLength(40);
            a.Property(x => x.PagoAnterior).HasColumnName("pago_anterior");
            a.Property(x => x.PagoNovo).HasColumnName("pago_novo");
            a.HasOne(x => x.Pedido).WithMany().HasForeignKey(x => x.PedidoId).OnDelete(DeleteBehavior.Cascade);
            a.HasOne(x => x.Administrador).WithMany().HasForeignKey(x => x.AdministradorId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
