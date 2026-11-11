class Usuario {
  final String id;
  final String nome;
  final String email;
  final String telefone;
  final String cpf;
  final String perfil;

  Usuario.fromJson(Map<String, dynamic> json)
    : id = json['id'] as String,
      nome = json['nome'] as String,
      email = json['email'] as String? ?? '',
      telefone = json['telefone'] as String,
      cpf = json['cpf'] as String? ?? '',
      perfil = json['perfil'] as String;
}

class Sessao {
  final Usuario usuario;
  final String token;
  final DateTime expiraEm;

  Sessao.fromJson(Map<String, dynamic> json)
    : usuario = Usuario.fromJson(json['usuario'] as Map<String, dynamic>),
      token = (json['token'] ?? json['access_token']) as String,
      expiraEm = DateTime.parse(json['expiraEm'] as String);

  const Sessao(this.usuario, this.token, this.expiraEm);
}
