using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoutoutObject : MonoBehaviour
{
    private Dictionary<int, MaterialHitted> cachedMaterials = new Dictionary<int, MaterialHitted>(); // caching dei materiali

    [SerializeField]
    private Transform targetObject;

    [SerializeField]
    private LayerMask wallMask;

    private Camera mainCamera;


    [SerializeField]
    private float cutoutSize = 0.1f;
    [SerializeField]
    private float falloffSize = 0.05f;


    [SerializeField]
    private float cutoutSpeedEffect = 3f;

    private float t = 0.0f;
    private float t1 = 0.0f;

    private void Awake()
    {
        mainCamera = GetComponent<Camera>();
    }

    // Update is called once per frame
    private void Update()
    {

        foreach (var item in cachedMaterials) {

            item.Value.isHitted = false;
            item.Value.material.SetFloat("_CutoutSize", Mathf.Lerp(item.Value.material.GetFloat("_CutoutSize"), 0f, t1));
            item.Value.material.SetFloat("_FalloffSize", Mathf.Lerp(item.Value.material.GetFloat("_FalloffSize"), 0f, t1));

        }
        t1 += cutoutSpeedEffect * Time.deltaTime;


        Vector2 cutoutPos = mainCamera.WorldToViewportPoint(targetObject.position);
        cutoutPos.y /= (Screen.width / Screen.height);

        Vector3 offset = targetObject.position - transform.position;
        RaycastHit[] hitObjects = Physics.RaycastAll(transform.position, offset, offset.magnitude, wallMask);


        // per ogni oggetto hittato dal raycast
        for(int i = 0; i < hitObjects.Length; i++) {


            // per ogni materiale dell'oggetto[i-esimo] hittato
            for (int j = 0; j < hitObjects[i].transform.GetComponent<Renderer>().materials.Length; j++) {


                // id istanza dell [j-esimo] materiale dell[i-esimo] oggetto hittato
                int istanceMaterialID = hitObjects[i].transform.GetComponent<Renderer>().materials[j].GetInstanceID();
                
                // materiale dell [j-esimo] materiale dell[i-esimo] oggetto hittato
                Material material = hitObjects[i].transform.GetComponent<Renderer>().materials[j];

                // controlla se il materiale hittato ? contenuto nel dizionario caching dei materiali hittati dal raycast
                if (cachedMaterials.ContainsKey(istanceMaterialID)) {

                    //Debug.Log("Materiale contenuto in cache");
                } else {
                    //Debug.Log("Materiale non in cache");
                    MaterialHitted matHitted = new MaterialHitted(material, false);

                    cachedMaterials.Add(istanceMaterialID, matHitted); // aggiunta del materiale in cache
                }

                // setta materiale come hittato
                cachedMaterials[istanceMaterialID].isHitted = true;
            }


            
        }


        foreach (var item in cachedMaterials) {

            if(item.Value.isHitted) {
                item.Value.material.SetVector("_CutoutPos", cutoutPos);
                item.Value.material.SetFloat("_CutoutSize", Mathf.Lerp(item.Value.material.GetFloat("_CutoutSize"), cutoutSize, t));
                item.Value.material.SetFloat("_FalloffSize", Mathf.Lerp(item.Value.material.GetFloat("_FalloffSize"), falloffSize, t));

                t += cutoutSpeedEffect * Time.deltaTime;

                t1 = 0;
            } else {
                t = 0;
            }
            
        }
    }
}

class MaterialHitted {

    public MaterialHitted(Material hittedMaterial, bool hitted) {
        this.material = hittedMaterial;
        this.isHitted = hitted;
    }

    public Material material;
    public bool isHitted;
}