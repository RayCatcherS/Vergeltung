# GDD Document
<table>
  <tr>
    <th>
        <img src="logo(Vergeltung).png" width="500"/>
    </th>
    <th>
        <img src="LogoStudio.png" width="250"/>
    </th>
  </tr>
</table>

<a title="https://github.com/RayCatcherS/Vergeltung"
href="https://github.com/RayCatcherS/Vergeltung" target="_blank" >
Link Progetto
</a>

&emsp;



# Indice  
### 1. [Introduzione](#1)  
 1. [Elevator Pitch](#1.1)  
 2. [Team](#1.2)

### 2. [Personaggi](#2) 

### 3. [Storia](#3) 

### 4. [Gameplay](#4) 
 1. [Obiettivi](#4.1)  
 2. [Abilità del giocatore](#4.2)
 3. [Meccaniche di gioco](#4.3)
 4. [Oggetti e armi](#4.4)
 5. [Progressione e sfida](#4.5)
 6. [Sconfitta](#4.6)

### 5. [Art Style](#5) 

### 6. [Musica e Suoni](#6) 

### 7. [Dettagli Tecnici](#7)
  2. [Capitolo diario di sviluppo 2 (Inventario e shooting)](#7.2) 
  3. [Capitolo diario di sviluppo 3 (Level Design)](#7.3) 
  4. [Capitolo diario di sviluppo 4 (FOV e Stati di allerta)](#7.4) 
  5. [Capitolo diario di sviluppo 5 (UI allerta)](#7.5)
  6. [Capitolo diario di sviluppo 6 (AI NPC)](#7.6)

### 8. [Mercato](#8)

### 9. [Idee](#9)


# <span id = "1">1. Introduzione</span> 
## <span id = "1.1">1.1 Elevator Pitch
Vergeltung è un gioco di genere `Stealth` con elementi `Top-Down Shooting` ambientato in universo distopico in cui le forze naziste hanno vinto la guerra usando delle tecnologie antiche.
I nazisti hanno addestrato i "cacciatori", forze armate specializzate nel cercare e sterminare i detentori di queste antiche tecnologie, rubandone tutte le tecnologie.
I dententori sono famiglie che tramandano la conoscenza di queste tecnologie nella segretezza più totale. Il protagonista è un giovane detentore che dopo aver subito l'abuso e l'omicidio dell'intera famiglia, riesce a sopravvivere fuggendo. Le forze nemiche attribuiranno una taglia
sulla testa del protagonista che durante la fuga deciderà di vendicarsi.
Il giocatore potrà compiere la vendetta del protagonista, infiltrandosi in uno dei quartieri cardine della fazione nemica dove gli assassini cacciatori della sua famiglia presidiano la zona.
Il protagonista dovrà fuggire se verrà scoperto cercando di salvare la pelle in ogni modo possible(fuggendo dalle finestre oppure confondendo i nemici).
Il protagonista dovrà riuscire ad appropiarsi degli strumenti nemici, piegando l'ambiente circostante contro gli stessi nemici, infiltrandosi tra i
vari edifici(civili e non) senza farsi scoprire,  raggiungendo le zone strategiche, mentre eserciti di nemici marciano per le strade.
Le zone strategiche saranno di vari livelli di difficoltà e saranno particolarmente militarizzate. Il protagonista scoprirà che queste zone strategiche,
sorvegliano l'accesso a dei monoliti realizzati con una tecnologia sconosciuta. Questi monoliti consentiranno di prendere fisicamente il controllo degli altri esseri umani. Il protagonista userà questa tecnologia per usare i civili e i soldati(anche in modo amorale) per costruire ed improvvisare la propria vendetta, sabotando ed utilizzando armi e dispositivi della fazione nemica.


Il giocatore conquistando le varie zone strategiche e accedendo ai monoliti, potrà creare una catena di controllo partendo dal protagonista,
permettendogli di sfruttare i character controllati come "vite sacrificabili" per il compimento della vendetta. Il gioco terminerà con una vittoria
quando il protagonista completerà tutti gli obiettivi. Oppure il gioco terminerà con una sconfitta nel caso in cui il protagonista morisse.



## <span id = "1.2">1.2 Team
Stefano Romanelli, sviluppo app e videogames, ho avute molte esperienze di sviluppo con l'engine unity e tool come
blender(modelli 3D e animazioni), photoshop che mi hanno permesso di creare una demo concettuale del gioco.
Progetti passati:
- Un serious game sviluppato per una startup(IOS - Android): https://apps.apple.com/it/app/my-clinical-trial-center/id1576341842
- Piattaforma web(Flutter web / Firebase) per organizzare eventi online: https://celebrateit.it/#/
- Trailer e illustrazioni di concept demo: https://www.instagram.com/ray_catcher/?utm_medium=copy_link



# <span id = "2">2. Personaggi</span>
Il protagonista è l'ultimo detentore dei segreti sulle antiche tecnologie, segreti che se entrassero nelle mani dei nemici renderebbe loro invinvibili.

# <span id = "3">3. Storia</span>
Il protagonista è un giovane detentore che dopo aver subito l'abuso e l'omicidio dell'intera famiglia, riesce a sopravvivere fuggendo. Le forze nemiche attribuiranno una taglia
sulla testa del protagonista che durante la fuga deciderà di vendicarsi.
Il giocatore potrà compiere la vendetta del protagonista, infiltrandosi in uno dei quartieri cardine della fazione nemica dove gli assassini cacciatori della sua famiglia presidiano la zona.

# <span id = "4">4. Gameplay</span>
  ## <span id = "4.1">4.1 Obiettivi</span>
  Gli obiettivi saranno vari, dall'uccidere i cacciatori al sabotare i monoliti o teconologie antiche rubate ai detentori. 
  
  
  ## <span id = "4.2">4.2 Abilità del giocatore</span>
  Il giocatore potrà prendere il controllo dei nemici o dei civili tramite l'abilità "controllo mentale", controllando fisicamente gli altri characters e usando le loro armi o i loro strumenti per accedere a zone che altrimenti sarebbero vietate.
  
  
  ## <span id = "4.3">4.3 Meccaniche di gioco</span>
  L'abilità del controllo mentale è la meccanica che consente di poter controllare characters i cui loro ruoli permettono l'accesso a zone altrimenti vietate. I civili controllati però non potranno farsi scoprire dalle guardie nemiche nell'utilizzare armi, perchè il loro ruolo non sarebbe compatibile con l'utilizzo delle armi. L'abilità del controllo richiederà delle munizioni speciali riscattabili disattivando i monoliti energetici. Il controllo di uno specifico ruolo avrà un costo in munizioni specifico(nemici: 2, civili: 1).
  La meccanica per il controllo mentale permetterà di accedere all'inventario del character controllato ad esempio delle armi.
  Questo permetterà di usare i characters controllati come vite a disposizione. Se il protagonista fa muorire un character controllato torna a controllare quello precedente e così via fino al personaggio principale.
  Il resto delle meccaniche sono spiegate all'interno dei devlogs:
  
  - [Capitolo diario di sviluppo 2 (Inventario e shooting)](#7.2) 
  - [Capitolo diario di sviluppo 3 (Level Design)](#7.3) 
  - [Capitolo diario di sviluppo 4 (FOV e Stati di allerta)](#7.4) 
  - [Capitolo diario di sviluppo 5 (UI allerta)](#7.5)
  - [Capitolo diario di sviluppo 6 (AI NPC)](#7.6)
  
 
  ## <span id = "4.5">4.5 Progressione e sfida</span>
  La progressione sarà data dal riuscire a raggiungere zone prima non accessibili, muovendosi in modo furtivo senza destare sospetti, rubando munizioni per il controllo mentale infiltrandosi nelle basi e utilizzandole sui characters per improvvisare una strategia per portare a termine gli obiettivi.
  ## <span id = "4.6">4.6 Sconfitta</span>


# <span id = "5">5. Art Style</span>
  Lo stile artistico è "Low Poly" scelto per questioni di semplicità realizzative, in questa demo vengono tutti realizzati da una persona, me.

# <span id = "6">6. Musica e Suoni</span>
  I suoni saranno importanti per riconoscere i nemici in avvicinamento o per riconoscere che un certo nemico è in uno stato sospetto oppure ostile.
  

# <span id = "7">7. Dettagli Tecnici</span>
  ### <span id = "7.2">7.2 [2 Capitolo (Inventario e shooting)](https://github.com/RayCatcherS/Vergeltung/blob/main/DevDiary/(2)diary_issue%236-8-18-17/README.md)</span>
  ### <span id = "7.3">7.3 [3 Capitolo (LD)](https://github.com/RayCatcherS/Vergeltung/blob/main/DevDiary/(3)diary_issue%2323/README.md)</span>
  ### <span id = "7.4">7.4 [4 Capitolo (FOV e Stati di allerta)](https://github.com/RayCatcherS/Vergeltung/blob/main/DevDiary/(4)diary_issue%2310/README.md)</span>
  ### <span id = "7.5">7.5 [5 Capitolo (UI allerta)](https://github.com/RayCatcherS/Vergeltung/blob/main/DevDiary/(5)%20diary_issue%2325/README.md)</span>
  ### <span id = "7.5">7.5 [6 Capitolo (AI NPC)](https://github.com/RayCatcherS/Vergeltung/blob/main/DevDiary/(6)%20diary_issue%2326/README.md)</span>

# <span id = "8">8. Mercato</span>



# <span id = "9">9. Idee</span>
(possibili idee meccaniche sperimentali)
- I nemici agiscono in modo differente comprendendo il tuo stile di gioco
- I nemici sono capaci di riconoscere l'ostilità anche di un proprio alleato, in base ad una certa sequenza di eventi.
- I livelli di gioco sono generati in modo procedurale
