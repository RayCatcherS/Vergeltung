# Devlog Capitolo 6(Diario di sviluppo issue 26)
![Image animator](cover.png)

- Mega capitolo Behaviour IA dei characters
<p>&nbsp;</p>
<p>&nbsp;</p>

# Stati dell'IA e implementazione Behaviour degli stati

- Nuovi stati di allerta
- Implementazione dei behaviour dei rispettivi stati del character
- Vari miglioramenti + implementazioni minori


Il modo con cui vengono gestiti gli stati del'IA può essere rappresentato con una struttra simile agli automi a stati finiti ma senza uno stato finale. In cui le transizioni sono le chiamate a dei metodi di check che verificano se si può entrare in uno stato o meno.
(ultima posizione in cui è stato rilevato il character)

> Sia il **character NPC A** istanza di una entità character tale che non sia player

L'istanza del componente **BaseNPCBehaviour** del character contiene l'implementazione dei metodi di check, i metodi check alarm per essere chiamati prevedono il passaggio di una posizione o di un character che potenzialmente può innescare uno stato di allarme. Se questa verifica da un esito positivo allora si può settare un certo stato di allarme che provocherà l'esecuzione di un certo behaviour o meno. Questi metodi di check possono essere reimplementati nelle classi figlie(CivilianNPCBheaviour, EnemyNPCBehaviour), per fare dei controlli più specifici o per avere implementazioni più specifiche rispetto al ruolo del character(civile o nemico)
> Esempi di metodi check alarm
> - suspiciousCheck
> - hostilityCheck
> - receiveWarnOfSouspiciousCheck
> - suspiciousCorpseFoundCheck
> - corpseFoundConfirmedCheck

Questi metodi di check sono chiamati dall'istanza del componente FOV del character che rappresenta i "sensori dell'IA".

## lastSeenFocusAlarmPosition

La **lastSeenFocusAlarmPosition** è una variabile Vector3(vettore posizione) e viene aggiornata dai metodi di check e rappresenta l'ultima posizione in cui qualcosa ha innescato un certo stato del character. Questa posizione verrà usata nell'esecuzione dei behaviour:

> ### Esempio 1:
> Nel check degli stati **suspiciousCheck e hostilityCheck** la **lastSeenFocusAlarmPosition** rappresenta l'ultima posizione in cui il character è stato rilevato.s
> ### Esempio 2:
> Nel check dello stato **receiveWarnOfSouspiciousCheck** la **lastSeenFocusAlarmPosition** rappresenta la posizione in c'è stata un'attività sospetta e che deve essere raggiunta da una guardia nemica.


Questa posizione viene anche usata dai behaviour degli stati di allarme Suspicious e Hostility alert del character quando il character che ha provocato l'allarme è fuori dal campo visivo. Quindi il character proverà a raggiungere la **lastSeenFocusAlarmPosition**, ovvero l'ultima posizione in cui è stato avvistato un character ostile.

<p>&nbsp;</p>

# Implementazioni Behaviour stati
I behaviour possono implementare una specializzazione nelle classi figle (EnemyNPCBehaviour, CivilianNPCBehaviour), infatti alcuni behaviour saranno differenti in base al ruolo del character.

## SuspiciousAlert state e suspiciousAlertBehaviour 
Implementato il **suspiciousAlertBehaviour** nelle specializzazioni CivilianNPCBehaviour e EnemyNPCBehaviour behaviour dello stato del character SuspiciousAlert

