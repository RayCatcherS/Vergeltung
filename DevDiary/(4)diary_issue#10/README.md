# Devlog Capitolo 4(Diario di sviluppo issue 10)

- Implementato componenete "CharacterFOV". Implementa tramite un doppio Field Of View la rilevazione del player

<p>&nbsp;</p>
<p>&nbsp;</p>

---

# FOV NPC Characters
> Sia il **character NPC A** tale che non sia player

## CharacterFOV trigonometria a gogo part. 2
Sviluppato questo componente per consentire agli NPC di rilevare ed intercettare l'utente giocatore.
I due campi visivi sono caratterizzati da due parametri: raggio e angolo visione.
I due campi visivi sono rappresentati con colori differenti, la prima(giallo-ravvicinato) e la seconda(ciano-distante).
Quando il player si trova in una di questi due campi visivi, viene utilizzato un raycast dal **character NPC A** al player (raycast magenta dal secondo FOV ciano-distante, raycast rosso dal primo FOV giallo-ravvicinato), se uno dei due raycast raggiunge con successo il character del player, **character NPC A** è riuscito ad intercettare il character player, altrimenti se il raycast viene interrotto da un ostacolo non sarà possibile intercettare il player.
Queste due aree quindi servono ad intercettare il player con 2 differenti livelli.
- Se il character del player viene intercettato tramite il secondo campo visivo(ciano-distante) dal **character NPC A**, allora il **character NPC A** avvierà alcuni check che stabiliranno se valutare il character del player come sospetto o meno.
- Se il character del player viene intercettato tramite il primo campo visivo(giallo-ravvicinato) dal **character NPC A**, allora **character NPC A** avvierà alcuni check che stabiliranno se valutare il character del player come ostile o meno.

![Image animator](characterFOV.gif)

<p>&nbsp;</p>

## Visualizzazione avanzata dei FOV da editor
Sono state implementate funzioni di visual debugging GIZMOS visualizzabili nell'editor di Unity, disegnando i parametri, raggio e angolo visione ed il raycast usato dai FOV per verificare la presenza di ostacoli tra il **character NPC A** e il player. Vengono visualizzati solo quando il player innesca uno dei due FOV.
Questi strumenti di visual debugging mi sono tornati molto utile al fine di testare e verificare il funzionamento dell'IA e degli stati di allarme.
- Primo FOV giallo-ravvicinato, raycast rosso
- Secondo FOV ciano-distante, raycast magenta

![Image animator](advancedEditorFOV.gif)


<p>&nbsp;</p>


## Area di allerta characters NPC
L'area di allerta è una terza area circolare che viene utilizzata per rilevare i Characters vicini al **character NPC A** che nel caso in cui rileva attività ostile informerà  e aggiornerà istantaneamente i character vicini(Dizionario character ostili)

<p>&nbsp;</p>

# Stati di allerta e Behaviour NPC
> Sia il **character NPC A** tale che non sia player

L'attivazione di uno stato di allerta comporta l'interruzione instantanea del task che stanno eseguendo
Gli stati di allerta di un **character NPC A** sono due:
- **SuspiciousAlert**, durante questo stato il **character NPC A** eseguirà l'implementazione del comportamento dello stato di SuspiciousAlert. Le specializzazione del **character NPC A** implementa comportamenti differenti(guardie nemiche e civili).
- **HostilityAlert**, durante questo stato il **character NPC A** eseguirà l'implementazione del comportamento dello stato di HostilityAlert. Le specializzazione del **character NPC A** implementa comportamenti differenti(guardie nemiche e civili). Ad esempio il civile nello stato di HostilityAlert cercherà di raggiungere e avvisare la guardia più vicina, la guardia nemica invece attaccherà.

<p>&nbsp;</p>

## Check(controlli) affinchè vengano impostati degli stati di allerta
Quando il character del player entra in uno dei due campi visivi(FOV) del **character NPC A**, vengono avviati dei check per verificare se il character del player è sospetto, ostile oppure nessuna delle due opzioni.
- Check **"isCharacterInProhibitedAreaCheck"**. Verifica se il character del player rilevato da **character NPC A** si trovi in una area proibita //se il ruolo del character è abilitato all'accesso di una certa area
- Check **"isUsedItemProhibitedCheck"**. Verifica se il character del player rilevato da **character NPC A**  stia impugnando in modo esplicito (!weaponPuttedAway) un item non consentito(weaponItem ecc ecc)
- Check **"isCharacterWantedCheck"**. Verifica se il character del player rilevato da **character NPC A** sia ricercato o meno usando un dizionario // sistema dizionari


