# Vergeltung — Roadmap di svecchiamento e pubblicazione

> Analisi tecnica e piano di lavoro per modernizzare il progetto (gioco stealth top-down, Unity)
> ed eventualmente pubblicarlo. Documento di partenza: ogni voce è pensata per diventare
> un branch/fork e una issue dedicata.
>
> Stato attuale: **Unity 2021.2.16f1**, URP 12.1.6, NavMesh via vecchio pacchetto GitHub
> `NavMeshComponents`, scena unica monolitica con lightmap baked.

---

## Verdetto generale

Per un progetto d'esame fatto da solo è più maturo della media: architettura a componenti
coerente, astrazioni reali (`AbstractNPCBehaviour` → Enemy/Civilian), pattern *Strategy* per i
comportamenti (`BehaviourProcess`), **tool da Editor custom** per il level design (spawn point,
activity/task con gizmi e linee di pattuglia) e una meccanica di **warp/possessione** degli NPC
con personalità.

I problemi non sono "scritto male" ma "scritto come un primo progetto": classi diventate enormi,
FSM dell'AI cablata a mano, scena monolitica, scelte tecniche oggi superate. **Tutto recuperabile
senza riscrivere da zero.**

### ⚠️ Tre segnalazioni immediate
1. **`using UnityEditor;` non protetto da `#if UNITY_EDITOR`** in script runtime
   (es. `BaseNPCBehaviourManager.cs`, `CharacterActivity.cs`). Causa tipica di build rotte —
   possibile collegamento col crash dell'eseguibile documentato nel README. **Da verificare.**
2. **Scena unica `Assets/Scenes/Livello.unity` (~375.000 righe)** con tutto dentro e lightmap baked.
3. **NavMesh col vecchio pacchetto GitHub `NavMeshComponents`** (csproj separato), non quello
   ufficiale moderno `com.unity.ai.navigation`.

---

## 1) Miglioramenti tecnici (codice / architettura / engine)

### Codice e architettura
- [ ] **Spezzare le God class**: `BaseNPCBehaviourManager` (1124), `InventoryManager` (983),
      `CharacterFOV` (845), `CharacterManager` (758). Separare responsabilità (timer di stato,
      sensing vs decisione, ecc.).
- [ ] **Rifare `setAlert()` come state machine guidata da dati**: oggi è una catena di `if` che
      codifica a mano le transizioni tra 8 stati → fragile. Tabella di transizioni / BT / HFSM.
- [ ] **Eliminare `GetComponent` ripetuti e `FindObjectsOfType`** (es. `GameModeController.Start`)
      → riferimenti iniettati / cache.
- [ ] **Proteggere tutto il codice Editor** con `#if UNITY_EDITOR` e spostarlo in **assembly
      definition** dedicate (Runtime / Editor separate).
- [ ] **Introdurre `.asmdef`** per modularizzare (Core, AI, Inventory, LevelTools): oggi tutto in
      `Assembly-CSharp`.
- [ ] **Normalizzare nomi e commenti**: mix IT/EN, refusi (`Souspicious`, `CoutoutObject`),
      encoding rotto (`�`) nei commenti.
- [ ] **Uniformare il modello di concorrenza**: convivono coroutine (tick AI) e `async void`/`Task`
      (spawn, force-stop) → fonte di race condition.

### Engine / grafica / build
- [ ] **Upgrade a Unity 6 LTS** (branch dedicato): abilita APV (GI volumetrica), Render Graph URP,
      GPU Resident Drawer/BRG, Forward+.
- [ ] **Spezzare la scena monolitica** in scene additive (Lighting / Geometry / Logic / NPC) e/o
      **Addressables**.
- [ ] **Sostituire la pipeline `.blend` → FBX con Blender 2.79** (vincolo fragile da README) con
      FBX versionati o Blender moderno.
- [ ] **Rivalutare l'illuminazione**: oggi day/night via *Magic Lightmap Switcher* (lightmap baked);
      con Unity 6 + APV valutare transizioni più pulite e se l'asset serve ancora.

## 2) Miglioramenti AI degli NPC
- [ ] **Da FSM cablata a Behaviour Tree / HFSM data-driven** (limite principale attuale).
- [ ] **Percezione "stimulus → confidence"** (crescita/decadimento) invece di booleani istantanei,
      per uno stealth leggibile e giusto. Mattoni già presenti: `CharacterFOV`, last seen position,
      `LoudArea`.
- [ ] **Comunicazione tra NPC strutturata**: formalizzare il "warn of suspicious" come eventi /
      percezione condivisa (chiamata rinforzi, propagazione allarme per zone).
- [ ] **Investigazione credibile**: sostituire `MoveNPCBetweenRandomPointsProcess` (punti random)
      con ricerca pesata attorno all'ultima posizione nota + copertura zona.
- [ ] **Tuning centralizzato in ScriptableObject** per ruolo (timer, velocità) invece di campi
      per-istanza.
- [ ] **Stimoli ambientali di primo livello** (corpi, luci spente, sabotaggi) integrati nell'AI.

