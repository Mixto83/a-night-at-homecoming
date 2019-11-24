using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public List<AudioClip> songs;
    public AudioClip crowdSound;
    public AudioSource musicSource;
    public AudioSource crowdSource;

    private void Awake()
    {
        crowdSource.clip = crowdSound;
    }

    public void ChangeSong(string style)
    {
        switch (style)
        {
            case "Rock":
                musicSource.clip = songs[0];
                break;
            case "Rap":
                musicSource.clip = songs[1];
                break;
            case "Classical":
                musicSource.clip = songs[2];
                break;
            case "Punk":
                musicSource.clip = songs[3];
                break;
            case "Techno":
                musicSource.clip = songs[4];
                break;
            case "Disco":
                musicSource.clip = songs[5];
                break;
            case "Romantic":
                musicSource.clip = songs[6];
                break;
            case "Pop":
                musicSource.clip = songs[7];
                break;
            case "Blues":
                musicSource.clip = songs[8];
                break;
            case "Ska":
                musicSource.clip = songs[9];
                break;
            case "Salsa":
                musicSource.clip = songs[10];
                break;
        }
        PlaySong();
    }

    private void PlaySong()
    {
        musicSource.Play();
    }

    public void PlayCrowd()
    {
        crowdSource.Play();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
