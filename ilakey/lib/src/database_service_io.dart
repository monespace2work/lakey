import 'dart:io';

import 'package:file_picker/file_picker.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'package:sqflite_common_ffi/sqflite_ffi.dart';

class DatabaseService {
  DatabaseService({this.autoDiscover = true});

  static const _lastDatabaseKey = 'lakey_database_path';
  static const _moduleTables = <String, String>{
    'Agents': 'Agents',
    'Contrats': 'ContratsTbl',
    'Affectations': 'Affectation',
    'Planning': 'PlanPoste',
    'Absences': 'Absences',
    'Heures supplémentaires': 'HeureSup',
    'Sorties': 'sortie',
    'Clients': 'Client',
    'Sites': 'SiteAffectation',
    'Postes': 'Postes',
    'Fonctions': 'Fonction',
    'Agences': 'Agences',
    'Événements': 'Events',
  };
  static const _displayColumns = <String, List<String>>{
    'Agents': ['NumMatr', 'NomPrenoms', 'Fonction', 'Actif', 'DateEntree'],
    'Contrats': [
      'NumContrat',
      'NomPrenoms',
      'TypContr',
      'DebutContr',
      'FinContr'
    ],
    'Affectations': ['DateAffect', 'AgentAffect', 'SiteAffect', 'FinAffect'],
    'Planning': ['DatePlan', 'Agent', 'Poste'],
    'Absences': ['DateDebut', 'DateFin', 'NomPrenom', 'Motif'],
    'Heures supplémentaires': ['DateHS', 'Agent', 'NbFaction', 'SiteHS'],
    'Sorties': ['DateSortie', 'NomsPrenoms', 'MotifSortie', 'DateEntreeRappel'],
    'Clients': ['NomClient', 'Ville', 'TypeClient', 'Actif'],
    'Sites': ['NomSite', 'Client', 'Localisation', 'Actif'],
    'Postes': ['Code', 'Designation', 'NomSite', 'Actif'],
    'Fonctions': ['Fonction', 'Description', 'Niveau', 'Actif'],
    'Agences': ['Designation', 'Ville', 'Responsable', 'Actif'],
    'Événements': ['DateEvt', 'SiteEvent', 'DescEvt', 'ResolEvt'],
  };

  final bool autoDiscover;
  Database? _database;
  String? _path;

  String? get path => _path;

  Future<void> initialize() async {
    if (!autoDiscover) return;

    sqfliteFfiInit();
    final preferences = await SharedPreferences.getInstance();
    final savedPath = preferences.getString(_lastDatabaseKey);
    if (savedPath != null && savedPath.isNotEmpty) {
      await openDatabase(savedPath);
      return;
    }

    final defaultPath = _findDefaultDatabase();
    if (defaultPath == null) {
      throw const FileSystemException(
        'ManA.sqlite est introuvable. Choisissez le fichier SQLite de Lakey.',
      );
    }
    await openDatabase(defaultPath);
  }

  Future<String?> pickDatabase() async {
    final result = await FilePicker.pickFile(
      type: FileType.custom,
      allowedExtensions: const ['sqlite', 'db'],
      dialogTitle: 'Choisir la base SQLite de Lakey',
    );
    final selectedPath = result?.path;
    if (selectedPath == null) return null;

    await openDatabase(selectedPath);
    return selectedPath;
  }

  Future<void> openDatabase(String databasePath) async {
    sqfliteFfiInit();
    final file = File(databasePath);
    if (!await file.exists()) {
      throw FileSystemException(
          'Le fichier SQLite n’existe pas.', databasePath);
    }

    final openedDatabase = await databaseFactoryFfi.openDatabase(
      file.absolute.path,
      options: OpenDatabaseOptions(
        readOnly: true,
        singleInstance: false,
        onConfigure: (database) async {
          await database.execute('PRAGMA foreign_keys = ON');
        },
      ),
    );

    try {
      final tables = await openedDatabase.rawQuery(
        "SELECT name FROM sqlite_master WHERE type = 'table' "
        "AND name IN ('Agents', 'Client')",
      );
      if (tables.length != 2) {
        throw const FormatException(
          'Cette base ne contient pas le schéma Lakey attendu (Agents, Client).',
        );
      }

      final absolutePath = file.absolute.path;
      await (await SharedPreferences.getInstance())
          .setString(_lastDatabaseKey, absolutePath);
      final previousDatabase = _database;
      _database = openedDatabase;
      _path = absolutePath;
      await previousDatabase?.close();
    } catch (_) {
      await openedDatabase.close();
      rethrow;
    }
  }

  bool supportsModule(String module) => _moduleTables.containsKey(module);

  Future<List<Map<String, Object?>>> queryModule(
    String module, {
    String search = '',
  }) async {
    final database = _requireDatabase();
    final table = _moduleTables[module];
    if (table == null) {
      throw UnsupportedError('Le module « $module » n’est pas encore relié.');
    }

    final tableInfo = await database.rawQuery('PRAGMA table_info("$table")');
    final existingColumns =
        tableInfo.map((column) => column['name']! as String).toSet();
    final columns = (_displayColumns[module] ?? const <String>[])
        .where(existingColumns.contains)
        .toList();
    if (columns.isEmpty) {
      throw FormatException(
        'Aucune colonne d’affichage connue pour la table $table.',
      );
    }

    final args = <Object?>[];
    var where = '';
    if (search.trim().isNotEmpty) {
      final searchableColumns = columns.where(
        (column) =>
            column.toLowerCase() != 'actif' &&
            column.toLowerCase() != 'favorit',
      );
      if (searchableColumns.isNotEmpty) {
        where =
            ' WHERE ${searchableColumns.map((column) => 'CAST("$column" AS TEXT) LIKE ?').join(' OR ')}';
        args.addAll(
          List<Object?>.filled(searchableColumns.length, '%${search.trim()}%'),
        );
      }
    }

    final selection = columns.map((column) => '"$column"').join(', ');
    final order =
        existingColumns.contains('Actif') ? ' ORDER BY "Actif" DESC' : '';
    final rows = await database.rawQuery(
      'SELECT $selection FROM "$table"$where$order LIMIT 100',
      args,
    );
    return rows;
  }

  Future<Map<String, int>> dashboardCounts() async {
    final database = _requireDatabase();
    final counts = <String, int>{};
    for (final entry in const <String, String>{
      'agents': 'Agents',
      'affectations': 'Affectation',
      'absences': 'Absences',
      'clients': 'Client',
    }.entries) {
      final result = await database.rawQuery(
        'SELECT COUNT(*) AS total FROM "${entry.value}"',
      );
      counts[entry.key] = result.first['total']! as int;
    }
    return counts;
  }

  Future<void> close() async {
    await _database?.close();
    _database = null;
    _path = null;
  }

  Database _requireDatabase() {
    final database = _database;
    if (database == null) {
      throw StateError('Aucune base SQLite Lakey n’est ouverte.');
    }
    return database;
  }

  String? _findDefaultDatabase() {
    final directories = <Directory>[
      Directory.current,
      File(Platform.resolvedExecutable).parent,
    ];
    final candidates = <String>{};

    for (final start in directories) {
      var directory = start;
      for (var level = 0; level < 8; level++) {
        candidates.add(
          '${directory.path}${Platform.pathSeparator}ManA.sqlite',
        );
        final parent = directory.parent;
        if (parent.path == directory.path) break;
        directory = parent;
      }
    }
    candidates.add(r'C:\LkyRess\ManA.sqlite');

    for (final candidate in candidates) {
      if (File(candidate).existsSync()) return candidate;
    }
    return null;
  }
}
