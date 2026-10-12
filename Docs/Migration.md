# Migration du 12 octobre 2026

## Provenance

- Source locale : `Necromancer x Magical Girl/lyo-3g-gameboy-s1p1-06`.
- Dernier commit source : `6ad4ec6` (`[CLEAN] scenes`), branche `main`.
- La source était propre et possédait un commit local non publié.
- Destination : `gto634/Necromancer-x-MagicalGirl`, branche `main`.
- Le commit initial `17b3d17`, le fichier `LICENSE` et les règles `.gitignore`
  existants de la destination sont préservés.
- L'historique et le dossier `.git` de la source ne sont pas copiés. GitHub
  identifie la destination comme un dépôt indépendant (`fork: false`).

La migration copie l'état actuel des fichiers enregistrés, y compris le commit
local non publié. Le dépôt source reste intact pour conserver son historique,
ses builds et ses autres fichiers locaux.

## Fichiers conservés

`Assets/`, `Packages/`, `ProjectSettings/`, `Portfolio/`, `.vsconfig` et
`GameDebugPanel.md` sont copiés. Les scripts, scènes de travail et finales,
prefabs, textures, modèles, sources `.blend`, ressources des packs et métadonnées
Unity sont conservés. Le manifeste et le verrouillage des packages sont copiés
sans changement, ainsi que le package embarqué Code Assist.
Ses symboles de débogage optionnels `.pdb` et leurs `.meta` ne sont pas copiés ;
ses assemblages `.dll`, scripts et métadonnées utiles sont conservés.

Les packs ou scènes non utilisés par les trois scènes de build ne sont pas
supprimés : une analyse d'utilisation en jeu n'a pas été réalisée, et ils
peuvent rester utiles pour le développement. Les licences et notices tierces
présentes sont conservées avec leurs ressources.

## Fichiers laissés dans la source

| Éléments | Raison |
| --- | --- |
| `.git/` de la source | Historique propre à l'ancien dépôt |
| `Library/`, `Logs/`, `obj/`, `UserSettings/`, `.vs/` | Caches et réglages locaux régénérables |
| `Build/`, `Recordings/` | Livrables et captures locaux, distincts des sources |
| `*.csproj`, `*.sln` | Fichiers IDE générés par Unity |
| `Temp.txt` | Fichier temporaire sans contenu utile |
| `*.blend1` et leurs `.meta` | Sauvegardes Blender ; les `.blend` originaux restent présents |
| `Assets/Unity.VisualScripting.Generated/` et son `.meta` | Cache Visual Scripting régénérable |
| Symboles `.pdb` de Code Assist et leurs `.meta` | Symboles de débogage optionnels, déjà ignorés par Git |
| Ancien `AGENTS.md` interne | Instantané Code Assist de l'ancien projet |

Les documents scolaires et dossiers de travail situés à côté de la source ne
font pas partie de ce transfert. Aucun fichier de la source n'est supprimé.

## Vérification

La copie sélectionnée comprend **5 246 fichiers**, soit **316 260 470 octets**
(environ 302 Mio), avant l'ajout de cette documentation.

La comparaison SHA-256 a validé les **5 246 fichiers** sans différence par
rapport à la source. Les trois scènes activées sont présentes et leurs `.meta`
correspondent aux GUID des réglages de build. Le manifeste et le verrouillage
JSON sont lisibles, et le package embarqué est présent. Aucun asset de jeu
de `Assets/_game/` n'est dépourvu de son `.meta`. Les caches, sauvegardes et
symboles exclus ne sont pas dans le nouveau dépôt.

Cette validation concerne l'intégrité des fichiers et de la configuration.
Aucune ouverture Unity, compilation, exécution du jeu ou génération d'un nouveau
build n'est effectuée pendant la migration.
