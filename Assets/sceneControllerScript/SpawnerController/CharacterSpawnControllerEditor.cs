using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CharacterSpawnController))]
public class CharacterSpawnControllerEditor : Editor {

    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        CharacterSpawnController spawner = (CharacterSpawnController)target;


        if (GUILayout.Button("create character spawn")) {
            Selection.activeObject = spawner.newCharacterSpawnPoint(spawner.role); // crea nuovo character spawn point e selezionalo
        }

        
    }
}
