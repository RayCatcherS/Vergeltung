# Debito tecnico — codice / architettura

Pulizia e modernizzazione del codice. È la base ("igiene + rete di sicurezza") che abilita gli
altri refactor. Basso rischio, alto valore.

## Obiettivi

- [ ] **Proteggere tutto il codice Editor** con `#if UNITY_EDITOR` e spostarlo in **assembly
      definition** dedicate (Runtime / Editor separate). Diversi script runtime hanno
      `using UnityEditor;` non protetto (es. `BaseNPCBehaviourManager.cs`, `CharacterActivity.cs`,
      `CharacterFOV.cs`, `ActivityTask.cs`…) → **rompe le build standalone**. Priorità alta.
- [ ] **Introdurre `.asmdef`** per modularizzare (Core, AI, Inventory, LevelTools): oggi è tutto in
      `Assembly-CSharp`. Velocizza la compilazione e impone confini chiari.
- [ ] **Spezzare le God class**: `BaseNPCBehaviourManager` (1124), `InventoryManager` (983),
      `CharacterFOV` (845), `CharacterManager` (758). Separare responsabilità (gestione timer di
      stato, sensing vs decisione, ecc.).
- [ ] **Rifare `setAlert()` come state machine guidata da dati**: oggi è una catena di `if` che
      codifica a mano le transizioni tra 8 stati → fragile. (Vedi [ai-npc.md](ai-npc.md).)
- [ ] **Eliminare `GetComponent` ripetuti e `FindObjectsOfType`** (es. `GameModeController.Start`,
      `ScenePowerController`) → riferimenti iniettati / cache.
- [ ] **Uniformare il modello di concorrenza**: convivono coroutine (tick AI) e `async void`/`Task`.
      Manca la cancellazione → all'uscita dal Play comparivano `MissingReferenceException`
      (mitigato in unalert con guardie `isProcessAlive()`/`Application.isPlaying`; la soluzione vera
      è propagare un `CancellationToken` — vedi [ai-npc.md](ai-npc.md)).
- [ ] **Normalizzare nomi e commenti**: mix IT/EN, refusi (`Souspicious`, `CoutoutObject`),
      encoding rotto (`�`) nei commenti.
- [ ] **Spezzare la scena monolitica** `Assets/Scenes/Livello.unity` (~375k righe) in scene
      additive (Lighting / Geometry / Logic / NPC) e/o **Addressables**.

## Note

- L'upgrade a Unity 6.3 (✅ fatto) ha già sistemato un CS0592 in `CharacterAreaManager`
  (`[SerializeField]` su proprietà).
