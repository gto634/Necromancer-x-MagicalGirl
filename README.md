# Necromancer × Magical Girl

Projet Unity de jeu 3D en coopération locale, issu du travail réalisé sur
`lyo-3g-gameboy-s1p1-06`. Ce dépôt GitHub indépendant conserve l'état enregistré
du projet au 12 octobre 2026, avec un nouvel historique propre à ce dépôt.

## Ouvrir le projet

1. Cloner ce dépôt.
2. Dans Unity Hub, ajouter le dossier racine contenant `Assets`, `Packages`
   et `ProjectSettings`.
3. Utiliser **Unity 6000.3.21f1**. Unity recréera ses caches et récupérera les
   packages lors de la première ouverture.
4. Ouvrir `Assets/_game/Scenes/Final/HomeScreen.unity`.

Les scènes activées dans les réglages de build sont, dans cet ordre :

- `HomeScreen`
- `Level1`
- `Level2`

Le projet utilise notamment URP 17.3.0, Input System 1.20.0, Cinemachine 3.1.7
et AI Navigation 2.0.14. Les sources Blender originales et les fichiers `.meta`
sont conservés. Les packs importés et le package embarqué Code Assist sont
également conservés pour préserver l'environnement du projet.

## Documentation

- [Panneau de debug](GameDebugPanel.md)
- [Périmètre et vérification de la migration](Docs/Migration.md)
- [Contexte technique](Docs/AI/UnityProjectContext.md)

`Portfolio/` contient les visuels de présentation déjà présents dans la source.
Les builds et enregistrements locaux ne sont pas intégrés au dépôt source.

## Licence et ressources tierces

Le fichier `LICENSE` préexistant est conservé. Les ressources et outils tiers
restent soumis à leurs licences respectives ; leurs fichiers et notices présents
dans la source sont conservés.
