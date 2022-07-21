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
        Handles.color = Color.red;
        Handles.Label(
            gameObject.transform.position,
            getAreaId().ToString()
        );
    }
#endif
}
