using System.Collections.Generic;

using Google;
using Google.Unity;
using System.Threading.Tasks;

using UnityEngine;

public class GoogleManager : Singleton<GoogleManager>
{

    // Copy this value from the google-service.json file.
    // oauth_client with type == 3, this is webClientId.
    private string webClientId = "992739635981-5jgc7vovnqe2fo827ccmplvddov9fr8v.apps.googleusercontent.com";
    private GoogleSignInConfiguration configuration;

    private GoogleDelegate<bool, GoogleSignInUser> GoogleSignInComplete;

    private void Awake()
    {
        //configuration = new GoogleSignInConfiguration
        //{
        //    //WebClientId = webClientId,
        //    WebClientId = webClientId,
        //    RequestIdToken = true
        //};

        ConfigurationGoogle();
    }

    public void ConfigurationGoogle()
    {
        configuration = new GoogleSignInConfiguration();
        configuration.WebClientId = webClientId;
        configuration.RequestIdToken = true;
    }

    /// <summary>
    /// Default sign in google.
    /// </summary>
    public void SignInGoogle()
    {
        GoogleSignIn.Configuration = configuration;
        GoogleSignIn.Configuration.UseGameSignIn = false;
        GoogleSignIn.Configuration.RequestIdToken = true;

        DebugManager.Instance.ShowLog("SignInGoogle", "Sign in google request.");
        GoogleSignIn.DefaultInstance.SignIn().ContinueWith(
          OnAuthenticationFinished);
    }

    internal void OnAuthenticationFinished(Task<GoogleSignInUser> task)
    {
        bool result = false;

        if (task.IsFaulted)
        {
            using (IEnumerator<System.Exception> enumerator =
                    task.Exception.InnerExceptions.GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    GoogleSignIn.SignInException error =
                            (GoogleSignIn.SignInException)enumerator.Current;
                    DebugManager.Instance.ShowLog("Error", "Got Error: " + error.Status + " " + error.Message);
                }
                else
                {
                    DebugManager.Instance.ShowLog("Error", "Got Unexpected Exception?!?" + task.Exception);
                }
            }
        }
        else if (task.IsCanceled)
        {
            DebugManager.Instance.ShowLog("Error", "Canceled");
        }
        else
        {
            DebugManager.Instance.ShowLog("Success", "DisplayName: " + task.Result.DisplayName + "!");
            DebugManager.Instance.ShowLog("Success", "IdToken: " + task.Result.IdToken + "!");
            result = true;
        }

        if (GoogleSignInComplete != null)
        {
            GoogleSignInComplete(result, task.Result);
        }
    }

    public void SignOutGoogle()
    {
        DebugManager.Instance.ShowLog("SignOutGoogle", "Sign out google request.");
        GoogleSignIn.DefaultInstance.SignOut();
    }

    public void DisconnectGoogle()
    {
        DebugManager.Instance.ShowLog("DisconnectGoogle", "Disconnect google request.");
        GoogleSignIn.DefaultInstance.Disconnect();
    }

    public void SignInGoogleSilently()
    {
        GoogleSignIn.Configuration = configuration;
        GoogleSignIn.Configuration.UseGameSignIn = false;
        GoogleSignIn.Configuration.RequestIdToken = true;

        DebugManager.Instance.ShowLog("SignInGoogleSilently", "Sign in google silently request.");
        GoogleSignIn.DefaultInstance.SignInSilently()
              .ContinueWith(OnAuthenticationFinished);
    }

    /// <summary>
    /// Google game sign in.
    /// </summary>
    /// <param name="callback"></param>
    public void GamesSignInGoogle(GoogleDelegate<bool, GoogleSignInUser> callback = null)
    {
        this.GoogleSignInComplete = callback;

        GoogleSignIn.Configuration = configuration;
        GoogleSignIn.Configuration.UseGameSignIn = true;
        GoogleSignIn.Configuration.RequestIdToken = false;

        DebugManager.Instance.ShowLog("GamesSignInGoogle", "Sign in game google request.");
        GoogleSignIn.DefaultInstance.SignIn().ContinueWith(
          OnAuthenticationFinished);
    }
}
