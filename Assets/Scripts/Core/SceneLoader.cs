using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadBattle()
    {
        SceneManager.LoadScene("Battle");
    }

    public void LoadLearningMenu()
    {
        SceneManager.LoadScene("LearningMenu");
    }

    public void LoadLetterLearning()
    {
        SceneManager.LoadScene("LetterLearning");
    }

    public void LoadWordLearning()
    {
        SceneManager.LoadScene("WordLearning");
    }

    public void LoadNameWorkshop()
    {
        SceneManager.LoadScene("NameWorkshop");
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}