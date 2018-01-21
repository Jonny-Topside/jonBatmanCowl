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

public class messages : MonoBehaviour {

    string from = "WuHFZ4IQ5HTIpfXpMh7CpmnFqKC2";
    string to = "z0yIsjPUEvWJVZLmPURdSx4NIr03";

    public GameObject text;

    // Use this for initialization
    void Start () {
        double dateReturn = (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds/1000;
        Debug.Log(dateReturn);

        //var post = HttpPost("hey its working");
        //sendMessage();
        // StartCoroutine(postData("Hey"));
        //Debug.Log(post);
        //getData();
        InvokeRepeating("getData", 0, 0.5f);
    }


    void sendMessage() {
        var post = HttpPost("hey its working");
    }

    public void getData()
    {
        StartCoroutine(getDataCoroutine());
    }

    string key_;
    public async Task<string> HttpPost(string message)
    {
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri("https://theapp-6afef.firebaseio.com/messages.json"));
        request.Method = "post";
        request.ContentType = "application/json";

        var json_1 = Newtonsoft.Json.JsonConvert.SerializeObject(new
        {
            fromId = from,
            text = message,
            timestamp = (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds / 1000,
            toId = to,
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

       
       JSONObject json = new JSONObject(result);


        key_ = json.GetField("name").ToString().Replace("\"", "");

        HttpWebRequest request_1 = (HttpWebRequest)WebRequest.Create(new Uri("https://theapp-6afef.firebaseio.com/user-messages/" + to + "/" + from +"/" +".json"));
        request_1.Method = "PATCH";
        request_1.ContentType = "application/json";

        JSONObject demo = new JSONObject("{ \"" + key_ + "\" : 1 }");

        Debug.Log(demo);

        var json_2 = Newtonsoft.Json.JsonConvert.SerializeObject(new {
            key_ = 1,
        });
        // Build a string with all the params, properly encoded.
        // We assume that the arrays paramName and paramVal are
        // of equal length:
        // Encode the parameters as form data:
        byte[] formData_1 =
            UTF8Encoding.UTF8.GetBytes(demo.ToString());

        Debug.Log(formData_1.ToString());
        //request.ContentLength = formData.Length;

        using (var stream = await Task.Factory.FromAsync<Stream>(request_1.BeginGetRequestStream, request_1.EndGetRequestStream, null))
        {
            stream.Write(formData_1, 0, formData_1.Length);
        }

        // Pick up the response:
        string result_1 = null;
        using (var response_1 = (HttpWebResponse)(await Task<WebResponse>.Factory.FromAsync(request_1.BeginGetResponse, request_1.EndGetResponse, null)))
        {
            StreamReader reader = new StreamReader(response_1.GetResponseStream());
            result_1 = reader.ReadToEnd();
        }


        HttpWebRequest request_2 = (HttpWebRequest)WebRequest.Create(new Uri("https://theapp-6afef.firebaseio.com/user-messages/" + from + "/" + to + "/" + ".json"));
        request_2.Method = "PATCH";
        request_2.ContentType = "application/json";

        using (var stream = await Task.Factory.FromAsync<Stream>(request_2.BeginGetRequestStream, request_2.EndGetRequestStream, null))
        {
            stream.Write(formData_1, 0, formData_1.Length);
        }

        // Pick up the response:
        string result_2 = null;
        using (var response_2 = (HttpWebResponse)(await Task<WebResponse>.Factory.FromAsync(request_2.BeginGetResponse, request_2.EndGetResponse, null)))
        {
            StreamReader reader = new StreamReader(response_2.GetResponseStream());
            result_2 = reader.ReadToEnd();
        }

        Debug.Log(result_2);

        return result;
    }

    List<Message> messagesList = new List<Message>();

    Double previosMEssage = 0;

    public IEnumerator getDataCoroutine()
    {
        messagesList.Clear();

        WWW www = new WWW("https://theapp-6afef.firebaseio.com/messages.json");
        yield return www;
        string responseData = www.text;


        JSONObject json = new JSONObject(responseData);
        
        List<string> keys = json.keys;

        if (keys != null)
        {
            foreach (var key in keys)
            {
                JSONObject text = json.GetField(key);

               messagesList.Add(new Message(text.GetField("fromId").ToString().Replace("\"",""), text.GetField("toId").ToString().Replace("\"", ""), Double.Parse(text.GetField("timestamp").ToString().Replace("\"", "")), text.GetField("text").ToString().Replace("\"", "")));
            }

            

            messagesList = messagesList.Where(x => x.to == to).ToList();
            messagesList.Sort((x, y) => x.timestamp.CompareTo(y.timestamp));

            if (previosMEssage != 0 &&!(previosMEssage == messagesList[messagesList.Count - 1].timestamp)) {
                //Debug.Log(messagesList[messagesList.Count - 1].message);
                text.GetComponent<TextMesh>().text = "Alfred :" + messagesList[messagesList.Count - 1].message;
                speak(messagesList[messagesList.Count - 1].message);
                CriminalDatabase d = FindObjectOfType<CriminalDatabase>();

                if (d != null)
                    d.PlayOnMessageRecieved();
            }
            
            previosMEssage = messagesList[messagesList.Count - 1].timestamp;
            //Debug.Log(messagesList.Count);
        }
    }

    void speak(string message_)
    {
        var soundManager = GameObject.Find("Audio Manager");
        TextToSpeech textToSpeech = soundManager.GetComponent<TextToSpeech>();
        textToSpeech.Voice = TextToSpeechVoice.Mark;
        textToSpeech.StartSpeaking(message_);
    }

    // Update is called once per frame
    void Update () {

		
	}
}

public class Message {
    public string to;
    public string from;
    public double timestamp;
    public string message;

    public Message(string to, string from, double timestamp, string message) {
        this.to = to;
        this.from = from;
        this.timestamp = timestamp;
        this.message = message;
    }

    
}
