using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

enum MainMenuState {
    mainMenuScreen,
    storyTextScreen,
    settingsScreen,
}
public class MainMenuManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int sceneToLoad = 1;
    
    
    [Header("References")]
    [SerializeField] private EventSystem eventSystem;

    [Header("Screen references")]
    [SerializeField] private MenuScreen mainMenuScreen;
    [SerializeField] private MenuScreen storyTextScreen;
    [SerializeField] private MenuScreen settingsScreen;
    [SerializeField] private MenuScreen loadingScreen;
    [SerializeField] private Slider loadingSlider;
    [SerializeField] private GameObject storyContentToScroll;

    [Header("Menu state")]
    [SerializeField] private MainMenuState mainMenuState = MainMenuState.mainMenuScreen;


    

    void Start() {
        initFirstMainMenuScreen();
    }

    

    public void handleBackMenu() {
        if(mainMenuState == MainMenuState.storyTextScreen || mainMenuState == MainMenuState.settingsScreen) {
            initFirstMainMenuScreen();
        }
    }

    public void initFirstMainMenuScreen() {
        mainMenuScreen.gameObject.SetActive(true);
        storyTextScreen.gameObject.SetActive(false);
        settingsScreen.gameObject.SetActive(false);

        eventSystem.SetSelectedGameObject(mainMenuScreen.firtButton.gameObject);
        mainMenuState = MainMenuState.mainMenuScreen;
    }

    public void initSettingsScreen() {
        mainMenuScreen.gameObject.SetActive(false);
        storyTextScreen.gameObject.SetActive(false);
        settingsScreen.gameObject.SetActive(true);

        eventSystem.SetSelectedGameObject(settingsScreen.firtButton.gameObject);
        mainMenuState = MainMenuState.settingsScreen;
    }

    public void initStoryTextScreen() {
        mainMenuScreen.gameObject.SetActive(false);
        storyTextScreen.gameObject.SetActive(true);
        settingsScreen.gameObject.SetActive(false);

        eventSystem.SetSelectedGameObject(storyTextScreen.firtButton.gameObject);
        mainMenuState = MainMenuState.storyTextScreen;
    }

    public void initLoadingScreen() {

        mainMenuScreen.gameObject.SetActive(false);
        storyTextScreen.gameObject.SetActive(false);
        settingsScreen.gameObject.SetActive(false);
        loadingScreen.gameObject.SetActive(true);


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

    public void scrollStory(Vector2 scrollInput) {

        if (mainMenuState == MainMenuState.storyTextScreen) {
            storyContentToScroll.gameObject.transform.position =
                new Vector3(
                    storyContentToScroll.gameObject.transform.position.x,
                    storyContentToScroll.gameObject.transform.position.y - (scrollInput.y * 1.5f),
                    storyContentToScroll.gameObject.transform.position.z
                );
        }
    }
}


