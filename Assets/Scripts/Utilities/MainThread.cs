using System;
using System.Collections.Generic;
using UnityEngine;

public class MainThread : MonoBehaviour
{
    #region Singleton
    static MainThread instance;
    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);
    }
    #endregion

    Queue<Action> actions = new Queue<Action>();

    public static void Dispach(Action action) => instance.actions.Enqueue(action);

    void Update()
    {
        lock (actions)
        {
            while (actions.Count > 0)
                actions.Dequeue()?.Invoke();
        }
    }
}