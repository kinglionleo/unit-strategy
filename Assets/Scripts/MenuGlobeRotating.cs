using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    Script to manage the rotating of the ranking globe
 */
public class MenuGlobeRotating : MonoBehaviour
{
    //Speed of drag
    public float rotSpeed = 20;
    void OnMouseDrag()
    {
        //Mouse interaction (not configured for mobile yet)
        float rotX = Input.GetAxis("Mouse X") * rotSpeed * Mathf.Deg2Rad;
        float rotY = Input.GetAxis("Mouse Y") * rotSpeed * Mathf.Deg2Rad;

        //Rotating directions
        transform.Rotate(Vector3.up, -rotX, Space.World);
        transform.Rotate(Vector3.right, rotY, Space.World);
    }
}
