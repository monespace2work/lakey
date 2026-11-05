import 'package:flutter_test/flutter_test.dart';

import 'package:ilakey/src/app.dart';
import 'package:ilakey/src/database_service.dart';

void main() {
  testWidgets('shows the local Lakey database connection screen', (
    tester,
  ) async {
    await tester.pumpWidget(
      IlakeyApp(
        light: false,
        databaseService: DatabaseService(autoDiscover: false),
      ),
    );
    await tester.pumpAndSettle();

    expect(find.text('Connecter la base Lakey'), findsOneWidget);
    expect(find.text('Choisir ManA.sqlite'), findsOneWidget);
  });
}
