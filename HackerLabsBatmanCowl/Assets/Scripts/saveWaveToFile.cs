using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class saveWaveToFile : MonoBehaviour {
    AudioClip wavClip;
	// Use this for initialization
	void Start () {
        wavClip = new AudioClip();
        Microphone.Start(null, false, 5, 44100);
        wait();
        SavWav.Save("testForSavingWaving", wavClip);
	}
	
    IEnumerator wait()
    {
        yield return new WaitForSeconds(5);
    }
	// Update is called once per frame
	void Update () {
		
	}
}
