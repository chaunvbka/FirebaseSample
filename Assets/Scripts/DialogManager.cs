using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DialogManager : Singleton<DialogManager>
{
    public void ShowDialog(string title, string message)
    {
        DialogNative.Instance.ShowDialog(title, message);
    }

    public void ShowPreloader(string title, string message)
    {
        DialogNative.Instance.ShowPreloader(title, message);
    }
}
