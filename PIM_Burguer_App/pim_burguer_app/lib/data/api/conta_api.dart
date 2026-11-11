import '../../models/pedido.dart';
import '../../models/usuario.dart';
import 'api_cliente.dart';

class ContaApi {
  final ApiCliente cliente;
  const ContaApi(this.cliente);

  Future<Sessao> autenticar(bool cadastrar, Map<String, Object?> dados) async =>
      Sessao.fromJson(
        objeto(
          await cliente.enviar(
            'POST',
            cadastrar ? 'api/conta/cadastro' : 'api/conta/login',
            corpo: dados,
          ),
        ),
      );

  Future<Usuario> atualizarPerfil(
    String token,
    String nome,
    String telefone,
  ) async => Usuario.fromJson(
    objeto(
      await cliente.enviar(
        'PUT',
        'api/conta/perfil',
        token: token,
        corpo: {'nome': nome, 'telefone': telefone},
      ),
    ),
  );

  Future<void> alterarSenha(String token, Map<String, Object?> dados) async {
    await cliente.enviar('PUT', 'api/conta/senha', token: token, corpo: dados);
  }

  Future<void> sair(String token) async {
    await cliente.enviar('POST', 'api/conta/sair', token: token);
  }
}