### Check "isCharacterInProhibitedAreaCheck"
Per realizzare la possibilità di controllore se un character si trovi in una area proibita o meno, sono state realizzate le "CharacterArea" (trigger BoxCollider) con id univoco.
ed un CharacterAreaManager assegnato ad ogni Character NPC. Il CharacterAreaManager allo spawn dell'NPC assegnerà l'id dell'area in cui l'NPC è spawnato all'NPC stesso, questo consetirà di verificare se un certo NPC appartiene o meno ad una certa area e se una certa area è proibita o meno.
Il check "isCharacterInProhibitedAreaCheck" utilizza anche il ruolo dei character. Ad esempio le guardie nemiche possono accedere a tutte le aree. Invece i civili non potranno trovarsi in aree proibite o che non sono di loro appartenenza.

<p>&nbsp;</p>

## Comunicazione degli stati di allerta
- Se il character del player resterà nel dizionario dei sospetti del **character NPC A** per più di 30 secondi allora verranno aggiornati i dizionari di tutti gli NPC della mappa con il nuovo character player dichiarato sospetto(stato di allerta sospetto).
- Se il character del player resterà nel dizionario degli ostili del **character NPC A** per più di 30 secondi allora verranno aggiornati i dizionari di tutti gli NPC della mappa con il nuovo character player dichiarato ostile(stato di allerta ostilità).

<p>&nbsp;</p>

## Comunicazione istantanea stati di allerta NPC vicini
Se altri NPC sono nell'area di allerta del **character NPC A**, e se nello stesso momento il **character NPC A** riconosce un'ostilità e quindi aggiunge il character player al dizionario degli ostili, gli NPC che sono nell'area di allerta del **character NPC A** area ricevono istantaneamente l'update sul loro dizionario degli **characters NPC OSTILI**

<p>&nbsp;</p>

## Behaviour NPC e stati di allerta
- Se il suspiciousCheck del **character NPC A** nei confronti del player è vero allora viene impostato lo stato di allerta **SuspiciousAlert** del **character NPC A** per 30 secondi(in ciclo await) ogni volta che il character player viene rilevato nel primo campo visivo(ciano) il timer del "suspicious alert" viene resettato nuovamente a 30. Per tutto il tempo che **character NPC A** sarà nello stato di "suspicious alert" seguirà il comportamento assegnato allo stato "suspicious alert". Al termine del ciclo dei 30 secondi lo stato "suspicious alert" verrà disattivato. Il comportamento prevede che si stoppi il sistema di task/activity del **character NPC A** e che il **character NPC A** segua e si avvicini al character del player

- Se l'hostilityCheck del **character NPC A** nei confronti del player è vero allora **character NPC A** va in stato **HostilityAlert** ed il character del player entra nel dizionario dei **characters NPC OSTILI**, viene attivato lo stato di allerta "hostility alert" del **character NPC A** per 30 secondi(in ciclo await) ogni volta che il character player viene rilevato nel secondo campo visivo(giallo) il timer del "hostility alert" viene resettato nuovamente a 30. Per tutto il tempo che **character NPC A** sarà nello stato di "hostility alert" seguirà il comportamento assegnato allo stato "hostility alert". Al termine del ciclo dei 30 secondi lo "hostility alert" verrà disattivato. Il comportamento prevede che si stoppi il sistema di task/activity del **character NPC A** e che il **character NPC A** segua a distanza e attacchi il character del player

Gli stati di allerta innescati sono visualizzabili con un'animazione punto esclamativo alla metal gear solid
- giallo per lo stato di allerta "SuspiciousAlert"
- rosso per lo stato di allerta "HostilityAlert"
<p>&nbsp;</p>
<p>&nbsp;</p>

---

<p>&nbsp;</p>
<p>&nbsp;</p>

# Vari fix
- Quando il character è morto ed è un player non ci sono più input sul giocatore
- Quando il character è morto ed è un player viene resettata l'UI e gli outline degli interactable objects focussati.