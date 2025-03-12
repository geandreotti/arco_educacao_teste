using System;
using System.Collections.Generic;
using UnityEngine;

public class PopupsManager : MonoBehaviour
{   

    public static PopupContent SetContent(string message, string confirm, string cancel, Action onConfirm = null, Action onCancel = null)
    {
        return new PopupContent(message, confirm, cancel, onConfirm, onCancel);
    }

    public static void ShowPopup(PopupContent content)
    {
        PopupController popup = Instantiate(Resources.Load<PopupController>("Prefabs/popup_canvas"));
        popup.Setup(content);
    }
}

public class PopupContent
{
    public string message;
    public string confirm;
    public string cancel;

    public Action OnConfirm;
    public Action OnCancel;

    public PopupContent(string message, string confirm, string cancel, Action onConfirm = null, Action onCancel = null)
    {
        this.message = message;
        this.confirm = confirm;
        this.cancel = cancel;

        this.OnConfirm = onConfirm;
        this.OnCancel = onCancel;
    }


}
