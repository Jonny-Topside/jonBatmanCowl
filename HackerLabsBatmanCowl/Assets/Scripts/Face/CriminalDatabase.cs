
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using System.Net;
using System.Net.Security;
using System.Threading.Tasks;
using System.Xml.Serialization;
using UnityEngine.UI;

#region EDITOR_ONLY
#if UNITY_EDITOR
using System.Security.Cryptography.X509Certificates;
#endif
#endregion

public class CriminalDatabase : MonoBehaviour
{
    List<Criminal_t> criminals;

    const string faceAPIKey = "f6633cc8206a495199dd7bcb6053afe8";

    const string personGroupID = "BatmanCriminalDatabase";

    bool databaseInitialized;

    [SerializeField]
    AudioSource source;
    [SerializeField]
    AudioClip clipWait;
    [SerializeField]
    AudioClip clipLoad;
    [SerializeField]
    AudioClip clipDone;
    [SerializeField]
    AudioClip clipBats;
    [SerializeField]
    AudioClip clipMessage;

    public bool DatabaseInitialized
    {
        get
        {
            return databaseInitialized;
        }

        set
        {
            databaseInitialized = value;
        }
    }

    void OnEnable()
    {
        source.clip = clipBats;
        source.Play();
        #region EDITOR_ONLY
#if UNITY_EDITOR
        ServicePointManager.ServerCertificateValidationCallback = MonoSecurityBypass;
#endif
        #endregion

        databaseInitialized = false;

        criminals = new List<Criminal_t>();

        InitializeDatabase();
    }

    void Update()
    {
        int x = criminals.Count;
    }

    #region EDITOR_ONLY
#if UNITY_EDITOR
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
#endif
    #endregion

    async void TestFunctionality()
    {
        string imageFilePath = Application.streamingAssetsPath + "\\sryantest2.jpg";
        FileStream fileStream = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read);
        BinaryReader binaryReader = new BinaryReader(fileStream);
        Criminal_t criminal = await GetCriminalFromImage(binaryReader.ReadBytes((int)fileStream.Length));

