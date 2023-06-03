
# EnginPasTangible

Moteur graphique qu'est pas croyable tellement qu'il est bien !<br>
> développé par *Victor LASSERRE* et *Valentin SERVIERES*

![Logo d'EnginPasTangible](./assets/icon.png)

---
⚠️ Le projet est toujours en cours de développement, il peut y avoir des bugs. ⚠️

---
## Images issues d'EnginPasTangible

![Evolution du moteur graphique](./screenshots/evolutionEPT.gif)<br>
Evolution du moteur graphique (première version à la v1.0.0)

![Ocean](./screenshots/mer.gif)<br>
Petite animation d'océan

![Ville](./screenshots/journuitmegavillev0.1.2.gif)<br>
Cycle jour/nuit dans une ville

---
## Contrôles
Les déplacements se font en vue aérienne.<br>

### Déplacements
Avancer/Reculer : <kbd>&uarr;</kbd>/<kbd>&darr;</kbd> ou <kbd>Z</kbd>/<kbd>S</kbd><br>
Gauche/Droite : <kbd>&larr;</kbd>/<kbd>&rarr;</kbd> ou <kbd>Q</kbd>/<kbd>D</kbd><br>
Monter/Descendre : <kbd>Space</kbd>/<kbd>Left Ctrl</kbd><br>

Sprint : <kbd>Left Shift</kbd><br>
<br>

### Contrôles caméra
La souris permet de choisir la direction<br>
Zoomer dézoomer : <kbd>Scroll</kbd><br>
Activer/Désactiver la vue orthogonale : <kbd>Tab</kbd><br>
<br>
### Divers
Play/Pause : <kbd>Escape</kbd><br>
Fermer la fenêtre : <kbd>Backspace</kbd><br>
<br>

### Special
Ces commandes peuvent être utilisées dans certaines scènes uniquement
Activer/Désactiver : <kbd>C</kbd> (dans artefacts.fs)<br>
Incrémenter/Décrémenter : <kbd>B</kbd>/<kbd>N</kbd> (dans evol.fs)<br>
<b>Uniquement dans demo et le programme principal</b> : scène précédente/suivante : <kbd>F</kbd>/<kbd>J</kbd>

---

## Installation


Pour pouvoir utiliser le moteur il vous suffit d'installer les librairies glfw.
> ### Linux

* Pour les distributions Debian :
  
  `sudo apt install libglfw3-dev`
* Pour Arch Linux :
  
  Si vous utilisez X11 (recommandé) : `sudo pacman -Sy glfw-x11`<br>
  Si vous utilisez Wayland : `sudo pacman -Sy glfw-wayland`


<br>
Puis, dans le repertoire de votre choix

`git clone https://github.com/MagicTINTIN/EnginPasTangible.git`<br>
`cd EnginPasTangible/`<br>
`chmod +x run_linux.sh`

Pour compiler EnginPasTangible, executez `./compileEPT.sh EnginPasTangible.c`

Une fois votre programme compilé, vous pouvez exécuter `./ept chemin/vers/un/fichier.fs`<br>
*(plusieurs fichiers sont disponibles dans shaders/)*<br>
Vous pouvez importer plusieurs fichiers .fs (max 100), pour changer de scène il vous suffit d'appuyer sur les touches <kbd>F</kbd> ou <kbd>J</kbd>

*Pour compiler et exécuter le programme en version debug utilisez `./run_linux main.c`*
<br><br>
Utilisez `git pull origin master` pour mettre à jour votre version du moteur graphique
<br><br>

> ### Windows & MacOs

<br>
Essayez d'adapter les méthodes montrées ci-dessus, aucune garantie que cela fonctionne (bonne chance).