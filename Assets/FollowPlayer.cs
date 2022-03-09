using UnityEngine;
using System.Collections;

public class FollowPlayer : MonoBehaviour {

    public GameObject player;        //Public variable to store a reference to the player game object
    public float cameraSpeed = 1;

    private Vector3 offset;            //Private variable to store the offset distance between the player and camera

    // Use this for initialization
    void Start() {
        //Calculate and store the offset value by getting the distance between the player's position and camera's position.
        offset = transform.position - player.transform.position;
    }

    // LateUpdate is called after Update each frame
    void Update() {
        // Set the position of the camera's transform to be the same as the player's, but offset by the calculated offset distance.

        Vector3 newPosition = player.transform.position + offset;
        
        transform.position = Vector3.Lerp(new Vector3(newPosition.x, 28f, newPosition.z), transform.position, Time.deltaTime * cameraSpeed);
    }
}