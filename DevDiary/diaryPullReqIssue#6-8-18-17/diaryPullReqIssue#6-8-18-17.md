# Diary di sviluppo issue 6-8-18-17

## Progettazione classi
- Implementate classe generalizzate: InventoryItem e classi figlie (WeaponItem, ActionObjectItem)
- La classe InventoryItem eredita le funzionalità della classe Interactable, permettendo di gestire le interazioni con le istanze di InventoryItem
- implmentata classe InventoryManager, componente dei character, implementata per gestire l'inventario del character
- Anche InventoryManager eredita le funzionalità della classe Interactable(permette di interagire con gli inventari dei character morti e quindi di accedere al loro inventario)
- Implementata raccolta degli Items tramite l'InventoryManager
![Immage classi](classScheme.JPG)

<p>&nbsp;</p>

### Specializzazioni Weapons
implementate tipologie di armi. Servono ad azionare animazioni o comportamenti del character differenti in base al tipo di arma usata
<pre><code>public enum WeaponType{
    melee,
    pistol,
    rifle,
    granade
}</code></pre>




<p>&nbsp;</p>
<p>&nbsp;</p>
<p>&nbsp;</p>

---

<p>&nbsp;</p>
<p>&nbsp;</p>
<p>&nbsp;</p>


## Animazioni e rigging
### Stati animator + blendTree stati
> Progettato animator a stati, serve a switchare l'animazione del character in base al tipo di arma impugnata. Gli stati vengono gestiti dall'InventoryMangare del character

![Image animator](animatorStatiCharacter.png)
![Image animator](animatorGif.gif)


<p>&nbsp;</p>

### Rigging character
Usato sistema di rigging di Unity. Permette di configurare le articolazioni di uno scheletro mentre è in corso una certa animazione sullo scheletro.
Questa funzionalità consente di tenere fisse le armi ad una "ancora" da cui saranno fisse e potranno sparare dalle posizioni prestabilite.
Inoltre le articolazioni del character seguiranno le ancore dell'arma.

| Esempio senza Rig: | Esempio con Rig: |
| ------------- |:-------------:|
| ![Image animator](SenzaRig1.gif) | ![Image animator](ConRig1.gif) |


| Esempio 2 senza Rig: | Esempio 2 con Rig: |
| ------------- |:-------------:|
| ![Image animator](SenzaRig2.gif) | ![Image animator](ConRig2.gif) |







<p>&nbsp;</p>
<p>&nbsp;</p>
<p>&nbsp;</p>

---

<p>&nbsp;</p>
<p>&nbsp;</p>
<p>&nbsp;</p>






## Implementazione sistema shooting dei characters
### Inventario
Implementato inventario character. Adesso è possibile raccogliere gli oggetti/armi(InventoryItem) tramite le funzionalità degli oggetti/armi stessi che ereditano le funzioni della classe Interactable. Gli Item che implementano le funzionalità della classe Interactable mettono a disposizioni delle interacions(eventi). Gli eventi delle interactions implementati vanno ad interagire con l'InventoryManager del character che ha interagito con quel determinato InventoryItem.

![Image animator](Inventario.gif)

<p>&nbsp;</p>

### Outline character mirati
Quando il character player(controllato dall'utente) mira un character viene visualizzato un outline sulla sua mesh. Consente di distinguere le varie categorie dei character(civili, nemici)

![Image animator](OutlineCharacterMirati.jpg)

<p>&nbsp;</p>

### Shooting weapons, fisica proiettile, effetti particellari (trigonometria a gogo)
- Implementato shooting delle armi
- Il proiettile ha componente con parametri sul danno, velocità e direzione. Il proiettile si muove tramite la fisica dell'engine in FixedUpdate verso la direzione configurata(direzione shooting arma).
- Essendo il calcolo degli eventi basati sulla fisica, se il proiettile è troppo rapido non rileva la maggior parte delle collisioni (effetto teletrasporto oltre i collider), per evitare problemi di collisioni basati sulla bassa frequenza di aggiornamento della fisica, è stato implementato un metodo per cui il proiettile calcola per ogni frame un rayCast nella posizione in avanti di qualche unità in più rispetto a dove si trova il proiettile, prevedendo le collisioni sul cammino.
- L'arma genera dei particles durante lo shooting (usato particle system di Unity)
- Il proiettile genera dei particles in base alla superficie collisa (usato particle system di Unity)

![Image animator](ShootingWeaponsFisicaproiettileEffettiparticellari.gif)

<p>&nbsp;</p>

### Metodo gunThroughWall
Il metodo gunThroughWall evita la possibilità che l'arma attraversando il muro possa far sparare dei colpi, attraversando i collider. Un raycast parte dalla testa del character e raggiunge l'arma. Se in questo raycast viene incontrato un hit dal raycast allora l'arma è entrata attraverso un muro

![Image animator](gunThroughWall.gif)






<p>&nbsp;</p>
<p>&nbsp;</p>
<p>&nbsp;</p>

---

<p>&nbsp;</p>
<p>&nbsp;</p>
<p>&nbsp;</p>





## Nuovi stati character
### Danno, health, death e attivazione ragdoll del character
- Aggiunta la vita, il danno e lo stato di morte
- Utilizzato tool unity per trasformare lo scheletro in ragdoll(collisioni e rigidbody sui bone del character vengono attivati quando il character muore)

![Image animator](Damage.gif)

<p>&nbsp;</p>

### Configurazione inventario player e dei character
Implementata funzionalità che consente di scegliere l'equipaggiamento del character o del player che spawnerà da un certo Spawn Point(tool level design Editor Unity)

![Image animator](equipaggiamentoCharacterSpawn.png)

<p>&nbsp;</p>

### Implementato accesso all'inventario del character una volta che è morto
Il player potrà ottenere una lista di interactaction che l'InventoryManager offrirà, ad esempio la lista di armi che si possono ottenere da quell'InventoryManager

<p>&nbsp;</p>
<p>&nbsp;</p>
<p>&nbsp;</p>

---

<p>&nbsp;</p>
<p>&nbsp;</p>
<p>&nbsp;</p>



### Varie funzionalità aggiuntive
- Implementato spawn player gestibile dallo spawn controller(tool level design Editor Unity)