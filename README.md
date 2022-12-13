# Vergeltung

Progetto Unity esame sviluppo di videogiochi

<table>
  <tr>
    <th>
        <img src="GDD/logo(Vergeltung).png" width="500"/>
    </th>
    <th>
        <img src="GDD/LogoStudio.png" width="250"/>
    </th>
  </tr>
</table>

## GDD Document
<a 
href="https://github.com/RayCatcherS/Vergeltung/blob/main/GDD/README.md" >
Documento Game Design
</a>

## Release
<a 
href="https://drive.google.com/file/d/1yh1IyQVBtu-V9M3YKwOgCYMApvnH9wm2/view?usp=sharing" >
Latest build
</a>

## Devlog - Documento Tecnico
- [2 Capitolo(Inventario e shooting)](https://github.com/RayCatcherS/Vergeltung/blob/main/DevDiary/(2)diary_issue%236-8-18-17/README.md)
- [3 Capitolo(LD)](https://github.com/RayCatcherS/Vergeltung/blob/main/DevDiary/(3)diary_issue%2323/README.md)
- [4 Capitolo(FOV e Stati di allerta)](https://github.com/RayCatcherS/Vergeltung/blob/main/DevDiary/(4)diary_issue%2310/README.md)
- [5 Capitolo(UI allerta)](https://github.com/RayCatcherS/Vergeltung/blob/main/DevDiary/(5)%20diary_issue%2325/README.md)
- [6 Capitolo(AI NPC)](https://github.com/RayCatcherS/Vergeltung/blob/main/DevDiary/(6)%20diary_issue%2326/README.md)
- [7 Capitolo(Warp)](https://github.com/RayCatcherS/Vergeltung/blob/main/DevDiary/(7)%20diary_issue%2349-28-42-43/README.md)
- [8 Capitolo(Game Goals)](https://github.com/RayCatcherS/Vergeltung/blob/main/DevDiary/(8)%20diary_issue%2347-29-31/README.md)

## Team
The whole project was developed by <b>Stefano Romanelli</b> (3D models, graphics, visual effects, level design, game mechanics, programming and project management) [``Stefano Romanelli``](https://github.com/RayCatcherS)


## Collaborazioni
The game music, the contextual musical effects in the game, and the main theme were created by [``Riccardo Buttafuoco``](https://www.instagram.com/rick._____________/)
## Note tecniche progetto
### Piattaforme di riferimento
- Windows 10
- Windows 11

## Configurazione sviluppo e requisiti
### Configurazione Blender
- Installare Blender versione 2.79
- Impostare dal sistema operativo l'eseguibile di blender 2.79 come predefinito per aprire i file .blend
- Avviare il progetto unity
- Reimportare tutti gli assets affinchè Unity riconosca i file .blend all'interno del progetto

Unity utilizzerà la versione predefinita di Blender per esportare dai .blend gli fbx. Utilizzare una versione di blender diversa dalla 2.93.2 può comportare incompatibilità e artefatti
### Requisiti
- Richiesto pad Xbox(One - Series X) o playstation(4-5)

### Problemi noti:
- La versione Unity utilizzata manda in crash l'applicazione compilata(eseguibile) una volta avviata. Per risolvere, tutti gli assets di tipo 'model' nell'inspector, nella sezione "model" devono avere la spunta attiva sulla voce "Read/Write"
