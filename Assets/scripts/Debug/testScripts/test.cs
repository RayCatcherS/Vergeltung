using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }


    private async Task waitingOnANavMeshPointTimerLoop() {


        float endTime = Time.time + 100;

        while (Time.time < endTime) {

            print("task");
            await Task.Yield();
        }
    }
}
