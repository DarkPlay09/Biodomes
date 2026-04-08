# Bio Domes

Projet d'application web pour l'UE19. Le projet prend la forme d'une solution .NET découpée en cinq projets :

- un projet d'application web Razor Page
- un projet de bibliothèque modélisant le domaine.
- un projet d'infrastructures, ce dernier reprenant notamment les éléments propres à EntityFrameworkCore.
- un projet de test permettant de tester le projet de bibliothèque modélisant le domaine.
- un projet de test permettant de tester le projet d'infrastructures.

La suite du document sera à compléter par vos soins.

## Base de données

**Créer une nouvelle migration**

```
dotnet ef migrations add NOM_DE_LA_MIGRATION --context BioDomesDbContext --project .\BioDomes.Infrastructures\BioDomes.Infrastructures.csproj --startup-project .\BioDomes.Web\BioDomes.Web.csproj --output-dir EntityFramework\Migrations
```

**Appliquer les migrations à la base**

```
dotnet ef database update --context BioDomesDbContext --project .\BioDomes.Infrastructures\BioDomes.Infrastructures.csproj --startup-project .\BioDomes.Web\BioDomes.Web.csproj
```

**Lister les migrations existantes**

```
dotnet ef migrations list --context BioDomesDbContext --project .\BioDomes.Infrastructures\BioDomes.Infrastructures.csproj --startup-project .\BioDomes.Web\BioDomes.Web.csproj
```

**Supprimer la dernière migration**

```
dotnet ef migrations remove --context BioDomesDbContext --project .\BioDomes.Infrastructures\BioDomes.Infrastructures.csproj --startup-project .\BioDomes.Web\BioDomes.Web.csproj
```

## Membres de l'équipe

**TODO :** indiquez les membres de votre équipe (de deux à trois membres).

**TODO :** indiquez quel membre est responsable du déploiement. Nous testerons l'application web depuis son URL https://ue19.cg.helmo.be/matricule.

## Construction de la solution

**TODO :** expliquez les étapes nécessaires à la récupération de votre solution, à sa construction et à son exécution dans un environnement de développement (tel que l'ordinateur de votre responsable).

## Fonctionnalités implémentées

**TODO :** pour chaque US décrite dans l'énoncé, indiquez son état d'avancement (non-faite, débutée, partiellement achevée, totalement achevée). Quand une US est débutée ou partiellement achevée, indiquez en quelques mots ce qui manque selon vous.

## Données de connexion

**TODO :** indiquez au minimum trois comptes dont un compte d'administrateur.

## Éléments techniques notables

**TODO :** indiquez les éléments techniques que vous jugez notables. Par exemple, l'utilisation d'un framework CSS autre que Bootstrap. Autre exemple, l'utilisation de bibliothèques JS pour l'affichage de graphiques.