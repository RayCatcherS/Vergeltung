using UnityEngine;
using System.Collections;

public class FollowPlayer : MonoBehaviour {

    [SerializeField]
    private Transform _objectToFollow;

    [SerializeField]
    private float speed = 2.0f;


    [SerializeField] private float zOffset = 10.0f;
    [SerializeField] private float yOffset = 10.0f;


    // getters
    public Transform objectToFollow {

        get { return _objectToFollow; }

        set {
            _objectToFollow = value;
        }
    }

    void Update() {
        float interpolation = speed * Time.deltaTime;

        Vector3 position = this.transform.position;
        position.z = Mathf.Lerp(this.transform.position.z, _objectToFollow.transform.position.z + zOffset, interpolation);
        position.y = Mathf.Lerp(this.transform.position.y, _objectToFollow.transform.position.y + yOffset, interpolation);
        position.x = Mathf.Lerp(this.transform.position.x, _objectToFollow.transform.position.x, interpolation);
        

        this.transform.position = position;
    }
}