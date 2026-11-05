import 'dart:io';

import 'package:flutter_test/flutter_test.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'package:sqflite_common_ffi/sqflite_ffi.dart';

import 'package:ilakey/src/database_service.dart';

void main() {
  late Directory temporaryDirectory;

  setUpAll(() {
    sqfliteFfiInit();
  });

  setUp(() async {
    SharedPreferences.setMockInitialValues({});
    temporaryDirectory = await Directory.systemTemp.createTemp('ilakey-test-');
  });

  tearDown(() async {
    await temporaryDirectory.delete(recursive: true);
  });

  test('reads Lakey data and dashboard counts without exposing private fields',
      () async {
    final path =
        '${temporaryDirectory.path}${Platform.pathSeparator}ManA.sqlite';
    final fixture = await databaseFactoryFfi.openDatabase(path);
    await fixture.execute(
      'CREATE TABLE Agents (NumMatr TEXT PRIMARY KEY, NomPrenoms TEXT, '
      'Fonction TEXT, Actif INTEGER, DateEntree TEXT, Nsecu TEXT)',
    );
    await fixture.execute(
      'CREATE TABLE Client (NomClient TEXT PRIMARY KEY, Ville TEXT, '
      'TypeClient TEXT, Actif INTEGER)',
    );
    await fixture.execute(
      'CREATE TABLE SiteAffectation (NomSite TEXT, Client TEXT, '
      'Localisation TEXT, Actif INTEGER)',
    );
    await fixture.execute(
      'CREATE TABLE Postes (Code TEXT, Designation TEXT, NomSite TEXT, '
      'Actif INTEGER)',
    );
    await fixture.execute('CREATE TABLE Affectation (IdAffec INTEGER)');
    await fixture.execute('CREATE TABLE Absences (IdOp INTEGER)');
    for (final (matricule, name, active) in [
      ('AG-0001', 'Koffi Mensah', 1),
      ('AG-0002', 'Ama Lawson', 0),
    ]) {
      await fixture.insert('Agents', {
        'NumMatr': matricule,
        'NomPrenoms': name,
        'Fonction': 'Agent',
        'Actif': active,
        'DateEntree': '2026-01-01',
        'Nsecu': 'private',
      });
    }
    await fixture.insert('Client', {
      'NomClient': 'Société active',
      'Actif': 1,
    });
    await fixture.insert('Client', {
      'NomClient': 'Société inactive',
      'Actif': 0,
    });
    await fixture.insert('SiteAffectation', {
      'NomSite': 'Site actif',
      'Actif': 1,
    });
    await fixture.insert('SiteAffectation', {
      'NomSite': 'Site inactif',
      'Actif': 0,
    });
    await fixture.insert('Postes', {'Code': 'P1', 'Actif': 1});
    await fixture.insert('Postes', {'Code': 'P2', 'Actif': 0});
    await fixture.insert('Affectation', {'IdAffec': 1});
    await fixture.close();

    final service = DatabaseService(autoDiscover: false);
    await service.openDatabase(path);

    expect(await service.dashboardCounts(), {
      'agents': 2,
      'affectations': 1,
      'absences': 0,
      'clients': 2,
    });
    final records = await service.queryModule('Agents', search: 'Koffi');
    expect(records, hasLength(1));
    expect(records.single['NomPrenoms'], 'Koffi Mensah');
    expect(records.single.containsKey('Nsecu'), isFalse);

    for (final (module, activeField) in [
      ('Agents', 'NomPrenoms'),
      ('Clients', 'NomClient'),
      ('Sites', 'NomSite'),
      ('Postes', 'Code'),
    ]) {
      final orderedRows = await service.queryModule(module);
      expect(
        orderedRows.map((row) => row[activeField]).toList(),
        module == 'Agents'
            ? ['Koffi Mensah', 'Ama Lawson']
            : module == 'Clients'
                ? ['Société active', 'Société inactive']
                : module == 'Sites'
                    ? ['Site actif', 'Site inactif']
                    : ['P1', 'P2'],
        reason: '$module should show active rows before inactive rows',
      );
    }

    await service.close();
  });
}
