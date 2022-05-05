using System.Threading.Tasks;
using UnityEngine;

#if UNITY_EDITOR_WIN && UNITY_STANDALONE_WIN
using XInputDotNetPure;
#endif

public class GamePadVibrationController : MonoBehaviour
{

    [SerializeField] private float impulseVibrationDurationTimeEnd;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void sendImpulse(float time, float force) {

#if UNITY_EDITOR_WIN && UNITY_STANDALONE_WIN
        GamePad.SetVibration(0, force, force);
#endif

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

#if UNITY_EDITOR_WIN && UNITY_STANDALONE_WIN
        GamePad.SetVibration(0, 0, 0);
#endif
    }
}
