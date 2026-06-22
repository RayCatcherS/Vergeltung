# NavMesh e level design "a prova d'industria"

## NavMesh

- [ ] **Migrare a `com.unity.ai.navigation`** (ufficiale, incluso in Unity 6); rimuovere il vecchio
      sorgente GitHub in `Assets/assets/NavMeshComponents/Scripts/` + i sample orfani
      `Assets/Samples/AI Navigation/...`. ⚠️ La classe `NavMeshSurface` legacy (namespace globale) va
      in conflitto con quella nuova (`Unity.AI.Navigation`): rimuovere il legacy **prima** e poi
      ri-agganciare/convertire i componenti `NavMeshSurface` nelle scene (GUID diverso → riferimenti
      da rimappare).
- [ ] **Bake da geometria di navigazione dedicata, non dalla mesh di rendering**: `NavMeshSurface`
      che raccoglie i sorgenti **"By Layer"** da un layer apposito (es. `Walkable`/`NavStatic`) con
      mesh semplificate/invisibili.
- [ ] **Porte e salti con `NavMeshLink`** (oggi gestiti via raycast nel behaviour + `NavMeshObstacle`).
- [ ] **`NavMeshModifierVolume`** per aree non camminabili / a costo alto (zone proibite — già
      concettuali in `CharacterAreaManager`).
- [ ] **Aree NavMesh come costi di gameplay** ("pattugliata", "scoperto", "in ombra").

## Costruzione livelli (workflow)

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
