using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Collections;
using UnityEngine.Windows.Speech;
using UnityEngine.UI;
using System;
using System.Net;
using System.Text;
using HoloToolkit.Unity;
using System.Threading.Tasks;
using System.Web;

public class BatmanFacialRecognition : MonoBehaviour
{
    const string faceAPIKey = "f6633cc8206a495199dd7bcb6053afe8";
    const string emotionAPIKey = "default";
    UnityEngine.XR.WSA.WebCam.PhotoCapture photoCaptureObject = null;

    Resolution cameraResolution;
    Vector3 cameraPosition = Vector3.zero;
    Quaternion cameraRotation = new Quaternion();
    UnityEngine.XR.WSA.Input.GestureRecognizer recognizer = null;
    public GameObject status = null;
    public GameObject criminal = null;
    public GameObject faceImage = null;
    public GameObject peopleCount = null;
   

    //public Text status_text;
    //public RawImage rawImage;

    [SerializeField]
    static public GameObject holoCanvas;
    private string hypo;
    DictationRecognizer dicRec;
    int rekt;
    ArrayList linesToTranslate;

    [SerializeField]
    private string recog;

    [SerializeField]
    private string stringToDisplay;

    string question;

    IEnumerator Wait()
    {
        yield return new WaitForSeconds(10);
    }
    void Start()
    {
        criminal.GetComponent<TextMesh> ().text= "";
        //question = "";
        //status.text = "";
        //status_text.text = "";
        //linesToTranslate = new ArrayList();
        // PostTestImage();
        //status = FindObjectOfType<TextMesh>().gameObject;
        //StartCoroutine(PostFace(File.ReadAllBytes(Path.Combine(Application.streamingAssetsPath, "image.jpg"))));
        //StartCoroutine(GetVisionDataFromImages(File.ReadAllBytes(Path.Combine(Application.streamingAssetsPath, "image.jpg"))));
        // Detect();

        //UnityEngine.XR.WSA.WebCam.PhotoCapture.CreateAsync(false, OnPhotoCaptureCreated);
        //StartCoroutine(CoroLoop());
        //startSpeachToText();
        Wait();
        BroadcastMessage("ScanRoom");
        InvokeRepeating("loop",15, 5);
        //StartCoroutine(GetVisionDataFromImages(File.ReadAllBytes(Path.Combine(Application.streamingAssetsPath, "image.jpg"))));
        //transalateData("or kya haalchal?");

        //StartCoroutine(getData());
        //startSpeachToText();
        //InvokeRepeating("getData", 0, 0.1f);
    }

    void translateText(string message_)
    {
        //var response = HttpPost(message_);
        //Debug.Log(response.ToString());
    }

    void startSpeachToText()
    {
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
        Debug.Log("Got started");
        dicRec.Start();
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


        JSONObject json = new JSONObject(responseData);

        List<string> keys = json.keys;



        if (keys != null)
        {

            foreach (var key in keys)
            {
                Debug.Log(json.GetField(key).ToString().Replace("\"", ""));
                speak(json.GetField(key).ToString().Replace("\"", ""));
            }
        }
    }

