using Facebook.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

public class SceneControl : MonoBehaviour
{
    public Button btnSignup;
    public InputField inputMail;
    public InputField inputPass;
    public InputField inputPhone;

    // Use this for initialization
    void Start()
    {
        DebugManager.Instance.Create();
        FirebaseManager.Instance.Create();
        FirebaseManager.Instance.CheckDependenciesAndInitFirebase();
        FacebookManager.Instance.Create();
        FacebookManager.Instance.FBInit();

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
        FacebookManager.Instance.FBLogOut();
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

    public void SignInPhoneNumber()
    {
        string phone = "";
        if (inputPhone != null)
        {
            phone = inputPhone.text;
            FirebaseManager.Instance.SignInPhoneNumber(phone);
        }
    }

    public void SignInFacebook()
    {
        FacebookManager.Instance.FBLogIn(HandleLogInResult);

    }

    private void HandleLogInResult(bool result)
    {
        if (result)
        {
            FirebaseManager.Instance.SignInFacebook();
        }
        else
        {
            DialogManager.Instance.ShowDialog("Sign-in facebook", "cannot login facebook.");
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
