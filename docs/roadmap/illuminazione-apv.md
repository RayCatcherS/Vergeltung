# Illuminazione (APV) e grafica

Revisione dell'illuminazione sulla base Unity 6 / URP 17, per migliorare il colpo d'occhio
mantenendo dinamicità.

## Obiettivi

- [ ] **Adottare gli Adaptive Probe Volumes (APV)**: bake automatico e leggero delle probe (niente
      più lightmap UV per ogni mesh); gli oggetti dinamici (player, NPC) ricevono GI in modo
      continuo mentre si muovono.
- [ ] **Sostituire Magic Lightmap Switcher con i Lighting Scenarios di APV** per il day/night
      (blend a runtime tra bake, senza ri-bake). MLS è ancora nel progetto come dipendenza da
      rimuovere (resta anche un patch temporaneo: `Assets/Magic Lightmap Switcher/Shaders/Standard/Shaders/MLS_Standard_Common.cginc`).
- [ ] **Ricreare i 2 effetti di SC Post Effects rimossi in migrazione** in post-processing nativo
      URP: `Danger` (overlay di danno/allarme → Vignette rossa o Fullscreen Pass Renderer Feature) e
      `TiltShift` (→ Depth of Field). SSAO e Decal erano già feature native e sono stati mantenuti.
- [ ] **Audit degli shadow caster punctual**: non tutte le luci devono proiettare ombre real-time
      (torce lontane / luci decorative → *Cast Shadows = Off*). Riduce il warning dell'atlas e
      migliora le performance. (Mitigato in migrazione alzando l'atlas additional lights a 2048 in
      `Ultra_PipelineAsset`.)
- [ ] **Reflection probes + tuning** come passo finale.

## Nota tecnica

In URP **non esiste** GI completamente real-time "zero bake" (Enlighten Realtime GI rimosso; SSGI è
solo HDRP). L'approccio giusto è ibrido: **luci dirette real-time** (torce, ombre) + **GI indiretta
via APV** (bake leggero) + **Lighting Scenarios** per il day/night.
