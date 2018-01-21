using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Show : MonoBehaviour {

    public GameObject[] gameObjects;
	// Use this for initialization
	void Start () {
        StartCoroutine(RemoveAfterSeconds(8));

        foreach (var gameObject in gameObjects)
        {
            gameObject.SetActive(false);
        }
    }


    IEnumerator RemoveAfterSeconds(int seconds)
    {
        yield return new WaitForSeconds(seconds);
        StartCoroutine(show());
    }

    IEnumerator show() {
        foreach (var gameObject in gameObjects)
        {
            yield return new WaitForSeconds(0.1f);
            gameObject.SetActive(true);
        }
    }
    // Update is called once per frame
    void Update () {
		
	}
}
