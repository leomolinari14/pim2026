import 'package:flutter/material.dart';

const papel = Color(0xFFF7F4ED);
const tinta = Color(0xFF242B27);
const marca = Color(0xFFA63D27);
const verde = Color(0xFF26322D);
const dourado = Color(0xFFE8BC63);

ThemeData temaPim() {
  final esquema = ColorScheme.fromSeed(seedColor: marca).copyWith(
    primary: marca,
    surface: papel,
    onSurface: tinta,
    secondary: verde,
  );
  return ThemeData(
    useMaterial3: true,
    colorScheme: esquema,
    scaffoldBackgroundColor: papel,
    appBarTheme: const AppBarTheme(
      backgroundColor: papel,
      foregroundColor: tinta,
    ),
    navigationBarTheme: const NavigationBarThemeData(
      backgroundColor: Colors.white,
      indicatorColor: Color(0xFFF8E0D6),
    ),
    cardTheme: CardThemeData(
      elevation: 0,
      color: Colors.white,
      margin: EdgeInsets.zero,
      shape: RoundedRectangleBorder(
        side: const BorderSide(color: Color(0xFFE5E2D9)),
        borderRadius: BorderRadius.circular(18),
      ),
    ),
    inputDecorationTheme: InputDecorationTheme(
      filled: true,
      fillColor: Colors.white,
      border: OutlineInputBorder(borderRadius: BorderRadius.circular(12)),
    ),
    filledButtonTheme: FilledButtonThemeData(
      style: FilledButton.styleFrom(
        minimumSize: const Size(48, 48),
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
      ),
    ),
  );
}
