﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;

public class ExcelStore : MonoBehaviour
{
    //Privates
    private static string filePath;
    private string fileName, tempIntensities, tempSampleNumber, tempDuration;
    public bool running = false, obtainingBaseline, baselineObtained, once = true;
    
    private int prevItems = 0;

    //Data Variables
    public string ID, testType;
    public List<double>  gdlX, gdlY, gdlZ, gdrX, gdrY, gdrZ, pdl, pdr, HR, HR_Buff, GSR, GSR_Buff, ConationLevels, PredictedConation;
    //public double[] ConationLevels;
    public List<String> timeStamp, data;
    public DataGathering naos;
    public ReceiveLiveStream eyeData;
    public Brain KerasBrain;
    public double BaseLineTime = 180,HR_Base, GSR_Base;
    public float loading, time;
    
    void Start()
    {
        filePath = string.Concat(Application.dataPath, "/data/");
        naos = GetComponent<DataGathering>();
        eyeData = GetComponent<ReceiveLiveStream>();
        KerasBrain = GetComponent<Brain>();
        loading = 0;
    }

	public void FixedUpdate()
	{
        if(naos.GetConnection() && obtainingBaseline && HR_Buff.Count < BaseLineTime && once)
        {        
            print("start mouse thing");
            StartCoroutine(CalculateBaseline());
            once = false;
        }

        if(running && eyeData != null && naos.GetConnection())
        {
            if (
                eyeData.GetGDLX() != 0 &&
                eyeData.GetGDLY() != 0 &&
                eyeData.GetGDLZ() != 0 &&
                eyeData.GetGDRX() != 0 &&
                eyeData.GetGDRY() != 0 &&
                eyeData.GetGDRZ() != 0 &&
                eyeData.GetPDL() != 0 &&
                eyeData.GetPDR() != 0 &&
                naos.GetHeartRate() != 0 &&
                naos.GetGsr() != 0
                )
            {               
            gdlX.Add(eyeData.GetGDLX());
            gdlY.Add(eyeData.GetGDLY());
            gdlZ.Add(eyeData.GetGDLZ());
            gdrX.Add(eyeData.GetGDRX());
            gdrY.Add(eyeData.GetGDRY());
            gdrZ.Add(eyeData.GetGDRZ());
            pdl.Add(eyeData.GetPDL());
            pdr.Add(eyeData.GetPDR());
            HR.Add(naos.GetHeartRate() - HR_Base);
            GSR.Add(naos.GetGsr());
            PredictedConation.Add(KerasBrain.prediction);
            }
        }
	}

    private void Write(string fileName)
    {
        if (!Directory.Exists(filePath))
        {
            Directory.CreateDirectory(filePath);
        }

		string[] lines = new string[gdlX.Count];

        string headers= "GDLX, GDLY,GDLZ, GDRX, GDRY, GDRZ, PL, PR, HR, GSR, ConationLevel, PredictedConation";

        lines[0] = headers;

        for (int i = 1; i < ConationLevels.Count; i++)
        {
            lines [i] =  gdlX[i].ToString() + ","+ gdlY[i].ToString() +","+ gdlZ[i].ToString() +","+ gdrX[i].ToString() +","+ 
                         gdrY[i].ToString() +","+ gdrZ[i].ToString() +","+ pdl[i].ToString() +","+ pdr[i].ToString()+ "," + 
                         HR[i].ToString() + ","  + GSR[i].ToString() + "," + ConationLevels[i].ToString() + "," + 
                         PredictedConation[i].ToString();
        }
       
        File.WriteAllLines(filePath + fileName + ".txt", lines);
		Reset();
        lines = null;
    }

    public void RecordOff()
    {
        running = false;
        fileName = "Data" + ID;
        Write(fileName);
    }

    public void RecordOn()
    {
        InitializeVariables();
        running = true;
    }

    public void Reset()
    {
        gdlX = null;
        gdlY = null;
        gdlZ = null;
        gdrX = null;
        gdrY = null;
        gdrZ = null;
        pdl = null;
        pdr = null;
		HR = null;
		GSR = null;
		fileName = null;
		ID = null;
		running = false;       
    }

    public void InitializeVariables()
    {
        gdlX = new List<double>();
        gdlY = new List<double>();
        gdlZ = new List<double>();
        gdrX = new List<double>();
        gdrY = new List<double>();
        gdrZ = new List<double>();
        pdl = new List<double>();
        pdr = new List<double>();
        HR = new List<double>();
        GSR = new List<double>();
        filePath = string.Concat(Application.dataPath, "/data/");
        naos = GetComponent<DataGathering>();
        eyeData = GetComponent<ReceiveLiveStream>();
        KerasBrain = GetComponent<Brain>();
    }

    public IEnumerator CalculateBaseline()
    {
         time = 0;
        while (time < 20)
        {
            time += Time.deltaTime;
            HR_Buff.Add(naos.GetHeartRate());
	        GSR.Add(naos.GetGsr());
            loading = HR_Buff.Count;
            yield return new WaitForSeconds(0.001f);
        }
        HR_Base = HR_Buff.Average(); //move decimal point 2 
        GSR_BAse = GSR_Buff.Average();
        obtainingBaseline = false;
        baselineObtained = true;
    }

    public void ObtainBaseline()
    {
        HR_Buff = new List<double>();
        HR_Base = 0;
        obtainingBaseline = true;
    }

    public void PlaceLabels(float ConationLevel)
    {
        var items = gdlX.Count;
        for (int i = prevItems; i < items-1; i++)
        {
            print(ConationLevel);
            ConationLevels.Add(ConationLevel);
        }
        prevItems = gdlX.Count;
    }
}
