using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System;

public static class AssetDataBase
{
    public static void SaveAsset(string path, object obj)
    {
        try
        {
            if (File.Exists(Application.persistentDataPath + path))
                Debug.LogWarning("save warning: " + "file already exists, override");
            FileStream file = File.Create(Application.persistentDataPath + path);
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(file, obj);
            file.Close();
        }
        catch (Exception e)
        {
            Debug.LogError("save error：" + e.Message);
        }
    }

    public static T LoadAsset<T>(string path)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        object obj = null;
        try
        {
            FileStream file = File.Open(Application.persistentDataPath + path, FileMode.Open, FileAccess.Read);
            obj = formatter.Deserialize(file);
            file.Close();
        }
        catch (Exception e)
        {
            Debug.LogError("load error：" + e.Message);
        }
        return (T)obj;
    }
}
