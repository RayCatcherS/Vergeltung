using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollManager : MonoBehaviour
{
    const int RAGDOLL_BONE_LAYER = 15;

    [SerializeField] private List<GameObject> ragdollBones;

    void Start()
    {
        getRagdollBones(gameObject);
        initRagdollCharacter();
    }

    private void getRagdollBones(GameObject GO) {

        foreach (Transform child in GO.transform) {
            if(child.gameObject.layer == RAGDOLL_BONE_LAYER) {
                ragdollBones.Add(child.gameObject);
            }
            getRagdollBones(child.gameObject);
        }

    }

    
    private void initRagdollCharacter() {
        if (!gameObject.GetComponent<CharacterManager>().isDead) {
            disableRagdoll();
        } else {
            enableRagdoll();
        }
    }

    public void enableRagdoll() {
        for (int i = 0; i < ragdollBones.Count; i++) {
            ragdollBones[i].GetComponent<Rigidbody>().isKinematic = false;


            if (ragdollBones[i].GetComponent<SphereCollider>() != null) {
                ragdollBones[i].GetComponent<SphereCollider>().enabled = true;

            } else if (ragdollBones[i].GetComponent<BoxCollider>() != null) {
                ragdollBones[i].GetComponent<BoxCollider>().enabled = true;

            } else if (ragdollBones[i].GetComponent<CapsuleCollider>() != null) {
                ragdollBones[i].GetComponent<CapsuleCollider>().enabled = true;
            }
        }
    }
    public void disableRagdoll() {
        for (int i = 0; i < ragdollBones.Count; i++) {
            ragdollBones[i].GetComponent<Rigidbody>().isKinematic = true;


            if (ragdollBones[i].GetComponent<SphereCollider>() != null) {
                ragdollBones[i].GetComponent<SphereCollider>().enabled = false;

            } else if (ragdollBones[i].GetComponent<BoxCollider>() != null) {
                ragdollBones[i].GetComponent<BoxCollider>().enabled = false;

            } else if (ragdollBones[i].GetComponent<CapsuleCollider>() != null) {
                ragdollBones[i].GetComponent<CapsuleCollider>().enabled = false;
            }
        }
    }
}
