import 'package:flutter_test/flutter_test.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:owner_app_flutter/main.dart';

void main() {
  testWidgets('App renders login screen when unauthenticated', (tester) async {
    await tester.pumpWidget(const ProviderScope(child: MoveWellApp()));
    expect(find.text('Owner Sign In'), findsOneWidget);
  });
}
