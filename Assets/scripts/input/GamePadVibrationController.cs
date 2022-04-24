using System.Threading.Tasks;
using UnityEngine;

using XInputDotNetPure;

public class GamePadVibrationController : MonoBehaviour
{

    [SerializeField] private float impulseVibrationDurationTimeEnd;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void sendImpulse(float time, float force) {

        GamePad.SetVibration(0, force, force);

        if (Time.time > impulseVibrationDurationTimeEnd) {

            impulseVibrationDurationTimeEnd = Time.time + time;
            _impulseLoop();

        } else {
            impulseVibrationDurationTimeEnd = impulseVibrationDurationTimeEnd + time;
        }
        
    }

    private async void _impulseLoop() {


        

        while(Time.time < impulseVibrationDurationTimeEnd) {
            await Task.Yield();
        }

        GamePad.SetVibration(0, 0, 0);
    }
}
