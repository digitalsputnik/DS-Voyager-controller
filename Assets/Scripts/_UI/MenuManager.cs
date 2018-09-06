using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
	public Menu current;

	void Start()
	{
		current.Show();
	}

	public void ShowMenu(Menu menu)
	{
		current.Hide();
		current = menu;
        menu.Show();
	}
}