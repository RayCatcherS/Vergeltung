# Devlog Capitolo 6(Diario di sviluppo issue 26)
![Image animator](cover.png)

- IA Behaviour dei character 
<p>&nbsp;</p>
<p>&nbsp;</p>

# Stati dell'IA e implementazione Behaviour degli stati

- Nuovi stati di allerta
- Implementazione dei behaviour dei rispettivi stati del character


Il modo con cui vengono gestiti gli stati del'IA può essere rappresentato con una struttra simile agli automi a stati finiti ma senza uno stato finale. In cui le transizioni sono le chiamate a dei metodi di check che verificano se si può entrare in uno stato o meno.
(ultima posizione in cui è stato rilevato il character)

> Sia il **character NPC A** istanza di una entità character tale che non sia player

L'istanza del componente **BaseNPCBehaviour** del **character NPC A** contiene l'implementazione dei metodi di check, i metodi check alarm per essere chiamati prevedono il passaggio di una posizione o di un character che potenzialmente può innescare uno stato di allarme. Se questa verifica da un esito positivo allora si può settare un certo stato di allarme che provocherà l'esecuzione di un certo behaviour o meno. Questi metodi di check possono essere reimplementati nelle classi figlie(CivilianNPCBheaviour, EnemyNPCBehaviour), per fare dei controlli più specifici o per avere implementazioni più specifiche rispetto al ruolo del character(civile o nemico)
> Esempi di metodi check alarm
> - suspiciousCheck
> - hostilityCheck
> - receiveWarnOfSouspiciousCheck
> - suspiciousCorpseFoundCheck
> - corpseFoundConfirmedCheck

Questi metodi di check sono chiamati dall'istanza del componente FOV del **character NPC A** che rappresenta i "sensori dell'IA".

## lastSeenFocusAlarmPosition

La **lastSeenFocusAlarmPosition** è una variabile Vector3(vettore posizione) e viene aggiornata dai metodi di check e rappresenta l'ultima posizione in cui qualcosa ha innescato un certo stato del character. Questa posizione verrà usata nell'esecuzione dei behaviour:

> ### Esempio 1:
> Nel check degli stati **suspiciousCheck e hostilityCheck** la **lastSeenFocusAlarmPosition** rappresenta l'ultima posizione in cui il character è stato rilevato.s
> ### Esempio 2:
> Nel check dello stato **receiveWarnOfSouspiciousCheck** la **lastSeenFocusAlarmPosition** rappresenta la posizione in c'è stata un'attività sospetta e che deve essere raggiunta da una guardia nemica.


Questa posizione viene anche usata dai behaviour degli stati di allarme Suspicious e Hostility alert del **character NPC A** quando il character che ha provocato l'allarme è fuori dal campo visivo. Quindi il **character NPC A** proverà a raggiungere la **lastSeenFocusAlarmPosition**, ovvero l'ultima posizione in cui è stato avvistato un character ostile.

<p>&nbsp;</p>

# Implementazioni Behaviour stati
I behaviour possono implementare una specializzazione nelle classi figle (EnemyNPCBehaviour, CivilianNPCBehaviour), infatti alcuni behaviour saranno differenti in base al ruolo del character.

## SuspiciousAlert state e suspiciousAlertBehaviour 
Implementato il **suspiciousAlertBehaviour** nelle specializzazioni CivilianNPCBehaviour e EnemyNPCBehaviour behaviour dello stato del character SuspiciousAlert

I Civili e i Nemici entrano nello stato di SuspiciousAlert in base al risultato dei check spiegati nel [capitolo 4](https://github.com/RayCatcherS/Vergeltung/blob/main/DevDiary/(4)diary_issue%2310/README.md).
> Durante lo stato di **SuspiciousAlert**, viene eseguito in loop il behaviour corrispondente **suspiciousAlertBehaviour**.
> Durante il **suspiciousAlertBehaviour** il **character NPC A** cercherà di avvicinarsi al character che ha rilevato come sospetto. Se il character che ha provocato l'allarme va fuori dalla portata del FOV del
> **character NPC A**, allora il **character NPC A** cercherà di raggiungere la **lastSeenFocusAlarmPosition** (l'ultima posizione in cui è stato visto il character), altrimenti se il character che ha provocato
> l'allarme è alla portata del fov del **character NPC A** cercherà di raggiungere la posizione del character, ruotando in direzione del character che ha scatenato l'allarme.
> Quando il character che ha scatenato l'allarme è fuori portata il **character NPC A** resterà fermo nell'ultima posizione in cui è stato avvistato il character che ha scatenato l'allarme.

![Image animator](SuspiciousAlertStateBehaviour.gif)

<p>&nbsp;</p>

## HostilityAlert state e hostilityAlertBehaviour 
Implementato il **suspiciousAlertBehaviour** nelle specializzazioni CivilianNPCBehaviour e EnemyNPCBehaviour behaviour dello stato del character HostilityAlert

### Implementazione EnemyNPCBehaviour:
> Durante lo stato di **HostilityAlert**, viene eseguito in loop il behaviour corrispondente **hostilityAlertBehaviour**.
> Durante l'**hostilityAlertBehaviour** il **character NPC A** nemico cercherà di avvicinarsi al character che ha rilevato come ostile. Se il character che ha provocato l'allarme va fuori dalla portata del FOV del
> **character NPC A**, allora il **character NPC A** cercherà di raggiungere la **lastSeenFocusAlarmPosition** (l'ultima posizione in cui è stato visto il character), altrimenti se il character che ha provocato
> l'allarme è alla portata del fov del **character NPC A** cercherà di raggiungere la posizione del character, ruotando(non istantaneamente ma usando una funzione di interpolazione) nella direzione del character che ha scatenato l'allarme.
> Inoltre il character aprirà il fuoco in base all'arma selezionata
> Quando il character che ha scatenato l'allarme è fuori portata il **character NPC A** resterà fermo nell'ultima posizione in cui è stato avvistato il character che ha scatenato l'allarme.

![Image animator](EnemyHostilityAlertStateBehaviour.gif)

### Implementazione CivilianNPCBehaviour:
> Durante lo stato di **HostilityAlert**, viene eseguito in loop il behaviour corrispondente **hostilityAlertBehaviour**.
> Durante l'**hostilityAlertBehaviour** il **character NPC A** civile chiederà al controller **SceneEntitiesController** tramite un metodo di restituire la prima guardia nemica più vicina che non è impegnata in
> alcuno stato di allerta(Ovvero le guardie nemiche che sono nello stato di **Unaler** state). Il **character NPC A** civile raggiungerà la guardia nemica correndo e la notificherà passando il
> **lastSeenFocusAlarmPosition** e avviando il **receiveWarnOfSouspiciousCheck**. Il behaviour non terminerà fino a quando il civile non avrà raggiunto la guardia da notificare.

![Image animator](CivilianHostilityAlertStateBehaviour.gif)