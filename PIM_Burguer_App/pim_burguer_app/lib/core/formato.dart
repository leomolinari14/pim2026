import 'package:intl/intl.dart';

final _brl = NumberFormat.currency(locale: 'pt_BR', symbol: r'R$');
String moeda(num valor) => _brl.format(valor);
String digitos(String valor) => valor.replaceAll(RegExp(r'[^0-9]'), '');
String dataHora(DateTime data) =>
    DateFormat('dd/MM/yyyy HH:mm').format(data.toLocal());

const pagamentos = {
  'Pix': 'Pix',
  'Dinheiro': 'Dinheiro',
  'CartaoDebito': 'Cartão de débito',
  'CartaoCredito': 'Cartão de crédito',
};

bool cpfValido(String texto) {
  final cpf = digitos(texto);
  if (cpf.length != 11 || cpf.split('').toSet().length == 1) return false;
  final numeros = cpf.split('').map(int.parse).toList();
  for (var posicao = 9; posicao <= 10; posicao++) {
    var soma = 0;
    for (var i = 0; i < posicao; i++) {
      soma += numeros[i] * (posicao + 1 - i);
    }
    if ((soma * 10 % 11) % 10 != numeros[posicao]) return false;
  }
  return true;
}
