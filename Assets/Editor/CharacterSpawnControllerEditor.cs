using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CharacterSpawnController))]
public class CharacterSpawnControllerEditor : Editor {

    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        CharacterSpawnController spawner = (CharacterSpawnController)target;


        if (GUILayout.Button("Create character spawn")) {
            Selection.activeObject = spawner.newCharacterSpawnPoint(spawner.role); // crea nuovo character spawn point e selezionalo
        }

        if (spawner.playerSpawnPointSetted) {

            if (GUILayout.Button("Remove spawn")) {
                spawner.removePlayerSpawnPoint();
            }
        } else {
            if (GUILayout.Button("Add player spawn")) {
                Selection.activeObject = spawner.addPlayerSpawnPoint();
            }
        }


    }
}
