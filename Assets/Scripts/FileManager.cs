using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class FileManager<T>
{
    /// <summary>
    /// Writes the instance of this class to the specified file in JSON format.
    /// </summary>
    /// <param name="filePath">The file name and full path to write to.</param>
    public static void WriteToFile(string filePath, T data)
    {
        string json = JsonUtility.ToJson(data, true);
        DebugManager.Instance.ShowLog("WriteToFile", string.Format("data: {0}", json));
        File.WriteAllText(filePath, json);
    }

    /// <summary>
    /// Returns a new T object read from the data in the specified file.
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static T ReadFromFile(string filePath)
    {
        // If the file doesn't exist then just return the default T object.
        if (!File.Exists(filePath))
        {
            DebugManager.Instance.ShowLog("ReadFromFile", string.Format("ReadFromFile({0}) -- file not found, returning new object", filePath));
            return default(T);
        }
        else
        {
            // If the file does exist then read the entire file to a string.
            string contents = File.ReadAllText(filePath);

            // If it happens that the file is somehow empty then tell us and return the default T object.
            if (string.IsNullOrEmpty(contents))
            {
                DebugManager.Instance.ShowLog("ReadFromFile", string.Format("File: '{0}' is empty. Returning default object", filePath));
                return default(T);
            }

            // Otherwise we can just use JsonUtility to convert the string to a new T object.
            return JsonUtility.FromJson<T>(contents);
        }
    }
}
