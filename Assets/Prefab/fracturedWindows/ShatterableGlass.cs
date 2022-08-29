using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ShatterableGlass : MonoBehaviour
{

    [SerializeField] private ParticleSystem particles;
    [Header("Refs")]
    [SerializeField] private BoxCollider glassCollider;
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private AudioSource audioSource;


    public void shatterGlass() {

        // set shape 
        ParticleSystem.ShapeModule particleShape = particles.shape;
        particleShape.shapeType = ParticleSystemShapeType.Box;
        particleShape.position = glassCollider.center;
        particleShape.scale = glassCollider.size;



        // disabilita componenti e starta particle effects
        NavMeshObstacle glassObstacle = gameObject.GetComponent<NavMeshObstacle>();
        if(glassObstacle != null) { // disabilita componente NavMeshObstacle se presente(finestre grandi)
            glassObstacle.enabled = false;
        }
        glassCollider.enabled = false;
        meshRenderer.enabled = false;
        particles.Play();

        audioSource = gameObject.GetComponent<AudioSource>();
        audioSource.Play();
    }
}
