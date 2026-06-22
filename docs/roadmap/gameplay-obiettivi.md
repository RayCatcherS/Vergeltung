# Gameplay e obiettivi

## Obiettivi

- [ ] **Refactor del sistema obiettivi** (parte "rushata" dichiarata dall'autore): oggi i goal sono
      stringhe con conteggi hardcoded in `GameModeController.Start()` e accoppiati alla UI.
      → ScriptableObject "Objective" con condizioni componibili, dipendenze (`unlockEventID` è un
      buon inizio), Objective Manager disaccoppiato dalla UI. Abilita missioni multiple e
      progressione.
- [ ] **Input rebinding + multi-device**: oggi il pad è obbligatorio (da README); l'Input System
      c'è già, va esposto il rebinding a UI e gestito pad/tastiera+mouse.
- [ ] **Feedback allerta più chiaro** (indicatore di sospetto direzionale, "ultimo punto visto").
- [ ] **Loop di gioco completo**: stati vittoria/sconfitta, checkpoint/save, transizioni livello
      (base `GameState` / `initWinState` minimale).
- [ ] **Tutorial/onboarding** della meccanica di warp (è peculiare, va insegnata).

## File chiave

`Assets/sceneControllerScript/gameModeController/GameModeController.cs`, `goalArea/GoalArea.cs`,
`gameMechanics/PlayerWarpController.cs`, `gameMechanics/GameInputManager.cs`,
`gameState/GameState.cs`.
