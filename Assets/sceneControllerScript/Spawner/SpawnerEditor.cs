using System.Collections;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SpawnController))]
public class SpawnerEditor : Editor {

    public override void OnInspectorGUI() {

        DrawDefaultInspector();
        SpawnController spawner = (SpawnController)target;


        if (GUILayout.Button("create character spawn")) {
            spawner.newCharacterSpawn(spawner.role);
        }
    }
}
