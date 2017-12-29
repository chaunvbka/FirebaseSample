using System.Collections;
using System.Collections.Generic;
using System;

using Firebase.Auth;

public class LocalFirebaseUser{
    public string Email
    {
        get;
        set;
    }
    public string UserId
    {
        get;
        set;
    }
    public IEnumerable<IUserInfo> ProviderData
    {
        get;
        set;
    }
    public string PhoneNumber
    {
        get;
        set;
    }
    public bool IsAnonymous
    {
        get;
        set;
    }
    public bool IsEmailVerified
    {
        get;
        set;
    }

    public Uri PhotoUrl
    {
        get;
        set;
    }
    public string DisplayName
    {
        get;
        set;
    }
    public string ProviderId
    {
        get;
        set;
    }
}
