using System;
using System.Collections.Generic;
using System.Threading.Tasks;
//For using Regex to check email pattern.
using System.Text.RegularExpressions;

using UnityEngine;

using Firebase;
using Firebase.Database;
using Firebase.Auth;

//For test
//using Firebase.Unity.Editor;

public class FirebaseManager : Singleton<FirebaseManager>
{

    private FirebaseApp firebaseApp = null;

    //For sign up new user or sign in existing user.
    private FirebaseAuth firebaseAuth = null;
    private FirebaseAuth otherAuth = null;
    //Note Dictionary
    //For save user data when signin.
    protected Dictionary<string, FirebaseUser> responseUserDic =
        new Dictionary<string, FirebaseUser>();
    //For save user data when signin.
    private FirebaseUser currentUser = null;
    // Flag set when a token is being fetched.  This is used to avoid printing the token
    // in IdTokenChanged() when the user presses the get token button.
    private bool fetchingToken = false;

    //Options used to setup secondary authentication object.
    private AppOptions otherAuthOptions = new AppOptions
    {
        ApiKey = "",
        AppId = "",
        ProjectId = ""
    };
    private DependencyStatus dependencyStatus = DependencyStatus.UnavailableOther;

    protected string email = "";
    protected string password = "";
    protected string displayName = "";
    protected string phoneNumber = "";
    protected string receivedCode = "";

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
                Debug.LogError(
                  "Could not resolve all Firebase dependencies: " + dependencyStatus);
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

