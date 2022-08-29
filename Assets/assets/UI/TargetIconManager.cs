using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class TargetIconManager : MonoBehaviour
{
    [SerializeField] private Canvas uiTargetCanvas;
    [SerializeField] private bool followObjectCenter = false;
    [SerializeField] private Vector3 followCenterTarget;


    public void enableTargetUI() {
        

        uiTargetCanvas.gameObject.SetActive(true);
        this.enabled = true;
    }

    public void disableTargetUI() {
        uiTargetCanvas.gameObject.SetActive(false);
        this.enabled = false;
    }

    private void Update() {

        if(uiTargetCanvas != null) {
            uiTargetCanvas.gameObject.transform.forward = Camera.main.transform.forward;

            if(followObjectCenter) {
                uiTargetCanvas.gameObject.transform.position = gameObject.transform.position + followCenterTarget;
            }
        }
    }
}
