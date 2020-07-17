using System;
using System.Collections.Generic;
using UnityEngine;

namespace DigitalSputnik
{
    public class MainThreadRunner : MonoBehaviour
    {
        #region Singleton
        public static MainThreadRunner Instance
        {
            get
            {
                if (_instance == null) Initialize();
                return _instance;
            }
        }

        private static void Initialize()
        {
            _instance = FindObjectOfType<MainThreadRunner>();

            if (_instance != null) return;
            
            var obj = new GameObject("Main Thread Runner");
            _instance = obj.AddComponent<MainThreadRunner>();
        }

        private static MainThreadRunner _instance;
        #endregion

        private readonly Queue<Action> _actions = new Queue<Action>();

        public void EnqueueAction(Action action)
        {
            lock (_actions) _actions.Enqueue(action);
        }
        
        private void Update()
        {
            lock (_actions)
            {
                while(_actions.Count > 0)
                    _actions.Dequeue()?.Invoke();   
            }
        }
    }
}