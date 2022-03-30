using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SpawnerScript : MonoBehaviour
{

    private static SpawnerScript _instance;
    private GameObject spawn;
    public static SpawnerScript Instance
    {
        get { return _instance; }
    }
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
    }
    
    /*
     * Basically, this method will only be called when something is selected as it will alternate between the active and inactive states
     */
    void Update()
    {
        // Go to a raycast on the ground
    }

    public void setActive(bool state)
    {
        this.gameObject.SetActive(state);
    }

    public void setSpawnObject(GameObject go)
    {
        spawn = go;
    }
}
