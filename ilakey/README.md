# ilakey

Portage Flutter de Lakey, ciblé Windows et connecté à la base locale SQLite.

## Windows

Depuis le dossier `ilakey` :

```powershell
flutter run -d windows
flutter build windows --release
```

Au démarrage, iLakey cherche `ManA.sqlite` dans le dossier courant, ses parents
et l'ancien emplacement `C:\LkyRess\ManA.sqlite`. Si le fichier n'est pas trouvé,
ou pour en choisir un autre, utilisez **Choisir ManA.sqlite** dans l'application.
Le chemin sélectionné est mémorisé pour les prochains démarrages.

La connexion est actuellement en lecture seule. Le tableau de bord et les
modules Agents, Contrats, Affectations, Planning, Absences, Heures
supplémentaires, Sorties, Clients, Sites, Postes, Fonctions, Agences et
Événements affichent les données réelles de la base sélectionnée.

L'écriture reste désactivée pendant le port des contrôles de validation,
permissions et opérations métier de Lakey. Les modules qui ne sont pas encore
reliés sont signalés dans l'interface au lieu d'afficher des données fictives.
