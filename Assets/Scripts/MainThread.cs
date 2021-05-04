using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoyagerController
{
    public class MainThread : MonoBehaviour
    {
        #region Singleton
        private static MainThread _instance;

        private void Awake()
        {
            if (_instance == null)
                _instance = this;
            else
                Destroy(this);
        }
        #endregion

        private readonly Queue<Action> _actions = new Queue<Action>();

        public static void Dispatch(Action action) => _instance._actions.Enqueue(action);

        public static void DispatchInSeconds(float time, Action action)
        {
            Dispatch(() =>
            {
                _instance.StartCoroutine(EnumDispatchInSeconds(time, action));
            });
        }

        private void Update()
        {
            lock (_actions)
            {
                while (_actions.Count > 0)
                    _actions.Dequeue()?.Invoke();
            }
        }

        private static IEnumerator EnumDispatchInSeconds(float time, Action action)
        {
            yield return new WaitForSeconds(time);
            action?.Invoke();
        }
    }   
}