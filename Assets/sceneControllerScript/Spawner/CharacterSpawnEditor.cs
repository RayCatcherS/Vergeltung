using System.Collections;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CharacterSpawn))]
public class CharacterSpawnEditor : Editor {
    public override void OnInspectorGUI() {

        DrawDefaultInspector();
        CharacterSpawn spawn = (CharacterSpawn)target;


        if(GUILayout.Button("Remove spawn")) {
            spawn.removeSpawn();
        }
    }
    
}
