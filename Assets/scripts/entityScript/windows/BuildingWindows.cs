using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingWindows : MonoBehaviour
{
    private const int WALLS_LAYER = 8;
    [SerializeField] private Material windowsLightMat;
    [SerializeField] private Material windowsNoLightMat;
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private int matPositionInRendererList;

    private bool _lightDisabled = false;
    public bool lightDisabled {
        set {
            _lightDisabled = value;
        }
    }

    private void Start() {
    }

    public void turnOffLigth() {

        if(!_lightDisabled) {
            StartCoroutine(lightOffTransition());
        }

    }

    public void turnOnLigth() {

        if(!_lightDisabled) {
            StartCoroutine(lightOnTransition());
        }

    }


    private IEnumerator lightOffTransition() {


        float timeWaitLightOff = Random.Range(0.05f, 0.5f);
        yield return new WaitForSeconds(timeWaitLightOff);
        setLightOff();

    }

    private IEnumerator lightOnTransition() {


        float timeWaitLightOff = Random.Range(0.05f, 0.5f);
        yield return new WaitForSeconds(timeWaitLightOff);
        setLightOn();
    }


    public void setLightOff() {

        Material[] mats = meshRenderer.materials;
        Material t = mats[matPositionInRendererList];

        mats[matPositionInRendererList] = windowsNoLightMat;

        if(gameObject.layer == WALLS_LAYER) {

            Vector3 cutoutPos = t.GetVector("_CutoutPos");
            mats[matPositionInRendererList].SetVector("_CutoutPos", cutoutPos);


            float cutoutSize = t.GetFloat("_CutoutSize");
            mats[matPositionInRendererList].SetFloat("_CutoutSize", cutoutSize);

            float falloffSize = t.GetFloat("_FalloffSize");
            mats[matPositionInRendererList].SetFloat("_FalloffSize", falloffSize);
        }

        meshRenderer.materials = mats;
    }

    private void setLightOn() {

        Material[] mats = meshRenderer.materials;
        Material t = mats[matPositionInRendererList];

        mats[matPositionInRendererList] = windowsLightMat;

        if(gameObject.layer == WALLS_LAYER) {

            Vector3 cutoutPos = t.GetVector("_CutoutPos");
            mats[matPositionInRendererList].SetVector("_CutoutPos", cutoutPos);


            float cutoutSize = t.GetFloat("_CutoutSize");
            mats[matPositionInRendererList].SetFloat("_CutoutSize", cutoutSize);

            float falloffSize = t.GetFloat("_FalloffSize");
            mats[matPositionInRendererList].SetFloat("_FalloffSize", falloffSize);
        }

        meshRenderer.materials = mats;
    }
}
