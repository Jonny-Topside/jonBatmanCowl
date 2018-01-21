
using CognitiveServices;
using HoloToolkit.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

using UnityEngine.Windows;

public class Hud : MonoBehaviour
{

    public Text InfoPanel;
    public Text AnalysisPanel;
    public Text ThreatAssessmentPanel;
    public Text DiagnosticPanel;

    UnityEngine.XR.WSA.WebCam.PhotoCapture _photoCaptureObject = null;
    IEnumerator coroutine;

    public string _subscriptionKey = "258dced5ef694cb3a1209b44f8b652c6";
    string _computerVisionEndpoint = "https://westcentralus.api.cognitive.microsoft.com/vision/v1.0/analyze?visualFeatures=Tags,Faces";
    string _ocrEndpoint = "https://westcentralus.api.cognitive.microsoft.com/vision/v1.0/ocr";

    //public TextToSpeechManager textToSpeechManager;

    void Start()
    {
        if (AnalysisPanel == null || ThreatAssessmentPanel == null || InfoPanel == null)
            return;

        AnalysisPanel.text = "ANALYSIS:\n**************\ntest\ntest\ntest";
        ThreatAssessmentPanel.text = "SCAN MODE XXXXX\nINITIALIZE";
        InfoPanel.text = "CONNECTING";
        status = FindObjectOfType<TextMesh>().gameObject;

        status.GetComponent<TextMesh>().text = "das";

        StartCoroutine(CoroLoop());
    }

    public GameObject status = null;

    IEnumerator<object> GetVisionDataFromImages(byte[] image)
    {

        Dictionary<string, string> requestHeaders = new Dictionary<string, string>
        {
            { "Ocp-Apim-Subscription-Key", _subscriptionKey },
            {"Content-Type", "application/octet-stream" },
        };

        WWW www = new WWW(_computerVisionEndpoint, image, requestHeaders);

        yield return www;
        string responseData = www.text; // Save the response as JSON string
        Debug.Log(responseData);
        status.GetComponent<TextMesh>().text = responseData;
    }

    /*
    private void OnEnable()
    {
        ServicePointManager.ServerCertificateValidationCallback = MonoSecurityBypass;
    }

    public bool MonoSecurityBypass(System.Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
        bool isOk = true;
        // If there are errors in the certificate chain,
        // look at each error to determine the cause.
        if (sslPolicyErrors != SslPolicyErrors.None)
        {
            for (int i = 0; i < chain.ChainStatus.Length; i++)
            {
                if (chain.ChainStatus[i].Status == X509ChainStatusFlags.RevocationStatusUnknown)
                {
                    continue;
                }
                chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
                chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 1, 0);
                chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
                bool chainIsValid = chain.Build((X509Certificate2)certificate);
                if (!chainIsValid)
                {
                    isOk = false;
                    break;
                }
            }
        }
        return isOk;
    }
    */

    IEnumerator CoroLoop()
    {
		int secondsInterval = 20;
		while (true) {
            AnalyzeScene();
            
            yield return new WaitForSeconds(secondsInterval);
		}
    }


    void OnPhotoCaptureCreated(UnityEngine.XR.WSA.WebCam.PhotoCapture captureObject)
    {
        _photoCaptureObject = captureObject;

        Resolution cameraResolution = UnityEngine.XR.WSA.WebCam.PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();

        UnityEngine.XR.WSA.WebCam.CameraParameters c = new UnityEngine.XR.WSA.WebCam.CameraParameters();
        c.hologramOpacity = 0.0f;
        c.cameraResolutionWidth = cameraResolution.width;
        c.cameraResolutionHeight = cameraResolution.height;
        c.pixelFormat = UnityEngine.XR.WSA.WebCam.CapturePixelFormat.BGRA32;

        captureObject.StartPhotoModeAsync(c, OnPhotoModeStarted);

    }

    void OnStoppedPhotoMode(UnityEngine.XR.WSA.WebCam.PhotoCapture.PhotoCaptureResult result)
    {
        _photoCaptureObject.Dispose();
        _photoCaptureObject = null;
    }

    private void OnPhotoModeStarted(UnityEngine.XR.WSA.WebCam.PhotoCapture.PhotoCaptureResult result)
    {
        if (result.success)
        {
            string filename = string.Format(@"image_taken.jpg");
            string filePath = System.IO.Path.Combine(Application.persistentDataPath, filename);

            //doing this to get formatted image
            _photoCaptureObject.TakePhotoAsync(filePath, UnityEngine.XR.WSA.WebCam.PhotoCaptureFileOutputFormat.JPG, OnCapturedPhotoToDisk);

        }
        else
        {
            DiagnosticPanel.text = "Say: Unable to start";

        }
    }

