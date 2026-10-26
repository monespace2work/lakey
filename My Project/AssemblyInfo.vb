Imports System
Imports System.Globalization
Imports System.Reflection
Imports System.Resources
Imports System.Runtime.InteropServices
Imports System.Windows

' Les informations générales relatives à un assembly dépendent de
' l'ensemble d'attributs suivant. Changez les valeurs de ces attributs pour modifier les informations
' associées à un assembly.

' Vérifiez les valeurs des attributs de l'assembly

<Assembly: AssemblyTitle("Lakey")>
<Assembly: AssemblyDescription("Gestionnaire des agents")>
<Assembly: AssemblyCompany("JPB")>
<Assembly: AssemblyProduct("Lakey")>
<Assembly: AssemblyCopyright("Copyright ©  2024")>
<Assembly: AssemblyTrademark("Lakey")>
<Assembly: ComVisible(false)>

'Pour commencer à générer des applications localisables, définissez
'<UICulture>CultureUtiliséePourCoder</UICulture> dans votre fichier .vbproj,
'dans <PropertyGroup>.  Par exemple, si vous utilisez le français
'dans vos fichiers sources, définissez <UICulture> à "fr-FR". Puis, supprimez les marques de commentaire de
'l'attribut NeutralResourceLanguage ci-dessous. Mettez à jour "fr-FR" dans
'la ligne ci-après pour qu'elle corresponde au paramètre UICulture du fichier projet.

'<Assembly: NeutralResourcesLanguage("en-US", UltimateResourceFallbackLocation.Satellite)>


'L'attribut ThemeInfo décrit l'emplacement des dictionnaires de ressources spécifiques à un thème et génériques.
'Premier paramètre : emplacement des dictionnaires de ressources spécifiques à un thème
'(utilisé si une ressource est introuvable dans la page,
' ou dictionnaires de ressources de l'application)

'2nd paramètre : emplacement du dictionnaire de ressources générique
'(utilisé si une ressource est introuvable dans la page,
'l'application et tous les dictionnaires de ressources spécifiques à un thème)
<Assembly: ThemeInfo(ResourceDictionaryLocation.None, ResourceDictionaryLocation.SourceAssembly)>



'Le GUID suivant est pour l'ID de la typelib si ce projet est exposé à COM
<Assembly: Guid("24f16d68-3ea4-42a3-a525-2199663041f9")>

' Les informations de version pour un assembly se composent des quatre valeurs suivantes :
'
'      Version principale
'      Version secondaire
'      Numéro de build
'      Révision
'
' Vous pouvez spécifier toutes les valeurs ou indiquer les numéros de build et de révision par défaut
' en utilisant '*', comme indiqué ci-dessous :
' <Assembly: AssemblyVersion("1.0.*")>

<Assembly: AssemblyVersion("3.1.1.1")>
<Assembly: AssemblyFileVersion("3.1.1.1")>