    public async Task<string> HttpPost(string message)
    {
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri("https://batman-982f9.firebaseio.com/.json"));
        request.Method = "PUT";
        request.ContentType = "application/json";
        var json_1 = Newtonsoft.Json.JsonConvert.SerializeObject(new
        {
            translate = message,//linesToTranslate[currentIndexForTranslation].ToString(),
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

        Debug.Log(result);
        return result;
    }


    private void DicationRecognizer_DictationResult(string text, ConfidenceLevel confidence)
    {
        // Debug.LogFormat("DictationResult: {0}", text);
        Debug.LogFormat("result : {0}", text);
        //linesToTranslate.Add(text);
        //var response = HttpPost(text);
        //Debug.Log(response.ToString());
        StartCoroutine(translate.Process("en", text));
        Debug.Log("Under this should be the spanish to english translation");
        Debug.Log(translate.translatedText);

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

    void loop()
    {
        //status_text.text = "Scanning";
        capture();
    }

    IEnumerator CoroLoop()
    {
        int secondsInterval = 10;
        while (true)
        {
            //AnalyzeScene();
            capture();//RunComputerVision(UnityEngine.Windows.File.ReadAllBytes(Path.Combine(Application.streamingAssetsPath, "image.jpg")));
            yield return new WaitForSeconds(secondsInterval);
        }
    }

    void capture()
    {
        UnityEngine.XR.WSA.WebCam.PhotoCapture.CreateAsync(false, OnPhotoCaptureCreated);
    }

    /*
    async void Detect() {
       CriminalDatabase.Criminal_t criminal =  await FindObjectOfType<CriminalDatabase>().GetCriminalFromImage(File.ReadAllBytes(Path.Combine(Application.streamingAssetsPath, "image.jpg")));
        Debug.Log(criminal.CriminalName);
    }
    */

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
    private void Update()
    {
        //UnityEngine.XR.WSA.WebCam.PhotoCapture.CreateAsync(false, OnPhotoCaptureCreated);
    }

    void PostTestImage()
    {
        string filePath = "G:\\Desktop\\20171110_190853.jpg";

        //StartCoroutine(PostFace(GetImageAsByteArray(filePath)));
    }

    void OnPhotoCaptureCreated(UnityEngine.XR.WSA.WebCam.PhotoCapture captureObject)
    {
        photoCaptureObject = captureObject;
        cameraResolution = UnityEngine.XR.WSA.WebCam.PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();

        UnityEngine.XR.WSA.WebCam.CameraParameters c = new UnityEngine.XR.WSA.WebCam.CameraParameters();
        c.hologramOpacity = 0.0f;
        c.cameraResolutionWidth = cameraResolution.width;
        c.cameraResolutionHeight = cameraResolution.height;
        c.pixelFormat = UnityEngine.XR.WSA.WebCam.CapturePixelFormat.PNG;
        captureObject.StartPhotoModeAsync(c, OnPhotoModeStarted);
    }

    void OnCapturedPhotoToMemory(UnityEngine.XR.WSA.WebCam.PhotoCapture.PhotoCaptureResult result, UnityEngine.XR.WSA.WebCam.PhotoCaptureFrame photoCaptureFrame)
    {
        if (result.success)
        {
            List<byte> imageBufferList = new List<byte>();
            // Copy the raw IMFMediaBuffer data into our empty byte list.
            photoCaptureFrame.CopyRawImageDataIntoBuffer(imageBufferList);

            var cameraToWorldMatrix = new Matrix4x4();
            photoCaptureFrame.TryGetCameraToWorldMatrix(out cameraToWorldMatrix);

            cameraPosition = cameraToWorldMatrix.MultiplyPoint3x4(new Vector3(0, 0, -1));
            cameraRotation = Quaternion.LookRotation(-cameraToWorldMatrix.GetColumn(2), cameraToWorldMatrix.GetColumn(1));

            Matrix4x4 projectionMatrix;
            photoCaptureFrame.TryGetProjectionMatrix(Camera.main.nearClipPlane, Camera.main.farClipPlane, out projectionMatrix);
            Matrix4x4 pixelToCameraMatrix = projectionMatrix.inverse;

            //StartCoroutine(PostFace(imageBufferList.ToArray()));
            StartCoroutine(GetVisionDataFromImages(imageBufferList.ToArray()));//, cameraToWorldMatrix,pixelToCameraMatrix));

        }
        photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);

    }

    void OnStoppedPhotoMode(UnityEngine.XR.WSA.WebCam.PhotoCapture.PhotoCaptureResult result)
    {
        photoCaptureObject.Dispose();
        photoCaptureObject = null;
    }

    private void OnPhotoModeStarted(UnityEngine.XR.WSA.WebCam.PhotoCapture.PhotoCaptureResult result)
    {
        if (result.success)
            photoCaptureObject.TakePhotoAsync(OnCapturedPhotoToMemory);
    }

    string VISIONKEY = "258dced5ef694cb3a1209b44f8b652c6"; // replace with your Computer Vision API Key

    string _computerVisionEndpoint = "https://westcentralus.api.cognitive.microsoft.com/vision/v1.0/analyze?visualFeatures=Tags,Faces";

    string faceLink = "https://westcentralus.api.cognitive.microsoft.com/vision/v1.0/analyze?visualFeatures=Faces&language=en";

    string ocrLink = "https://westcentralus.api.cognitive.microsoft.com/vision/v1.0/ocr";
    string emotionURL1 = "https://westcentralus.api.cognitive.microsoft.com/vision/v1.0/analyze?visualFeatures=Tags,Description&language=en";

    static byte[] GetImageAsByteArray(string imageFilePath)
    {
        FileStream fileStream = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read);
        BinaryReader binaryReader = new BinaryReader(fileStream);
        return binaryReader.ReadBytes((int)fileStream.Length);
    }

