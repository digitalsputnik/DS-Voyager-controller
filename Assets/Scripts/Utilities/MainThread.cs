using System;
using System.Collections;
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

    public static void DispachInSeconds(float time, Action action)
    {
        Dispach(() =>
        {
            instance.StartCoroutine(IEnumDispachInSeconds(time, action));
        });
    }

    void Update()
    {
        lock (actions)
        {
            while (actions.Count > 0)
                actions.Dequeue()?.Invoke();
        }
    }

    static IEnumerator IEnumDispachInSeconds(float time, Action action)
    {
        yield return new WaitForSeconds(time);
        action?.Invoke();
    }
}