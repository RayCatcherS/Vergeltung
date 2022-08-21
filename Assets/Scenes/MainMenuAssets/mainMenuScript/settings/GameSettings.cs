using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class ResolutionSetting {
    [SerializeField] int width;
    public int getWidth {
        get { return width; }
    }
    [SerializeField] int height;
    public int getHeight {
        get { return height; }
    }
    [SerializeField] string resolutionName;
    public string getResolutionName {
        get { return resolutionName; }
    }

    public ResolutionSetting(int widthValue, int heightValue, string name) {
        width = widthValue;
        height = heightValue;
        resolutionName = name;

    }
    
}

public class GameSettings : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Text resolutionLabel;
    [Header("Resolution settings states")]
    [SerializeField] private List<ResolutionSetting> resolutionSettings;
    [SerializeField] private int selectedResolution = 0;

    private void Start() {

        string resolutionName = Screen.width.ToString() + "x" + Screen.height.ToString();


        bool platformResolutionExist = false;
        for(int i = 0; i < resolutionSettings.Count; i++) {

            if(resolutionSettings[i].getWidth == Screen.width && resolutionSettings[i].getHeight == Screen.height) {
                platformResolutionExist = true;
                selectedResolution = i;
            }
        }

        if(!platformResolutionExist) {
            resolutionSettings.Add(
            new ResolutionSetting(
                Screen.width,
                Screen.height,
                resolutionName
            )
        );
            selectedResolution = resolutionSettings.Count - 1;
        }
        


        setResolution(resolutionSettings[selectedResolution]);



        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void setNextResolutionSettings() {

        selectedResolution++; 
        if(selectedResolution > resolutionSettings.Count - 1) {
            selectedResolution = 0;
        }
        setResolution(resolutionSettings[selectedResolution]);
    }

    public void setPreviousResolutionSettings() {

        selectedResolution--;
        if (selectedResolution < 0) {
            selectedResolution = resolutionSettings.Count - 1;
        }
        setResolution(resolutionSettings[selectedResolution]);
    }


    public void setResolution(ResolutionSetting resolutionSetting) {
        Screen.SetResolution(resolutionSetting.getWidth, resolutionSetting.getHeight, true);
        resolutionLabel.text = resolutionSetting.getResolutionName;
    }
}

