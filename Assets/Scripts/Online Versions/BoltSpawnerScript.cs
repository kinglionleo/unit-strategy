using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using Photon.Bolt;


public class BoltSpawnerScript : GlobalEventListener
{

    private static BoltSpawnerScript _instance;
    private GameObject myBase;
    private Camera cam;
    public LayerMask ground;

    private GameObject spawn;
    private GameObject hold;

    private int resources;
    private int research;
    private int resourceCap;
    private float timer;
    private float startTime;
    private bool canIncrease;
    private int gathererCount;
    private int rateMultiplier;

    public float increaseDelay;
    public Material blueprint;
    public Text resourceText;
    public Text researchText;
    public Text resourceCapText;
    public float spawnRadius;
    public GameObject builder;
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
            spawnBase(new Vector3(0, 2f, 48), Quaternion.Euler(0f, 0f, 0f));
        }
        else
        {
            spawnBase(new Vector3(0, 2f, -48), Quaternion.Euler(0f, 180f, 0f));
        }
        resources = 0;
        research = 0;
    
        gathererCount = 0;

        addResourceCap(100);
        addResource(50);

        timer = 0;
        startTime = 0;
        rateMultiplier = 1;
    }

    public override void SceneLoadRemoteDone(BoltConnection connection, IProtocolToken token)
    {
        canIncrease = true;
    }


    void Awake()
    {
        canIncrease = false;
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
        if (canIncrease)
        {
            timer += Time.deltaTime;
        }

        if (startTime == 0 && canIncrease)
        {
            startTime = BoltNetwork.ServerTime;
        }

        if( BoltNetwork.ServerTime - startTime >= 180)
        {
            Debug.Log("3x increase");
            rateMultiplier = 3;
        }
        
        
        if (canIncrease && timer >= (increaseDelay / rateMultiplier))
        {
            addResource(5);
            timer = 0;
        }

        if(BoltHUDListener.Instance.selected == null)
        {
            Destroy(hold);
            hold = null;
            return;
        }

        if (spawn == null) return;

        // This allows for only one creation of an object "sticking" to where your mouse is.
        if (hold == null)
        {
            hold = Instantiate(spawn.transform.GetChild(1).gameObject);
            hold.transform.localScale = new Vector3(hold.transform.localScale.x * spawn.transform.localScale.x,
                                                    hold.transform.localScale.y * spawn.transform.localScale.y,
                                                    hold.transform.localScale.z * spawn.transform.localScale.z);
            
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
                if (spawn.gameObject.GetComponent<BoltBuilding>() != null)
                {
                    if (resources >= spawn.gameObject.GetComponent<BoltUnit>().getCost() &&
                        research >= spawn.gameObject.GetComponent<BoltUnit>().getResearchRequirement()) {
                        GameObject myBuilder = BoltNetwork.Instantiate(builder, myBase.transform.position + 3 * Vector3.Normalize(new Vector3(0, 2, 0) - myBase.transform.position), transform.rotation);
                        myBuilder.GetComponent<BoltBuilder>().SetBuildingToSpawn(spawn);
                        myBuilder.GetComponent<BoltBuilder>().SetSpawnLocation(hit.point);
                        addResource(spawn.gameObject.GetComponent<BoltUnit>().getCost() * -1);
                    }
                }
                
                else if (myBase != null &&
                         canSpawn(hit.point))
                {
                    if (resources >= spawn.gameObject.GetComponent<BoltUnit>().getCost() &&
                        research >= spawn.gameObject.GetComponent<BoltUnit>().getResearchRequirement())
                    {
                        if (spawn.gameObject.GetComponent<BoltGatherer>() != null)
                        {
                            if (gathererCount < 3)
                            {
                                BoltNetwork.Instantiate(spawn, hit.point, transform.rotation);
                                addResource(spawn.gameObject.GetComponent<BoltUnit>().getCost() * -1);
                            }
                        }

                        else
                        {
                            BoltNetwork.Instantiate(spawn, hit.point, transform.rotation);
                            addResource(spawn.gameObject.GetComponent<BoltUnit>().getCost() * -1);
                        }
                    }
                }
                else
                {
                    spawn = null;
                    Destroy(hold);
                    hold = null;
                    BoltHUDListener.Instance.DeselectAll();
                }
                
            }
            
        }

        if(Input.GetMouseButtonDown(1))
        {
            spawn = null;
            Destroy(hold);
            hold = null;
            BoltHUDListener.Instance.DeselectAll();
        }

    }

    private bool canSpawn(Vector3 location)
    {
        bool canSpawn = Vector3.Distance(myBase.transform.position, location) <= spawnRadius;
        foreach (var unit in BoltUnitManager.Instance.buildingList)
        {
            if (unit.GetComponent<BoltBuilding>() != null)
            {
                if (Vector3.Distance(unit.transform.position, location) <= unit.GetComponent<BoltBuilding>().spawnRadius)
                {
                    canSpawn = true;
                }
            }
        }
        return canSpawn;
    }

    public void addResource(int amount)
    {
        resources += amount;
        resources = Math.Min(resources, resourceCap);
        resourceText.text = resources.ToString();
    }

    public void addResourceCap(int amount)
    {
        resourceCap += amount;
        resourceCapText.text = resourceCap.ToString();
    }

    public void addResearch(int amount)
    {
        research += amount;
        researchText.text = research.ToString();

    }

    public void spawnObject(GameObject unit)
    {   
        spawn = unit;
    }

    public void setBase(GameObject thisBase)
    {
        myBase = thisBase;
    }

    public GameObject getBase()
    {
        return myBase; 
    }

    public void AddGatherer(int amount)
    {
        gathererCount += amount;
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
