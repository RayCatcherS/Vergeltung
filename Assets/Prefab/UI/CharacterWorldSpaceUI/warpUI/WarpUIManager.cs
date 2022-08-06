using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarpUIManager : MonoBehaviour {

    [Header("Refereces")]

    [SerializeField] private GameObject uiObj;
    public void setActiveWarpUI(bool value) {
        uiObj.SetActive(value);
    }

    public void setOrientationTowardsTheCamera() {
        uiObj.transform.forward = Camera.main.transform.forward;
    }
}
