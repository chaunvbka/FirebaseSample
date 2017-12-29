using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;

public class DebugManager : Singleton<DebugManager>
{
    public GameObject DialogBox;
    public bool IsDebug = true;
    public Text LogMsg;

    private string logString = "";
    private const int kMaxLogSize = 16382;

    private void Start()
    {
        //string logText = "123456";
        //Debug.LogError(logText.Length);
        //Debug.Log(logText.Substring(3));
    }

    //public override void Create()
    //{
    //}

    //public override void Destroy()
    //{
    //    UnityEngine.Object.Destroy(this.gameObject);
    //}

    private void RegularExpressionsEmail()
    {
        //Pass length is at least 8 characters.
        //Email: sample@host, 6<=sample.Length<=30.
        //The first character of your username should be a letter (a-z) or number: ^([a-z0-9])
        //Continue with "a-z, 0-9, ." is pattern: ([a-z0-9.][a-z0-9.]+)$
        //Continue with @host is pattern: @([a-z0-9]+)\.([a-z\.]{2,6})$
        Regex regex = new Regex(@"^([a-z0-9])([a-z0-9.][a-z0-9.]+)@([a-z0-9]+)\.([a-z\.]{2,6})$");
        //Regex regex = new Regex(@"^([a-z(\[^A-Z\])0-9.]+)");
        Match match = regex.Match("2dot.2asdasda@a2s.com");
        bool compare = regex.IsMatch("2dot.2asdasda@a2s.com");
        if (compare)
        {
            Debug.Log(match.Value);
            Debug.Log(compare);
        }
        else
        {
            Debug.Log(match.Value);
            Debug.Log(compare);
        }
    }

    /// <summary>
    ///  Output text to the debug log text field, as well as the console.
    ///  Call IsDebug before DebugLog.
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    private string DebugLog(string s)
    {
        Debug.Log(s);
        logString += s + "\n";

        while (logString.Length > kMaxLogSize)
        {
            int index = logString.IndexOf("\n");
            logString = logString.Substring(index + 1);
        }

        return logString;
    }

    public void ShowLog(string tile, string logmsg)
    {
        if (IsDebug)
        {
            if (string.IsNullOrEmpty(logmsg))
            {
                logmsg = "---";
            }
            string s = tile + ": " + logmsg;
            DebugLog(s);
            if (this.LogMsg != null)
            {
                this.LogMsg.text = logString;
            }
        }
    }

    public void ClearLog()
    {
        if (this.LogMsg != null)
        {
            this.LogMsg.text = "";
            this.logString = "";
        }
    }
}
