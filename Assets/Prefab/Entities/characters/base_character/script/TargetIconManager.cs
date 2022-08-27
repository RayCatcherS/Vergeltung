using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class TargetIconManager : MonoBehaviour
{
    [SerializeField] private Canvas uiTargetCanvas;
    private bool updateUIRotationIsStopping = false;

    private bool targetIconStopped = true;



    public void enableTargetUI() {
        

        uiTargetCanvas.gameObject.SetActive(true);
        updateUIRotationIsStopping = false;
        targetIconStopped = false;



        updateUIRotation();
    }

    public async Task disableTargetUI() {


            uiTargetCanvas.gameObject.SetActive(false);
            updateUIRotationIsStopping = true;

            while(!targetIconStopped) {
                await Task.Yield();
            }


            
        
        return;
    }


    private async void updateUIRotation() {

        while(!updateUIRotationIsStopping) {

            if(uiTargetCanvas != null) {
                uiTargetCanvas.gameObject.transform.forward = Camera.main.transform.forward;
            } else {
                break;
            }
            
            await Task.Yield();
        }

        targetIconStopped = true;
        
    }
}
