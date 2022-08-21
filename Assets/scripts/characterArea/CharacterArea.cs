using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CharacterArea : MonoBehaviour
{
    public int getAreaId() {

        return this.GetInstanceID();
    }

#if UNITY_EDITOR
    void OnDrawGizmos() {

        SceneView sceneView = SceneView.lastActiveSceneView;

        // calcola distanza tra la camera e lo spawn point
        float scenViewCameraDistance = Vector3.Distance(sceneView.camera.transform.position, transform.position);

        if(scenViewCameraDistance < 60f) {
            Handles.color = Color.red;
            Handles.Label(
                gameObject.transform.position,
                getAreaId().ToString()
            );
        }
        
    }
#endif
}
