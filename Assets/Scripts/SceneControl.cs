using Facebook.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

using Firebase;
using Firebase.Database;
using Firebase.Auth;
using Google;

public class SceneControl : MonoBehaviour
{
    public Button btnSignup;
    public InputField inputMail;
    public InputField inputPass;
    public InputField inputPhone;
    public InputField inputVerificationCode;

    public Text displayName;


    // Use this for initialization
    private void Awake()
    {
        DebugManager.Instance.Create();

        FirebaseManager.Instance.Create();
        FirebaseManager.Instance.CheckDependenciesAndInitFirebase();

        FacebookManager.Instance.Create();
        FacebookManager.Instance.FBInit(HandleInitFB);

        //GoogleManager.Instance.Create();
        //GoogleManager.Instance.ConfigurationGoogle();

        if (btnSignup != null)
        {
            btnSignup.onClick.AddListener(() =>
            {
                SignUpEmailPassword();
            });
        }
    }

    private void Start()
    {
        //LoginData loginData = new LoginData();
        //loginData = loginData.Read();
        //Debug.Log(loginData.signinMethod);
        //Auto sign-in after the first login.
        //if (loginData.signinMethod == LoginMethod.Anonymous)
        //{
        //    FirebaseManager.Instance.SignInAnonymous();
        //}
        
        ///Note: cannot call FBLogin here because FBInit result not complete.
        //FirebaseManager.Instance.RequestCredential();
    }

    private void HandleInitFB(bool result)
    {
        if (result)
        {
            FirebaseManager.Instance.RequestCredentialAndReauthAndRetrieveData(HandleRetrieveData);
        }
    }

    private void HandleRetrieveData(SignInResult userData)
    {
        if (userData != null)
        {
            this.displayName.text = userData.User.DisplayName;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SignInAnonymous()
    {
        //Error when run.
        //LoginData loginData = new LoginData();
        //loginData.signinMethod = LoginMethod.Anonymous;
        //loginData.Save(loginData);
        FirebaseManager.Instance.SignInAnonymous();
    }

    public void SignOut()
    {
        FirebaseManager.Instance.SignOut();

        ///If facebook login. Logout FB
        //FacebookManager.Instance.FBLogOut();
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

    public void SignInPhoneNumberWithCredential()
    {
        string verificationCode = "";
        if (inputVerificationCode != null)
        {
            verificationCode = inputVerificationCode.text;
            FirebaseManager.Instance.SignInPhoneNumberWithCredential(verificationCode);
        }
    }

    public void LinkWithGoogleAcount()
    {
        FirebaseManager.Instance.SignInLinkCredentialAsync();
    }

    public void UnlinkAuthProvider()
    {
        FirebaseManager.Instance.UnlinkAuthProvider();
    }

    public void SignInGoogle()
    {
        FirebaseManager.Instance.SignInGoogle();
        //GoogleManager.Instance.GamesSignInGoogle(HandeGoogleSignIn);
    }

    private void HandeGoogleSignIn(bool result, GoogleSignInUser data)
    {
        DebugManager.Instance.ShowLog("HandeGoogleSignIn", string.Format("result: {0}, data: {1}", result, data.DisplayName));
        if (result)
        {
            if (data != null)
            {
                string googleIdToken = data.IdToken;
                //SignInGoogleAsync(googleIdToken);
                DebugManager.Instance.ShowLog("HandeGoogleSignIn", "IdToken: " + googleIdToken + "!");
            }
        }
    }

    public void SignInGoogle1()
    {
        GoogleManager.Instance.SignInGoogle();
    }

    public void SignInGoogle2()
    {
        GoogleManager.Instance.SignInGoogleSilently();
    }

    public void SignOutGoogle()
    {
        GoogleManager.Instance.SignOutGoogle();
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
            DialogManager.Instance.ShowDialog("Sign-in facebook", "Cannot login facebook.");
        }
    }

    //private void OnGUI()
    //{

    //}
}
