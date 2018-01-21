using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity;

public class Speech : MonoBehaviour {

    // Use this for initialization
    void Start()
    {
        var soundManager = GameObject.Find("Audio Manager");
        TextToSpeech textToSpeech = soundManager.GetComponent<TextToSpeech>();
        textToSpeech.Voice = TextToSpeechVoice.Mark;
        textToSpeech.StartSpeaking("Welcome to the Holographic App ! You can use Gaze, Gesture and Voice Command to interact with it!");

        Debug.Log("dasd");
    }

    // Update is called once per frame
    void Update()
    {

    }
}
