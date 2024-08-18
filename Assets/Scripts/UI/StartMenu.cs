using UnityEngine;
using UnityEngine.UI;

public class StartMenu : MonoBehaviour
{
    [SerializeField] private Button playGame;
    [SerializeField] private Button quitGame;

    [SerializeField] private string NextScene;

    private void Start()
    {
        playGame.onClick.AddListener(playGameClick);
        quitGame.onClick.AddListener(quitGameClick);
    }

    private void playGameClick()
    {
        BJ.SceneTransitionManager.LoadNewScene(NextScene);
    }

    private void quitGameClick()
    {
        Application.Quit();
    }
}
