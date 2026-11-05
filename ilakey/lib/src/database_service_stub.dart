class DatabaseService {
  DatabaseService({this.autoDiscover = true});

  final bool autoDiscover;

  String? get path => null;

  Future<void> initialize() async {
    throw UnsupportedError(
      'La connexion directe à SQLite est disponible uniquement sous Windows.',
    );
  }

  Future<String?> pickDatabase() async {
    throw UnsupportedError(
      'La sélection d’une base SQLite est disponible uniquement sous Windows.',
    );
  }

  Future<void> openDatabase(String databasePath) async {
    throw UnsupportedError(
      'La connexion directe à SQLite est disponible uniquement sous Windows.',
    );
  }

  bool supportsModule(String module) => false;

  Future<List<Map<String, Object?>>> queryModule(
    String module, {
    String search = '',
  }) async {
    throw UnsupportedError(
      'La connexion directe à SQLite est disponible uniquement sous Windows.',
    );
  }

  Future<Map<String, int>> dashboardCounts() async {
    throw UnsupportedError(
      'La connexion directe à SQLite est disponible uniquement sous Windows.',
    );
  }

  Future<void> close() async {}
}
