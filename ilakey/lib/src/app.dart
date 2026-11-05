import 'dart:async';

import 'package:flutter/material.dart';

import 'database_service.dart';

const ink = Color(0xFF172D46);
const navy = Color(0xFF10243A);
const teal = Color(0xFF16A697);
const canvas = Color(0xFFF4F7FA);

const modules = <(String, IconData)>[
  ('Tableau de bord', Icons.grid_view_rounded),
  ('Agents', Icons.groups_2_rounded),
  ('Entrées', Icons.person_add_alt_1_rounded),
  ('Contrats', Icons.description_rounded),
  ('Affectations', Icons.assignment_ind_rounded),
  ('Planning', Icons.calendar_month_rounded),
  ('Absences', Icons.event_busy_rounded),
  ('Congés', Icons.beach_access_rounded),
  ('Heures supplémentaires', Icons.more_time_rounded),
  ('Sorties', Icons.logout_rounded),
  ('Clients', Icons.apartment_rounded),
  ('Contrats clients', Icons.article_rounded),
  ('Sites', Icons.location_city_rounded),
  ('Postes', Icons.work_outline_rounded),
  ('État des clients', Icons.fact_check_rounded),
  ('Structure', Icons.account_tree_rounded),
  ('Fonctions', Icons.badge_rounded),
  ('Agences', Icons.business_rounded),
  ('Traitements', Icons.query_stats_rounded),
  ('Suppléments', Icons.auto_awesome_mosaic_rounded),
  ('Rappels & notes', Icons.sticky_note_2_rounded),
  ('Événements', Icons.event_note_rounded),
  ('Paramètres', Icons.settings_rounded),
];

class IlakeyApp extends StatelessWidget {
  const IlakeyApp({super.key, required this.light, this.databaseService});

  final bool light;
  final DatabaseService? databaseService;

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'ilakey',
      debugShowCheckedModeBanner: false,
      theme: ThemeData(
        useMaterial3: true,
        scaffoldBackgroundColor: canvas,
        colorScheme: ColorScheme.fromSeed(seedColor: teal, primary: teal),
        fontFamily: 'Segoe UI',
      ),
      home: Home(light: light, databaseService: databaseService),
    );
  }
}

class Home extends StatefulWidget {
  const Home({super.key, required this.light, this.databaseService});

  final bool light;
  final DatabaseService? databaseService;

  @override
  State<Home> createState() => _HomeState();
}

class _HomeState extends State<Home> {
  late final DatabaseService _database;
  String _selected = 'Tableau de bord';
  String _query = '';
  String? _connectionError;
  Future<Map<String, int>>? _statsFuture;
  Future<List<Map<String, Object?>>>? _recordsFuture;
  Timer? _searchDebounce;

  @override
  void initState() {
    super.initState();
    _database = widget.databaseService ?? DatabaseService();
    _connect();
  }

  @override
  void dispose() {
    _searchDebounce?.cancel();
    unawaited(_database.close());
    super.dispose();
  }

  Future<void> _connect() async {
    try {
      await _database.initialize();
      if (_database.path != null) _reload();
    } catch (error) {
      if (mounted) setState(() => _connectionError = error.toString());
    }
    if (mounted) setState(() {});
  }

  void _reload() {
    if (_database.path == null) return;
    setState(() {
      _connectionError = null;
      _statsFuture = _database.dashboardCounts();
      _recordsFuture =
          _selected == 'Tableau de bord' || !_database.supportsModule(_selected)
              ? null
              : _database.queryModule(_selected, search: _query);
    });
  }

  void _selectModule(String module) {
    setState(() {
      _selected = module;
      _recordsFuture = _database.path == null ||
              module == 'Tableau de bord' ||
              !_database.supportsModule(module)
          ? null
          : _database.queryModule(module, search: _query);
    });
  }

