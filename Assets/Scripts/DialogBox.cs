using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class DialogBox : MonoBehaviour
{
	static DialogBox dialogBox;

	public static event DialogBoxCallbackEventHandler OnUserCallback;
	public delegate void DialogBoxCallbackEventHandler(object sender, DialogBoxCallbackEventArgs e);

	[Header("UI Elements")]
	[SerializeField] GameObject container;
	[SerializeField] Text title;
	[SerializeField] Image type;
	[SerializeField] Text info;
	[SerializeField] Text ignore;
	[SerializeField] Text respond;

	[Space(5)]

    //All Importance images, based on DialogImportance enum. Sprites can be used later.
	[SerializeField] Sprite[] typeIcons = new Sprite[(int)DialogBoxType.Count];

	public bool isShowing { get; private set; }

	void Start()
	{
		dialogBox = this;

		if (container.activeInHierarchy)
			isShowing = true;
	}

	public static bool Show(DialogBoxSettings settings)
	{
		if (dialogBox.isShowing)
			return false;
            
		dialogBox.title.text = settings.Title;
		dialogBox.type.sprite = dialogBox.typeIcons[(int)settings.Type];
		dialogBox.info.text = settings.Info;
		dialogBox.ignore.text = settings.IgnoreBtnText;
		dialogBox.respond.text = settings.RespondBtnText;
		dialogBox.container.SetActive(true);
		dialogBox.isShowing = true;
		dialogBox.ignore.transform.parent.gameObject.SetActive(settings.ShowIgnoreBtn);

		return true;
	}

    public static bool ShowInfo(string info)
	{
		DialogBoxSettings settings = new DialogBoxSettings()
		{
			Title = "INFO",
			Info = info,
			Type = DialogBoxType.Info,
			RespondBtnText = "OK",
			ShowIgnoreBtn = false
        };

		return Show(settings);
	}

    public void Ignore()
	{
		dialogBox.OnUserClickButton(new DialogBoxCallbackEventArgs(DialogBoxButtonType.Ignore));
		Close();
	}

    public void Respond()
	{
		dialogBox.OnUserClickButton(new DialogBoxCallbackEventArgs(DialogBoxButtonType.Respond));
		Close();
	}

    void Close()
	{
		dialogBox.isShowing = false;
		dialogBox.container.SetActive(false);
	}
    
	protected virtual void OnUserClickButton(DialogBoxCallbackEventArgs e)
    {
		DialogBoxCallbackEventHandler handler = OnUserCallback;
        if (handler != null)
        {
            handler(this, e);
        }
    }
}

public struct DialogBoxSettings
{
	public string Title { get; set; }
	public string Info { get; set; }
	public DialogBoxType Type { get; set; }
	public string IgnoreBtnText { get; set; }
	public string RespondBtnText { get; set; }
	public bool ShowIgnoreBtn { get; set; }
}

public enum DialogBoxType
{
	Info,
    Warning,
    Error,
    Count //Not importance type, just to get count of all importances.
}

public class DialogBoxCallbackEventArgs : EventArgs
{
	public DialogBoxCallbackEventArgs(DialogBoxButtonType buttonType)
	{
		buttonClicked = buttonType;
	}

	public DialogBoxButtonType buttonClicked;
}

public enum DialogBoxButtonType
{
	Ignore,
    Respond
}