using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPrefsHelper : Manager<PlayerPrefsHelper>
{
    public void SetString(string key, string value)
    {
        PlayerPrefs.SetString(key, value);
    }

    public void SetInt(string key, int value)
    {
        PlayerPrefs.SetInt(key, value);
    }

    public string GetString(string key, string defaultValue = null)
    {
        return PlayerPrefs.GetString(key, defaultValue);
    }

    public int GetInt(string key, int defaultValue)
    {
        return PlayerPrefs.GetInt(key, defaultValue);
    }
}