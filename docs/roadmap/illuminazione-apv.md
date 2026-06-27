# Illuminazione (APV) e grafica

Revisione dell'illuminazione su base Unity 6.3 / URP 17: rimuovere il vecchio bake con swap di
lightmap (Magic Lightmap Switcher) e adottare tecniche moderne, migliorando il colpo d'occhio
**senza rompere la meccanica di gioco** che dipende dalle luci.

> Branch dedicato: `new-illumination`. Affrontare i passi **in ordine**, uno per commit/fork.

## ⚠️ Vincolo di gameplay (da preservare intatto)

L'illuminazione **non è decorativa né un ciclo day/night**: è agganciata al **sabotaggio della
corrente**. Quando il player spegne i generatori (`ScenePowerController.turnOffPower`):
swap lightmap → `noLight`, spegnimento luci, apertura cancelli elettrici e **malus al FOV degli
NPC** (`applyFOVMalus`); allo scadere del timer tutto si ripristina. Qualunque soluzione deve
mantenere **due stati di luce globali commutabili a runtime**. La logica FOV-malus/cancelli/audio
**non si tocca**: cambiamo solo *come* la scena diventa buia, non *cosa* succede quando lo diventa.

## Stato pipeline attuale (rilevato)

- **Forward+** attivo → nessun limite pratico di luci a schermo (bene per lo stealth top-down).
- **APV spento**: ancora **light probe legacy** (`m_LightProbeSystem: 0`); scenari APV disabilitati.
- **SSAO** attivo ma a intensità minima (0.1); **Decal** attivo.
- HDR on, MSAA off, soft shadows on, **Color Grading in LDR**, **nessun Volume Profile globale**.

## Decisione di backend GI — scelta: **B (ibrido)**

- **A — APV + Lighting Scenarios:** bake due scenari (corrente on/off), blend a runtime. URP 17 lo
  supporta. Rimpiazzo più fedele, ma setup/memoria/2 bake più onerosi.
- **B — Luci dirette real-time + APV (un solo bake) per ambient/bounce** ✅ _scelto_: il diretto è
  già controllato spegnendo le luci; il "power off" = spegni luci + abbassa ambient/exposure. Più
  semplice, iterazione rapida, zero dipendenze esterne. Scenario APV "off" **opzionale** solo se
  vorremo far cambiare anche il rimbalzo.
- _C — restare baked senza MLS:_ scartato (senza scenari APV non esiste swap nativo di set lightmap).

---

## Passi (ordine di esecuzione)

### 1. Rimozione switcher (codice) — ✅ FATTO
- [x] `ScenePowerController`: rimosse le 2 chiamate `LightMapSwitcher.SwitchToLightmap()` (lo switch
      delle lightmap da codice). FOV-malus / cancelli / generatori / audio **intatti**.
- [x] Eliminati `Assets/scripts/Environment/LightMapSwitcher.cs` e
      `Assets/FixOutlineObjectMagicLightSwap.cs` (orfani).
- [x] `Outline.cs` (QuickOutline): rimossa iscrizione a `ChangedLightMap` + handler/coroutine hack.
- [ ] **Manuale (Editor):** togliere il componente `LightMapSwitcher` rimasto orfano (missing script)
      su `Livello.unity` e `testScene.unity` (un click; o via `MissingScriptRemover`).

### 1b. Rimozione fisica del plugin MLS (passo successivo, in Editor)
- [ ] Rimuovere in blocco i componenti `MLSDynamicRenderer` / `MLSStaticRenderer` dagli oggetti di
      scena, poi eliminare la cartella `Assets/Magic Lightmap Switcher/` (incl. il patch temporaneo
      `Shaders/Standard/Shaders/MLS_Standard_Common.cginc`). Il plugin ora è già scollegato dal
      codice di gioco, quindi è isolato e sicuro da rimuovere.

### 2. Stato luce on/off nativo
- [ ] Piccolo `SceneLightingState` (o estensione di `ScenePowerController`) che espone
      `SetPowered(bool)`: gestisce ambient/exposure e (se servirà) lo scenario APV. Sostituisce la
      semantica di `LightMapSwitcher` e dell'`Action ChangedLightMap`.

