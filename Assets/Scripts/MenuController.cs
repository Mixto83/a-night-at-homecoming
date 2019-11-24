using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class MenuController: MonoBehaviour
{
    public AudioClip buttonSound;
    public AudioSource source;
    private SimpleTimer pressedTimer;


    private void Awake()
    {
        source.clip = buttonSound;
        pressedTimer = new SimpleTimer(1);
    }

    private void Update()
    {
        pressedTimer.Update();
        if (pressedTimer.isFinished())
        {
            NextScene();
        }
    }

    public void PlayAudio()
    {
        source.Play();
        pressedTimer.start();
    }

    private void NextScene()
    {      
        SceneManager.LoadScene("TestScene");
    }
}