        // Specify valid options to construct a secondary authentication object.
        //if (otherAuthOptions != null &&
        //    !(string.IsNullOrEmpty(otherAuthOptions.ApiKey) ||
        //      string.IsNullOrEmpty(otherAuthOptions.AppId) ||
        //      string.IsNullOrEmpty(otherAuthOptions.ProjectId)))
        //{
        //    try
        //    {
        //        otherAuth = FirebaseAuth.GetAuth(FirebaseApp.Create(
        //          otherAuthOptions, "Secondary"));
        //        otherAuth.StateChanged += AuthStateChanged;
        //        otherAuth.IdTokenChanged += IdTokenChanged;
        //    }
        //    catch (Exception)
        //    {
        //        Debug.Log("ERROR: Failed to initialize secondary authentication object.");
        //    }
        //}
    }

    /// <summary>
    /// Asynchronously creates and becomes an anonymous user.
    /// Request anonymous sign-in and wait until asynchronous call completes.
    /// </summary>
    public Task SignInAnonymous()
    {
        if (firebaseAuth != null)
        {
            //Debug.LogError("==========");
            Task task = firebaseAuth.SignInAnonymouslyAsync().ContinueWith(HandleSignInResult);
            DialogManager.Instance.ShowPreloader("Sign-in", "Waiting for sign in...");
            DebugManager.Instance.ShowLog("Sign-in", "Sign-in operation request.");
            return task;
        }
        else
        {
            return Task.FromResult(0);
        }
    }

    private void HandleSignInResult(Task<FirebaseUser> authTask)
    {
        TaskCompletion(authTask, "Sign-in");
    }

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
            string newDisplayName = displayName;
            Task task = firebaseAuth.CreateUserWithEmailAndPasswordAsync(email, password)
                .ContinueWith(HandleCreateUserAsync);
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
        bool complete = TaskCompletion(authTask, "Sign-up");
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
        displayName = newDisplayName ?? displayName;

        UserProfile userProfile = new UserProfile
        {
            DisplayName = displayName,
            PhotoUrl = firebaseAuth.CurrentUser.PhotoUrl
        };

        Task task = firebaseAuth.CurrentUser.UpdateUserProfileAsync(userProfile)
            .ContinueWith(HandleUpdateUserProfile);
        DebugManager.Instance.ShowLog("UpdateUserProfileAsync", "Update-user-profile operation request.");
        return task;
    }

    private void HandleUpdateUserProfile(Task authTask)
    {
        if (TaskCompletion(authTask, "Update user profile"))
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
                .ContinueWith(HandleSignInResult);
            DialogManager.Instance.ShowPreloader("Sign-in", "Waiting for sign in...");
            DebugManager.Instance.ShowLog("Sign-in", "Sign-in operation request.");
            return task;
        }
        return Task.FromResult(0);
    }
    #endregion

    /// <summary>
    /// To write data to the Database, you need an instance of DatabaseReference
    /// </summary>
    public void GetDatabaseReference()
    {
        // Get the root reference location of the database.
        DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference;
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
            DebugManager.Instance.ShowLog("AuthStateChanged", responseAuth.App.Name);

            responseUserDic.TryGetValue(responseAuth.App.Name, out tempUser);
            if (tempUser != null)
            {
                DebugManager.Instance.ShowLog("AuthStateChanged", tempUser.UserId);
                DebugManager.Instance.ShowLog("AuthStateChanged", responseUserDic[responseAuth.App.Name].UserId);
                DebugManager.Instance.ShowLog("AuthStateChanged", this.currentUser.UserId);
            }
            else
            {
                DebugManager.Instance.ShowLog("AuthStateChanged", "tempUser == null");
            }

            if (responseAuth.CurrentUser != null)
            {
                DebugManager.Instance.ShowLog("AuthStateChanged", "responseAuth.CurrentUser != null");
                if (string.IsNullOrEmpty(responseAuth.CurrentUser.UserId))
                {
                    DebugManager.Instance.ShowLog("AuthStateChanged", "userid: null");
                }
                DebugManager.Instance.ShowLog("AuthStateChanged", "userid: " + responseAuth.CurrentUser.UserId);
            }
            else
            {
                DebugManager.Instance.ShowLog("AuthStateChanged", "responseAuth.CurrentUser == null");
            }
        }
        else
        {
            DebugManager.Instance.ShowLog("AuthStateChanged", "responseAuth data is null.");
        }

        if (responseAuth == firebaseAuth && responseAuth.CurrentUser != tempUser)
        {
            bool signedIn = tempUser != responseAuth.CurrentUser && responseAuth.CurrentUser != null;
            DebugManager.Instance.ShowLog("AuthStateChanged", "Go1: " + signedIn.ToString());
            if (this.currentUser != null)
                DebugManager.Instance.ShowLog("AuthStateChanged", "Go1------: " + this.currentUser.UserId);
            if (!signedIn && tempUser != null)
            {
                DebugManager.Instance.ShowLog("AuthStateChanged", "Signed out - local userID: " + tempUser.UserId);
                DebugManager.Instance.ShowLog("Sign-out", "Sign-out completed.");
                return;
            }
            DebugManager.Instance.ShowLog("AuthStateChanged", "Go2: " + signedIn.ToString());
            if (signedIn)
            {
                DebugManager.Instance.ShowLog("AuthStateChanged", "Go3: " + signedIn.ToString());
                //Debug.Log("Signed in " + tempUser.UserId); Crash because tempUser = null.
                DebugManager.Instance.ShowLog("AuthStateChanged", "Go4: " + signedIn.ToString());
                //Save data
                tempUser = responseAuth.CurrentUser;
                //responseUserDic[responseAuth.App.Name] = tempUser;
                this.currentUser = responseAuth.CurrentUser;
                if (!responseUserDic.ContainsKey(responseAuth.App.Name))
                {
                    responseUserDic.Add(responseAuth.App.Name, responseAuth.CurrentUser);
                }
                else
                {
                    responseUserDic[responseAuth.App.Name] = responseAuth.CurrentUser;
                }

                DebugManager.Instance.ShowLog("AuthStateChanged", "Go5: " + signedIn.ToString());
                displayName = tempUser.DisplayName;
                DebugManager.Instance.ShowLog("AuthStateChanged", "Signed in " + tempUser.UserId);
                DebugManager.Instance.ShowLog("AuthStateChanged", "displayName: " + displayName);
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

        if (responseAuth == firebaseAuth && responseAuth.CurrentUser != null && !fetchingToken)
        {
            responseAuth.CurrentUser.TokenAsync(false).ContinueWith(
              task => DebugManager.Instance.ShowLog("IdTokenChanged",
              String.Format("Token[0:8] = {0}", task.Result.Substring(0, 8))));
        }
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

}