    IEnumerator<object> GetVisionDataFromImages(byte[] image)
    {

        Dictionary<string, string> requestHeaders = new Dictionary<string, string>
        {
            { "Ocp-Apim-Subscription-Key", VISIONKEY },
            {"Content-Type", "application/octet-stream" },
        };

        WWW www = new WWW(emotionURL1, image, requestHeaders);

        yield return www;
        string responseData = www.text; // Save the response as JSON string
        Debug.Log(responseData);
        JSONObject json = new JSONObject(responseData);

        if (json.list.Count == 0)
        {
            peopleCount.GetComponent<TextMesh>().text = "No faces found";
            yield break;
        }
       
           

        JSONObject tags = json.GetField("tags");
        JSONObject desc = json.GetField("description");
        JSONObject captions = desc.GetField("captions");

        foreach (var data in captions.list)
        {
            string description = data.GetField("text").ToString();
            status.GetComponent<TextMesh>().text = description;
            post(description);
            Debug.Log(description);
        }


        StartCoroutine(GetFaces(image));
        //StartCoroutine(GetOcr(image));

        //StartCoroutine(GetOcr(image));

        //foreach (var tag in tags.list)
        //{
        //    string name = tag.GetField("name").ToString();
        //    name = name.Replace("\"", "");
        //    if (name.Equals("text") || name.Contains("screenshot"))
        //    {
        //        StartCoroutine(GetOcr(image));
        //    }
        //    else if (name.Equals("person"))
        //    {
        //        StartCoroutine(GetFaces(image));
        //    }
        //}

    }

    async void post(string message) {
        await GetComponent<messages>().HttpPost("Vision Data: " + message);
    }

