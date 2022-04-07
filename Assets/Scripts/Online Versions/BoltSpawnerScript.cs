using System;
using UnityEngine;
using UnityEngine.AI;
using Photon.Bolt;


public class BoltSpawnerScript : GlobalEventListener
{

    private static BoltSpawnerScript _instance;
    private Camera cam;
    public LayerMask ground;

    private GameObject spawn;
    private GameObject hold;

    public Material blueprint;
    public GameObject enemy;
    public GameObject basic;
    public GameObject sniper;
    public GameObject tank;
    public static BoltSpawnerScript Instance
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
            // NavMeshAgent[] navMeshAgents;
            // navMeshAgents = hold.GetComponents<NavMeshAgent>();
            // navMeshAgents[0].enabled = false;
            hold.gameObject.GetComponent<NavMeshAgent>().enabled = false;
            //((GameObject)hold).GetComponent<Collider>().enabled = false;

            // This is to prevent the bug where the blueprint will be added to unitsSelected (as the game detects the click on it) which will then
            // become null after destroying the blueprint.
            hold.gameObject.layer = 2;

            // The 1 refers to the fact that models will always be the second child
            for (int i = 0; i < hold.transform.GetChild(1).childCount; i++)
            {
                setBlueprintMaterial(hold.transform.GetChild(1).GetChild(i).gameObject);
            }

            hold.gameObject.SetActive(true);
        }

        RaycastHit hit;
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, ground))
        {
            hold.transform.position = hit.point + new Vector3(0, 0.6f, 0);
        }

        if (Input.GetMouseButtonDown(0))
        {
            ray = cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, ground))
            {
                BoltNetwork.Instantiate(spawn, hit.point, transform.rotation);
                spawn = null;
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
        this.gameObject.SetActive(true);
    }

    public void spawnBasic()
    {
        spawn = basic;
        this.gameObject.SetActive(true);
    }

    public void spawnSniper()
    {
        spawn = sniper;
        this.gameObject.SetActive(true);
    }

    public void spawnTank()
    {
        spawn = tank;
        this.gameObject.SetActive(true);
    }

    /*
     * This will iterate through all of a gameobject's material and set each to the blueprint material
     */
    private void setBlueprintMaterial(GameObject g)
    {
        Material[] materials = new Material[g.GetComponent<Renderer>().materials.Length];
        for (int i = 0; i < materials.Length; i++)
        {
            materials[i] = blueprint;
        }

        g.GetComponent<Renderer>().materials = materials;
    }
}