    void OnCapturedPhotoToDisk(UnityEngine.XR.WSA.WebCam.PhotoCapture.PhotoCaptureResult result)
    {
        if (result.success)
        {
            string filename = string.Format(@"terminator_analysis.jpg");
            string filePath = System.IO.Path.Combine(Application.persistentDataPath, filename);

            byte[] image = System.IO.File.ReadAllBytes(filePath);
            StartCoroutine(GetVisionDataFromImages(image));
            ReadWords(image);

        }
        else
        {
            DiagnosticPanel.text = "DIAGNOSTIC\n**************\n\nFailed to save Photo to disk.";
            InfoPanel.text = "ABORT";
        }
        _photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
    }


    // Update is called once per frame
    void Update()
    {

    }

    void AnalyzeScene()
    {
        InfoPanel.text = "CALCULATION PENDING";
        UnityEngine.XR.WSA.WebCam.PhotoCapture.CreateAsync(false, OnPhotoCaptureCreated);
    }

    public void GetTagsAndFaces(byte[] image)
    {

        try
        {
            coroutine = RunComputerVision(image);
            //coroutine = RunComputerVision(UnityEngine.Windows.File.ReadAllBytes(Path.Combine(Application.streamingAssetsPath, "image.jpg")));
            StartCoroutine(coroutine);
        }
        catch (Exception)
        {

            DiagnosticPanel.text = "DIAGNOSTIC\n**************\n\nGet Tags failed.";
            InfoPanel.text = "ABORT";
        }
    }

    public void ReadWords(byte[] image)
    {

        try
        {
            coroutine = Read(image);
            StartCoroutine(coroutine);
        }
        catch (Exception)
        {

            DiagnosticPanel.text = "DIAGNOSTIC\n**************\n\nRead Words failed.";
            InfoPanel.text = "ABORT";
        }
    }

    IEnumerator RunComputerVision(byte[] image)
    {
        var headers = new Dictionary<string, string>() {
            { "Ocp-Apim-Subscription-Key", _subscriptionKey },
            { "Content-Type", "application/octet-stream" }
        };

        WWW www = new WWW(_computerVisionEndpoint, image, headers);
        yield return www;

        List<string> tags = new List<string>();
        var jsonResults = www.text;
        var myObject = JsonUtility.FromJson<AnalysisResult>(jsonResults);
        foreach (var tag in myObject.tags)
        {
            tags.Add(tag.name);
        }
        AnalysisPanel.text = "ANALYSIS:\n***************\n\n" + string.Join("\n", tags.ToArray());

        List<string> faces = new List<string>();
        foreach (var face in myObject.faces)
        {
            faces.Add(string.Format("{0} scanned: age {1}.", face.gender, face.age));
        }
        if(faces.Count > 0)
        {
            InfoPanel.text = "MATCH";
        }else
        {
            InfoPanel.text = "ACTIVE SPATIAL MAPPING";
        }

        ThreatAssessmentPanel.text = "SCAN MODE 43984\nTHREAT ASSESSMENT\n\n" + string.Join("\n", faces.ToArray());
    }

    IEnumerator Read(byte[] image)
    {
        var headers = new Dictionary<string, string>() {
            { "Ocp-Apim-Subscription-Key", _subscriptionKey },
            { "Content-Type", "application/octet-stream" }
        };

        WWW www = new WWW(_ocrEndpoint, image, headers);
        yield return www;

        List<string> words = new List<string>();
        var jsonResults = www.text;
        var ocrResults = JsonUtility.FromJson<OcrResults>(jsonResults);
        foreach (var region in ocrResults.regions)
        foreach (var line in region.lines)
        foreach (var word in line.words)
        {
            words.Add(word.text);
        }

        string textToRead = string.Join(" ", words.ToArray());

        if (textToRead.Length > 0)
        {
            DiagnosticPanel.text = "(language=" + ocrResults.language + ")\n" + textToRead;
            if (ocrResults.language.ToLower() == "en")
            {
                //textToSpeechManager.SpeakText(textToRead);
            }
        }else
        {
            DiagnosticPanel.text = string.Empty;
        }
    }
}
