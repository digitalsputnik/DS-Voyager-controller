using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VoyagerApp.UI
{
    public class MenuContainer : MonoBehaviour
    {
        [SerializeField] Transform container    = null;
        [SerializeField] Menu startMenu         = null;

        public Menu current { get; private set; }
        internal Menu[] menus;

        internal virtual void Start()
        {
            FetchMenus();

            if (startMenu != null)
                ShowMenu(startMenu);
        }

        void FetchMenus()
        {
            List<Menu> _menus = new List<Menu>();
            foreach (Transform trans in container)
            {
                Menu menu = trans.GetComponent<Menu>();
                if (menu != null)
                    _menus.Add(menu);
            }
            menus = _menus.ToArray();
        }

        public virtual void ShowMenu(Menu menu)
        {
            if (menu == current) return;

            menus.Where(_ => _.Open).ToList().ForEach(_ => _.Open = false);
            current = menu;
            if (menu != null) menu.Open = true;
        }
    }
}