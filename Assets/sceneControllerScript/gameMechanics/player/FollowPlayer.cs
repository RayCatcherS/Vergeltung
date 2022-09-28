using UnityEngine;
using System.Collections;

public class FollowPlayer : MonoBehaviour {

    // se attivo permette di seguire più velocemente il target in modalità SwitchCharacterMode
    public bool cameraSwitchCharacterModeEnable = false;

    [SerializeField] private Camera mapCamera;

    [SerializeField]
    private Transform _objectToFollow;

    [SerializeField]
    private float speed = 3.5f;


    [SerializeField] private float zOffset = 10.0f;
    [SerializeField] private float yOffset = 10.0f;
    // getters
    public Transform objectToFollow {

        get { return _objectToFollow; }

        set {
            _objectToFollow = value;

            mapCamera.transform.position = new Vector3(
                _objectToFollow.transform.position.x,
                mapCamera.transform.position.y,
                _objectToFollow.transform.position.z
            );

            mapCamera.transform.parent = _objectToFollow.transform; 
        }
    }

    void Update() {
        float interpolation = !cameraSwitchCharacterModeEnable ? speed * Time.deltaTime : (speed * 10) * Time.deltaTime;

        Vector3 position = this.transform.position;
        position.z = Mathf.Lerp(this.transform.position.z, _objectToFollow.transform.position.z + zOffset, interpolation);
        position.y = Mathf.Lerp(this.transform.position.y, _objectToFollow.transform.position.y + yOffset, interpolation);
        position.x = Mathf.Lerp(this.transform.position.x, _objectToFollow.transform.position.x, interpolation);
        

        this.transform.position = position;
    }
}