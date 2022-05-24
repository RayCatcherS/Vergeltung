# Devlog Capitolo 4(Diario di sviluppo issue 10)

- Implementato componenete "CharacterFOV". Implementa tramite un doppio Field Of View la rilevazione del player

<p>&nbsp;</p>
<p>&nbsp;</p>

---

## FOV NPC Characters
> Sia il **character NPC A** tale che non sia player

> Siano i due dizionari di ID dei character: **characters NPC SOSPETTI** e **characters NPC OSTILI** contenuti nei character NPC

### CharacterFOV trigonometria a gogo part. 2
Sviluppato questo componente per consentire agli NPC di rilevare ed intercettare l'utente giocatore.
I due campi visivi sono caratterizzati da due parametri: raggio e angolo visione.
I due campi visivi sono rappresentati con colori differenti, la prima(giallo) e la seconda(ciano).
Quando il player si trova in una di questi due campi visivi, viene sparatato un raycast dall'NPC al player (raycast magenta FOV 2, raycast rosso FOV 1), se uno dei due raycast raggiunge con successo il player, il player allora è stato intercettato dall'NPC, altrimenti c'è un ostacolo e non è possibile intercettare il player.
Queste due aree quindi servono ad intercettare il player con 2 differenti livelli.
Sia il **character NPC A** tale che non sia player
- Se il player viene intercettato tramite il primo campo visivo(ciano) dal **character NPC A**, allora il **character NPC A** in base ad alcuni check stabilirà se far entrare l'ID del character del player nel dizionario degli **characters NPC SOSPETTI**. Se il character del player entra nel dizionario dei **characters NPC SOSPETTI**.
- Se il player viene intercettato tramite il secondo campo visivo(giallo) dal **character NPC A**, allora **character NPC A** in base ad alcuni check stabilirà se far entrare l'ID del character del player nel dizionario degli **characters NPC OSTILI**. Se il character del player entra nel dizionario dei **characters NPC OSTILI**.

![Image animator](characterFOV.gif)

<p>&nbsp;</p>



### Area di allerta characters NPC
L'area di allerta viene utilizzata per rilevare i Characters vicini all'NPC e nel caso informarli o aggiornali istantaneamente su eventuali eventi

<p>&nbsp;</p>

## Gestione stati di allerta e Behaviour NPC
> Sia il **character NPC A** tale che non sia player

> Siano i due dizionari di ID dei character: **characters NPC SOSPETTI** e **characters NPC OSTILI** contenuti in tutti i character NPC

### Comunicazione stati di allerta (livello locale e globale)
- Se il character del player è un sospetto per **character NPC A** e se il character del player **character NPC A** cercherà di avvicinarsi al character player. Se il character del player resterà nel dizionario dei sospetti del **character NPC A** per più di 30 secondi allora verranno aggiornati i dizionari di tutti gli NPC della mappa con il nuovo character player dichiarato sospetto(stato di allerta sospetto).
- Se il character del player entra nel dizionario dei character ostili del **character NPC A**, il **character NPC A** cercherà di attaccare il character player. Se il character del player resterà nel dizionario degli ostili del **character NPC A** per più di 30 secondi allora verranno aggiornati i dizionari di tutti gli NPC della mappa con il nuovo character player dichiarato ostile(stato di allerta ostilità).

<p>&nbsp;</p>

### Comunicazione istantanea stati di allerta NPC vicini
Se altri NPC sono nell'area di allerta del **character NPC A**, e se nello stesso momento il **character NPC A** riconosce un'ostilità e quindi aggiunge il character player al dizionario degli ostili, gli NPC che sono nell'area di allerta del **character NPC A** area ricevono istantaneamente l'update sul loro dizionario degli **characters NPC OSTILI**

<p>&nbsp;</p>

### Behaviour NPC e stati di allerta
- Se il character del player entra nel dizionario dei **characters NPC SOSPETTI**, viene attivato lo stato di allerta "suspicious alert" del **character NPC A** per 30 secondi(in ciclo await) ogni volta che il character player viene rilevato nel primo campo visivo(ciano) il timer del "suspicious alert" viene resettato nuovamente a 30. Per tutto il tempo che **character NPC A** sarà nello stato di "suspicious alert" seguirà il comportamento assegnato allo stato "suspicious alert". Al termine del ciclo dei 30 secondi lo stato "suspicious alert" verrà disattivato. Il comportamento prevede che si stoppi il sistema di task/activity del **character NPC A** e che il **character NPC A** segua e si avvicini al character del player()

- Se il character del player entra nel dizionario dei **characters NPC OSTILI**, viene attivato lo stato di allerta "hostility alert" del **character NPC A** per 30 secondi(in ciclo await) ogni volta che il character player viene rilevato nel secondo campo visivo(giallo) il timer del "hostility alert" viene resettato nuovamente a 30. Per tutto il tempo che **character NPC A** sarà nello stato di "hostility alert" seguirà il comportamento assegnato allo stato "hostility alert". Al termine del ciclo dei 30 secondi lo "hostility alert" verrà disattivato. Il comportamento prevede che si stoppi il sistema di task/activity del **character NPC A** e che il **character NPC A** segua a distanza e attacchi il character del player()

<p>&nbsp;</p>
<p>&nbsp;</p>

---

<p>&nbsp;</p>
<p>&nbsp;</p>