  Future<void> _chooseDatabase() async {
    try {
      final path = await _database.pickDatabase();
      if (path == null || !mounted) return;
      _reload();
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Base SQLite ouverte : $path')),
      );
    } catch (error) {
      if (!mounted) return;
      setState(() => _connectionError = error.toString());
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Ouverture de la base impossible : $error')),
      );
    }
  }

  int _bottomIndex() {
    final index = const [
      'Tableau de bord',
      'Agents',
      'Clients',
      'Structure',
      'Paramètres',
    ].indexOf(_selected);
    return index < 0 ? 0 : index;
  }

  @override
  Widget build(BuildContext context) {
    if (_database.path == null) return _databaseGate();

    final compact = widget.light || MediaQuery.sizeOf(context).width < 800;
    if (compact) {
      return Scaffold(
        appBar: AppBar(
          title: _brand(),
          actions: [
            IconButton(
              tooltip: 'Actualiser',
              onPressed: _reload,
              icon: const Icon(Icons.refresh),
            ),
            IconButton(
              tooltip: 'Ouvrir une base SQLite',
              onPressed: _chooseDatabase,
              icon: const Icon(Icons.folder_open),
            ),
          ],
        ),
        drawer: Drawer(child: _navigation()),
        body: _page(),
        bottomNavigationBar: NavigationBar(
          selectedIndex: _bottomIndex(),
          onDestinationSelected: (index) {
            _selectModule(
              const [
                'Tableau de bord',
                'Agents',
                'Clients',
                'Structure',
                'Paramètres',
              ][index],
            );
          },
          destinations: const [
            NavigationDestination(
              icon: Icon(Icons.grid_view),
              label: 'Accueil',
            ),
            NavigationDestination(icon: Icon(Icons.groups), label: 'Agents'),
            NavigationDestination(
              icon: Icon(Icons.apartment),
              label: 'Clients',
            ),
            NavigationDestination(
              icon: Icon(Icons.account_tree),
              label: 'Structure',
            ),
            NavigationDestination(
              icon: Icon(Icons.settings),
              label: 'Réglages',
            ),
          ],
        ),
      );
    }

    return Scaffold(
      body: Row(
        children: [
          SizedBox(width: 250, child: _navigation()),
          Expanded(
            child: Column(
              children: [
                _toolbar(),
                Expanded(child: _page()),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _databaseGate() {
    final error = _connectionError;
    return Scaffold(
      body: Center(
        child: ConstrainedBox(
          constraints: const BoxConstraints(maxWidth: 520),
          child: Padding(
            padding: const EdgeInsets.all(28),
            child: Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                _brand(),
                const SizedBox(height: 22),
                const Text(
                  'Connecter la base Lakey',
                  style: TextStyle(
                    color: ink,
                    fontSize: 22,
                    fontWeight: FontWeight.w800,
                  ),
                ),
                const SizedBox(height: 10),
                Text(
                  error ??
                      'Sélectionnez le fichier ManA.sqlite pour afficher les '
                          'données locales de Lakey.',
                  textAlign: TextAlign.center,
                  style: const TextStyle(color: Color(0xFF68788A)),
                ),
                const SizedBox(height: 20),
                FilledButton.icon(
                  onPressed: _chooseDatabase,
                  icon: const Icon(Icons.folder_open),
                  label: const Text('Choisir ManA.sqlite'),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }

  Widget _brand() {
    return Row(
      mainAxisSize: MainAxisSize.min,
      children: [
        Container(
          width: 36,
          height: 36,
          decoration: BoxDecoration(
            color: teal,
            borderRadius: BorderRadius.circular(11),
          ),
          child: const Icon(Icons.hub, color: Colors.white),
        ),
        const SizedBox(width: 10),
        const Text(
          'ilakey',
          style:
              TextStyle(fontWeight: FontWeight.w800, color: ink, fontSize: 18),
        ),
      ],
    );
  }

  Widget _navigation() {
    const groups = <(String, int, int)>[
      ('Vue générale', 0, 1),
      ('Équipe', 1, 10),
      ('Clients', 10, 15),
      ('Organisation', 15, 18),
      ('Gestion', 18, 23),
    ];

    return Container(
      color: navy,
      child: SafeArea(
        child: Column(
          children: [
            Padding(
              padding: const EdgeInsets.all(20),
              child: Row(
                children: [
                  _brand(),
                  const Spacer(),
                  if (MediaQuery.sizeOf(context).width < 800)
                    IconButton(
                      onPressed: () => Navigator.pop(context),
                      icon: const Icon(Icons.close, color: Colors.white),
                    ),
                ],
              ),
            ),
            Expanded(
              child: ListView(
                children: [
                  for (final (label, start, end) in groups) ...[
                    Padding(
                      padding: const EdgeInsets.fromLTRB(20, 14, 8, 5),
                      child: Text(
                        label.toUpperCase(),
                        style: const TextStyle(
                          color: Colors.white38,
                          fontSize: 10,
                          letterSpacing: 1,
                        ),
                      ),
                    ),
                    for (var index = start; index < end; index++)
                      Material(
                        color: Colors.transparent,
                        child: ListTile(
                          dense: true,
                          selected: _selected == modules[index].$1,
                          selectedTileColor: teal,
                          textColor: Colors.white70,
                          selectedColor: Colors.white,
                          leading: Icon(modules[index].$2, size: 18),
                          title: Text(
                            modules[index].$1,
                            style: const TextStyle(fontSize: 12),
                          ),
                          onTap: () {
                            _selectModule(modules[index].$1);
                            if (MediaQuery.sizeOf(context).width < 800) {
                              Navigator.pop(context);
                            }
                          },
                        ),
                      ),
                  ],
                ],
              ),
            ),
            const Padding(
              padding: EdgeInsets.all(14),
              child: Text(
                'Données locales · SQLite',
                style: TextStyle(color: Colors.white60, fontSize: 11),
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _toolbar() {
    return Container(
      height: 66,
      color: Colors.white,
      padding: const EdgeInsets.symmetric(horizontal: 24),
      child: Row(
        children: [
          Text(
            _selected,
            style: const TextStyle(color: ink, fontWeight: FontWeight.w700),
          ),
          const Spacer(),
          SizedBox(
            width: 230,
            height: 38,
            child: TextField(
              onChanged: (value) {
                _query = value;
                _searchDebounce?.cancel();
                _searchDebounce = Timer(
                  const Duration(milliseconds: 250),
                  () {
                    if (mounted && _database.path != null) {
                      setState(() {
                        _recordsFuture = _database.queryModule(
                          _selected,
                          search: _query,
                        );
                      });
                    }
                  },
                );
              },
              decoration: const InputDecoration(
                hintText: 'Rechercher...',
                prefixIcon: Icon(Icons.search, size: 18),
                filled: true,
                fillColor: canvas,
                border: OutlineInputBorder(borderSide: BorderSide.none),
              ),
            ),
          ),
          const SizedBox(width: 8),
          IconButton(
            tooltip: 'Ouvrir une base SQLite',
            onPressed: _chooseDatabase,
            icon: const Icon(Icons.folder_open),
          ),
          IconButton(
            tooltip: 'Actualiser',
            onPressed: _reload,
            icon: const Icon(Icons.refresh),
          ),
        ],
      ),
    );
  }

  Widget _page() =>
      _selected == 'Tableau de bord' ? _dashboard() : _modulePage();

  Widget _dashboard() {
    final stats = _statsFuture;
    if (stats == null) {
      return const Center(child: CircularProgressIndicator());
    }
    return FutureBuilder<Map<String, int>>(
      future: stats,
      builder: (context, snapshot) {
        if (snapshot.hasError) return _errorPanel(snapshot.error.toString());
        if (!snapshot.hasData) {
          return const Center(child: CircularProgressIndicator());
        }

        final counts = snapshot.data!;
        return LayoutBuilder(
          builder: (context, constraints) {
            final width = constraints.maxWidth;
            return SingleChildScrollView(
              padding: EdgeInsets.all(width < 600 ? 17 : 28),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    'Tableau de bord',
                    style: TextStyle(
                      color: ink,
                      fontWeight: FontWeight.w800,
                      fontSize: width < 600 ? 21 : 25,
                    ),
                  ),
                  const SizedBox(height: 5),
                  const Text(
                    'Résumé des données de la base locale Lakey.',
                    style: TextStyle(color: Color(0xFF7C8998)),
                  ),
                  const SizedBox(height: 22),
                  Wrap(
                    spacing: 12,
                    runSpacing: 12,
                    children: [
                      _stat(
                        width,
                        'Agents',
                        counts['agents'] ?? 0,
                        Icons.groups,
                        teal,
                      ),
                      _stat(
                        width,
                        'Affectations',
                        counts['affectations'] ?? 0,
                        Icons.work,
                        const Color(0xFF5684D2),
                      ),
                      _stat(
                        width,
                        'Absences',
                        counts['absences'] ?? 0,
                        Icons.event_busy,
                        const Color(0xFFE59D40),
                      ),
                      _stat(
                        width,
                        'Clients',
                        counts['clients'] ?? 0,
                        Icons.apartment,
                        const Color(0xFF8A70CD),
                      ),
                    ],
                  ),
                  const SizedBox(height: 20),
                  _panel(
                    'Base SQLite active',
                    Row(
                      children: [
                        const Icon(Icons.storage, color: teal),
                        const SizedBox(width: 12),
                        Expanded(
                          child: SelectableText(
                            _database.path ?? '',
                            style: const TextStyle(
                              color: ink,
                              fontSize: 13,
                            ),
                          ),
                        ),
                        TextButton.icon(
                          onPressed: _chooseDatabase,
                          icon: const Icon(Icons.folder_open),
                          label: const Text('Changer'),
                        ),
                      ],
                    ),
                  ),
                  const SizedBox(height: 12),
                  const Text(
                    'Consultation en lecture seule pendant le port des '
                    'opérations métier de Lakey.',
                    style: TextStyle(color: Colors.black45, fontSize: 11),
                  ),
                ],
              ),
            );
          },
        );
      },
    );
  }

  Widget _stat(
    double width,
    String title,
    int value,
    IconData icon,
    Color color,
  ) {
    final cardWidth = width > 1100
        ? (width - 36) / 4
        : width > 650
            ? (width - 12) / 2
            : (width - 12) / 2;
    return SizedBox(
      width: cardWidth,
      child: Container(
        padding: const EdgeInsets.all(16),
        decoration: BoxDecoration(
          color: Colors.white,
          borderRadius: BorderRadius.circular(14),
          border: Border.all(color: const Color(0xFFEBF0F3)),
        ),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              children: [
                Expanded(
                  child: Text(
                    title,
                    style: const TextStyle(
                      color: Color(0xFF788797),
                      fontSize: 12,
                    ),
                  ),
                ),
                Icon(icon, color: color),
              ],
            ),
            const SizedBox(height: 12),
            Text(
              '$value',
              style: const TextStyle(
                color: ink,
                fontSize: 25,
                fontWeight: FontWeight.w800,
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _modulePage() {
    final records = _recordsFuture;
    return LayoutBuilder(
      builder: (context, constraints) {
        return SingleChildScrollView(
          padding: const EdgeInsets.all(22),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                _selected,
                style: const TextStyle(
                  fontSize: 24,
                  fontWeight: FontWeight.w800,
                  color: ink,
                ),
              ),
              const SizedBox(height: 6),
              Text(
                'Données provenant de ${_database.path ?? 'la base Lakey'}.',
                style: const TextStyle(color: Color(0xFF7C8998)),
              ),
              const SizedBox(height: 20),
              if (!_database.supportsModule(_selected))
                _panel(
                  'Module non relié',
                  Text(
                    'Les opérations métier de « $_selected » doivent encore '
                    'être portées depuis Lakey.',
                    style: const TextStyle(color: Color(0xFF68788A)),
                  ),
                )
              else if (records == null)
                const Center(child: CircularProgressIndicator())
              else
                FutureBuilder<List<Map<String, Object?>>>(
                  future: records,
                  builder: (context, snapshot) {
                    if (snapshot.hasError) {
                      return _errorPanel(snapshot.error.toString());
                    }
                    if (!snapshot.hasData) {
                      return const Center(child: CircularProgressIndicator());
                    }
                    final rows = snapshot.data!;
                    if (rows.isEmpty) {
                      return _panel(
                        'Aucun résultat',
                        const Text(
                          'Aucune ligne ne correspond à cette recherche.',
                          style: TextStyle(color: Color(0xFF68788A)),
                        ),
                      );
                    }

                    return _panel(
                      'Données récentes (${rows.length})',
                      Column(
                        children: [
                          for (var index = 0; index < rows.length; index++)
                            _recordTile(rows[index], index),
                          if (rows.length == 100)
                            const Padding(
                              padding: EdgeInsets.only(top: 10),
                              child: Text(
                                'Affichage limité aux 100 premières lignes.',
                                style: TextStyle(
                                  color: Colors.black45,
                                  fontSize: 11,
                                ),
                              ),
                            ),
                        ],
                      ),
                    );
                  },
                ),
              if (_database.supportsModule(_selected))
                const Padding(
                  padding: EdgeInsets.only(top: 8),
                  child: Text(
                    'Les enregistrements actifs sont affichés en premier.',
                    style: TextStyle(color: Colors.black45, fontSize: 11),
                  ),
                ),
              const SizedBox(height: 12),
              SizedBox(
                width: constraints.maxWidth,
                child: const Text(
                  'Lecture seule · Les fonctions de création et de modification '
                  'seront activées après le port des règles de validation '
                  'et des permissions Lakey.',
                  style: TextStyle(color: Colors.black45, fontSize: 11),
                ),
              ),
            ],
          ),
        );
      },
    );
  }

  Widget _recordTile(Map<String, Object?> row, int index) {
    final preferredFields = switch (_selected) {
      'Agents' || 'Entrées' => const ['NomPrenoms', 'NumMatr'],
      'Clients' || 'Contrats clients' || 'État des clients' => const [
          'NomClient',
        ],
      'Contrats' => const ['NumContrat', 'NomPrenoms'],
      'Affectations' => const ['AgentAffect', 'SiteAffect'],
      'Planning' => const ['Agent', 'Poste'],
      'Absences' || 'Congés' => const ['NomPrenom', 'Motif'],
      _ => const <String>[],
    };
    final heading = preferredFields
        .map((field) => row[field])
        .where((value) => value != null && value.toString().trim().isNotEmpty)
        .firstOrNull;
    final visibleValues = row.entries
        .where(
            (entry) => entry.value != null && entry.value.toString().isNotEmpty)
        .where((entry) => entry.key != preferredFields.firstOrNull)
        .toList();

    return ListTile(
      contentPadding: EdgeInsets.zero,
      dense: true,
      leading: CircleAvatar(
        radius: 16,
        backgroundColor: const Color(0xFFE4F5F2),
        child: Text(
          '${index + 1}',
          style: const TextStyle(color: teal, fontSize: 11),
        ),
      ),
      title: Text(
        heading?.toString() ??
            visibleValues.firstOrNull?.value?.toString() ??
            'Enregistrement',
        style: const TextStyle(fontSize: 13, color: ink),
      ),
      subtitle: visibleValues.isEmpty
          ? null
          : Text(
              visibleValues
                  .take(3)
                  .map((entry) => '${entry.key} : ${entry.value}')
                  .join('  ·  '),
              maxLines: 2,
              overflow: TextOverflow.ellipsis,
              style: const TextStyle(fontSize: 11),
            ),
    );
  }

  Widget _errorPanel(String error) => _panel(
        'Erreur SQLite',
        Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            SelectableText(error, style: const TextStyle(color: Colors.red)),
            const SizedBox(height: 10),
            TextButton.icon(
              onPressed: _chooseDatabase,
              icon: const Icon(Icons.folder_open),
              label: const Text('Choisir une autre base'),
            ),
          ],
        ),
      );

  Widget _panel(String title, Widget child) {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(18),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(14),
        border: Border.all(color: const Color(0xFFEBF0F3)),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            title,
            style: const TextStyle(
              color: ink,
              fontSize: 14,
              fontWeight: FontWeight.w700,
            ),
          ),
          const SizedBox(height: 12),
          child,
        ],
      ),
    );
  }
}
