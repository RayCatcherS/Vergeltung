using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ActivityTask))]
public class TaskEditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        ActivityTask activityPoint = (ActivityTask)target;


        if (GUILayout.Button("Remove Task")) {
            activityPoint.removeActivityTask(); // crea nuovo character spawn point e selezionalo
        }


    }
}
