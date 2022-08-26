using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class TargetIconManager : MonoBehaviour
{
    [SerializeField] private Canvas uiTargetCanvas;
    private bool updateUIRotationIsStopping = false;

    public void enableTargetUI() {
        uiTargetCanvas.gameObject.SetActive(true);

        updateUIRotation();
    }

    public void disableTargetUI() {
        uiTargetCanvas.gameObject.SetActive(false);
        updateUIRotationIsStopping = true;
    }


    private async void updateUIRotation() {

        while(!updateUIRotationIsStopping) {
            uiTargetCanvas.gameObject.transform.forward = Camera.main.transform.forward;
            await Task.Yield();
        }
        
    }
}
