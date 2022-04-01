using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SpawnerScript : MonoBehaviour
{

    private static SpawnerScript _instance;
    private Camera cam;
    public LayerMask ground;

    private GameObject spawn;
    private GameObject hold;

    public Material blueprint;
    public GameObject enemy;
    public GameObject basic;
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
            cam = Camera.main;
        }
    }
    
    /*
     * Basically, this method will only be called when something is selected as it will alternate between the active and inactive states
     */
    void Update()
    {
        if (spawn == null) return;

        // This allows for only one creation of an object "sticking" to where your mouse is.
        if (hold == null)
        {
            hold = Instantiate(spawn);
            hold.gameObject.tag = "Blueprint";
            setBlueprintMaterial(hold.gameObject);
            hold.gameObject.SetActive(false);
        }

        RaycastHit hit;
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, ground))
        {
            hold.SetActive(true);
            hold.transform.position = hit.point + new Vector3(0,0.5f,0);
        }
        else
        {
            hold.SetActive(false);
        }

        if (Input.GetMouseButtonDown(0))
        {
            ray = cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, ground))
            {
                Instantiate(spawn, hit.point, transform.rotation);
                Destroy(hold);
                hold = null;
            }

            setActive(false);
        }

        

        // Go to a raycast on the ground
    }

    public void setActive(bool state)
    {
        this.gameObject.SetActive(state);
    }

    public void spawnEnemy()
    {
        spawn = enemy;
    }

    /*
     * This will iterate through all of a gameobject's material and set each to the blueprint material
     */
    private void setBlueprintMaterial(GameObject g)
    {
        Material[] materials = new Material[g.GetComponent<Renderer>().materials.Length];
        for(int i=0; i<materials.Length; i++)
        {
            materials[i] = blueprint;
        }

        g.GetComponent<Renderer>().materials = materials;
    }
}
