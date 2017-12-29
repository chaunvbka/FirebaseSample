using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogNative : Singleton<DialogNative> {

    public void ShowDialog(string title, string message)
    {
        //MNPopup popup = new MNPopup("title", "dialog message");
        MNPopup popup = new MNPopup(title, message);
        popup.AddAction("Ok", () => {
            Debug.Log("OK action callback");
        });
        popup.AddAction("Cancel", () => {
            Debug.Log("Cancel action callback");
        });
        popup.AddDismissListener(() => {
            Debug.Log("dismiss listener");
        });
        popup.Show();
    }

    public void ShowPreloader(string title, string message)
    {
        MNP.ShowPreloader(title, message);
        Invoke("OnPreloaderTimeOut", 3f);
    }

    private void OnPreloaderTimeOut()
    {
        MNP.HidePreloader();
    }
}
