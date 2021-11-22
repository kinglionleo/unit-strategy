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

    public event Action<Vector3> onMoveToPoint;
    public void MoveToPoint(Vector3 location)
    {
        if (onMoveToPoint != null)
        {
            onMoveToPoint(location);
        }
    }

}
