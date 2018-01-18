using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseData<T>
{
    private static string filePath;
    private static string fileName;

    public BaseData()
    {
        fileName = typeof(T) + ".json";
    }

    public virtual void Save(T data)
    {
        filePath = Application.dataPath + "/FileData/" + fileName;
        FileManager<T>.WriteToFile(filePath, data);
    }

    public virtual T Read()
    {
        filePath = Application.dataPath + "/FileData/" + fileName;
        T data = FileManager<T>.ReadFromFile(filePath);
        return data;
    }

    public override string ToString()
    {
        return base.ToString();
    }
}
