using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.Networking;
using System;
using System.Collections.Generic;

public class ImageToComputerVisionAPI : MonoBehaviour {

    string VISIONKEY = "2780801cb2824aaca3531b0b3071d01a"; // replace with your Computer Vision API Key

    string emotionURL = "https://eastus.api.cognitive.microsoft.com/vision/v1.0";

    public string fileName { get; private set; }
    string responseData;

    // Use this for initialization
    void Start () {
	    fileName = Path.Combine(Application.streamingAssetsPath, "cityphoto.jpg"); // Replace with your file
    }
	
	// Update is called once per frame
	void Update () {
	
        // This will be called with your specific input mechanism
        if(Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(GetVisionDataFromImages(File.ReadAllBytes(fileName)));
        }

	}

    IEnumerator GetVisionDataFromImages(byte[] image)
    {

        Dictionary<string, string> requestHeaders = new Dictionary<string, string>
        {
            { "Ocp-Apim-Subscription-Key", VISIONKEY },
            {"Content-Type", "application/octet-stream" },
        };

        WWW www = new WWW(emotionURL, image, requestHeaders);

        yield return www;
        string responseData = www.text; // Save the response as JSON string
        Debug.Log(responseData);
       
        GetComponent<ParseComputerVisionResponse>().ParseJSONData(responseData);
    }
    /// <summary>
    /// Get emotion data from the Cognitive Services Emotion API
    /// Stores the response into the responseData string
    /// </summary>
    /// <returns> IEnumerator - needs to be called in a Coroutine </returns>

}