## 3) Miglioramenti gameplay / comandi
- [ ] **Refactor del sistema obiettivi** (parte rushata): oggi goal a stringhe con conteggi
      hardcoded in `Start()` e accoppiati alla UI. → ScriptableObject "Objective" con condizioni
      componibili, dipendenze (`unlockEventID` è un buon inizio), Objective Manager disaccoppiato.
- [ ] **Input rebinding + multi-device** (oggi pad obbligatorio da README); l'Input System c'è già.
- [ ] **Feedback allerta più chiaro** (sospetto direzionale, "ultimo punto visto").
- [ ] **Loop di gioco completo**: vittoria/sconfitta, checkpoint/save, transizioni livello
      (base `GameState` / `initWinState` minimale).
- [ ] **Tutorial/onboarding** della meccanica di warp.

## 4) Funzionalità future (backlog)
- [ ] Più livelli con caricamento additivo + selezione missione.
- [ ] Sistema di **save/load** e progressione.
- [ ] Difficoltà / modificatori (Ironman, no-kill run, ...).
- [ ] Espansione del warp come *core fantasy* (catene di possessione, limiti, costi).
- [ ] Audio dinamico legato all'allarme (base `GameSoundtrackController` presente).
- [ ] Accessibilità (rimappatura completa, opzioni colore/outline, scala UI).
- [ ] Localizzazione testi.
- [ ] Packaging Steam/itch, achievements.

---

## NavMesh e level design "a prova d'industria"

### NavMesh
- [ ] **Migrare a `com.unity.ai.navigation`** (ufficiale, incluso in Unity 6); via il vecchio repo
      GitHub `NavMeshComponents`.
- [ ] **Bake da geometria di navigazione dedicata, non dalla mesh di rendering**: `NavMeshSurface`
      che raccoglie i sorgenti **"By Layer"** da un layer apposito (es. `Walkable`/`NavStatic`) con
      mesh semplificate/invisibili.
- [ ] **Porte e salti con `NavMeshLink`** (oggi gestiti via raycast nel behaviour + `NavMeshObstacle`).
- [ ] **`NavMeshModifierVolume`** per aree non camminabili / a costo alto (zone proibite — già
      concettuali in `CharacterAreaManager`).
- [ ] **Aree NavMesh come costi di gameplay** ("pattugliata", "scoperto", "in ombra").

### Costruzione livelli (workflow)
- [ ] **Separare SEMPRE collision mesh da render mesh**: collider primitivi o mesh di collisione
      semplificate dedicate; niente `MeshCollider` grafico per il movimento.
- [ ] **Strategia di layer a tre piani distinti**: physics layers (+ documentare la collision
      matrix), NavMesh include layers, raycast/perception layers. Base layer già buona
      (`Walls`, `door`, `navMeshCollider`, `interactable`, `character`, `meshShadow`...).
- [ ] **Greybox prima, arte dopo**: blockout modulari a griglia (1m/2m), valida gameplay+navmesh,
      poi sostituisci con l'arte.
- [ ] **Prefab + nesting + varianti** per elementi ricorrenti (porte, luci, macchinari).
- [ ] **Scene additive per ruolo** (geometria / luci / logica / NPC).
- [ ] **Occlusion culling + LOD + reflection/light probes** come passo di ottimizzazione finale.

---

## Sequenza consigliata (ogni step = branch isolato)

1. **Igiene + rete di sicurezza** — `#if UNITY_EDITOR` / asmdef, fix build, normalizzazione
   encoding/nomi. _Veloce, basso rischio, abilita il resto._
2. **Upgrade engine** a Unity 6 LTS (NavMesh ufficiale + URP moderno).
3. **Refactor mirati** — prima il sistema obiettivi, poi la FSM dell'AI.
4. **Grafica / illuminazione** sulla nuova base.

---

## Mappa rapida del codice (riferimenti)

| Area | File chiave |
|------|-------------|
| AI core / FSM allerta | `Assets/Prefab/Entities/characters/base_character/script/npcBehaviourManager/BaseNPCBehaviourManager.cs` |
| AI astrazione + stati | `.../npcBehaviourManager/AbstractNPCBehaviour.cs` (enum `CharacterAlertState`) |
| AI per ruolo | `.../npcBehaviourManager/EnemyNPCBehaviourManager.cs`, `CivilianNPCBehaviourManager.cs` |
| Comportamenti (Strategy) | `.../behaviourProcess/*` |
| Percezione | `.../characterFov/CharacterFOV.cs` |
| Tool LD: spawn | `Assets/sceneControllerScript/SpawnerController/CharacterSpawnController.cs`, `CharacterSpawnPoint.cs` |
| Tool LD: activity/task | `.../NPCActivities/CharacterActivity.cs`, `CharacterActivityManager.cs`, `ActivityTask.cs` |
| Editor custom | `Assets/Editor/*.cs` |
| Obiettivi (rushati) | `Assets/sceneControllerScript/gameModeController/GameModeController.cs`, `goalArea/GoalArea.cs` |
| Warp/possessione | `Assets/sceneControllerScript/gameMechanics/PlayerWarpController.cs` |
| Inventario | `Assets/scripts/entityScript/inventory/InventoryManager.cs` |
| Input | `Assets/sceneControllerScript/gameMechanics/GameInputManager.cs` |