### 3. Adozione APV (Adaptive Probe Volumes)
- [ ] Abilitare APV nel pipeline asset (`m_LightProbeSystem: 1`) e piazzare il/i Probe Volume.
- [ ] Bake leggero: GI indiretta uniforme su statici **e** dinamici (player/NPC/warp) → elimina
      l'hack dei MLSRenderer e le lightmap UV per-mesh.
- [ ] **Scena lightmap-free**: niente più lightmap né UV2 sulle mesh (la GI vive nelle probe APV,
      campionate per posizione nel mondo). Il diretto/ombre restano real-time sulle luci di gameplay.
- [ ] (Opzionale) Lighting Scenario "power off" se vorremo rimbalzo diverso al buio.

### 4. Outline (QuickOutline) — scollegato da MLS ✅
- [x] Rimossa la sottoscrizione a `ChangedLightMap` (+ handler/coroutine hack) in
      `Assets/assets/importedAssets/QuickOutline/Scripts/Outline.cs`. **L'effetto outline resta
      invariato**: erano solo l'aggancio allo swap lightmap di MLS, non la logica di rendering.
- [x] **Render in URP 17 confermato funzionante** dall'autore → nessuna sostituzione necessaria.
      (Se in futuro vorremo un outline più ricco, valutabile come Renderer Feature URP — non urgente.)

### 5. Post-processing nativo URP (il "glow-up")
- [ ] **Volume Profile globale** con **Tonemapping ACES/Neutral + Color Grading in HDR** (oggi LDR):
      è ciò che dà più "carino" di qualsiasi altra cosa.
- [ ] **Depth of Field** → ricrea il **TiltShift** rimosso (look diorama, perfetto per vista dall'alto).
- [ ] **Danger** rimosso → Vignette rossa pulsante via *Fullscreen Pass Renderer Feature* o override
      di Volume pilotato dallo stato di allerta.
- [ ] **Bloom** misurato, **Vignette** leggera, **Film Grain** sottile.
- [ ] **SSAO**: alzare l'intensità (oggi 0.1) per profondità.

### 6. Sorgenti luminose e rifiniture
- [ ] **Light Cookies** per coni torcia e pattern finestre (possono sostituire i `lightCone` mesh).
- [ ] **Shadow bias**: nel `Ultra_PipelineAsset` `m_ShadowDepthBias` e `m_ShadowNormalBias` sono a
      **0** → causa classica di shadow acne (puntinatura/striature). Tararli appena le ombre saranno
      ridefinite con APV.
- [ ] **Audit shadow caster punctual**: non tutte le luci devono proiettare ombre real-time (torce
      lontane / decorative → *Cast Shadows = Off*). Riduce warning atlas e costa meno. (Mitigato in
      migrazione alzando l'atlas additional lights a 2048.)
- [ ] **Reflection probes** (box projection già attivo) per terreno/Stylized Water.
- [ ] **Lens flare** (data-driven + screen-space, già supportati) sulle sorgenti forti.

## Note tecniche e aspettative oneste

- In URP **non esiste** GI completamente real-time "zero bake" (Enlighten Realtime GI rimosso; SSGI
  è solo HDRP). Da qui l'ibrido: dirette real-time + GI indiretta APV.
- **Nebbia volumetrica / raggi di luce veri**: **non** nativi in URP (roba HDRP). Si **finge** bene
  con i coni mesh `lightCone` già esistenti o con un asset — niente promesse di volumetrico vero.
- **STP** (upscaling temporale) solo se/quando serviranno performance.

## Trappole incontrate (troubleshooting)

- **Scena tutta bianca/nera dopo Clear Baked Data**: causa = **materiale skybox corrotto** che,
  con ambient in modalità *Skybox* (`m_AmbientMode: 0`), iniettava valori HDR/`NaN` nell'ambient
  (`m_AmbientIntensity: NaN`, `m_AmbientSkyColor: NaN`... nel RenderSettings di `Livello.unity`). Le
  lightmap cotte lo mascheravano; pulito il bake, il `NaN` arriva a schermo (né ACES né il clamp del
  Bloom domano un `NaN`; **Stop NaN** sulle camere era OFF). Fix: skybox sano / ambient **Flat** con
  colore finito, e attivare **Stop NaN** come rete di sicurezza.
