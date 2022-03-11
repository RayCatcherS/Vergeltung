using System.Collections;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CharacterSpawnPoint))]
public class CharacterSpawnPointEditor : Editor {
    public override void OnInspectorGUI() {

        DrawDefaultInspector();
        CharacterSpawnPoint spawn = (CharacterSpawnPoint)target;


        if(GUILayout.Button("Remove spawn")) {
            spawn.removeSpawnPoint();
        }
    }

    private void OnSceneGUI() {
        
    }

    

}
