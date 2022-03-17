using UnityEngine;
using System.Collections;

public class FollowPlayer : MonoBehaviour {

    [SerializeField]
    private GameObject objectToFollow;

    [SerializeField]
    private float speed = 2.0f;

    [SerializeField]
    private float zOffset = 10.0f;

    void Update() {
        float interpolation = speed * Time.deltaTime;

        Vector3 position = this.transform.position;
        position.z = Mathf.Lerp(this.transform.position.z, objectToFollow.transform.position.z + zOffset, interpolation);
        position.x = Mathf.Lerp(this.transform.position.x, objectToFollow.transform.position.x, interpolation);

        this.transform.position = position;
    }
}