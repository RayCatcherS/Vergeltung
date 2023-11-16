using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightMapSwitcher : MonoBehaviour
{
    public Texture2D[] lightmaps; // Array contenente le diverse lightmaps bakeate

    void Start() {

        LightmapData[] lightmapData = new LightmapData[lightmaps.Length];


        // Assegna le lightmaps dal tuo array a quelle nella scena
        for(int i = 0; i < lightmaps.Length; i++) {

            lightmapData[i] = new LightmapData();
            lightmapData[i].lightmapColor = lightmaps[i];
            

        }

        LightmapSettings.lightmaps = lightmapData;
    }

    // Metodo per cambiare la lightmap attiva durante l'esecuzione
    public void SwitchToLightmap(int index) {
        if(index >= 0 && index < lightmaps.Length) {
            LightmapSettings.lightmaps[index].lightmapColor = lightmaps[index];
            Debug.Log("switched");
        } else {
            Debug.LogError("Indice lightmap non valido.");
        }
    }
}
