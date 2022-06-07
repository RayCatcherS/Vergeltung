using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

enum MainMenuState {
    firstMainMenuScreen,
    storyTextScreen,
    settingsScreen,
}
public class MainMenuInputController : MonoBehaviour
{
    [SerializeField] private int sceneToLoad = 1;
    [SerializeField] private Slider loadingSlider;
    [SerializeField] private GameObject mainMenuScreen;


    void Start() {
        
    }

    public void loadFirstMainMenuScreen() {
        mainMenuScreen.SetActive(true);
    }

    public void startGame() {

        mainMenuScreen.SetActive(false);
        loadingSlider.gameObject.SetActive(true);

        StartCoroutine(LoadSceneAsynchronously(sceneToLoad));
        
    }

    IEnumerator LoadSceneAsynchronously(int selectedScene) {

        AsyncOperation operation = SceneManager.LoadSceneAsync(selectedScene);

        //TODO Loading screen active

        while(!operation.isDone) {
            float progress = Mathf.Clamp01(operation.progress / .9f);
            loadingSlider.value = progress;

            yield return null;
        }
    }
}


