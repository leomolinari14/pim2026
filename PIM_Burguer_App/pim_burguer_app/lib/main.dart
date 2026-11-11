import 'package:flutter/material.dart';
import 'package:flutter_localizations/flutter_localizations.dart';
import 'package:provider/provider.dart';

import 'core/app_estado.dart';
import 'core/configuracao.dart';
import 'core/rotas.dart';
import 'core/tema.dart';
import 'data/api/api_cliente.dart';
import 'data/api/conta_api.dart';
import 'data/api/pedido_api.dart';
import 'data/api/produto_api.dart';
import 'data/db/app_database.dart';
import 'data/repositories/pedido_repository.dart';
import 'data/repositories/produto_repository.dart';
import 'models/pedido.dart';
import 'ui/telas/carrinho_screen.dart';
import 'ui/telas/checkout_screen.dart';
import 'ui/telas/home_screen.dart';
import 'ui/telas/login_screen.dart';
import 'ui/telas/pedidos_screen.dart';
import 'ui/telas/perfil_screen.dart';
import 'ui/widgets/componentes.dart';

void main() {
  WidgetsFlutterBinding.ensureInitialized();
  final Uri endereco;
  try {
    endereco = enderecoApi();
  } on FormatException catch (erro) {
    runApp(
      MaterialApp(
        debugShowCheckedModeBanner: false,
        theme: temaPim(),
        home: Scaffold(
          body: SafeArea(
            child: Center(
              child: Mensagem(
                Icons.settings_outlined,
                'Configure a API',
                texto: erro.message,
              ),
            ),
          ),
        ),
      ),
    );
    return;
  }
  runApp(
    ChangeNotifierProvider(
      create: (_) {
        final api = ApiCliente(endereco);
        final banco = AppDatabase(endereco.toString());
        return AppEstado(
          api,
          ContaApi(api),
          ProdutoRepository(ProdutoApi(api), banco),
          PedidoRepository(PedidoApi(api), banco),
          banco,
        )..iniciar();
      },
      child: const PimBurguerApp(),
    ),
  );
}

class PimBurguerApp extends StatelessWidget {
  const PimBurguerApp({super.key});

  @override
  Widget build(BuildContext context) => MaterialApp(
    title: 'PIM Burguer',
    debugShowCheckedModeBanner: false,
    theme: temaPim(),
    locale: const Locale('pt', 'BR'),
    supportedLocales: const [Locale('pt', 'BR')],
    localizationsDelegates: GlobalMaterialLocalizations.delegates,
    routes: {
      Rotas.home: (_) => const HomeScreen(),
      Rotas.login: (_) => const LoginScreen(),
      Rotas.carrinho: (_) => const CarrinhoScreen(),
      Rotas.checkout: (_) => const CheckoutScreen(),
      Rotas.perfil: (_) => const PerfilScreen(),
      Rotas.senha: (_) => const PerfilScreen(senha: true),
      Rotas.cupons: (context) => CuponsScreen(
        selecionar: ModalRoute.of(context)?.settings.arguments == true,
      ),
      Rotas.pedido: (context) {
        final argumento = ModalRoute.of(context)!.settings.arguments;
        return argumento is Pedido
            ? PedidoScreen(argumento.id, confirmado: argumento)
            : PedidoScreen(argumento as String);
      },
    },
  );
}
