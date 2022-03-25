using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(CharacterActivity))]
public class CharacterActivityEditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        CharacterActivity activityManager = (CharacterActivity)target;


        if (GUILayout.Button("New Task")) {
            Selection.activeObject = activityManager.newTask(); // crea nuovo activity point e selezionalo
        }

        if (GUILayout.Button("Remove activity")) {
            activityManager.removeActivity();
        }

    }
}
