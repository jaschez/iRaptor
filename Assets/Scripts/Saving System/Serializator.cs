using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public static class Serializator<T>
{
    private static readonly string SAVEDIR = "/Saved/";
    private static readonly string ABSOLUTE_PATH = Application.persistentDataPath + SAVEDIR;

    public static void Save(T element, string localPath)
    {
        string path = ABSOLUTE_PATH + localPath;

        BinaryFormatter bf = new BinaryFormatter();

        if (File.Exists(path))
        {
            File.Delete(path);
        }

        FileInfo file = new FileInfo(ABSOLUTE_PATH);
        file.Directory.Create();

        FileStream stream = new FileStream(path, FileMode.Create);
        bf.Serialize(stream, element);
        stream.Close();

    }

    public static T Load(string localPath)
    {
        string path = ABSOLUTE_PATH + localPath;

        if (File.Exists(path))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            T element = (T)bf.Deserialize(stream);

            stream.Close();

            return element;
        }
        else
        {
            Debug.LogError("Path not found");
            return default(T);
        }
    }
}
