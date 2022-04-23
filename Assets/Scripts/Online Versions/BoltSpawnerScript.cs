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
    private float timer;
    private bool canIncrease;
    private int gathererCount;

    public Material blueprint;
    public Text resourceText;
    public Text researchText;
    public float spawnRadius;
    public GameObject basic;
    public GameObject sniper;
    public GameObject tank;
    public GameObject juggernaut;
    public GameObject superSniper;
    public GameObject gatherer;
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
        resources = 0;
        research = 0;
        gathererCount = 0;
        addResource(50);
        timer = 0;
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
        timer += Time.deltaTime;
        
        if (canIncrease && timer >= 1.5)
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
                if(myBase != null &&
                   Vector3.Distance(myBase.transform.position, hit.point) <= spawnRadius)
                {
                    if (resources >= spawn.gameObject.GetComponent<BoltUnit>().getCost() &&
                        research >= spawn.gameObject.GetComponent<BoltUnit>().getResearchRequirement())
                    {
                        BoltNetwork.Instantiate(spawn, hit.point, transform.rotation);
                        addResource(spawn.gameObject.GetComponent<BoltUnit>().getCost() * -1);
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

    public void addResource(int amount)
    {
        resources += amount;
        resourceText.text = resources.ToString();
    }

    public void addResearch(int amount)
    {
        research += amount;
        researchText.text = research.ToString();

    }

    public void spawnBasic()
    {
        spawn = basic;
    }

    public void spawnSniper()
    {
        spawn = sniper;
    }

    public void spawnTank()
    {
        spawn = tank;
    }

    public void spawnJuggernaut()
    {
        spawn = juggernaut;
    }

    public void spawnSuperSniper()
    {
        spawn = superSniper;
    }

    public void spawnObject(GameObject unit)
    {
        if(unit.GetComponent<BoltGatherer>() != null)
        {
            if(gathererCount < 3)
            {
                spawn = unit;
            }
            return;
        }
        
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
