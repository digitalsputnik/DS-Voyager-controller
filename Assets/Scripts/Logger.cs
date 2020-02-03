using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;

public class Logger : MonoBehaviour
{
    static Logger instance;

    Queue<string> _writeQueue = new Queue<string>();

    bool _writing;
    string _path;

    public static void Info(string message)
    {
        instance._writeQueue.Enqueue(AddDateToString(message));
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            var dire = Path.Combine(Application.persistentDataPath, "logs");
            if (Directory.Exists(dire) == false)
                Directory.CreateDirectory(dire);

            _path = DateTime.UtcNow.ToString("yyyy-M-dd HH-mm-ss") + ".txt";
            _path = Path.Combine(dire, _path);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (!_writing && _writeQueue.Count > 0)
        {
            new Thread(WriteThread).Start();
        }
    }

    void WriteThread()
    {
        _writing = true;
        using (StreamWriter writer = File.AppendText(instance._path))
        {
            while (_writeQueue.Count > 0)
            {
                writer.WriteLine(_writeQueue.Dequeue());
            }
        }
        _writing = false;
    }

    static string AddDateToString(string message)
    {
        return $"[{DateTime.UtcNow.ToString("yyyy.M.dd HH:mm:ss.fff")}]\n{message}\n";
    }
}