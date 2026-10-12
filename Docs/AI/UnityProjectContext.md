# Contexte Unity

<!-- unity-onboarding:generated:start -->

Analyse statique du 12 octobre 2026, source au commit `6ad4ec6`. Le nouveau
dépôt contient une copie des fichiers sélectionnés, sans l'ancien historique.

## Environnement confirmé

- Unity **6000.3.21f1**, révision `c02631ffc030`.
- URP **17.3.0**, pipeline référencé dans `ProjectSettings/GraphicsSettings.asset`.
- Input System **1.20.0**, `activeInputHandler: 1`.
- Cinemachine **3.1.7**, AI Navigation **2.0.14**, Animation Rigging **1.4.1**.
- Autres dépendances conservées : Localization 1.5.13, ProBuilder 6.1.2,
  Recorder 5.1.7, Timeline 1.8.12, uGUI 2.0.0, Visual Scripting 1.9.12.
- Package embarqué : `Packages/com.merry-yellow.code-assist`, version 1.4.22.
- Aucun chemin de package local externe dans le manifeste.

## Organisation et architecture

`Assets/_game/` contient les scripts, scènes, prefabs, ressources et créations
artistiques du jeu. `Assets/_pack/` contient les ressources importées et leurs
scripts/outils. `Portfolio/` contient les visuels de présentation.

Les scripts de jeu reposent sur des MonoBehaviour ; `GameManager` est un
singleton persistant entre scènes et conserve les joueurs/alliés.
`SceneLoadingScreen` utilise `SceneManager.LoadSceneAsync` avec un prefab
chargé depuis `Resources/UI/LoadingScreen`. `LevelTransition` relie le système
de dialogue aux transitions. Ces constats viennent des scripts, sans test en jeu.

Aucun assemblage `.asmdef` propre au gameplay repéré. Les assemblages identifiés
concernent surtout les packs tiers et les outils d'éditeur. Les exemples de code
lus utilisent des champs privés préfixés `_`, `[SerializeField]` et aucun
namespace de gameplay ; ce n'est pas une convention uniforme vérifiée.

## Scènes et validation

Ordre des scènes activées :

1. `Assets/_game/Scenes/Final/HomeScreen.unity`
2. `Assets/_game/Scenes/Final/Levels/Level1/Level1.unity`
3. `Assets/_game/Scenes/Final/Levels/Level2/Level2.unity`

D'autres scènes enregistrées sont conservées mais désactivées dans les réglages
de build. Unity Test Framework 1.6.0 est installé ; aucune suite de tests propre
au projet repérée. Aucun outil Unity MCP exposé dans cette session. Code Assist
est un outil embarqué, pas une preuve de connexion actuelle à l'éditeur.

Compilation, comportement en Play Mode, performances et cible active de l'éditeur
ne sont pas vérifiés. La validation de cette intervention porte sur la copie
des fichiers, les GUID conservés et la configuration ; voir `../Migration.md`.

## Sources examinées

`ProjectSettings/ProjectVersion.txt`, `GraphicsSettings.asset`,
`ProjectSettings.asset`, `EditorBuildSettings.asset`, `Packages/manifest.json`,
`Packages/packages-lock.json`, le manifeste Code Assist, les listes de scripts
et d'assemblages, `GameManager.cs`, `LevelTransition.cs`, `SceneLoadingScreen.cs`,
`GameDebugPanel.md` et les consignes originales du projet.

Le cadre pédagogique est repris dans `../../AGENTS.md`.

<!-- unity-onboarding:generated:end -->
