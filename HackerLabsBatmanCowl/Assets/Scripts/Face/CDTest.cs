using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class CDTest : MonoBehaviour
{
    Text text;
    CriminalDatabase database;
    bool runOnce;

    void OnEnable()
    {
        text = GetComponent<Text>();
        database = FindObjectOfType<CriminalDatabase>();

        runOnce = true;
    }

    void Update()
    {
        if(database.DatabaseInitialized && runOnce)
        {
            runOnce = false;

            RunCriminalAnalysis();
        }
    }

    async void RunCriminalAnalysis()
    {
        string imageFilePath = Application.streamingAssetsPath + "\\sryantest2.jpg";
        FileStream fileStream = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read);
        BinaryReader binaryReader = new BinaryReader(fileStream);
        CriminalDatabase.Criminal_t criminal = await database.GetCriminalFromImage(binaryReader.ReadBytes((int)fileStream.Length));

        text.text = database.ProcessCriminalData(criminal);
    }
}