    IEnumerator GetFaces(byte[] image)
    {
        Dictionary<string, string> requestHeaders = new Dictionary<string, string>
        {
            { "Ocp-Apim-Subscription-Key", VISIONKEY },
            {"Content-Type", "application/octet-stream" },
        };

        WWW www = new WWW(faceLink, image, requestHeaders);

        yield return www;
        string responseData = www.text; // Save the response as JSON string
        Debug.Log(responseData);
        JSONObject json = new JSONObject(responseData);

        peopleCount.GetComponent<TextMesh>().text = json.GetField("faces").list.Count.ToString() + " Faces Found.";

        
    int count = 0;
        float offsetY = 0;
        foreach (var result in json.GetField("faces").list)
        {

            var p = result.GetField("faceRectangle");

            string id = string.Format("{0},{1},{2},{3}", p.GetField("left"), p.GetField("top"), p.GetField("width"), p.GetField("height"));

            try
            {
                var source = new Texture2D(0, 0);
                source.LoadImage(image);
                var dest = new Texture2D((int)p["width"].i, (int)p["height"].i);
                dest.SetPixels(source.GetPixels((int)p["left"].i, cameraResolution.height - (int)p["top"].i - (int)p["height"].i, (int)p["width"].i, (int)p["height"].i));
                dest.Apply();
                byte[] justThisFace = dest.EncodeToPNG();
                string filepath = Path.Combine(Application.persistentDataPath, "face_" + count.ToString() + ".png");
                File.WriteAllBytes(filepath, justThisFace);

                //rawImage.texture = dest;

                faceImage.GetComponent<Renderer>().material.mainTexture = dest;


                if (FindObjectOfType<CriminalDatabase>().DatabaseInitialized)
                {
                    GetCriminalFromImage(justThisFace);
                }
                // TODO: parse response (contained in `json` variable) as appropriate

            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }
    }

    async void GetCriminalFromImage(byte[] image)
    {
        CriminalDatabase.Criminal_t crim = await FindObjectOfType<CriminalDatabase>().GetCriminalFromImage(image);

        string crimResult = "";

        if (crim != null)
        {
            crimResult = FindObjectOfType<CriminalDatabase>().ProcessCriminalData(crim);
        }

        Debug.Log(crimResult);


        criminal.GetComponent<TextMesh>().text = crimResult;
        await GetComponent<messages>().HttpPost(crimResult);
    }

    IEnumerator GetOcr(byte[] image)
    {
        Dictionary<string, string> requestHeaders = new Dictionary<string, string>
        {
            { "Ocp-Apim-Subscription-Key", VISIONKEY },
            {"Content-Type", "application/octet-stream" },
        };

        WWW www = new WWW(ocrLink, image, requestHeaders);

        yield return www;
        string responseData = www.text; // Save the response as JSON string
        Debug.Log(responseData);
        JSONObject json = new JSONObject(responseData);

        string language = json.GetField("language").ToString();

        //status.GetComponent<TextMesh>().text = "Detected Text";
        

        JSONObject regions = json.GetField("regions");
        int count = 0;
        string text = "";

        

        foreach (var box in regions.list)
        {

            speak("   Text box " + (++count).ToString());
            int lineCount = 0;
            foreach (var line in box.GetField("lines").list)
            {
                speak("   Reading Line  " + (++lineCount).ToString());
                string lineString = "";
                foreach (var word in line.GetField("words").list)
                {
                    string gettext = word.GetField("text").ToString();
                    StartCoroutine(translate.Process("en", gettext));
                    Debug.Log(translate.translatedText);
                    lineString += " " + word.GetField("text").ToString();
                }

                lineString = lineString.Replace("\"", "");


                status.GetComponent<TextMesh>().text = "Translations detected -> " + lineString;
                //status_text.text += "\n" + lineString;

                //translate the line;
                speak(lineString);
                //linesToTranslate.Add(lineString);
                //Debug.Log(lineString);
            }
        }

        var response = HttpPost(text);
        //Debug.Log(linesToTranslate.Count.ToString());
        //Debug.Log(linesToTranslate[0]);
        //Debug.Log(response.ToString());
    }

    void speak(string message_)
    {
        var soundManager = GameObject.Find("Audio Manager");
        TextToSpeech textToSpeech = soundManager.GetComponent<TextToSpeech>();
        textToSpeech.Voice = TextToSpeechVoice.Mark;
        textToSpeech.StartSpeaking(message_);
    }

    IEnumerator postData(String message_)
    {
        var json_1 = Newtonsoft.Json.JsonConvert.SerializeObject(new
        {
            user = "batman",
            message = message_,
        });

        Dictionary<string, string> header = new Dictionary<string, string>
                    {
            { "Method", "POST " },
            {"Content-Type", "application/json" },

        };

        WWW www_ = new WWW("https://batman-982f9.firebaseio.com/messages.json", Encoding.UTF8.GetBytes(json_1), header);

        yield return www_;
        string responseData_ = www_.text; // Save the response as JSON string
        Debug.Log(responseData_);
    }

    void transalateData(String message_)
    {
        /*
        var json_1 = Newtonsoft.Json.JsonConvert.SerializeObject(new
        {
            message = message_,
        });

        Dictionary<string, string> header = new Dictionary<string, string>
                    {
            { "Method", "PUT " },
            {"Content-Type", "application/json" },

        };

        WWW www_ = new WWW("https://batman-982f9.firebaseio.com/translate.json", Encoding.UTF8.GetBytes(json_1), header);

        yield return www_;
        string responseData_ = www_.text; // Save the response as JSON string
        Debug.Log(responseData_);
        */

        /*
        var json_1 = Newtonsoft.Json.JsonConvert.SerializeObject(new
        {
            translate = message_,
        });
        HttpWebRequest request = WebRequest.CreateHttp("https://batman-982f9.firebaseio.com/.json");
        request.Method = "PUT";
       
        using (var writer = new StreamWriter(request.GetRequestStream()))
        {
            writer.Write(json_1);
        }

        var response = request.GetResponse() as HttpWebResponse;
        Debug.Log(response.ToString());
        */

    }


    IEnumerator<object> PostFace(byte[] imageData)
    {
        string url = "https://westcentralus.api.cognitive.microsoft.com/face/v1.0/detect?returnFaceAttributes=age,gender,headPose,smile,facialHair,glasses";
        Dictionary<string, string> requestHeaders = new Dictionary<string, string>
        {
            { "Ocp-Apim-Subscription-Key", faceAPIKey },
            {"Content-Type", "application/octet-stream" },
        };

        WWW request = new WWW(url, imageData, requestHeaders);

        yield return request;

        string response = request.text;

        JSONObject jObj = new JSONObject(response);

        //JSONObject[] jArray = JsonUtility.FromJson<JSONObject[]>(response);


        if (jObj.list.Count == 0)
        {
           status.GetComponent<TextMesh>().text = "No Faces";
            Debug.LogWarning("No face found.");
            yield break;
        }

        string faceRect = string.Empty;
        Dictionary<string, TextMesh> meshes = new Dictionary<string, TextMesh>();

        // Do this later.
        string id = "";
        for (int i = 0; i < jObj.Count; i++)
        {
            id = "\n" + jObj[i].GetField("faceId").ToString();
        }

        //GetComponentInChildren<TextMesh>().text = "Face ID: " + id;

        Debug.Log("Face ID: " + id);
    }

    void Awake()
    {
        Camera.main.nearClipPlane = 100.0f;
        recognizer = new UnityEngine.XR.WSA.Input.GestureRecognizer();
        recognizer.TappedEvent += (source, tapCount, ray) =>
        {
            OnScan();
        };

        recognizer.StartCapturingGestures();
    }

    void OnScan()
    {
        OnClear();
        //status.GetComponent<TextMesh>().text = "Scanning";
        UnityEngine.XR.WSA.WebCam.PhotoCapture.CreateAsync(false, OnPhotoCaptureCreated);
    }

    void OnClear()
    {
        /*
        //status.GetComponent<TextMesh>().text = "Ready";
O
        //Gamebject[] gameObjects = GameObject.FindGameObjectsWithTag("faceBounds");
        //foreach (GameObject enemy in gameObjects)
          //  Destroy(enemy);

        //gameObjects = GameObject.FindGameObjectsWithTag("faceText");
        foreach (GameObject enemy in gameObjects)
            Destroy(enemy);

        gameObjects = GameObject.FindGameObjectsWithTag("facePicture");
        foreach (GameObject enemy in gameObjects)S
            Destroy(enemy);

        gameObjects = GameObject.FindGameObjectsWithTag("emoteText");
        foreach (GameObject enemy in gameObjects)
            Destroy(enemy);*/
    }

    void OnReset()
    {
        Camera.main.nearClipPlane = 100;
    }

    void OnInitiate()
    {
        Camera.main.nearClipPlane = 0.85f;
    }
}
