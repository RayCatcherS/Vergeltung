# Devlog Capitolo 8(Diario di sviluppo issues 47-29-31)
![Image animator](cover.png)

- Sviluppo del sistema obiettivi di gioco
<p>&nbsp;</p>
<p>&nbsp;</p>

# Sistema gestione degli obiettivi di gioco
Per gestire gli obiettivi della partita è stata implementata la classe **"GameModeController"**. Questa classe incapsula le istanze(lista) di **"GameGoals"** classe che rappresenta un obiettivo di gioco. 
La classe **"GameGoals"** contiene: id dell'obiettivo(nome), quantità di sotto-obiettivi da portare a termine e quantità di obiettivi portati a termine.
La classe **"GameModeController"** espone un metodo(pubblico) per aggiornare lo stato di un obiettivo di gioco a cui bisognerà passare l'id dell'obiettivo di gioco e il tipo di modifica che si vuole effettuare(enum). Per qualsiasi oggetto che è in relazione con il **"GameModeController"** può aggiornare lo stato degli obiettivi.


## UI degli obiettivi
L'UI degli obiettivi è visualizzabile in alto a sinistra. La lista è visualizzabile la lista di istanze di Goal in ordine.
Per ogni goal si può selezionare una lista di icone(sprite images) configurando l'istanza di **"GameGoals"**.
Quando un goal è completo l'icona diventa un simbolo di visto.
Quando un goal è completo, viene barrato.

![Image animator](obiettivi.png)

## UI degli obiettivi in game
Per indicare esplicitamente gli oggetti che cambiano lo stato degli obiettivi sono stati implementati degli indicatori che segnalano l'obiettivo. Questi indicatori ruotano in direzione della camera principale e sono sul layer UI, in modo che vengano visualizzati dalla camera addetta alla visualizzazione della world UI. In questo modo la world UI starà sempre al di sopra di qualsiasi oggetto di gioco

![Image animator](worldUI.png)


<p>&nbsp;</p>

# GameSoundtrackController per mixare la musica di gioco in base al contesto
Implementato un controller sullo stato di gioco, che stabilisce lo stato globale del giocatore(stato sospetto, ostilità, stato di non allerta). Questo controller permette di riprodurre vari pezzi della theme music in base alla situazione di gioco(suspicious, hostility, unalert). Questo aiuta a far percepire al giocatore lo stato di pericolo in cui si trova.
## Tecnicamente
Ogni volta che un character entra in uno dei 3 stati di allerta principali (suspicious, hostility, unalert) viene aggiunto il character all'interno di un dizionario {statoAllerta(enum), instanceID(CharacterManager)}.
Dopodiché il controller avvierà un controllo su quale stato dell'animator avviare.

### Il controller in base agli stati di allerta presenti nel dizionario agirà nei seguenti modi:

- Se nel dizionario esistono solo character civili in stato di suspicious oppure hostility e nessun character nemico(in suspicious o hostility), il trigger dell'animator verrà settato su suspicious
- Se nel dizionario esiste anche solo un character guardia nemica in stato suspicious e non ci sono altri character nemici in stato di hostility, il trigger dell'animator verrà settato su suspicious
- Se nel dizionario esiste anche solo un character guardia nemica in stato di hostility, il trigger dell'animator verrà settato su hostility
- Se nel dizionario non è presente nessun elemento l'animator verrà settato su unalert

<p>&nbsp;</p>

# Vulnerabilità colpi alle spalle(esecuzione)

# Vari miglioramenti e implementazioni
- Sabotare un generatore elettrico attirerà una guardia(in stato di warn). 

![Image animator](cables.png)

- Risolti vari bugs
- Le aree vietate ai civili sono indicate dal cartello accesso vietato
- Implementato menu pausa e tasto per uscire dal gioco. Gli stati di gioco sono gestiti dal controller GameState. Quando si è in pausa il Time del gioco viene settato a 0. Essendo tutte le meccaniche basate sullo scorrimento del tempo, tutto verrà stoppato.

![Image animator](pausa.png)


## Effetti audio
- Implementato suoni dei passi dei characters, corsa e cammino. I characters controllati dal giocatore principale emetteranno dei suoni con i passi del character solo se questi correranno
- Suoni sabotaggio generatori corrente elettrica(in-out)
- Suono apertura chiusura porte(Trigger timeline animazioni)
- Effetto audio alla raccolta di items
- Suoni interazioni con le console
- Suono apertura e chiusura dei cancelli(Trigger timeline animazioni)
- Suono al cambio di un character tramite il warp

