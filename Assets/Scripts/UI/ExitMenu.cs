using UnityEngine;
using UnityEngine.UI;

public class ExitMenu : MonoBehaviour
{
    [SerializeField] private Button mainMenu;

    [SerializeField] private string NextScene;

    private void Start()
    {
        mainMenu.onClick.AddListener(playGameClick);
    }

    private void playGameClick()
    {
        BJ.SceneTransitionManager.LoadNewScene(NextScene);
    }
}
