using UnityEngine;
using UnityEngine.SceneManagement;

public class ReloadGame : MonoBehaviour
{


    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            RestartGame();
        }
    }
}
