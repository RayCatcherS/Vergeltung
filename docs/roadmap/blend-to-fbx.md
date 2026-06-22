# Migrazione `.blend` → FBX

## Contesto

Il progetto importa **58 file `.blend`** diretti: Unity li converte in FBX al volo lanciando
Blender, quindi **chiunque apra il progetto deve avere Blender installato** (versione compatibile
Unity 6: **3.0.1–4.2**; oggi serve **Blender 4.2 LTS**). Vincolo fragile per condivisione e build.
Già presenti anche 42 FBX "veri".

## Obiettivo

Convertire i `.blend` in **FBX committati** ed eliminare del tutto la dipendenza da Blender per
aprire/buildare il progetto, **senza perdere la comodità dell'auto-aggiornamento**.

## Approccio

- [ ] **Add-on Blender con handler `save_post`** (registrato una volta nello startup, vale per tutti
      i `.blend`): a ogni salvataggio esporta l'FBX con impostazioni fisse e Unity-correct
      (`axis_forward=-Z`, `axis_up=Y`, apply transform, smoothing). → stessa comodità di adesso,
      zero dipendenza Blender per il progetto. Sorgenti `.blend` idealmente **fuori** da `Assets/`.
- [ ] **Remap dei riferimenti** (il punto delicato): sostituire `.blend` con `.fbx` cambia il GUID
      dell'asset → i riferimenti nella scena si romperebbero. Due tecniche:
  - **a)** trucco preservazione GUID: rinominare `model.blend.meta` → `model.fbx.meta` (stesso GUID)
        mantenendo gli stessi nomi mesh → i riferimenti sopravvivono;
  - **b)** script `AssetDatabase` per remap in massa.
- [ ] **Pilota su un modello** prima di applicare a tutti (verificare scala/orientamento/riferimenti).

## Note

- L'auto-aggiornamento "salvo in Blender → Unity aggiorna" **esiste già** con l'import diretto
  `.blend`; l'unico motivo della migrazione è togliere la dipendenza dalla versione di Blender.
