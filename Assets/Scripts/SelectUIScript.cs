using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectUIScript : MonoBehaviour
{
    // Inspector-given value which determines how long the selection cursor will be shown for.
    public float showTime;

    public Material red;
    public Material yellow;

    // Timer to keep track of passed time
    private float timer;
    private bool timerStart;

    // Reference to this object. There will only be one instance of this object during runtime.
    private static SelectUIScript _instance;

    /**
     * Gets the Instance of this object.
    **/
    public static SelectUIScript Instance
    {
        get { return _instance; }
    }

    /**
     * Sets the instance of this object and the timer-related fields.
    **/
    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
        timer = 0;
        timerStart = false;
    }

    /**
     * When the program starts running, automatically hide the cursor.
    **/
    void Start()
    {
        this.gameObject.SetActive(false);
    }

    public void setAttackMoveMaterials()
    {
        this.transform.GetChild(0).gameObject.GetComponent<Renderer>().material = red;
        this.transform.GetChild(1).gameObject.GetComponent<Renderer>().material = red;
    }

    public void setIgnoreMoveMaterials()
    {
        this.transform.GetChild(0).gameObject.GetComponent<Renderer>().material = yellow;
        this.transform.GetChild(1).gameObject.GetComponent<Renderer>().material = yellow;
    }

    /**
     * Every fixed frame, it will check for if the timer has started.
     * Because the boolean for this can only be set to true when the cursor is displayed,
     * we know that we can wait (showTime) seconds before telling the cursor to hide.
    **/
    private void FixedUpdate()
    {
        if (timerStart)
        {
            timer += Time.deltaTime;
            if (timer > showTime)
            {
                hide();
                timer = 0;
                timerStart = false;
            }
        }

    }

    /**
     * This will show the cursor at a given location, and handles the logic for hiding it after a given timeframe.
    **/
    public void showAtLocation(Vector3 location)
    {
        this.gameObject.transform.position = location;
        this.gameObject.SetActive(true);
        
        // Setting the timer to zero handles the case where the user rightclicks again while the cursor is already displayed.
        timer = 0;
        timerStart = true;
        
    }

    private void hide()
    {
        this.gameObject.SetActive(false);
    }
}
