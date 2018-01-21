using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weather : MonoBehaviour {

    public GameObject weather;
    public GameObject icon;

    const string API_KEY = "http://api.openweathermap.org/data/2.5/weather?zip=32792,us&appid=5a9834eab8978959bcfebf888918a85e";

    // Use this for initialization
    void Start () {
        StartCoroutine(PostFace());
    }

    IEnumerator<object> PostFace()
    {
      
        WWW request = new WWW(API_KEY);

        WWW www = new WWW(API_KEY);
        yield return www;
        if (www.error == null)
        {
            JSONObject jObj = new JSONObject(www.text);
            JSONObject wether = jObj.GetField("weather");
            Debug.Log(jObj.ToString());
            Debug.Log(wether.ToString());

            if(wether.Count>0)
            {
                weather.GetComponent<TextMesh>().text = wether[0].GetField("description").ToString().Replace("\"","");
                string iconCode = wether[0].GetField("icon").ToString().Replace("\"", "");
                var iconUrl = "http://openweathermap.org/img/w/" + iconCode + ".png";
                Texture2D tex;
                tex = new Texture2D(4, 4, TextureFormat.DXT1, false);
                WWW www_1 = new WWW(iconUrl);
                yield return www_1;
                www_1.LoadImageIntoTexture(tex);
                icon.GetComponent<Renderer>().material.mainTexture = tex;
            }

           
        }
        else
        {
            Debug.Log("ERROR: " + www.error);
        }
    }

    // Update is called once per frame
    void Update () {
		
	}
}
