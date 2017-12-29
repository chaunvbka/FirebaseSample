using UnityEngine;

/// <summary>
/// Be aware this will not prevent a non singleton constructor
///   such as `T myT = new T();`
/// To prevent that, add `protected T () {}` to your singleton class.
/// 
/// As a note, this is made as MonoBehaviour because we need Coroutines.
/// </summary>
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;

    private static object _lock = new object();
    private static bool applicationIsQuitting = false;

    public static T Instance
    {
        get
        {
            if (applicationIsQuitting)
            {
                Debug.LogWarning("[Singleton] Instance '" + typeof(T) +
                    "' already destroyed on application quit." +
                    " Won't create again - returning null.");
                return null;
            }

            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = (T)FindObjectOfType(typeof(T));

                    if (FindObjectsOfType(typeof(T)).Length > 1)
                    {
                        Debug.LogError("[Singleton] Something went really wrong " +
                            " - there should never be more than 1 singleton!" +
                            " Reopening the scene might fix it.");
                        return _instance;
                    }

                    if (_instance == null)
                    {
                        GameObject singleton = new GameObject();
                        _instance = singleton.AddComponent<T>();
                        singleton.name = "(singleton) " + typeof(T).ToString();

                        DontDestroyOnLoad(singleton);

                        Debug.Log("[Singleton] An instance of " + typeof(T) +
                            " is needed in the scene, so '" + singleton +
                            "' was created with DontDestroyOnLoad.");
                    }
                    else
                    {
                        Debug.Log("[Singleton] Using instance already created: " +
                            _instance.gameObject.name);
                    }
                }

                return _instance;
            }
        }
    }

    /// <summary>
    /// This function is called when the MonoBehaviour will be destroyed.
    /// OnDestroy will only be called on game objects that have previously been active.
    /// </summary>
    private void OnDestroy()
    {
        //Debug.LogWarning("[Singleton] Instance '" + typeof(T) +
        //    "' already destroyed when the MonoBehaviour destroyed.");
    }

    /// <summary>
    /// Sent to all game objects before the application is quit.
    /// In the editor this is called when the user stops playmode.
    /// </summary>
    private void OnApplicationQuit()
    {
        //Debug.Log("Application quit.");
        applicationIsQuitting = true;
    }

    /// <summary>
    /// Sent to all GameObjects when the application pauses.
    /// </summary>
    /// <param name="hasFocus"></param>
    private void OnApplicationFocus(bool hasFocus)
    {
        //Debug.Log("Application focus: " + hasFocus);
    }

    /// <summary>
    /// Sent to all GameObjects when the player gets or loses focus.
    /// </summary>
    /// <param name="pauseStatus"></param>
    private void OnApplicationPause(bool pauseStatus)
    {
        //Debug.Log("Application pause: " + pauseStatus);
    }

    /// <summary>
    /// Create an instance of Singleton class.
    /// </summary>
    public virtual void Create()
    {
    }

    /// <summary>
    /// Destroy an instance of Singleton class.
    /// </summary>
    public virtual void Destroy()
    {
        if (_instance != null)
        {
            UnityEngine.Object.Destroy(this.gameObject);
        }
    }
}