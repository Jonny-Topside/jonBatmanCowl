/*
using Firebase;
using Firebase.Database;
using Firebase.Storage;
using Firebase.Unity.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Database : MonoBehaviour {
    DatabaseReference mDatabase;
    StorageReference mStorage;

    // Use this for initialization
    void Start () {
        InitializeFirebase();

        // Get the root reference location of the database.
        

        string key = mDatabase.Child("scores").Push().Key;
        LeaderBoardEntry entry = new LeaderBoardEntry("AS", 55);
        Dictionary<string, System.Object> entryValues = entry.ToDictionary();

        Dictionary<string, System.Object> childUpdates = new Dictionary<string, System.Object>();
        childUpdates["/scores/" + key] = entryValues;
        childUpdates["/user-scores/" + "Abhijeet" + "/" + key] = entryValues;

        mDatabase.UpdateChildrenAsync(childUpdates);
    }

    protected virtual void InitializeFirebase()
    {
        FirebaseApp app = FirebaseApp.DefaultInstance;
        // NOTE: You'll need to replace this url with your Firebase App's database
        // path in order for the database connection to work correctly in editor.
        app.SetEditorDatabaseUrl("https://batman-982f9.firebaseio.com/");
        if (app.Options.DatabaseUrl != null) app.SetEditorDatabaseUrl(app.Options.DatabaseUrl);

        mDatabase = FirebaseDatabase.DefaultInstance.RootReference;
        mStorage = FirebaseStorage.DefaultInstance.RootReference;
    }

    // Update is called once per frame
    void Update () {
		
	}
}
*/