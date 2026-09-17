using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadBattle()
    {
        SceneManager.LoadScene("Battle");
    }
}