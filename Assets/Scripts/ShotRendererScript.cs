using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShotRendererScript : MonoBehaviour
{

    public LineRenderer shotLineRenderer;
    // The amount of time the shot should take to reach the target.
    public float shotTimeLength; // must be faster than attack speed unless we change to 

    // The offset (usually vertical) from the Unit's position, to make it look like it
    // is shooting from the correct place in the mesh.
    public Vector3 shotStartOffset;

    // The size of the shot/bullet.
    public float shotSize;

    // The endLocation of the shot
    private Vector3 shotEndLocation;
    
    private Unit myUnit;

    // The time the shot was fired.
    private float shotStartTime;
    // If the startShot has been called.
    private bool shotFired;
    // Whether the shot time count has started or not.
    private bool shotAnimationStarted;
    

    // Awake is called once in the lifetime of a gameObject instance
    void Awake()
    {
        myUnit = this.transform.parent.gameObject.GetComponent<Unit>();

        shotLineRenderer = this.GetComponent<LineRenderer>();
        //shotLineRenderer.alignment
        shotLineRenderer.startWidth = 0.1f;
        shotLineRenderer.endWidth = 0.1f;
        shotLineRenderer.positionCount = 2;
        shotLineRenderer.useWorldSpace = true;
        // Color shotColor = new Color(80, 190, 200, 50);
        // shotLineRenderer.startColor = shotColor;
        // shotLineRenderer.endColor = Color.yellow;
        shotStartTime = 0;
        shotAnimationStarted = false;
    }

    // // Start is called before the first frame update
    // void Start() {
        
    // }

    // Update is called once per frame
    void Update()
    {
        Vector3 shotStartLocation = this.transform.position + shotStartOffset;
        // if shot is "fired" (by startShot method call), run the animation
        Debug.Log("Shot started? " + shotAnimationStarted);
        if (shotFired) {
            shotFired = false;
            this.gameObject.SetActive(true);    
            shotStartTime = Time.time;
            //shotLineRenderer.SetPosition(1, shotEndLocation);
            Debug.Log("Shot started! " + shotEndLocation);
            shotAnimationStarted = true;
        }
        else {
            // if the shot has been shooting for length shotTimeLength, stop the shot animation.
            if (shotStartTime + shotTimeLength <= Time.time) {
                Debug.Log(shotStartTime + shotTimeLength + " " + Time.time);

                //shotLineRenderer.SetPosition(1, this.transform.position + shotStartOffset);
                shotAnimationStarted = false;
                this.gameObject.SetActive(false);
            }
            else if (shotAnimationStarted) { // is shot is still shooting
                // End - start location
                float timePassedPercent = (Time.time - shotStartTime) / shotTimeLength;

                Vector3 deltaVector = (shotEndLocation - shotStartLocation) * timePassedPercent;
                Vector3 unitVector = (deltaVector / deltaVector.magnitude);
                Vector3 shotSizeVector = unitVector * shotSize;
                
                shotLineRenderer.SetPosition(0, shotStartLocation + deltaVector);
                shotLineRenderer.SetPosition(1, shotStartLocation + deltaVector + shotSizeVector);
            }
        }
    }

    public void startShot(Vector3 shotEndLocation) {
        this.shotEndLocation = shotEndLocation;
        Debug.Log("Shot started! " + shotEndLocation);
        shotFired = true;
    }
}
