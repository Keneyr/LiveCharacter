using UnityEngine;
using System.IO;
using System;
using System.Xml.Serialization;

public static class AssetDataBase
{
    public static void SaveAsset<T>(string path, object obj)
    {
        try
        {
            if (File.Exists(Application.persistentDataPath + path))
                Debug.LogWarning("save warning: " + "file already exists, override");
            // FileStream file = File.Create(Application.persistentDataPath + path);
            FileStream file = File.Open(Application.persistentDataPath + path, FileMode.Create, FileAccess.ReadWrite);
            StreamWriter sw = new StreamWriter(file, new System.Text.UTF8Encoding(false));
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            serializer.Serialize(sw, obj);
            sw.Close();
            file.Close();
        }
        catch (Exception e)
        {
            Debug.LogError("save error：" + e.Message);
        }
    }

    public static T LoadAsset<T>(string path)
    {
        object obj = null;
        try
        {
            FileStream file = File.Open(Application.persistentDataPath + path, FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(file, true);
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            obj = serializer.Deserialize(sr);
            sr.Close();
            file.Close();
        }
        catch (Exception e)
        {
            Debug.LogError("load error：" + e.Message);
        }
        return (T)obj;
    }
}