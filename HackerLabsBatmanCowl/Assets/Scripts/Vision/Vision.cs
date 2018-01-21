/*
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

public class Vision : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}

    public void setup(byte[] image)
    {
        Debug.Log("Welcome to the Azure Cognitive Services - Computer Vision.");
        Debug.Log("Please enter the path of the image:");
        //string path = Console.ReadLine();

       // var image = File.ReadAllBytes(path);

        var tags = GetImageTags(image);
        tags.Wait();

        Debug.Log(string.Join(", ", tags.Result));
        Debug.Log("Press key to exit!");
    }

    public async System.Threading.Tasks.Task<string[]> GetImageTags(byte[] image)
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "ccba9139e5fc4879bb7f47ae9c903009");

        string requestParameters = "visualFeatures=Categories,Tags,Description,Faces,ImageType,Color,Adult&language=en";
        string uri = "https://westus.api.cognitive.microsoft.com/vision/v1.0/analyze?" + requestParameters;

        using (var content = new ByteArrayContent(image))
        {
            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            var response = await client.PostAsync(uri, content);
            var jsonText = await response.Content.ReadAsStringAsync();
            Debug.Log(jsonText);

            var json = JObject.Parse(jsonText);
            var captions = json.SelectToken("description.tags");

            return captions.Select(x => x.ToString()).ToArray();
        }

        return null;
    }

    public async Task<string> Ocr(byte[] image)
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "ccba9139e5fc4879bb7f47ae9c903009");
        string uri = "https://westus.api.cognitive.microsoft.com/vision/v1.0/ocr";

        using (var content = new ByteArrayContent(image))
        {
            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            var response = await client.PostAsync(uri, content);

            var jsonText = await response.Content.ReadAsStringAsync();
            return jsonText;
        }
    }

    // Update is called once per frame
    void Update () {
		
	}
} */
