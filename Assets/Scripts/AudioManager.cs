using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{

    [SerializeField] private AudioClip[] soundList;
    private static AudioManager instance;
    private AudioSource audioSource; 


    private void Awake()
    {
        instance = this; 

    }


    public static void PlayAudio()
    {

    }
   
}
