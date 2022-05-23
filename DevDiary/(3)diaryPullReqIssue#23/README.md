# Devlog Capitolo 3(Diario di sviluppo issue 23)

![Immage cover](VergeltungCoverIssue.png)
- Bake illuminazione + Post processing
- Completamento dello scenario(edifici)
- Meccaniche sabotaggio
- Console programmabili(level design)
- Gate militari
- Avanzamento completamento level design e mappa di gioco
- Implementata gravità character

<p>&nbsp;</p>
<p>&nbsp;</p>

---

## Applicazione effetti Post processing + Bake illuminazione
Per raggiungere il concept visivo desiderato sono stati applicati effetti: Vignette, Bloom, Color Adjustments, Chromatic Aberration, Tonemapping, White Balance. Sono state inoltre settate le impostazioni per il Bake dell'illuminazione. Tutti i modelli poligonali che ho progettato ed esportato con Blender sono predisposti per il calcolo dell'illuminazione(UV Mapping)

### Baked lighting: OFF, Post processing: OFF
![Image animator](bakeOffNoPostProcessing.png)

### Baked lighting: ON, Post processing: OFF
![Image animator](VergeltungNoPostProcessing.png)

### Baked lighting: ON, Post processing: ON
![Image animator](VergeltungPostProcessing.png)

<p>&nbsp;</p>

## Nuovi edifici
Modellazione, UV Mapping, applicazione materiali

### Edificio civile 2
| ![Image animator](newBuilding1.png) | ![Image animator](newBuilding2.png) |

### Edificio militare 1
| ![Image animator](militarBuilding1.png) | ![Image animator](militarBuilding2.png) |




<p>&nbsp;</p>
<p>&nbsp;</p>

---

<p>&nbsp;</p>
<p>&nbsp;</p>



## Meccaniche di gioco e strumenti di interazione
### Sabotaggio task degli NPC
Sono state implementate delle meccaniche per sabotare i task degli NPC riutilizzando il codice già sviluppato(Interactable Objects e Weapon Item)

![Image animator](VergeltungSabotage.gif)


<p>&nbsp;</p>

### Interaction Console (level design)
Progettate le interaction console. Le interaction console sono dei gameObject che permettono di esporre delle interaction ai character o NPC della mappa(interactable objects) e di associare degli eventi alle singole interaction create. In questo esempio l'evento dell'apertura di un cancello.
Gli eventi supportati sono a singolo stato o a doppio stato, questi possono essere ripetibili o meno.

![Image animator](interactionConsole.png)

<p>&nbsp;</p>

### Gate zone militari
Progettate animazioni e stati del gate.
Sabotando/sovraccaricando i generatori elettrici verranno aperti tutti i gate delle basi militari. Questi si richiuderanno automaticamente quando tornerà l'energia elettrica(timer).

![Image animator](VergeltungGate.gif)

<p>&nbsp;</p>

### Espedienti Gate bugs
Per evitare che il player utente possa sfruttare il collider del gate per scavalacare o accedere a zone non consentite, viene azionato un collider alla chiusura del gate sposta il player oltre la chiusura del gate.

![Image animator](VergeltungSafeGate.gif)


<p>&nbsp;</p>

### Gate NavMesh Obstacle
Applicato il Navmesh Obstacle sul gate per ostacolare o liberare in tempo reale il percorso di un NavMesh Agent

![Image animator](GateNavMeshObstacle.gif)

<p>&nbsp;</p>
<p>&nbsp;</p>

---

<p>&nbsp;</p>
<p>&nbsp;</p>




## Miglioramenti

### Implementata forza di gravità character giocato
- Implementato "GroundCheck" permette tramite un raycast rilevare se il character del player "isGrounded".
- Se il player "is not Grounded" allora oltre che al vettore movimento, viene sommato anche il vettore della forza gravitazionale (Vector3.Down * 9.81)

| Senza gravità: | Con gravità: |
| ------------- |:-------------:|
| ![Image animator](VergeltungNoGravity.gif) | ![Image animator](VergeltungGravity.gif) |

### Applicati vari miglioramenti e risolti alcuni bug