
# EnginPasTangible

Moteur graphique qu'est pas croyable tellement qu'il est bien !<br>
> développé par *Victor LASSERRE* et *Valentin SERVIERES*

![Logo d'EnginPasTangible](./assets/icon.png)

---
⚠️ Le projet est en cours de création, il peut y avoir des bugs. ⚠️

---
## Images issues d'EnginPasTangible

### V0.2.2

![Capture d'écran de la toute première version](./screenshots/v0.2.2.png)<br>
Capture d'écran de la toute première version d'EnginPasTangible (v0.2.2)

### V0.2.5

![Capture d'écran de la première version avec une source lumineuse](./screenshots/EnginPasTangiblev0.2.5.gif)<br>
Rotation autour du centre contrôlée par la souris (v0.2.5)

### V0.4.2

![Capture d'écran des premiers essais des ombres](./screenshots/EnginPasTangiblev0.4.2.gif)<br>
Cycle jour nuit dans une scene avec des tours et déplacements clavier souris (v0.4.2)

### V0.5.1

![Capture d'écran d'une scène complexe avec plusieurs couleurs et des réflexions](./screenshots/EnginPasTangiblev0.5.1.gif)<br>
Ajout des couleurs d'objets avec réflexions (v0.5.1)

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
<b>Uniquement dans demo</b> : scène précédente/suivante : <kbd>F</kbd>/<kbd>J</kbd>

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
Puis, Dans le repertoire de votre choix

`git clone https://github.com/MagicTINTIN/EnginPasTangible.git`<br>
`cd EnginPasTangible/`<br>
`chmod +x run_linux.sh`

Pour compiler et exécuter le programme utilisez `./run_linux main.c`
<br><br>
Utilisez `git pull origin master` pour mettre à jour votre version du moteur graphique
<br><br>

> ### Windows & MacOs

<br>
Essayez d'adapter les méthodes montrées ci-dessus, aucune garantie que cela fonctionne (bonne chance).