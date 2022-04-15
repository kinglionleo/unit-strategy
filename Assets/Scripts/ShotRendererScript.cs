using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class ShotRendererScript : MonoBehaviour
{

    public LineRenderer shotLineRenderer;
    // The amount of time the shot should take to reach the target.
    // The speed that the shot travels in m/s... i think?
    public float shotVelocity; // must be faster than attack speed unless we change to 
    public float shotTimeLength;

    // The offset (usually vertical) from the Unit's position, to make it look like it
    // is shooting from the correct place in the mesh.
    public Vector3 shotStartOffset;

    // The size of the shot/bullet - a percentage of the totalDistance
    public float shotSize;
    // the start location of the shot.
    private Vector3 shotStartLocation;
    // The endLocation of the shot
    private Vector3 shotEndLocation;
    
    private Unit myUnit;
    private Enemy myEnemy;

    private GameObject target;

    // The time the shot was fired.
    private float shotStartTime;
    // If the startShot has been called.
    private bool shotFired;
    // Whether the shot time count has started or not.
    private bool shotAnimationStarted;
    

    // Awake is called once in the lifetime of a gameObject instance
    void Awake()
    {
        myUnit = this.transform.parent.GetComponent<Unit>();
        myEnemy = null;
        if(myUnit == null) {
            myEnemy = this.transform.parent.GetComponent<Enemy>();
        }

        shotLineRenderer = this.GetComponent<LineRenderer>();
        //shotLineRenderer.alignment
        shotLineRenderer.startWidth = 0.1f;
        shotLineRenderer.endWidth = 0.05f;
        shotLineRenderer.positionCount = 2;
        shotLineRenderer.numCapVertices = 2;
        shotLineRenderer.useWorldSpace = true;
        this.shotSize = 0.1f;
        //this.shotTimeLength = 0.2f;
        this.shotVelocity = 20f;
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
        if (target == null) {
            shotLineRenderer.SetPosition(0, new Vector3(0,0,0));
            shotLineRenderer.SetPosition(1, new Vector3(0,0,0));
            this.gameObject.SetActive(false);
            return; //the target is already destroyed.
        }
        //if (this.transform.parent.Get)
        // if shot is "fired" (by startShot method call), run the animation
        //Debug.Log("Shot started? " + shotAnimationStarted);
        if (shotFired) {
            shotFired = false; // reset immediately for next shot.
            shotStartTime = Time.time;
            //shotLineRenderer.SetPosition(1, shotEndLocation);
            //Debug.Log("Shot started! " + shotEndLocation);
            shotAnimationStarted = true;
        }
        else {
            // if the shot has been shooting for length shotTimeLength, stop the shot animation.
            if (shotStartTime + shotTimeLength <= Time.time) {
                shotLineRenderer.SetPosition(0, new Vector3(0,0,0));
                shotLineRenderer.SetPosition(1, new Vector3(0,0,0));
                //Debug.Log(shotStartTime + shotTimeLength + " " + Time.time);

                //shotLineRenderer.SetPosition(1, this.transform.position + shotStartOffset);
                shotAnimationStarted = false;
                this.gameObject.SetActive(false);
            }
            else if (shotAnimationStarted) { // is shot is still shooting
                // End - start location
                //this.gameObject.SetActive(true);
                float timePassedPercent = (Time.time - shotStartTime) / shotTimeLength;
                float smallestShotSize = 0.5f;
                shotEndLocation = target.transform.position;
                // essentially, we want the shot to go up to the "shotSizeVector" away from the final location
                Vector3 distanceVector = shotEndLocation - shotStartLocation;
                Vector3 unitVector = (distanceVector / distanceVector.magnitude);
                Vector3 shotSizeVector = unitVector * smallestShotSize;
                if ((Mathf.Max((distanceVector * shotSize).magnitude, (unitVector * smallestShotSize).magnitude) != (unitVector * smallestShotSize).magnitude)) {
                    shotSizeVector = distanceVector * shotSize;
                }
                
                Vector3 deltaVector = (distanceVector - shotSizeVector) * timePassedPercent;
                
                shotLineRenderer.SetPosition(0, shotStartLocation + deltaVector + shotSizeVector);
                shotLineRenderer.SetPosition(1, shotStartLocation + deltaVector);
                
                
                //shotLineRenderer.SetPosition(1, shotEndLocation);
            }
        }
    }

    private void calculateShotTimeLengthOnVelocity(GameObject target) {
        if (myUnit != null) {
            shotStartLocation = myUnit.transform.position + shotStartOffset;
            //Debug.Log("Unit:" + (target.transform.position.magnitude - shotStartLocation.magnitude) / shotVelocity);
        }
        else {
            shotStartLocation = myEnemy.transform.position + shotStartOffset;
            Debug.Log("Enemy's shotstartoffset is" + shotStartOffset);
        }
        this.shotTimeLength = Math.Abs((target.transform.position - shotStartLocation).magnitude / shotVelocity);
    }

    public void startShot(GameObject target) {
        if (target == null) {
            shotLineRenderer.SetPosition(0, new Vector3(0,0,0));
            shotLineRenderer.SetPosition(1, new Vector3(0,0,0));
            this.gameObject.SetActive(false);
            return; //the target is already destroyed.
        }
        this.target = target;
        shotEndLocation = target.transform.position;
        this.gameObject.SetActive(true);
        //Debug.Log("Shot started! " + shotEndLocation);
        shotFired = true;
        calculateShotTimeLengthOnVelocity(target);
    }
}
