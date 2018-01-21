
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows.Speech;
using SimpleJSON;
using System.Collections;
using System.Threading.Tasks;
using System.Net;
using System;
using System.Text;
using System.IO;

public class SpeechToText : MonoBehaviour {
    [SerializeField]
    static public GameObject holoCanvas;
    private string hypo;
    DictationRecognizer dicRec;
    int rekt;

    [SerializeField]
    private string recog;
    
    [SerializeField]
    private string stringToDisplay;

    private void Awake()
    {
        //holoCanvas = GameObject.FindGameObjectWithTag("can");
        //recog = holoCanvas.AddComponent<Text>();
    }

    
    // Use this for initialization
    


    void Start () {
        /*
        dicRec = new DictationRecognizer();

        // dicRec.DictationResult += (text, confidence) =>
        //  {
        //      Debug.LogFormat("Dictation result: {0}", text);
        //      recog.text += text + "\n";
        //  };
        dicRec.DictationResult += DicationRecognizer_DictationResult;
        dicRec.DictationHypothesis += DictationRecognizer_DictationHypothesis;
        dicRec.DictationComplete += DictationRecognizer_DictationComplete;
        dicRec.DictationError += DictationRecognizer_DictationError;

        dicRec.Start();
        Debug.Log("Got started");
        InvokeRepeating("getData", 0, 0.1f);
        */
        

    }
    private void Update()
    {
        // if(recog.text != null)
        //GameObject.FindGameObjectWithTag("lieText").GetComponent<Text>().text = recog.text;
    }
    private void FixedUpdate()
    {

    }
}
