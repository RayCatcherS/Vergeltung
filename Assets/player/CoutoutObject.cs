using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoutoutObject : MonoBehaviour
{
    private Dictionary<int, CacheMaterial> cachedMaterials = new Dictionary<int, CacheMaterial>(); // caching dei materiali

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

    private void Awake()
    {
        mainCamera = GetComponent<Camera>();
    }

    // Update is called once per frame
    private void Update()
    {

        Vector2 cutoutPos = mainCamera.WorldToViewportPoint(targetObject.position);
        cutoutPos.y /= (Screen.width / Screen.height);

        Vector3 offset = targetObject.position - transform.position;
        RaycastHit[] hitObjects = Physics.RaycastAll(transform.position, offset, offset.magnitude, wallMask);

        List<Material> raycastHitObjMaterials = new List<Material>();

        // ottieni i materiali di tutti gli oggetti hittati dal ray cast
        // per tutti i gameObject hittati dal Raycast
        for(int i = 0; i < hitObjects.Length; i++) {


            // ottieni i materiali dell'oggetto hittato
            for (int j = 0; j < hitObjects[i].transform.GetComponent<Renderer>().materials.Length; j++) {
                raycastHitObjMaterials.Add(hitObjects[i].transform.GetComponent<Renderer>().materials[j]);
            }
        }

        

        // segna i CacheMaterial della cache che non sono gli stessi materiali degli oggetti hittati dal RaycastHit come non hittati
        // inoltre setta a [t = 0] il tempo dell'effetto lerp per i CacheMaterial che precedentemente erano hittati
        foreach (var item in cachedMaterials) {
            
            // flag per verificare che l'[item.Key] sia contenuto o meno
            // nella lista dei materiali hittati dal RaycastHit
            bool isCacheMatHitted = false; 
            for(int i = 0; i < raycastHitObjMaterials.Count; i++) {
                
                // se il CacheMaterial fa parte dei materiali hittati dal RaycastHit
                if(raycastHitObjMaterials[i].GetInstanceID() == item.Key) {
                    isCacheMatHitted = true;
                }
            }
            

            
            // se il CacheMaterial non fa parte dei materiali degli oggetti hittati nel RaycastHit
            if(!isCacheMatHitted) {
                if(item.Value.isHitted == true) {
                    item.Value.t = 0f;
                }
                item.Value.isHitted = false;
            }




            // per ogni CacheMaterial della lista cache materiali setta i valori in base allo stato
            if(item.Value.isHitted) {
                item.Value.material.SetVector("_CutoutPos", cutoutPos);
                item.Value.material.SetFloat("_CutoutSize", Mathf.Lerp(item.Value.material.GetFloat("_CutoutSize"), cutoutSize, item.Value.t));
                item.Value.material.SetFloat("_FalloffSize", Mathf.Lerp(item.Value.material.GetFloat("_FalloffSize"), falloffSize, item.Value.t));
                
            } else {
                
                item.Value.isHitted = false;
                item.Value.material.SetFloat("_CutoutSize", Mathf.Lerp(item.Value.material.GetFloat("_CutoutSize"), 0f, item.Value.t));
                item.Value.material.SetFloat("_FalloffSize", Mathf.Lerp(item.Value.material.GetFloat("_FalloffSize"), 0f, item.Value.t));
                
            }

            if(item.Value.t < 1) {
                item.Value.t += cutoutSpeedEffect * Time.deltaTime;
            }
            
        }


        // per ogni materiale degli oggetti hittati dal raycast
        for(int i = 0; i < raycastHitObjMaterials.Count; i++) {

            int istanceMaterialID = raycastHitObjMaterials[i].GetInstanceID();
            
            Material material = raycastHitObjMaterials[i];

            // controlla se il materiale hittato ? contenuto nel dizionario caching dei materiali hittati dal raycast
            if (cachedMaterials.ContainsKey(istanceMaterialID)) {

                //Debug.Log("Materiale contenuto in cache");
            } else {
                //Debug.Log("Materiale non in cache");
                CacheMaterial matHitted = new CacheMaterial(material, false, 0);

                cachedMaterials.Add(istanceMaterialID, matHitted); // aggiunta del materiale in cache
            }

            
            // se il materiale non era già hittato setta il tempo dell'effetto a 0
            if(cachedMaterials[istanceMaterialID].isHitted == false) {
                cachedMaterials[istanceMaterialID].t = 0;
            }
            cachedMaterials[istanceMaterialID].isHitted = true; // setta materiale come hittato
        }
    }
}

class CacheMaterial {

    public CacheMaterial(Material hittedMaterial, bool hitted, float time) {
        this.material = hittedMaterial;
        this.isHitted = hitted;
        this.t = time;
    }

    public Material material;
    public bool isHitted;

    public float t = 0;
}