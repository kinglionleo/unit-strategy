using System;
using UnityEngine;
using UnityEngine.AI;
using Photon.Bolt;


public class BoltSpawnerScript : GlobalEventListener
{

    private static BoltSpawnerScript _instance;
    private Vector3 myBaseLocation;
    private Camera cam;
    public LayerMask ground;

    private GameObject spawn;
    private GameObject hold;

    public Material blueprint;
    public float spawnRadius;
    public GameObject enemy;
    public GameObject basic;
    public GameObject sniper;
    public GameObject tank;
    public GameObject hq;
    public static BoltSpawnerScript Instance
    {
        get { return _instance; }
    }

    /*
     * Once the scene has finished loading locally, we create our base.
     */

    
    public override void SceneLoadLocalDone(string scene, IProtocolToken token)
    {
        Debug.Log("Loaded!");
        if (!scene.Equals("OnlineTesting"))
        {
            return;
        }
        if(BoltNetwork.IsClient)
        {
            spawnBase(new Vector3(12.78f, 2.5f, 12.78f), Quaternion.Euler(0f, 0f, 0f));
        }
        else
        {
            spawnBase(new Vector3(-12.78f, 2.5f, -12.78f), Quaternion.Euler(0f, 180f, 0f));
        }
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
            hold = Instantiate(spawn.transform.GetChild(1).gameObject);
            hold.transform.localScale = new Vector3(hold.transform.localScale.x * spawn.transform.localScale.x,
                                                    hold.transform.localScale.y * spawn.transform.localScale.y,
                                                    hold.transform.localScale.z * spawn.transform.localScale.z);
            // NavMeshAgent[] navMeshAgents;
            // navMeshAgents = hold.GetComponents<NavMeshAgent>();
            // navMeshAgents[0].enabled = false;
            //((GameObject)hold).GetComponent<Collider>().enabled = false;

            // This is to prevent the bug where the blueprint will be added to unitsSelected (as the game detects the click on it) which will then
            // become null after destroying the blueprint.

            // The 1 refers to the fact that models will always be the second child
            for (int i = 0; i < hold.transform.childCount; i++)
            {
                setBlueprintMaterial(hold.transform.GetChild(i).gameObject);
            }

            hold.gameObject.SetActive(true);
        }

        RaycastHit hit;
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, ground))
        {
            hold.transform.position = hit.point;
        }

        if (Input.GetMouseButtonDown(0))
        {
            ray = cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, ground))
            {
                if(myBaseLocation != null && Vector3.Distance(myBaseLocation, hit.point) <= spawnRadius )
                {
                    BoltNetwork.Instantiate(spawn, hit.point, transform.rotation);
                    spawn = null;
                    Destroy(hold);
                    hold = null;
                    setActive(false);
                }
                
            }
            
        }

        if(Input.GetMouseButtonDown(1))
        {
            spawn = null;
            Destroy(hold);
            hold = null;
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

    public void setBaseLocation(Vector3 position)
    {
        myBaseLocation = position;
    }

    private void spawnBase(Vector3 position, Quaternion rotation)
    {
        BoltNetwork.Instantiate(hq, position, rotation);
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
