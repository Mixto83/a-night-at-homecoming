using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class MenuController: MonoBehaviour
{
    public void NextScene()
    {
        SceneManager.LoadScene("TestScene");
    }
}
