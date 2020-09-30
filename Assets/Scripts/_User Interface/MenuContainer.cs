using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VoyagerController.UI
{
    public class MenuContainer : MonoBehaviour
    {
        [SerializeField] private Transform _container    = null;
        [SerializeField] private Menu _startMenu         = null;

        public Menu Current { get; private set; }
        internal Menu[] Menus;

        internal virtual void Start()
        {
            FetchMenus();

            if (_startMenu != null)
                ShowMenu(_startMenu);
        }

        private void FetchMenus()
        {
            var menus = new List<Menu>();
            
            foreach (Transform trans in _container)
            {
                var menu = trans.GetComponent<Menu>();
                if (menu != null) menus.Add(menu);
            }
            
            Menus = menus.ToArray();
        }

        public virtual void ShowMenu(Menu menu)
        {
            if (menu == Current) return;

            Menus
                .Where(m => m.Open)
                .ToList()
                .ForEach(m => m.Open = false);
            
            Current = menu;
            
            if (menu) menu.Open = true;
        }
    }
}