        Debug.LogFormat("Criminal Name: {0}", criminal.CriminalName);
    }

    public void PlayOnMessageRecieved()
    {
        source.clip = clipMessage;
        source.Play();
    }

    IEnumerator Wait()
    {
        float delta = 0;
        source.clip = clipWait;

        while(true)
        {
            if(delta >= 10.0f)
            {
                source.clip = clipDone;
                source.Play();
                databaseInitialized = true;
                yield break;
            }

            delta += 1;
            yield return new WaitForSeconds(1f);
            source.Play();
        }
    }

    async Task<bool> CheckExistingPersonGroup()
    {
        string url = "https://westcentralus.api.cognitive.microsoft.com/face/v1.0/persongroups/batcowldb";
        HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
        request.Method = "GET";
        request.Headers["Ocp-Apim-Subscription-Key"] = faceAPIKey;

        HttpWebResponse response = null;

        try
        {
            response = await request.GetResponseAsync() as HttpWebResponse;
        }
        catch (WebException e)
        {
            Debug.LogFormat("CHECK HTTP RESPONSE {0}", e.Message);
            return false;
        }

        Debug.LogFormat("CHECK HTTP RESPONSE {0} MESSAGE {1}", response.StatusCode, response.StatusDescription);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            Stream stream = response.GetResponseStream();

            string content;

            using (StreamReader reader = new StreamReader(stream))
            {
                content = await reader.ReadToEndAsync();
            }

            Debug.LogFormat("CHECK RESPONSE CONTENT {0}", content);
        }

        if (response.StatusCode == HttpStatusCode.OK)
        {
            Debug.LogFormat("CHECK REPONSE {0}, Person Group Exists.", response.StatusCode);
            return true;
        }
        else
        {
            Debug.LogFormat("CHECK RESPONSE {0}, Person Group Will Be Created.", response.StatusCode);
            return false;
        }
    }


    async Task<bool> InitializePersonGroup()
    {
        if (await CheckExistingPersonGroup())
            return true;

        string url = "https://westcentralus.api.cognitive.microsoft.com/face/v1.0/persongroups/batcowldb";

        HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
        request.Method = "PUT";
        request.ContentType = "application/json";
        request.Headers["Ocp-Apim-Subscription-Key"] = faceAPIKey;

        using (var requestStream = await Task.Factory.FromAsync<Stream>(request.BeginGetRequestStream, request.EndGetRequestStream, null))
        {
            string body = "{ \"name\" : \"" + personGroupID + "\" }";
            byte[] data = System.Text.Encoding.UTF8.GetBytes(body);
            requestStream.Write(data, 0, data.Length);
        }

        HttpWebResponse response = await request.GetResponseAsync() as HttpWebResponse;

        Debug.LogFormat("INITIALIZE HTTP RESPONSE {0} MESSAGE {1}", response.StatusCode, response.StatusDescription);

        if (response.StatusCode == HttpStatusCode.OK)
            return true;

        return false;
    }

    async Task<bool> DeletePersonGroup()
    {
        string url = "https://westcentralus.api.cognitive.microsoft.com/face/v1.0/persongroups/batcowldb";
        HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
        request.Method = "DELETE";
        request.Headers["Ocp-Apim-Subscription-Key"] = faceAPIKey;

        HttpWebResponse response = await request.GetResponseAsync() as HttpWebResponse;

        Debug.LogFormat("DELETE HTTP RESPONSE {0} MESSAGE {1}", response.StatusCode, response.StatusDescription);

        Stream stream = response.GetResponseStream();

        string content;

        using (StreamReader reader = new StreamReader(stream))
        {
            content = await reader.ReadToEndAsync();
        }

        Debug.LogFormat("DELETE RESPONSE CONTENT {0}", content);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            Debug.LogFormat("DELETE RESPONSE {0}, Person Group Deleted.", response.StatusCode);
            return true;
        }

        return false;
    }

    async void InitializeDatabase()
    {
        await DeletePersonGroup();
        File.Delete(Path.Combine(Application.dataPath, "criminalDatabase.batdb"));

        if (await InitializePersonGroup())
        {
            if (File.Exists(Path.Combine(Application.dataPath, "criminalDatabase.batdb")))
            {
                using (FileStream stream = new FileStream(Path.Combine(Application.dataPath, "criminalDatabase.batdb"), FileMode.Open, FileAccess.Read))
                {
                    XmlSerializer serializer = new XmlSerializer(criminals.GetType());

                    criminals = serializer.Deserialize(stream) as List<Criminal_t>;

                    databaseInitialized = true;
                }
            }
            else
            {
                CriminalInformation_t crim = new CriminalInformation_t();

                crim.Crimes.Add(new Crime_t(Crime_t.CrimeType.Murder, 674));
                crim.Crimes.Add(new Crime_t(Crime_t.CrimeType.Larceny, 238));
                crim.Warrants.Add(new Warrant_t(Warrant_t.WarrantType.Arrest, new Crime_t(Crime_t.CrimeType.Murder, 21)));
                crim.Warrants.Add(new Warrant_t(Warrant_t.WarrantType.Arrest, new Crime_t(Crime_t.CrimeType.Larceny, 10)));

                criminals.Add(new Criminal_t("Jonathan Ribarro", "Deathbringer", 20, Path.Combine(Application.streamingAssetsPath, "jribarro.jpg"), crim));

                crim = new CriminalInformation_t();

                crim.Crimes.Add(new Crime_t(Crime_t.CrimeType.Arson, 343));
                crim.Crimes.Add(new Crime_t(Crime_t.CrimeType.Assault, 79));
                crim.Crimes.Add(new Crime_t(Crime_t.CrimeType.Pickpocketing, 12));
                crim.Warrants.Add(new Warrant_t(Warrant_t.WarrantType.WantedQuestioning, new Crime_t(Crime_t.CrimeType.GrandLarceny, 2)));

                criminals.Add(new Criminal_t("Dusty Langeberg", "Firehazard", 18, Path.Combine(Application.streamingAssetsPath, "dlangeberg.jpg"), crim));

                crim = new CriminalInformation_t();

                crim.Crimes.Add(new Crime_t(Crime_t.CrimeType.Larceny, 27));
                crim.Crimes.Add(new Crime_t(Crime_t.CrimeType.ImpersonationOfALawEnforcementOfficer, 12));
                crim.Crimes.Add(new Crime_t(Crime_t.CrimeType.CriminalImpersonation, 54));
                crim.Crimes.Add(new Crime_t(Crime_t.CrimeType.HotProwlBurglary, 69));
                crim.Crimes.Add(new Crime_t(Crime_t.CrimeType.TooFabulous, 348));
                crim.Warrants.Add(new Warrant_t(Warrant_t.WarrantType.Arrest, new Crime_t(Crime_t.CrimeType.Abduction, 8)));

                criminals.Add(new Criminal_t("Shae Ryan", "The Chameleon", 26, Path.Combine(Application.streamingAssetsPath, "sryan.jpg"), crim));

                //crim = new CriminalInformation_t();

                //criminals.Add(new Criminal_t("", "Cameraman", 0, Path.Combine(Application.streamingAssetsPath, ".jpg"), crim));

                crim = new CriminalInformation_t();

                crim.Crimes.Add(new Crime_t(Crime_t.CrimeType.SavingTheWorld, 6749));
                crim.Warrants.Add(new Warrant_t(Warrant_t.WarrantType.WantedQuestioning, new Crime_t(Crime_t.CrimeType.SavingTheWorld, 248)));

                criminals.Add(new Criminal_t("James Hobson", "The Hacksmith", 27, Path.Combine(Application.streamingAssetsPath, "jhobson.jpg"), crim));


                for (int i = 0; i < criminals.Count; i++)
                {
                    criminals[i] = await AddPersonToGroup(criminals[i]);
                    criminals[i] = await AddFaceToPerson(criminals[i]);
                }

                if (await TrainGroup())
                    Debug.Log("TRAINING INITIALIZED");
                else
                    Debug.Log("TRAINING RUN/FAIL");

                StartCoroutine(Wait());

                using (FileStream stream = new FileStream(Path.Combine(Application.dataPath, "criminalDatabase.batdb"), FileMode.OpenOrCreate, FileAccess.Write))

                {
                    XmlSerializer serializer = new XmlSerializer(criminals.GetType());

                    serializer.Serialize(stream, criminals);
                }
            }

        }
    }

    async Task<bool> CheckForPerson(string personID)
    {
        string url = "https://westcentralus.api.cognitive.microsoft.com/face/v1.0/persongroups/batcowldb/persons";

        HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
        request.Method = "GET";
        request.ContentType = "application/json";
        request.Headers["Ocp-Apim-Subscription-Key"] = faceAPIKey;

        HttpWebResponse response = await request.GetResponseAsync() as HttpWebResponse;

        Stream stream = response.GetResponseStream();

        string content;

        using (StreamReader reader = new StreamReader(stream))
        {
            content = await reader.ReadToEndAsync();
        }

        Debug.LogFormat("CHECK PERSON CONTENT {0}", content);

        if (content.Contains(personID))
            return true;
        else
            return false;
    }

    async Task<Criminal_t> AddPersonToGroup(Criminal_t criminal)
    {
        //if (await CheckForPerson(criminal.CriminalID))
        //    return criminal;

        string url = "https://westcentralus.api.cognitive.microsoft.com/face/v1.0/persongroups/batcowldb/persons";

        HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
        request.Method = "POST";
        request.ContentType = "application/json";
        request.Headers["Ocp-Apim-Subscription-Key"] = faceAPIKey;

        using (var requestStream = await Task.Factory.FromAsync<Stream>(request.BeginGetRequestStream, request.EndGetRequestStream, null))
        {
            string body = "{ \"name\" : \"" + criminal.CriminalName + "\" }";
            byte[] data = System.Text.Encoding.UTF8.GetBytes(body);
            requestStream.Write(data, 0, data.Length);
        }

        HttpWebResponse response = await request.GetResponseAsync() as HttpWebResponse;

        Stream stream = response.GetResponseStream();
        string content;

        using (StreamReader reader = new StreamReader(stream))
        {
            content = await reader.ReadToEndAsync();
        }

        Debug.LogFormat("ADD PERSON RESPONSE CONTENT {0}", content);

        JSONObject jObj = new JSONObject(content);

        criminal.CriminalID = jObj.GetField("personId").ToString().Replace("\"", "");

        return criminal;
    }

    async Task<bool> CheckForExistingFace(string personID)
    {
        string url = "https://westcentralus.api.cognitive.microsoft.com/face/v1.0/persongroups/batcowldb/persons/" + personID;

        HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
        request.Method = "GET";
        request.ContentType = "application/json";
        request.Headers["Ocp-Apim-Subscription-Key"] = faceAPIKey;

        HttpWebResponse response = await request.GetResponseAsync() as HttpWebResponse;

        string content;

        using (StreamReader reader = new StreamReader(response.GetResponseStream()))
        {
            content = await reader.ReadToEndAsync();
        }

        JSONObject jObj = new JSONObject(content);

        if (jObj.HasField("persistedFaceIds"))
            return true;

        return false;
    }

    async Task<Criminal_t> AddFaceToPerson(Criminal_t criminal)
    {
        if (!criminal.CriminalIsNew)
            if (await CheckForExistingFace(criminal.CriminalID) && criminal.CriminalName != "Shae Ryan")
                return criminal;

        string url = "https://westcentralus.api.cognitive.microsoft.com/face/v1.0/persongroups/batcowldb/persons/" + criminal.CriminalID + "/persistedFaces";

        HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
        request.Method = "POST";
        request.ContentType = "application/octet-stream";
        request.Headers["Ocp-Apim-Subscription-Key"] = faceAPIKey;

        using (var requestStream = await Task.Factory.FromAsync<Stream>(request.BeginGetRequestStream, request.EndGetRequestStream, null))
        {
            requestStream.Write(criminal.CriminalImage, 0, criminal.CriminalImage.Length);
            //requestStream.Close();
        }

        HttpWebResponse response = null;
        try
        {
            response = await request.GetResponseAsync() as HttpWebResponse;
        }
        catch (WebException e)
        {
            string error = "";
            using (StreamReader reader = new StreamReader(e.Response.GetResponseStream()))
            {
                error = await reader.ReadToEndAsync();
            }
            Debug.LogFormat("ADD FACE ERROR {0} WITH MESSAGE {1}", e.Status, error);
            return null;
        }

        Stream stream = response.GetResponseStream();
        string jstring;

        using (StreamReader reader = new StreamReader(stream))
        {
            jstring = await reader.ReadToEndAsync();
        }

        Debug.Log(jstring);

        JSONObject jObj = new JSONObject(jstring);

        criminal.CriminalFaceID = jObj.GetField("persistedFaceId").ToString().Replace("\"", "");

        return criminal;
    }

    async Task<bool> TrainGroup()
    {
        string url = "https://westcentralus.api.cognitive.microsoft.com/face/v1.0/persongroups/batcowldb/train";

        HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
        request.Method = "POST";
        request.ContentType = "application/json";
        request.Headers["Ocp-Apim-Subscription-Key"] = faceAPIKey;

        using (var requestStream = await Task.Factory.FromAsync<Stream>(request.BeginGetRequestStream, request.EndGetRequestStream, null))
        {
            string body = "{}";
            byte[] data = System.Text.Encoding.UTF8.GetBytes(body);
            requestStream.Write(data, 0, data.Length);
        }

        HttpWebResponse response = await request.GetResponseAsync() as HttpWebResponse;

        Debug.LogFormat("TRAIN RESPONSE {0} MESSAGE {1}", response.StatusCode, response.StatusDescription);

        if (response.StatusCode == HttpStatusCode.Accepted)
            return true;

        return false;
    }

    async Task<string[]> DetectFace(byte[] image)
    {
        string url = "https://westcentralus.api.cognitive.microsoft.com/face/v1.0/detect";

        HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
        request.Method = "POST";
        request.ContentType = "application/octet-stream";
        request.Headers["Ocp-Apim-Subscription-Key"] = faceAPIKey;

        Debug.LogFormat("DETECT IMAGE UPLOAD BYTE SIZE: {0}", image.Length);

        using (var requestStream = await Task.Factory.FromAsync<Stream>(request.BeginGetRequestStream, request.EndGetRequestStream, null))
        {
            requestStream.Write(image, 0, image.Length);
            //requestStream.Close();
        }

        HttpWebResponse response = await request.GetResponseAsync() as HttpWebResponse;

        Stream stream = response.GetResponseStream();
        string content;

        using (StreamReader reader = new StreamReader(stream))
        {
            content = await reader.ReadToEndAsync();
        }

        JSONObject obj = new JSONObject(content);

        string[] faceIds = new string[obj.list.Count];

        Debug.LogFormat("DETECT RESPONSE MESSAGE {0}", content);

        for (int i = 0; i < obj.list.Count; i++)
        {
            faceIds[i] = obj[i].GetField("faceId").ToString().Replace("\"", "");
        }

        return faceIds;
    }

    async Task<Criminal_t> IdentifyCriminal(string faceID)
    {
        string url = "https://westcentralus.api.cognitive.microsoft.com/face/v1.0/identify";

        HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
        request.Method = "POST";
        request.ContentType = "application/json";
        request.Headers["Ocp-Apim-Subscription-Key"] = faceAPIKey;

        using (var requestStream = await Task.Factory.FromAsync<Stream>(request.BeginGetRequestStream, request.EndGetRequestStream, null))
        {
            string body = "{ \"personGroupId\":\"batcowldb\", \"faceIds\":[ \"" + faceID + "\" ], \"maxNumOfCandidatesReturned\":1, \"confidenceThreshold\": 0.5 }";
            byte[] data = System.Text.Encoding.UTF8.GetBytes(body);
            requestStream.Write(data, 0, data.Length);
        }

        HttpWebResponse response = await request.GetResponseAsync() as HttpWebResponse;

        Stream stream = response.GetResponseStream();
        string message;

        using (StreamReader reader = new StreamReader(stream))
        {
            message = await reader.ReadToEndAsync();
        }

        JSONObject obj = new JSONObject(message);

        Debug.LogFormat("IDENTIFY MESSAGE {0} LIST {1}", message, obj.list.Count);

        if (obj.list.Count > 0)
        {
            try
            {
                if (obj[0].GetField("candidates")[0] && obj[0].GetField("candidates")[0].GetField("confidence").f >= 0.5f)
                {
                    Criminal_t crim = criminals.Find(a => a.CriminalID.Equals(obj[0].GetField("candidates")[0].GetField("personId").ToString().Replace("\"", "")));

                    if (crim != null)
                        return crim;
                }
            }
            catch (NullReferenceException e)
            {
                Debug.LogFormat("No face found, {0}", e.Message);
                return null;
            }
        }

        return null;
    }

    async Task<Criminal_t> Identify(string[] faces)
    {
        for (int i = 0; i < faces.Length; i++)
        {
            Criminal_t criminal = await IdentifyCriminal(faces[i]);

            if (criminal != null)
                return criminal;
        }

        return null;
    }

    public async Task<Criminal_t> GetCriminalFromImage(byte[] image)
    {
        return await Identify(await DetectFace(image));
    }

    public string ProcessCriminalData(Criminal_t criminal)
    {
        string criminalData = "";

        criminalData = "Name: " + criminal.CriminalName + "\n";
        criminalData += "Alias: " + criminal.CriminalNickname + "\n";
        criminalData += "Age: " + criminal.CriminalAge + "\n";
        criminalData += "Crimes:";

        CriminalInformation_t info = criminal.CriminalInformation;

        foreach (Crime_t crime in info.Crimes)
        {
            criminalData += string.Format("\n{0} counts of {1}", crime.Count, crime.ToString());
        }

        criminalData += "\nActive Warrants:";

        foreach (Warrant_t warrant in info.Warrants)
        {
            criminalData += string.Format("\nWanted for {0} for {1} counts of {2}", warrant.ToString(), warrant.Crime.Count, warrant.Crime.ToString());
        }

        source.clip = clipLoad;
        source.Play();


        return criminalData;
    }

    #region classes
    [Serializable]
    public class Criminal_t
    {
        string criminalName;
        string criminalNickname;
        int criminalAge;
        byte[] criminalImage;
        string criminalID;
        string criminalFaceID;
        bool criminalIsNew;

        CriminalInformation_t criminalInformation;

        public Criminal_t() { }

        public Criminal_t(string name, string nickname, int age, string imagePath, CriminalInformation_t crimInfo)
        {
            criminalID = string.Empty;
            criminalFaceID = string.Empty;
            criminalName = name;
            criminalAge = age;
            criminalNickname = nickname;

            FileStream fileStream = new FileStream(imagePath, FileMode.Open, FileAccess.Read);
            BinaryReader binaryReader = new BinaryReader(fileStream);
            criminalImage = binaryReader.ReadBytes((int)fileStream.Length);
            criminalInformation = crimInfo;
            criminalIsNew = true;

        }


        public string CriminalName
        {
            get
            {
                return criminalName;
            }

            set
            {
                criminalName = value;
            }
        }

        public int CriminalAge
        {
            get
            {
                return criminalAge;
            }

            set
            {
                criminalAge = value;
            }
        }

        public byte[] CriminalImage
        {
            get
            {
                return criminalImage;
            }

            set
            {
                criminalImage = value;
            }
        }

        public string CriminalID
        {
            get
            {
                return criminalID;
            }

            set
            {
                criminalID = value;
            }
        }

        public string CriminalFaceID
        {
            get
            {
                return criminalFaceID;
            }

            set
            {
                criminalFaceID = value;
            }
        }

        public string CriminalNickname
        {
            get
            {
                return criminalNickname;
            }

            set
            {
                criminalNickname = value;
            }
        }

        public bool CriminalIsNew
        {
            get
            {
                return criminalIsNew;
            }

            set
            {
                criminalIsNew = value;
            }
        }

        public CriminalInformation_t CriminalInformation
        {
            get
            {
                return criminalInformation;
            }

            set
            {
                criminalInformation = value;
            }
        }
    }

    [Serializable]
    public class CriminalInformation_t
    {
        /// <summary>
        /// Crimes this Criminal has been officially charged with
        /// </summary>
        public List<Crime_t> Crimes
        {
            get;
            set;
        }

        /// <summary>
        /// Crimes this Criminal is wanted for
        /// </summary>
        public List<Warrant_t> Warrants
        {
            get;
            set;
        }

        public CriminalInformation_t()
        {
            Crimes = new List<Crime_t>();
            Warrants = new List<Warrant_t>();
        }
    }

    [Serializable]
    public class Warrant_t
    {

        public enum WarrantType
        {
            Arrest,
            WantedQuestioning,
        };

        public WarrantType Warrant
        {
            get;
            set;
        }

        public Crime_t Crime
        {
            get;
            set;
        }

        public Warrant_t() { }

        public Warrant_t(WarrantType t, Crime_t c)
        {
            Warrant = t;
            Crime = c;
        }

        public override string ToString()
        {
            string result;

            switch (Warrant)
            {
                case WarrantType.Arrest:
                    result = "arrest";
                    break;
                case WarrantType.WantedQuestioning:
                    result = "questioning in connection with";
                    break;
                default:
                    result = "no warrant";
                    break;
            }

            return result;
        }
    }

    [Serializable]
    public class Crime_t
    {
        public enum CrimeType
        {
            Abduction,
            AnimalCruelty,
            Arson,
            Assault,
            AttemptedMurder,
            Battery,
            Blackmail,
            BlasphemousLibel,
            BombThreat,
            Burglary,
            CapitalMurder,
            ChildAbduction,
            ChildPorn,
            Classicide,
            ConcealingBirth,
            Conspiracy,
            ConspiracyToCommitMurder,
            CriminalImpersonation,
            CriminalPossessionWeapon,
            DangerousDriving,
            DeadlyWeapon,
            DeathThreat,
            Defamation,
            Democide,
            Desertion,
            DisorderlyConduct,
            DomesticViolence,
            DUI,
            DrugPossession,
            Embezzlement,
            EmploymentFraud,
            Endangerment,
            Extortion,
            FailureToAppear,
            FailureToObeyPolice,
            FalseAccounting,
            FalseImprisonment,
            FetalAbduction,
            ForcibleEntry,
            Forgery,
            Fraud,
            Genocide,
            Ghosting,
            GrandLarceny,
            Hacking,
            Harassment,
            HatCrime,
            HotProwlBurglary,
            IdentityCleansing,
            IllegalEntry,
            IllegalImmigration,
            ImpersonationOfALawEnforcementOfficer,
            IndecentExposure,
            InsuranceFraud,
            Intimidation,
            Jaywalking,
            JuryTampering,
            Larceny,
            Loitering,
            MotorVehicleTheft,
            MovingViolation,
            Murder,
            Mutiny,
            ObscenePhoneCall,
            ObstructionOfJustice,
            ObtainingMoneyDeception,
            ObtainingPropertyDeception,
            ObtainingServiceDeception,
            Perjury,
            PettyTheft,
            PhoneCloning,
            Pickpocketing,
            PossessionOfStolenGoods,
            PublicIntoxication,
            PublicNuisance,
            Racket,
            RecklessBurning,
            RecklessEndangerment,
            RefustalToServePublicOffice,
            Robbery,
            Sabotage,
            SavingTheWorld,
            Sedition,
            Shoplifting,
            Solicitation,
            SolicitationToMurder,
            StagedCrash,
            Stalking,
            Stowaway,
            Tampering,
            Terrorism,
            TooFabulous,
            Treachery,
            Treason,
            Trespassing,
            UnlawfulAssembly,
            UnlicensedBroadcast,
            Vandalism,
            WarProfiteering,
            WebcamBlackmail,
            WitnessTampering,
        };

        public Crime_t() { }

        public Crime_t(CrimeType t, int count)
        {
            Crime = t;
            Count = count;
        }

        /// <summary>
        /// The crime committed.
        /// </summary>
        public CrimeType Crime
        {
            get;
            set;
        }

        /// <summary>
        /// Number of times the crime has been committed.
        /// </summary>
        public int Count
        {
            get;
            set;
        }

        public override string ToString()
        {
            string result;

            switch (Crime)
            {
                case CrimeType.Abduction:
                    result = "abduction";
                    break;
                case CrimeType.AnimalCruelty:
                    result = "animal cruelty";
                    break;
                case CrimeType.Arson:
                    result = "arson";
                    break;
                case CrimeType.Assault:
                    result = "assault";
                    break;
                case CrimeType.AttemptedMurder:
                    result = "attempted murder";
                    break;
                case CrimeType.Battery:
                    result = "battery";
                    break;
                case CrimeType.Blackmail:
                    result = "blackmail";
                    break;
                case CrimeType.BlasphemousLibel:
                    result = "blasphemous libel";
                    break;
                case CrimeType.BombThreat:
                    result = "falsifying a bomb threat";
                    break;
                case CrimeType.Burglary:
                    result = "burglary";
                    break;
                case CrimeType.CapitalMurder:
                    result = "capital murder";
                    break;
                case CrimeType.ChildAbduction:
                    result = "child abduction";
                    break;
                case CrimeType.ChildPorn:
                    result = "solicitation of child pornography";
                    break;
                case CrimeType.Classicide:
                    result = "classicide";
                    break;
                case CrimeType.ConcealingBirth:
                    result = "concealing birth";
                    break;
                case CrimeType.Conspiracy:
                    result = "conspiracy";
                    break;
                case CrimeType.ConspiracyToCommitMurder:
                    result = "conspiracy to commit murder";
                    break;
                case CrimeType.CriminalImpersonation:
                    result = "criminal impersonation";
                    break;
                case CrimeType.CriminalPossessionWeapon:
                    result = "criminal possession of a weapon";
                    break;
                case CrimeType.DangerousDriving:
                    result = "reckless driving";
                    break;
                case CrimeType.DeadlyWeapon:
                    result = "assault with a deadly weapon";
                    break;
                case CrimeType.DeathThreat:
                    result = "threatening to murder";
                    break;
                case CrimeType.Defamation:
                    result = "criminal defamation";
                    break;
                case CrimeType.Democide:
                    result = "democide";
                    break;
                case CrimeType.Desertion:
                    result = "desertion";
                    break;
                case CrimeType.DisorderlyConduct:
                    result = "disorderly conduct";
                    break;
                case CrimeType.DomesticViolence:
                    result = "domestic violence";
                    break;
                case CrimeType.DUI:
                    result = "driving under the influence";
                    break;
                case CrimeType.DrugPossession:
                    result = "possession of an illegal substance";
                    break;
                case CrimeType.Embezzlement:
                    result = "embezzlement";
                    break;
                case CrimeType.EmploymentFraud:
                    result = "employment fraud";
                    break;
                case CrimeType.Endangerment:
                    result = "endangerment";
                    break;
                case CrimeType.Extortion:
                    result = "extortion";
                    break;
                case CrimeType.FailureToAppear:
                    result = "failure to appear in court";
                    break;
                case CrimeType.FailureToObeyPolice:
                    result = "failure to obey police orders";
                    break;
                case CrimeType.FalseAccounting:
                    result = "false accounting";
                    break;
                case CrimeType.FalseImprisonment:
                    result = "false imprisonment";
                    break;
                case CrimeType.FetalAbduction:
                    result = "fetal abduction";
                    break;
                case CrimeType.ForcibleEntry:
                    result = "criminal forced entry";
                    break;
                case CrimeType.Forgery:
                    result = "forgery";
                    break;
                case CrimeType.Fraud:
                    result = "fraud";
                    break;
                case CrimeType.Genocide:
                    result = "genocide";
                    break;
                case CrimeType.Ghosting:
                    result = "ghosting";
                    break;
                case CrimeType.GrandLarceny:
                    result = "grand larceny";
                    break;
                case CrimeType.Hacking:
                    result = "hacking";
                    break;
                case CrimeType.Harassment:
                    result = "harassment";
                    break;
                case CrimeType.HatCrime:
                    result = "hat crime";
                    break;
                case CrimeType.HotProwlBurglary:
                    result = "hot prowl burglary";
                    break;
                case CrimeType.IdentityCleansing:
                    result = "identity cleansing";
                    break;
                case CrimeType.IllegalEntry:
                    result = "breaking and entering";
                    break;
                case CrimeType.IllegalImmigration:
                    result = "illegal immigration";
                    break;
                case CrimeType.ImpersonationOfALawEnforcementOfficer:
                    result = "impersonation of a law enforcement officer";
                    break;
                case CrimeType.IndecentExposure:
                    result = "indecent exposure";
                    break;
                case CrimeType.InsuranceFraud:
                    result = "insurance fraud";
                    break;
                case CrimeType.Intimidation:
                    result = "intimidation";
                    break;
                case CrimeType.Jaywalking:
                    result = "jaywalking";
                    break;
                case CrimeType.JuryTampering:
                    result = "jury tampering";
                    break;
                case CrimeType.Larceny:
                    result = "larceny";
                    break;
                case CrimeType.Loitering:
                    result = "loitering";
                    break;
                case CrimeType.MotorVehicleTheft:
                    result = "motor vehicle theft";
                    break;
                case CrimeType.MovingViolation:
                    result = "moving violation";
                    break;
                case CrimeType.Murder:
                    result = "murder";
                    break;
                case CrimeType.Mutiny:
                    result = "mutiny";
                    break;
                case CrimeType.ObscenePhoneCall:
                    result = "making an obscene phone call";
                    break;
                case CrimeType.ObstructionOfJustice:
                    result = "obstruction of justice";
                    break;
                case CrimeType.ObtainingMoneyDeception:
                    result = "obtaining money via deception";
                    break;
                case CrimeType.ObtainingPropertyDeception:
                    result = "obtaining property via deception";
                    break;
                case CrimeType.ObtainingServiceDeception:
                    result = "obtaining a service via deception";
                    break;
                case CrimeType.Perjury:
                    result = "perjury";
                    break;
                case CrimeType.PettyTheft:
                    result = "petty theft";
                    break;
                case CrimeType.PhoneCloning:
                    result = "phone cloning";
                    break;
                case CrimeType.Pickpocketing:
                    result = "pickpocketing";
                    break;
                case CrimeType.PossessionOfStolenGoods:
                    result = "possession of stolen goods";
                    break;
                case CrimeType.PublicIntoxication:
                    result = "public intoxication";
                    break;
                case CrimeType.PublicNuisance:
                    result = "being a nuisance to the public";
                    break;
                case CrimeType.Racket:
                    result = "racketeering";
                    break;
                case CrimeType.RecklessBurning:
                    result = "reckless burning";
                    break;
                case CrimeType.RecklessEndangerment:
                    result = "reckless endangerment";
                    break;
                case CrimeType.RefustalToServePublicOffice:
                    result = "refusing to serve a public office";
                    break;
                case CrimeType.Robbery:
                    result = "robbery";
                    break;
                case CrimeType.Sabotage:
                    result = "sabotage";
                    break;
                case CrimeType.SavingTheWorld:
                    result = "saving the world";
                    break;
                case CrimeType.Sedition:
                    result = "sedition";
                    break;
                case CrimeType.Shoplifting:
                    result = "shoplifting";
                    break;
                case CrimeType.Solicitation:
                    result = "solicitation";
                    break;
                case CrimeType.SolicitationToMurder:
                    result = "solicitation to murder";
                    break;
                case CrimeType.StagedCrash:
                    result = "staging an accident";
                    break;
                case CrimeType.Stalking:
                    result = "stalking";
                    break;
                case CrimeType.Stowaway:
                    result = "stowaway";
                    break;
                case CrimeType.Tampering:
                    result = "criminal tampering";
                    break;
                case CrimeType.Terrorism:
                    result = "terrorism";
                    break;
                case CrimeType.TooFabulous:
                    result = "being too fabulous";
                    break;
                case CrimeType.Treachery:
                    result = "criminal treachery";
                    break;
                case CrimeType.Treason:
                    result = "treason";
                    break;
                case CrimeType.Trespassing:
                    result = "trespassing";
                    break;
                case CrimeType.UnlawfulAssembly:
                    result = "unlawful assembly";
                    break;
                case CrimeType.UnlicensedBroadcast:
                    result = "unlicensed broadcasting";
                    break;
                case CrimeType.Vandalism:
                    result = "criminal vandalism";
                    break;
                case CrimeType.WarProfiteering:
                    result = "profiteering from war";
                    break;
                case CrimeType.WebcamBlackmail:
                    result = "webcam blackmail";
                    break;
                case CrimeType.WitnessTampering:
                    result = "witness tampering";
                    break;
                default:
                    result = "none";
                    break;
            }

            return result;
        }
    }

    #endregion
}
