
# EnginPasTangible

Moteur graphique qu'est pas croyable tellement qu'il est bien !<br>
> développé par *Victor LASSERRE* et *Valentin SERVIERES*

![Logo d'EnginPasTangible](./assets/enginpastangible.png)

---
Le projet est en cours de création, il peut y avoir des bugs.

## Installation

Pour pouvoir utiliser le moteur il vous suffit d'installer les librairies glfw.
> ### Linux

* Pour les distributions Debian :
  
  `sudo apt install libglfw3-dev`
* Pour Arch Linux :
  
  Si vous utilisez X11 (recommandé) : `sudo pacman -Sy glfw-x11`

  Si vous utilisez Wayland : `sudo pacman -Sy glfw-wayland`

Dans le repertoire de votre choix

`git clone https://git.etud.insa-toulouse.fr/serviere/EnginPasTangible.git`

`cd EnginPasTangible/`

`chmod +x run_linux.sh`

Pour compiler et exécuter le programme utilisez `./run_linux main.c`

Utilisez `git pull origin master` pour mettre à jour votre version du moteur graphique

> ### Windows & MacOs

Essayez d'adapter les méthodes montrées ci-dessus, aucune garantie que cela fonctionne (bonne chance).