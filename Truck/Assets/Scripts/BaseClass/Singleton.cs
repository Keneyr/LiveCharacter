using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    public static T instance;

    protected void Awake()
    {
        if(instance == null)
        {
            instance = (T)this;
        }
    }
}
