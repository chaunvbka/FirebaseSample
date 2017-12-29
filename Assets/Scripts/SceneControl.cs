using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SceneControl : MonoBehaviour
{
    public Button btnSignup;
    public InputField inputMail;
    public InputField inputPass;

    // Use this for initialization
    void Start()
    {
        DebugManager.Instance.Create();
        FirebaseManager.Instance.Create();
        FirebaseManager.Instance.CheckDependenciesAndInitFirebase();

        if (btnSignup != null)
        {
            btnSignup.onClick.AddListener(() =>
            {
                SignUpEmailPassword();
            });
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SignInAnonymous()
    {
        FirebaseManager.Instance.SignInAnonymous();
    }

    public void SignOut()
    {
        FirebaseManager.Instance.SignOut();
    }

    public void SignUpEmailPassword()
    {
        //Debug.Log("===: " + inputMail.text);
        string email = "";
        string password = "";

        if (inputMail != null && inputPass != null)
        {
            email = inputMail.text;
            password = inputPass.text;
            FirebaseManager.Instance.SignUpEmailPassword(email, password);
        }
    }

    public void ClearLog()
    {
        DebugManager.Instance.ClearLog();
    }

    //private void OnGUI()
    //{

    //}
}
