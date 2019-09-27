using System.Collections.Generic;
using UnityEngine;
using VoyagerApp.Workspace.Views;

namespace VoyagerApp.Workspace
{
    public class WorkspaceSelection : MonoBehaviour
    {
        #region Singleton
        public static WorkspaceSelection instance;
        void Awake() => instance = this;
        #endregion

        public List<LampItemView> Selected => selected;

        public bool Enabled
        {
            get => selectingEnabled;
            set => selectingEnabled = value;
        }

        public bool ShowSelection
        {
            get => showSelection;
            set
            {
                showSelection = value;
                if (value)
                    selected.ForEach(_ => _.Select());
                else
                    selected.ForEach(_ => _.Deselect());
            }
        }

        public delegate void SelectionHandler(WorkspaceSelection selection);
        public event SelectionHandler onSelectionChanged;

        //[SerializeField] float clickTime                = 0.2f;
        [SerializeField] List<LampItemView> selected    = new List<LampItemView>();
        [SerializeField] bool selectingEnabled;
        [SerializeField] bool showSelection;
        
        Camera cam;
        float mouseDownTime;

        public void SelectLamp(LampItemView view)
        {
            if (!Selected.Contains(view))
            {
                view.Select();
                selected.Add(view);
                onSelectionChanged?.Invoke(this);
            }
        }

        public void DeselectLamp(LampItemView view)
        {
            if (Selected.Contains(view))
            {
                view.Deselect();
                selected.Remove(view);
                onSelectionChanged?.Invoke(this);
            }
        }

        public void Clear()
        {
            selected.ForEach(s => s.Deselect());
            selected.Clear();
            onSelectionChanged?.Invoke(this);
        }

        void Start()
        {
            cam = Camera.main;
            WorkspaceManager.instance.onItemRemoved += Instance_onItemRemoved;
        }

        private void Instance_onItemRemoved(WorkspaceItemView item)
        {
            if (item is LampItemView lampView)
            {
                if (selected.Contains(lampView))
                    selected.Remove(lampView);
            }
        }

        void Update()
        {
            if (!selectingEnabled) return;

            //if (Input.GetMouseButtonDown(0))
            //    mouseDownTime = Time.time;

            //if (Input.GetMouseButtonUp(0))
            //{
            //    if (Time.time - mouseDownTime < clickTime)
            //        CheckOnLamp();
            //}
        }

        bool CheckOnLamp()
        {
            Vector2 point = cam.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(point, Vector2.zero);
            if (hit.collider != null)
            {
                var lamp = hit.collider.GetComponentInParent<LampItemView>();
                if (lamp != null) HandleLampSelection(lamp);
                return true;
            }

            return false;
        }

        void HandleLampSelection(LampItemView lamp)
        {
            if (lamp.Selected)
            {
                lamp.Deselect();
                selected.Remove(lamp);
            }
            else
            {
                lamp.Select();
                selected.Add(lamp);
            }

            onSelectionChanged?.Invoke(this);
        }
    }
}