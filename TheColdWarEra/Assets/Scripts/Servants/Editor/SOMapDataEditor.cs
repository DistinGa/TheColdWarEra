﻿using UnityEngine;
using UnityEditor;
using UnityEngine.Serialization;

[CustomEditor(typeof(SOMApData))]
public class TestOnInspector : Editor
{
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button(new GUIContent("Export", "Из игры в файл")))
            Export();
        if (GUILayout.Button(new GUIContent("Import", "Из файла в игру")))
            Import();
        if (GUILayout.Button(new GUIContent("В текст", "В текст")))
            Serialize();
        if (GUILayout.Button(new GUIContent("Из текста", "Из текста")))
            DeSerialize();

        DrawDefaultInspector();
    }

    void Export()
    {
        MapData[] md = (Selection.activeObject as SOMApData).md;

        int i = 0;
        foreach (var c in GameObject.Find("Countries").GetComponentsInChildren<CountryScript>())
        {
            md[i].ObjectName = c.gameObject.name;
            md[i].name = c.Name;

            md[i].Region = c.Region;
            md[i].Authority = c.Authority;
            md[i].Score = c.Score;
            md[i].Support = c.Support;
            md[i].SovInf = c.SovInf;
            md[i].AmInf = c.AmInf;
            md[i].NInf = c.NInf;
            md[i].GovForce = c.GovForce;
            md[i].OppForce = c.OppForce;
            md[i].KGB = c.KGB;
            md[i].CIA = c.CIA;

            i++;
        }
    }

    void Import()
    {
        MapData[] md = (Selection.activeObject as SOMApData).md;
        CountryScript c;

        int i = 0;
        foreach (var item in md)
        {
            c = GameObject.Find(item.ObjectName).GetComponent<CountryScript>();

            c.Region = md[i].Region;
            c.Authority = md[i].Authority;
            c.Score = md[i].Score;
            c.Support = md[i].Support;
            c.SovInf = md[i].SovInf;
            c.AmInf = md[i].AmInf;
            c.NInf = md[i].NInf;
            c.GovForce = md[i].GovForce;
            c.OppForce = md[i].OppForce;
            c.KGB = md[i].KGB;
            c.CIA = md[i].CIA;

            i++;
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    }

    void Serialize()
    {
        SOMApData md = (Selection.activeObject as SOMApData);
        md.TextData = "";
        string str = EditorJsonUtility.ToJson(md);
        md.TextData = str;
    }

    void DeSerialize()
    {
        SOMApData md = (Selection.activeObject as SOMApData);
        EditorJsonUtility.FromJsonOverwrite(md.TextData, md);
    }
}