I Civili e i Nemici entrano nello stato di SuspiciousAlert in base al risultato dei check spiegati nel [capitolo 4](https://github.com/RayCatcherS/Vergeltung/blob/main/DevDiary/(4)diary_issue%2310/README.md).
> Durante lo stato di **SuspiciousAlert**, viene eseguito in loop il behaviour corrispondente **suspiciousAlertBehaviour**.
> Durante il **suspiciousAlertBehaviour** il character cercherà di avvicinarsi al character che ha rilevato come sospetto. Se il character che ha provocato l'allarme va fuori dalla portata del FOV del
> character, allora il character cercherà di raggiungere la **lastSeenFocusAlarmPosition** (l'ultima posizione in cui è stato visto il character), altrimenti se il character che ha provocato
> l'allarme è alla portata del fov del character cercherà di raggiungere la posizione del character, ruotando in direzione del character che ha scatenato l'allarme.
> Quando il character che ha scatenato l'allarme è fuori portata il character resterà fermo nell'ultima posizione in cui è stato avvistato il character che ha scatenato l'allarme.
> Durante questo stato il campo visivo è maggiore.

![Image animator](SuspiciousAlertStateBehaviour.gif)

<p>&nbsp;</p>

## HostilityAlert state e hostilityAlertBehaviour 
Implementato il **suspiciousAlertBehaviour** nelle specializzazioni CivilianNPCBehaviour e EnemyNPCBehaviour behaviour dello stato del character HostilityAlert

### Implementazione EnemyNPCBehaviour:
> Durante lo stato di **HostilityAlert**, viene eseguito in loop il behaviour corrispondente **hostilityAlertBehaviour**.
> Durante l'**hostilityAlertBehaviour** il character nemico cercherà di avvicinarsi al character che ha rilevato come ostile. Se il character che ha provocato l'allarme va fuori dalla portata del FOV del
> character, allora il character cercherà di raggiungere la **lastSeenFocusAlarmPosition** (l'ultima posizione in cui è stato visto il character), altrimenti se il character che ha provocato
> l'allarme è alla portata del fov del character cercherà di raggiungere la posizione del character, ruotando(non istantaneamente ma usando una funzione di interpolazione) nella direzione del character che ha scatenato l'allarme.
> Inoltre il character aprirà il fuoco in base all'arma selezionata
> Quando il character che ha scatenato l'allarme è fuori portata il character resterà fermo nell'ultima posizione in cui è stato avvistato il character che ha scatenato l'allarme.
> Durante questo stato il campo visivo è maggiore.

![Image animator](EnemyHostilityAlertStateBehaviour.gif)

### Implementazione CivilianNPCBehaviour:
> Durante lo stato di **HostilityAlert**, viene eseguito in loop il behaviour corrispondente **hostilityAlertBehaviour**.
> Durante l'**hostilityAlertBehaviour** il character civile chiederà al controller **SceneEntitiesController** tramite un metodo di restituire la prima guardia nemica più vicina che non è impegnata in
> alcuno stato di allerta(Ovvero le guardie nemiche che sono nello stato di **Unaler** state). Il character civile raggiungerà la guardia nemica correndo e la notificherà passando il
> **lastSeenFocusAlarmPosition** e avviando il **receiveWarnOfSouspiciousCheck**. Il behaviour non terminerà fino a quando il civile non avrà raggiunto la guardia da notificare. Il character civile potrà notificare una sola guardia alla volta.
> I civili non chiameranno la guardia più vicina se riceveranno lo stato di **hostilityAlertBehaviour** in modo indotto, quindi in caso lo stato sia stato indotto i civili resteranno fermi. I civili chiameranno la
> guardia più vicina solo se hanno attivato lo stato **HostilityAlert** tramite il loro stesso FOV. Gli stati indotti vengono spiegati nel [capitolo 4](https://github.com/RayCatcherS/Vergeltung/blob/main/DevDiary/(4)diary_issue%2310/README.md#comunicazione-istantanea-stati-di-allerta-npc-vicini)
> Durante questo stato il campo visivo è maggiore.

![Image animator](CivilianHostilityAlertStateBehaviour.gif)


<p>&nbsp;</p>

## SuspiciousCorpseFoundAlert state e suspiciousCorpseFoundAlertBehaviour
Questo è un nuovo stato di allerta e ha meno priorità degli stati **HostilityAlert** e **SuspiciousAlert**, vuol dire che questo stato viene terminato dando priorità a stati come
**HostilityAlert** e **SuspiciousAlert**.
> Questo behaviour rappresenta il sospetto di aver trovato un cadavere.
> Durante lo stato di **SuspiciousCorpseFoundAlert**, viene eseguito in loop il behaviour corrispondente **suspiciousCorpseFoundAlertBehaviour**.
> Durante l'**suspiciousCorpseFoundAlertBehaviour** i character civili e nemici cercheranno di raggiungere il cadavere correndo utilizzando la **lastSeenFocusAlarmPosition** che corrisponderà alla posizione in cui è
> stato trovato morto il character.

![Image animator](suspiciousCorpseFoundAlertBehaviour.gif)


<p>&nbsp;</p>

## CorpseFoundConfirmedAlert state e corpseFoundConfirmedAlertBehaviour
Questo è un nuovo stato di allerta e ha meno priorità degli stati **HostilityAlert** e **SuspiciousAlert**, vuol dire che questo stato viene terminato dando priorità nel caso si attivino gli stati
**HostilityAlert** e **SuspiciousAlert**.

### Implementazione EnemyNPCBehaviour:
> Questo behaviour rappresenta lo stato di allarme dell'aver confermato la presenza di un cadavere.
> Durante lo stato di **CorpseFoundConfirmedAlert** del character nemico si avvicinerà al cadavere per qualche secondo per poi tornare allo stato di Unalert.
> Durante questo stato il campo visivo è maggiore.

![Image animator](EnemycorpseFoundConfirmedAlertBehaviour.gif)

### Implementazione CivilianNPCBehaviour:
Questo behaviour rappresenta lo stato di allarme dell'aver confermato la presenza di un cadavere.
> Durante lo stato di **CorpseFoundConfirmedAlert** il character civile chiederà al controller **SceneEntitiesController** tramite un metodo di restituire la prima guardia nemica più vicina che non è impegnata in
> alcuno stato di allerta(Ovvero le guardie nemiche che sono nello stato di **Unaler** state). Il character civile raggiungerà la guardia nemica correndo e la notificherà passando il
> **lastSeenFocusAlarmPosition** e avviando il **receiveWarnOfSouspiciousCheck**. Il behaviour non terminerà fino a quando il civile non avrà raggiunto la guardia da notificare. Il character civile potrà notificare
> una sola guardia alla volta.

![Image animator](CivilianCorpseFoundConfirmedAlert.gif)


<p>&nbsp;</p>

## WarnOfSuspiciousAlert state e warnOfSouspiciousAlertBehaviour
Questo è un nuovo stato di allerta e ha meno priorità degli stati **HostilityAlert** e **SuspiciousAlert**, vuol dire che questo stato viene terminato dando priorità a stati come
**HostilityAlert** e **SuspiciousAlert**.

Questo behaviour viene attivato quando il character nemico riceve un warn da un civile il cui behaviour è quello dell'**HostilityAlert** o dell'aver confermato la presenza di un cadavere (**CorpseFoundConfirmedAlert**). Nello stato di **WarnOfSuspiciousAlert** il nemico ricevere una posizione dal civile (**lastSeenFocusAlarmPosition**), questa posizione verrà raggiunta di corsa dal character nemico, simulando uno stato di indagine. Alla scadenza del timer del behaviour il character nemico tornerà nello stato di unalert.
Per adesso questo behaviour è implementato solo dalle guardie.

![Image animator](WarnOfSuspiciousAlert.gif)








<p>&nbsp;</p>

# Miglioramenti vari:
- Tutti gli oggetti che offrono delle interazioni(interactableObjects) sono indicati con questo shader programmato tramite lo strumento **Shader Graph** di Unity. In questo modo sarà più intuitivo capire con quali oggetti è possibile interagire.

| ![Image animator](InteractableObj.gif) | ![Image animator](shaderGraph.gif) |

- Alcune porte si potranno aprire solo se si possiede una certa chiave
- Le porte chiuse a chiave possono essere aperte senza scassinarle da un certo lato(configurabile)
- Gli agenti possono avere 3 opzioni per il movimento camminata lenta, camminata normale, corsa(sono anche configurate le rispettive animazioni)
- Gli NPC quando eseguono un task ruotano la loro direzione in direzione del task(rotazione interpolata non diretta)
- I vetri delle abitazioni possono essere rotti con i proiettili, questo permetterà al giocatore di fuggire dalle finestre nel caso in cui si dovesse improvvisare una fuga.

![Image animator](brokenGlass.gif)

- Migliorata la propagazione istantanea di uno stato di allerta ai character vicini. La propagazione avviene solo se un raggio partendo dal character *A* riesce ad arrivare al character *B*. In altre parole se il linecast arriva fino a *B* e non vengono rilevati ostacoli tra *A* e *B*, allora verrà comunicato lo stato di allerta. Previene le situazioni in cui viene propagato lo stato di allerta anche a NPC che sono fuori dall'edificio e che non potrebbero avere appunto delle comunicazioni. Come si può vedere dall'animazione il segnale dall'arme non viene propagato oltre gli ostacoli.

![Image animator](InstantAlert.gif)


<p>&nbsp;</p>

# Schermate UI:
Implementati menu + navigazione fra menu.
## Menu principale, Impostazioni e storia
Nel menù principale si potranno settare le impostazioni grafiche. Puoi scrollare la storia prima di iniziare a giocare.

![Image animator](mainMenu.png)

![Image animator](settings.png)

![Image animator](story.png)

## Schermata di game over
Dalla schermata di game over puoi ricominciare il gioco oppure tornare al menu principale.

![Image animator](gameOver.png)
