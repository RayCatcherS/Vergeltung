# Vergeltung — Roadmap

Piano di modernizzazione del progetto (gioco stealth top-down, Unity) verso la pubblicazione.
Ogni file di questa cartella è il **brief di un'iniziativa** (= un fork/PR dedicato): quando apri
il fork, leggi e aggiorni il file corrispondente.

> **Stato engine attuale:** Unity 6.3 LTS (6000.3.18f1), URP 17, Input System.
> Scena unica monolitica `Assets/Scenes/Livello.unity` con lightmap baked.
> NavMesh ancora sul vecchio pacchetto GitHub `NavMeshComponents`.

## Verdetto generale

Per un progetto d'esame fatto da solo è più maturo della media: architettura a componenti
coerente, astrazioni reali (`AbstractNPCBehaviour` → Enemy/Civilian), pattern *Strategy* per i
comportamenti (`BehaviourProcess`), tool da Editor custom per il level design (spawn point,
activity/task con gizmi) e una meccanica di **warp/possessione** degli NPC con personalità.

I limiti sono "da primo progetto": classi diventate enormi, FSM dell'AI cablata a mano, scena
monolitica. Tutto recuperabile senza riscrivere da zero.

## Iniziative

| Topic | File | Stato |
|-------|------|-------|
| Migrazione a Unity 6.3 LTS (URP 17) | _(storia nei commit/PR)_ | ✅ Fatto |
| Debito tecnico — codice/architettura | [debito-tecnico-codice.md](debito-tecnico-codice.md) | ⏳ Pianificato |
| AI degli NPC | [ai-npc.md](ai-npc.md) | ⏳ Pianificato |
| Gameplay e obiettivi | [gameplay-obiettivi.md](gameplay-obiettivi.md) | ⏳ Pianificato |
| NavMesh e level design | [navmesh-level-design.md](navmesh-level-design.md) | ⏳ Pianificato |
| Migrazione `.blend` → FBX | [blend-to-fbx.md](blend-to-fbx.md) | ⏳ Pianificato |
| Illuminazione (APV) | [illuminazione-apv.md](illuminazione-apv.md) | ⏳ Pianificato |
| Funzionalità future (backlog) | [funzionalita-future.md](funzionalita-future.md) | 💡 Backlog |

## Sequenza consigliata

1. **Igiene + rete di sicurezza** ([debito tecnico](debito-tecnico-codice.md)): `#if UNITY_EDITOR`/asmdef, normalizzazione → abilita tutto il resto.
2. **[NavMesh](navmesh-level-design.md)** moderno + workflow level design.
3. **Refactor mirati**: prima il [sistema obiettivi](gameplay-obiettivi.md), poi la [FSM/AI](ai-npc.md).
4. **[Illuminazione/APV](illuminazione-apv.md)** e **[migrazione FBX](blend-to-fbx.md)**.

## Mappa rapida del codice

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
