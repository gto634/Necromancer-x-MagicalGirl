# Panneau de debug

En mode Play, cliquer dans la vue Game puis appuyer sur **F3**, ou cliquer sur
**Debug [F3]** en haut à droite. F3 ou Échap referme le panneau.
Le jeu est mis en pause et les commandes des joueurs sont bloquées pendant son
utilisation. Fermer le panneau rétablit la vitesse précédente et les commandes.

Depuis HomeScreen, choisir NEC seul, MG seule ou Duo, puis **Level2 direct**.
Le solo fonctionne au clavier ou à la manette. Le duo demande un clavier et une
manette, ou deux manettes. Dans une partie en cours, les joueurs et leurs
périphériques sont conservés par défaut. Décocher cette option pour faire une
nouvelle sélection.

Les boutons Level1, Level2 direct, Recharger et Accueil utilisent le chargement
existant. La conservation des sbires est activée par défaut et peut être décochée.
Les PV sont réinitialisés par les personnages au chargement d'une scène ; les
options d'invincibilité du panneau restent actives entre niveaux pendant la session.

Choisir Necromancer ou Magical Girl pour gérer :

- les PV exacts, les PV maximum et des dégâts de test de 10 PV ;
- l'invincibilité du joueur ;
- l'ajout et le retrait de sbires, ou le retrait de toute l'armée ;
- les PV et l'invincibilité des sbires actuels et futurs.

Les ajouts sont limités à 100 sbires par clic et nécessitent un NavMesh valide.
Ils passent par PlayerCommander.Convert pour obtenir le bon propriétaire et
la bonne IA. Le retrait libère aussi les éventuelles réservations d'interaction.

L'invincibilité ignore les dégâts normaux. Le bouton Appliquer permet néanmoins
de forcer les PV pour un test. Mettre les PV à zéro déclenche les conséquences
normales de la mort. Après une mort en PvP, recharger le niveau pour réinitialiser
le personnage et ses effets de mort.

Le panneau se crée automatiquement depuis
`Assets/_game/Resources/Debug/GameDebugPanel.prefab`. Ses références et la touche
d'ouverture se règlent dans ce prefab ; rien n'est à ajouter aux scènes.
Le script principal est `Assets/_game/Scripts/Ui/GameDebugPanel.cs`.

Il est actif dans l'éditeur et les **Development Builds**. Il ne se lance pas
dans un build normal, et la protection de debug contre les dégâts est exclue
de ces builds.
