using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scan : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}

     void ScanRoom() {
        gameObject.GetComponentInChildren<SpriteRenderer>().enabled = true;
    }

    // Update is called once per frame
    void Update () {
		
	}
}
