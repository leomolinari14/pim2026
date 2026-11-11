import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../../core/app_estado.dart';
import '../../core/rotas.dart';
import '../../core/tema.dart';
import '../widgets/componentes.dart';
import 'catalogo_screen.dart';
import 'pedidos_screen.dart';
import 'perfil_screen.dart';

class HomeScreen extends StatefulWidget {
  const HomeScreen({super.key});

  @override
  State<HomeScreen> createState() => _HomeScreenState();
}

class _HomeScreenState extends State<HomeScreen> {
  int _aba = 0;
  int _versaoPedidos = 0;

  @override
  Widget build(BuildContext context) {
    final estado = context.watch<AppEstado>();
    return Scaffold(
      appBar: AppBar(
        title: Row(
          children: [
            const Icon(Icons.lunch_dining, color: marca),
            const SizedBox(width: 8),
            Flexible(
              child: Text(
                const ['PIM Burguer', 'Meus pedidos', 'Minha conta'][_aba],
                style: const TextStyle(fontWeight: FontWeight.w800),
              ),
            ),
          ],
        ),
        actions: [
          IconButton(
            tooltip: estado.unidades == 1
                ? 'Carrinho (1 item)'
                : 'Carrinho (${estado.unidades} itens)',
            onPressed: estado.inicializando || estado.erroLocal != null
                ? null
                : () async {
                    ScaffoldMessenger.of(context).hideCurrentSnackBar();
                    await Navigator.pushNamed(context, Rotas.carrinho);
                    if (mounted) setState(() => _versaoPedidos++);
                  },
            icon: Badge(
              isLabelVisible: estado.unidades > 0,
              label: Text('${estado.unidades}'),
              child: const Icon(Icons.shopping_bag_outlined),
            ),
          ),
          const SizedBox(width: 8),
        ],
      ),
      body: SafeArea(
        top: false,
        bottom: false,
        child: switch (_aba) {
          0 => const CatalogoScreen(),
          1 =>
            estado.usuario == null
                ? const EntrarAviso()
                : PedidosScreen(
                    key: ValueKey((estado.usuario!.id, _versaoPedidos)),
                  ),
          _ => const ContaScreen(),
        },
      ),
      bottomNavigationBar: NavigationBar(
        selectedIndex: _aba,
        onDestinationSelected: (i) => setState(() => _aba = i),
        destinations: const [
          NavigationDestination(
            icon: Icon(Icons.restaurant_menu),
            label: 'Cardápio',
          ),
          NavigationDestination(
            icon: Icon(Icons.receipt_long_outlined),
            label: 'Pedidos',
          ),
          NavigationDestination(
            icon: Icon(Icons.person_outline),
            label: 'Conta',
          ),
        ],
      ),
    );
  }
}
