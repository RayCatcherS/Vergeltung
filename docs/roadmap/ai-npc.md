# AI degli NPC

Evoluzione del sistema di comportamento NPC. Oggi: FSM di allerta a 8 stati
(`CharacterAlertState`) con transizioni cablate a mano in `setAlert()`, pattern *Strategy*
(`BehaviourProcess`), tick via coroutine ogni 0.1s, percezione in `CharacterFOV`.

## Obiettivi

- [ ] **Da FSM cablata a Behaviour Tree / HFSM data-driven** (limite principale): elimina la catena
      di `if` di `setAlert()` e rende i comportamenti leggibili/estendibili.
- [ ] **Refactor concorrenza con `CancellationToken`**: propagare un token (es.
      `Application.exitCancellationToken` in Unity 6) attraverso gli `await` dei `BehaviourProcess`,
      così i loop async si fermano puliti all'uscita dal Play / distruzione oggetto. Attualmente
      mitigato solo con guardie `isProcessAlive()` in `ActivityTask` / `GenericUnalertProcess`; gli
      altri process (suspicious/hostility/corpse-found) hanno lo stesso pattern da sistemare.
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

## File chiave

`.../npcBehaviourManager/{BaseNPCBehaviourManager,AbstractNPCBehaviour,EnemyNPCBehaviourManager,CivilianNPCBehaviourManager}.cs`,
`.../behaviourProcess/*`, `.../characterFov/CharacterFOV.cs`.
