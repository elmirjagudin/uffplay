using System;
using System.IO;
using UnityEngine;

[Serializable]
class PersistedData
{
    public string LastVideoFile = "";
}

public class Persisted
{
    const string FILE = "persist.json";

    static PersistedData _data = null;
    static PersistedData data
    {
        get
        {
            if (_data == null)
            {
                _data = Load();
            }
            return _data;
        }
    }

    static string DataFilePath()
    {
        return Path.Combine(Application.persistentDataPath, FILE);
    }

    static PersistedData Load()
    {
        var filePath = DataFilePath();

        if (!File.Exists(filePath))
        {
            /* no persisted values found, use defaults */
            return new PersistedData();
        }

        return JsonUtility.FromJson<PersistedData>(File.ReadAllText(filePath));
    }

    static void Save()
    {
        using (var fileWriter = new StreamWriter(DataFilePath()))
        {
            fileWriter.Write(JsonUtility.ToJson(data, true));
        }

    }

    public static string LastVideoFile
    {
        get { return data.LastVideoFile; }
        set
        {
            data.LastVideoFile = value;
            Save();
        }
    }

}
