using System;
using System.Collections.Generic;
using System.Threading.Tasks;
//For using Regex to check email pattern.
using System.Text.RegularExpressions;

using UnityEngine;

using Firebase;
using Firebase.Database;
using Firebase.Auth;
using Firebase.Unity;

using Google;

//For test
//using Firebase.Unity.Editor;

public class FirebaseManager : Singleton<FirebaseManager>
{
    //private FirebaseApp firebaseApp = null;

    //For sign up new user or sign in existing user.
    private FirebaseAuth firebaseAuth = null;
    //Note Dictionary
    //For save user data when signin.
    protected Dictionary<string, FirebaseUser> currentUserData =
        new Dictionary<string, FirebaseUser>();
    // Flag set when a token is being fetched.  This is used to avoid printing the token
    // in IdTokenChanged() when the user presses the get token button.
    //private bool fetchingToken = false;

    private DependencyStatus dependencyStatus = DependencyStatus.UnavailableOther;
    private string verificationID = "";

    private FirebaseDelegate<SignInResult> RetrieveDataWithReauthDelegate;

    //public override void Create()
    //{
    //}

    // When the app starts, check to make sure that we have
    // the required dependencies to use Firebase, and if not,
    // add them if possible.
    public void CheckDependenciesAndInitFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                InitFirebase();
            }
            else
            {
                DebugManager.Instance.ShowLog("CheckDependencies", "Could not resolve all Firebase dependencies: "
                    + dependencyStatus);
            }
        });
    }

    public void ShowDependenciesStatus()
    {
        if (dependencyStatus != DependencyStatus.Available)
        {
            GUILayout.Label("One or more Firebase dependencies are not present.");
            GUILayout.Label("Current dependency status: " + dependencyStatus.ToString());
            return;
        }
    }

    private void InitFirebase()
    {
        //#if  UNITY_EDITOR
        //firebaseApp = FirebaseApp.DefaultInstance;
        //Debug.LogError("===name: " + FirebaseApp.DefaultName);
        //firebaseApp.SetEditorDatabaseUrl("https://fir-sample-734d1.firebaseio.com");
        //new System.Uri("https://wh-unity-test.firebaseio.com");
        //#else

        //#endif
        // fireInstance = FirebaseApp.GetInstance("appsample");
        //fireInstance.Options.ProjectId = "fir-sample-734d1";

        //DebugLog("Setting up Firebase Auth");
        firebaseAuth = FirebaseAuth.DefaultInstance;
        firebaseAuth.StateChanged += AuthStateChanged;
        firebaseAuth.IdTokenChanged += IdTokenChanged;

        //Get the currently signed-in user. 
        //AuthStateChanged(this, null);
    }

    public void RequestCredentialAndReauthAndRetrieveData(FirebaseDelegate<SignInResult> callback = null)
    {
        this.RetrieveDataWithReauthDelegate = callback;
        //If login facebook
        FacebookManager.Instance.FBLogIn(HandleLogInFB);
        //If login google

    }

    private void HandleLogInFB(bool result)
    {
        DebugManager.Instance.ShowLog("GetCredential Facebook", "result: " + result);
        if (result)
        {
            string accessToken = FacebookManager.Instance.GetAccessTokenFacebook();
            if (string.IsNullOrEmpty(accessToken))
            {
                //DialogManager.Instance.ShowDialog("GetCredential Facebook", "AccessToken facebook null");
                DebugManager.Instance.ShowLog("GetCredential Facebook", "AccessToken facebook null");
                return;
            }

            //credential facebook
            Credential credential = FacebookAuthProvider.GetCredential(accessToken);

            DebugManager.Instance.ShowLog("GetCredential Facebook", "credential: " + credential.Provider);
            ReauthenticateAndRetrieveData(credential);
        }
        else
        {
            //DialogManager.Instance.ShowDialog("Sign-in facebook", "Cannot login facebook.");
            DebugManager.Instance.ShowLog("Sign-in facebook", "Cannot login facebook.");
        }
    }

    private Task ReauthenticateAndRetrieveData(Credential credential)
    {
        if (firebaseAuth != null)
        {
            if (credential == null)
            {
                return Task.FromResult<SignInResult>(null);
            }

            Task task = firebaseAuth.CurrentUser
                .ReauthenticateAndRetrieveDataAsync(credential).ContinueWith(HandleRetrieveUserData);
            //DialogManager.Instance.ShowPreloader("ReAuth", "Waiting for reauthenticate...");
            DebugManager.Instance.ShowLog("ReAuth", "ReAuth operation request.");
            return task;
        }
        else
        {
            return Task.FromResult<SignInResult>(null);
        }
    }

    private void HandleRetrieveUserData(Task<SignInResult> userData)
    {
        bool complete = TaskCompletion(userData, "TaskCompletion-->ReAuth");
        if (complete)
        {
            SignInResult result = userData.Result;
            DebugManager.Instance.ShowLog("GetReAuthData", "SignInResult: " + result.User.ProviderId);
            if (this.RetrieveDataWithReauthDelegate != null)
            {
                this.RetrieveDataWithReauthDelegate(result);
            }
        }
        else
        {
            if (this.RetrieveDataWithReauthDelegate != null)
            {
                this.RetrieveDataWithReauthDelegate(null);
            }
        }
    }

    #region Sign in anonymous
    /// <summary>
    /// Asynchronously creates and becomes an anonymous user.
    /// Request anonymous sign-in and wait until asynchronous call completes.
    /// </summary>
    public Task SignInAnonymous()
    {
        if (firebaseAuth != null)
        {
            //Debug.LogError("==========");
            Task task = firebaseAuth.SignInAnonymouslyAsync().ContinueWith(HandleSignInAnonymous);
            //DialogManager.Instance.ShowPreloader("Sign-in", "Waiting for sign in...");
            DebugManager.Instance.ShowLog("Sign-in", "Sign-in operation request.");
            return task;
        }
        else
        {
            return Task.FromResult(0);
        }
    }

    private void HandleSignInAnonymous(Task<FirebaseUser> authTask)
    {
        TaskCompletion(authTask, "TaskCompletion-->Sign-in");
    }

    #endregion

    #region Sign up new user
    /// <summary>
    /// Sign up new user with email and password
    /// Email format include alphanumeric(a character that is either a letter or a number), and dot.
    /// A simple address format of user@host.
    /// Password is at least 8 character(include alphanumeric and any symbol).
    /// </summary>
    public Task SignUpEmailPassword(string email, string password)
    {
        if (firebaseAuth != null)
        {
            //bool checkEmail = CheckEmail(email);
            //if (checkEmail == false)
            //{
            //    return Task.FromResult(0);
            //}

            //bool checkPass = CheckPassword(password);
            //if (checkPass == false)
            //{
            //    return Task.FromResult(0);
            //}

            // This passes the current displayName through to HandleCreateUserAsync
            // so that it can be passed to UpdateUserProfile().  displayName will be
            // reset by AuthStateChanged() when the new user is created and signed in.
            //string newDisplayName = displayName;
            Task task = firebaseAuth.CreateUserWithEmailAndPasswordAsync(email, password)
                .ContinueWith(HandleCreateUserAsync);
            //DialogManager.Instance.ShowPreloader("Sign-up", "Waiting for sign up...");
            DebugManager.Instance.ShowLog("Sign-up", "Sign-up operation request.");
            //return firebaseAuth.CreateUserWithEmailAndPasswordAsync(email, password)
            //  .ContinueWith((task) =>
            //  {
            //      return HandleCreateUserAsync(task, newDisplayName: newDisplayName);
            //  }).Unwrap();

            return task;
        }
        else
        {
            // Nothing to update, so just return a completed Task.
            return Task.FromResult(0);
        }
    }

    private bool EmailPattern(string email)
    {
        bool compare = false;

        Regex regex = new Regex(@"^([a-z0-9])([a-z0-9.][a-z0-9.]+)@([a-z0-9]+)\.([a-z\.]{2,6})$");
        compare = regex.IsMatch(email);

        return compare;
    }

    private bool CheckEmail(string email)
    {
        bool condition = false;
        if (string.IsNullOrEmpty(email))
        {
            condition = false;
            DialogManager.Instance.ShowDialog("Email Error!", "Enter a email.");
        }
        else
        {
            int len = email.Length;
            if (len < 6)
            {
                condition = false;
                DialogManager.Instance.ShowDialog("Email Error!", "Email is at least 6 characters.");
            }
            else
            {
                bool compare = EmailPattern(email);
                if (compare)
                {
                    condition = true;
                }
                else
                {
                    condition = false;
                    DialogManager.Instance.ShowDialog("Email Error!", "Email pattern is wrong.");
                }
            }
        }
        return condition;
    }

    private bool CheckPassword(string pass)
    {
        bool condition = false;
        if (string.IsNullOrEmpty(pass))
        {
            condition = false;
            DialogManager.Instance.ShowDialog("Password Error!", "Enter a password.");
        }
        else
        {
            int len = pass.Length;
            if (len < 8)
            {
                condition = false;
                DialogManager.Instance.ShowDialog("Password Error!", "Password is at least 8 characters.");
            }
            else
            {
                condition = true;
            }
        }
        return condition;
    }

    private void HandleCreateUserAsync(Task<FirebaseUser> authTask)
    {
        //if (LogTaskCompletion(authTask, "Sign-up"))
        //{
        //    if (firebaseAuth.CurrentUser != null)
        //    {
        //        DebugManager.Instance.ShowLog("Sign-up", String.Format("User Info: {0}  {1}",
        //            firebaseAuth.CurrentUser.Email,
        //            firebaseAuth.CurrentUser.ProviderId));
        //        return UpdateUserProfileAsync(newDisplayName: newDisplayName);
        //    }
        //}
        //// Nothing to update, so just return a completed Task.
        //return Task.FromResult(0);
        bool complete = TaskCompletion(authTask, "TaskCompletion-->Sign-up");
        if (complete)
        {
            DebugManager.Instance.ShowLog("Sign-up", String.Format("User Info: {0}  {1} {2}",
                firebaseAuth.CurrentUser.Email,
                firebaseAuth.CurrentUser.ProviderId,
                firebaseAuth.CurrentUser.DisplayName));
        }
    }

    // Update the user's display name with the currently selected display name.
    public Task UpdateUserProfileAsync(string newDisplayName = null, Uri newPhotoUrl = null)
    {
        if (firebaseAuth.CurrentUser == null)
        {
            DebugManager.Instance.ShowLog("UpdateUserProfileAsync", "Not signed in, unable to update user profile");
            return Task.FromResult(0);
        }
        //displayName = newDisplayName ?? displayName;

        UserProfile userProfile = new UserProfile
        {
            DisplayName = "newDisplayName",
            PhotoUrl = firebaseAuth.CurrentUser.PhotoUrl
        };

        Task task = firebaseAuth.CurrentUser.UpdateUserProfileAsync(userProfile)
            .ContinueWith(HandleUpdateUserProfile);
        DebugManager.Instance.ShowLog("UpdateUserProfileAsync", "Update-user-profile operation request.");
        return task;
    }

    private void HandleUpdateUserProfile(Task authTask)
    {
        if (TaskCompletion(authTask, "TaskCompletion-->Update user profile"))
        {
            //DisplayDetailedUserInfo(auth.CurrentUser, 1);
            DebugManager.Instance.ShowLog("Update user profile", "Updating user profile be successful.");
        }
    }
    #endregion

    #region Sign in with email and password
    public Task SignInEmailPassword(string email, string password)
    {
        if (firebaseAuth != null)
        {
            //bool checkEmail = CheckEmail(email);
            //if (checkEmail == false)
            //{
            //    return Task.FromResult(0);
            //}

            //bool checkPass = CheckPassword(password);
            //if (checkPass == false)
            //{
            //    return Task.FromResult(0);
            //}

            Task task = firebaseAuth.SignInWithEmailAndPasswordAsync(email, password)
                .ContinueWith(HandleSignInEmailPassword);
            //DialogManager.Instance.ShowPreloader("Sign-in", "Waiting for sign in...");
            DebugManager.Instance.ShowLog("Sign-in", "Sign-in operation request.");
            return task;
        }
        return Task.FromResult(0);
    }

    private void HandleSignInEmailPassword(Task<FirebaseUser> authTask)
    {
        TaskCompletion(authTask, "TaskCompletion-->Sign-in Facebook");
    }
    #endregion

    #region Sign in with Facebook
    public Task SignInFacebook()
    {
        if (firebaseAuth != null)
        {
            string accessToken = FacebookManager.Instance.GetAccessTokenFacebook();
            if (string.IsNullOrEmpty(accessToken))
            {
                //DialogManager.Instance.ShowDialog("Sign-in Facebook", "AccessToken facebook null");
                DebugManager.Instance.ShowLog("Sign-in Facebook", "AccessToken facebook null");
                return Task.FromResult(0);
            }
            Credential credential = FacebookAuthProvider.GetCredential(accessToken);
            Task task = firebaseAuth.SignInWithCredentialAsync(credential)
                .ContinueWith(HandleSignInFacebook);
            //DialogManager.Instance.ShowPreloader("Sign-in", "Waiting for sign in...");
            DebugManager.Instance.ShowLog("Sign-in", "Sign-in facebook operation request.");

            return task;
        }
        else
        {
            // Nothing to update, so just return a completed Task.
            return Task.FromResult(0);
        }
    }

    private void HandleSignInFacebook(Task<FirebaseUser> authTask)
    {
        TaskCompletion(authTask, "TaskCompletion-->Sign-in Facebook");
    }
    #endregion

    #region Sign in with credential
    public Task SigninWithCredentialAsync(Credential credential, string signinMethod)
    {
        //Firebase.Auth.Credential cred = Firebase.Auth.EmailAuthProvider.GetCredential(email, password);
        if (firebaseAuth != null)
        {
            Task task = firebaseAuth.SignInWithCredentialAsync(credential).ContinueWith(HandleSigninWithCredential);
            //DialogManager.Instance.ShowPreloader("Sign-in " + signinMethod, "Waiting for sign in...");
            DebugManager.Instance.ShowLog("Sign-in " + signinMethod, "Sign-in operation request.");
            return task;
        }
        return Task.FromResult(0);
    }

    private void HandleSigninWithCredential(Task<FirebaseUser> authTask)
    {
        TaskCompletion(authTask, "TaskCompletion-->Sign-in PhoneNumber");
    }
    #endregion

    #region Sign in with PhoneNumber
    public Task SignInPhoneNumber(string phoneNumber)
    {
        PhoneAuthProvider provider = PhoneAuthProvider.GetInstance(firebaseAuth);
        // Set the phone authentication timeout to a minute.
        uint phoneAuthTimeoutMs = 60 * 1000;
        provider.VerifyPhoneNumber(phoneNumber, phoneAuthTimeoutMs, null,
              verificationCompleted: (credential) =>
              {
                  // Auto-sms-retrieval or instant validation has succeeded (Android only).
                  // There is no need to input the verification code.
                  // `credential` can be used instead of calling GetCredential().
                  DebugManager.Instance.ShowLog("SignInPhoneNumber", "verificationCompleted=>credential: " + credential);
              },
              verificationFailed: (error) =>
              {
                  // The verification code was not sent.
                  // `error` contains a human readable explanation of the problem.
                  DebugManager.Instance.ShowLog("SignInPhoneNumber", "verificationFailed=>error: " + error.ToString());
              },
              codeSent: (id, token) =>
              {
                  // Verification code was successfully sent via SMS.
                  // `id` contains the verification id that will need to passed in with
                  // the code from the user when calling GetCredential().
                  // `token` can be used if the user requests the code be sent again, to
                  // tie the two requests together.
                  DebugManager.Instance.ShowLog("SignInPhoneNumber", "codeSent=>id: " + id.ToString());
                  DebugManager.Instance.ShowLog("SignInPhoneNumber", "codeSent=>token: " + token.ToString());
                  verificationID = id.ToString();
                  //DialogManager.Instance.ShowDialog("SignInPhoneNumber", "Enter a verification code.");
              },
              codeAutoRetrievalTimeOut: (id) =>
              {
                  // Called when the auto-sms-retrieval has timed out, based on the given
                  // timeout parameter.
                  // `id` contains the verification id of the request that timed out.
                  DebugManager.Instance.ShowLog("SignInPhoneNumber", "codeAutoRetrievalTimeOut=>id: " + id.ToString());
              });

        return Task.FromResult(0);
    }

    public Task SignInPhoneNumberWithCredential(string verificationCode)
    {
        if (firebaseAuth != null)
        {
            if (string.IsNullOrEmpty(verificationCode))
            {
                //DialogManager.Instance.ShowDialog("Sign-in PhoneNumber", "verificationCode null");
                DebugManager.Instance.ShowLog("Sign-in PhoneNumber", "verificationCode null");
                return Task.FromResult(0);
            }

            PhoneAuthProvider provider = PhoneAuthProvider.GetInstance(firebaseAuth);
            Credential credential = provider.GetCredential(this.verificationID, verificationCode);

            Task task = firebaseAuth.SignInWithCredentialAsync(credential)
                    .ContinueWith(HandleSignInPhoneNumber);

            return task;
        }
        else
        {
            return Task.FromResult(0);
        }
    }

    private void HandleSignInPhoneNumber(Task<FirebaseUser> authTask)
    {
        TaskCompletion(authTask, "TaskCompletion-->Sign-in PhoneNumber");
    }
    #endregion

    #region Link multiple Auth Providers
    //1.Sign in the user using any authentication provider or method.
    //2.Get a Firebase.Auth.Credential for the new authentication provider.
    //3.Sign in with auth.CurrentUser.LinkWithCredentialAsync(credential).
    public Task SignInLinkCredentialAsync()
    {
        if (firebaseAuth != null)
        {
            Task linkCredentical = null;
            Credential credential = null;
            GoogleSignIn.Configuration = new GoogleSignInConfiguration
            {
                RequestIdToken = true,
                // Copy this value from the google-service.json file.
                // oauth_client with type == 3
                WebClientId = "992739635981-5jgc7vovnqe2fo827ccmplvddov9fr8v.apps.googleusercontent.com"
            };
            Task<GoogleSignInUser> signIn = GoogleSignIn.DefaultInstance.SignIn();
            //TaskCompletionSource<FirebaseUser> signInCompleted = new TaskCompletionSource<FirebaseUser>();
            signIn.ContinueWith(
                task =>
                {
                    if (task.IsCanceled)
                    {
                        //signInCompleted.SetCanceled();
                        DebugManager.Instance.ShowLog("Sign in Google", "canceled");
                    }
                    else if (task.IsFaulted)
                    {
                        //signInCompleted.SetException(task.Exception);
                        DebugManager.Instance.ShowLog("Sign in Google", "faulted");
                    }
                    else
                    {
                        credential = GoogleAuthProvider.GetCredential(task.Result.IdToken, null);
                        DebugManager.Instance.ShowLog("Sign in Google", "success: " + credential.Provider);

                        DebugManager.Instance.ShowLog("Sign-in Link Goole", "Sign-in Link Goole operation request.");
                        linkCredentical = firebaseAuth.CurrentUser.LinkWithCredentialAsync(credential)
                            .ContinueWith(HandleSignInLinkCredential);
                    }
                }
                );

            return linkCredentical;
            //return Task.FromResult(0);
        }
        else
        {
            return Task.FromResult(0);
        }
    }

    private void HandleSignInLinkCredential(Task<FirebaseUser> authTask)
    {
        TaskCompletion(authTask, "TaskCompletion-->Sign-in multiple auth provider");
    }
    #endregion

    #region Unlink an auth provider from a user account
    public Task UnlinkAuthProvider()
    {
        if (firebaseAuth != null)
        {
            string email = "chaunvbka@gmail.com";
            //string providerList = "";
            //string providerIdString = "";
            Task unlinkTask = null;
            Task<IEnumerable<string>> fetchProviderTask = FirebaseAuth.DefaultInstance.FetchProvidersForEmailAsync(email);
            //TaskCompletionSource<FirebaseUser> signInCompleted = new TaskCompletionSource<FirebaseUser>();
            fetchProviderTask.ContinueWith(
                fpTask =>
                {
                    if (fpTask.IsCanceled)
                    {
                        //signInCompleted.SetCanceled();
                        DebugManager.Instance.ShowLog("Fetch provider", "canceled");
                    }
                    else if (fpTask.IsFaulted)
                    {
                        //signInCompleted.SetException(task.Exception);
                        DebugManager.Instance.ShowLog("Fetch provider", "faulted");
                    }
                    else if (fpTask.IsCompleted)
                    {
                        DebugManager.Instance.ShowLog("Fetch provider", "success");

                        foreach (string provider in fpTask.Result)
                        {
                            //providerList += provider + ", ";
                            //providerIdString = provider;

                            DebugManager.Instance.ShowLog("Fetch provider", "provider: " + provider);

                            DebugManager.Instance.ShowLog("Unlink provider", "Unlink provider operation request.");
                            unlinkTask = firebaseAuth.CurrentUser.UnlinkAsync(provider)
                                .ContinueWith(HandleUnlinkProvider);
                        }
                        //DebugManager.Instance.ShowLog("Fetch provider", "Listing providers: " + providerList);
                        //DebugManager.Instance.ShowLog("Fetch provider", "Last provider: " + providerIdString);

                        //DebugManager.Instance.ShowLog("Unlink provider", "Unlink last provider.");
                        //DebugManager.Instance.ShowLog("Unlink provider", "Unlink provider operation request.");
                        //unlinkTask = FirebaseAuth.DefaultInstance.CurrentUser.UnlinkAsync(providerIdString)
                        //    .ContinueWith(HandleUnlinkProvider);
                    }
                }
                );

            return unlinkTask;
            //return Task.FromResult(0);
        }
        else
        {
            return Task.FromResult(0);
        }
    }

    private void HandleUnlinkProvider(Task<FirebaseUser> authTask)
    {
        TaskCompletion(authTask, "TaskCompletion-->Unlink provider");
    }
    #endregion

    #region Sign in with Google
    public void SignInGoogle()
    {
        GoogleManager.Instance.GamesSignInGoogle(HandeGoogleSignIn);
    }

    private void HandeGoogleSignIn(bool result, GoogleSignInUser data)
    {
        DebugManager.Instance.ShowLog("HandeGoogleSignIn", string.Format("result: {0}, data: {1}", result, data.DisplayName));
        if (result)
        {
            if (data != null)
            {
                string googleIdToken = data.IdToken;
                SignInGoogleAsync(googleIdToken);
            }
        }
    }

    public Task SignInGoogleAsync(string googleIdToken)
    {
        if (firebaseAuth != null)
        {
            if (string.IsNullOrEmpty(googleIdToken))
            {
                //DialogManager.Instance.ShowDialog("Sign-in Google", "googleIdToken null");
                DebugManager.Instance.ShowLog("Sign-in Google", "googleIdToken null");
                return Task.FromResult(0);
            }
            Credential credential = GoogleAuthProvider.GetCredential(googleIdToken, null);
            Task task = firebaseAuth.SignInWithCredentialAsync(credential)
                .ContinueWith(HandleSignInGoogle);
            //DialogManager.Instance.ShowPreloader("Sign-in", "Waiting for sign in...");
            DebugManager.Instance.ShowLog("Sign-in", "Sign-in Google operation request.");

            return task;
        }
        else
        {
            // Nothing to update, so just return a completed Task.
            return Task.FromResult(0);
        }
    }

    private void HandleSignInGoogle(Task<FirebaseUser> authTask)
    {
        TaskCompletion(authTask, "TaskCompletion-->Sign-in Google");
    }
    #endregion

    private bool TaskCompletion(Task task, string operation)
    {
        bool complete = false;
        if (task.IsCanceled)
        {
            DebugManager.Instance.ShowLog(operation, operation + " canceled.");
        }
        else if (task.IsFaulted)
        {
            DebugManager.Instance.ShowLog(operation, operation + " faulted.");
            foreach (Exception exception in task.Exception.Flatten().InnerExceptions)
            {
                string authErrorCode = "";
                FirebaseException firebaseEx = exception as FirebaseException;
                if (firebaseEx != null)
                {
                    authErrorCode = String.Format("AuthError.{0}: ",
                      ((AuthError)firebaseEx.ErrorCode).ToString());
                }
                DebugManager.Instance.ShowLog(operation, authErrorCode + exception.ToString());
            }
        }
        else if (task.IsCompleted)
        {
            complete = true;
            DebugManager.Instance.ShowLog(operation, operation + " completed.");
        }
        return complete;
    }

    public void SignOut()
    {
        if (firebaseAuth != null)
        {
            firebaseAuth.SignOut();
            DebugManager.Instance.ShowLog("Sign-out", "Sign-out operation request.");
        }
    }

    // Track state changes of the auth object.
    void AuthStateChanged(object sender, EventArgs eventArgs)
    {
        DebugManager.Instance.ShowLog("AuthStateChanged", "AuthStateChanged.");
        //Server response.
        FirebaseAuth responseAuth = sender as FirebaseAuth;
        FirebaseUser tempUser = null;
        if (responseAuth != null)
        {
            currentUserData.TryGetValue(responseAuth.App.Name, out tempUser);
        }
        else
        {
            DebugManager.Instance.ShowLog("AuthStateChanged", "responseAuth data is null.");
        }

        if (responseAuth == firebaseAuth && responseAuth.CurrentUser != tempUser)
        {
            bool signedIn = tempUser != responseAuth.CurrentUser && responseAuth.CurrentUser != null;

            if (!signedIn && tempUser != null)
            {
                DebugManager.Instance.ShowLog("AuthStateChanged", "Signed out - local userID: " + tempUser.UserId);
                DebugManager.Instance.ShowLog("Sign-out", "Sign-out completed.");
                return;
            }

            if (signedIn)
            {
                //Save firebase data.
                tempUser = responseAuth.CurrentUser;

                if (!currentUserData.ContainsKey(responseAuth.App.Name))
                {
                    currentUserData.Add(responseAuth.App.Name, responseAuth.CurrentUser);
                }
                else
                {
                    currentUserData[responseAuth.App.Name] = responseAuth.CurrentUser;
                }

                DebugManager.Instance.ShowLog("AuthStateChanged", "Sign-in completed.");
                DebugManager.Instance.ShowLog("AuthStateChanged", "displayName: " + tempUser.DisplayName);
                DebugManager.Instance.ShowLog("AuthStateChanged", "ProviderId: " + tempUser.ProviderId);
                //DisplayDetailedUserInfo(responseUser, 1);
                return;
            }
        }
    }

    // Track ID token changes.
    void IdTokenChanged(object sender, EventArgs eventArgs)
    {
        DebugManager.Instance.ShowLog("IdTokenChanged", "IdTokenChanged.");
        FirebaseAuth responseAuth = sender as FirebaseAuth;

        //if (responseAuth == firebaseAuth && responseAuth.CurrentUser != null && !fetchingToken)
        //{
        //    responseAuth.CurrentUser.TokenAsync(false).ContinueWith(
        //      task => DebugManager.Instance.ShowLog("IdTokenChanged",
        //      String.Format("Token[0:8] = {0}", task.Result.Substring(0, 8))));
        //}
    }

    /// <summary>
    /// To write data to the Database, you need an instance of DatabaseReference
    /// </summary>
    public void GetDatabaseReference()
    {
        // Get the root reference location of the database.
        DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    private void OnGUI()
    {
        if (dependencyStatus != DependencyStatus.Available)
        {
            GUILayout.Label("One or more Firebase dependencies are not present.");
            GUILayout.Label("Current dependency status: " + dependencyStatus.ToString());
            //GUILayout.s
            return;
        }
    }

    void OnDestroy()
    {
        firebaseAuth.StateChanged -= AuthStateChanged;
        firebaseAuth.IdTokenChanged -= IdTokenChanged;
        firebaseAuth = null;
    }
}
