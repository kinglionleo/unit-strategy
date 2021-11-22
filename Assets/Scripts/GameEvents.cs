using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEvents : MonoBehaviour
{

    public static GameEvents current;

    private void Awake()
    {
        current = this;
    }

    public event Action<int> testAction;
    public void someTestAction(int id)
    {
        if (testAction != null)
        {
            //some action
        }
    }

}
