using UnityEngine;
using UnityEngine.UI;

public class StartMenu : MonoBehaviour
{
    [SerializeField] private Button playGame;
    [SerializeField] private Button quitGame;
    private void Start()
    {
        playGame.onClick.AddListener(playGameClick);
    }

    public void playGameClick()
    {
        Debug.Log("hello");
    }
}
