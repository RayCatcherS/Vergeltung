using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CharacterActivityManager))]
public class CharacterActivitiesManagerEditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        CharacterActivityManager activityManager = (CharacterActivityManager)target;


        if (GUILayout.Button("New activity")) {
            Selection.activeObject = activityManager.newCharacterActivity(); 
        }


    }
}
