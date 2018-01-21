
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

    public void getData()
    {
        StartCoroutine(getDataCoroutine());
    }

    public IEnumerator getDataCoroutine()
    {
        WWW www = new WWW("https://batman-982f9.firebaseio.com/translations.json");
        yield return www;
        string responseData = www.text;
        Debug.Log(responseData);
    }
    // Use this for initialization
    public static async Task<string> HttpPost(string message_)
    {
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri("https://batman-982f9.firebaseio.com/.json"));
        request.Method = "PUT";
        request.ContentType = "application/json";
        var json_1 = Newtonsoft.Json.JsonConvert.SerializeObject(new
        {
            translate = message_,
        });
        // Build a string with all the params, properly encoded.
        // We assume that the arrays paramName and paramVal are
        // of equal length:
        // Encode the parameters as form data:
        byte[] formData =
            UTF8Encoding.UTF8.GetBytes(json_1.ToString());
        //request.ContentLength = formData.Length;

        using (var stream = await Task.Factory.FromAsync<Stream>(request.BeginGetRequestStream, request.EndGetRequestStream, null))
        {
            stream.Write(formData, 0, formData.Length);
        }

        // Pick up the response:
        string result = null;
        using (var response = (HttpWebResponse)(await Task<WebResponse>.Factory.FromAsync(request.BeginGetResponse, request.EndGetResponse, null)))
        {
            StreamReader reader = new StreamReader(response.GetResponseStream());
            result = reader.ReadToEnd();
        }

        return result;
    }

    private void DicationRecognizer_DictationResult(string text, ConfidenceLevel confidence)
    {
       // Debug.LogFormat("DictationResult: {0}", text);
      //  Debug.LogFormat("result : {0}", text);
        var response = HttpPost(text.ToString());
        
      //  Debug.LogFormat("final translation: {0}", text);
       // StartCoroutine(translate.Process("en", text));
        Debug.Log(response.ToString());

    }
    private void DictationRecognizer_DictationHypothesis(string text)
    {
        //do stuff
        //  Debug.LogFormat("Dictation Hypothesis: {0}", text);
        Debug.LogFormat("hypothesis {0}", text);
    }
    private void DictationRecognizer_DictationComplete(DictationCompletionCause cause)
    {
        //do something
        if (cause != DictationCompletionCause.Complete)
            Debug.LogErrorFormat("Dictation completed unsuccessfully");
    }
    private void DictationRecognizer_DictationError(string error, int hresult)
    {
        //do something
        Debug.LogErrorFormat("Dictation error: {0}; Hresult = {1}.", error, hresult);
    }

    void Start () {
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
