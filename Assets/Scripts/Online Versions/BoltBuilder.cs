using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Bolt;

public class BoltBuilder : BoltUnit
{

    public float buildingSpawnTime;

    protected GameObject buildingToSpawn;
    protected Vector3 locationToSpawn;
    // Start Equivalent
    public override void Attached()
    {

        // This ensures that we are only modifying the health variable if we are the owner when the Network instantiates.
        if(entity.IsOwner)
        {
            state.Health = maxHealth;
            state.TrueHealth = maxHealth;
        }
        spawnTime = BoltNetwork.ServerTime;
        state.AddCallback("Health", HealthCallback);

        if (entity.IsOwner)
        {
            this.gameObject.tag = "Player";
            this.gameObject.layer = 7;
            BoltUnitManager.Instance.unitList.Add(this.gameObject);
        }
        else
        {
            this.gameObject.tag = "Enemy";
            this.gameObject.layer = 0;
            BoltUnitManager.Instance.enemyList.Add(this.gameObject);
        }
    }

    // Destroy Equivalent
    public override void Detached()
    {
        Debug.Log("Detached");
        if (entity.IsOwner)
        {
            BoltUnitManager.Instance.unitList.Remove(this.gameObject);
            // To make sure that units that die get deselected
            BoltUnitManager.Instance.unitsSelected.Remove(this.gameObject);
        }
        else
        {
            BoltUnitManager.Instance.enemyList.Remove(this.gameObject);
        }

    }
    void Awake()
    {
        currentHealth = maxHealth;
    }

   


    public override void SimulateOwner()
    {

        if (Mathf.Abs(this.transform.position.x - locationToSpawn.x) < 0.2 && Mathf.Abs(this.transform.position.z - locationToSpawn.z) < 0.2)
        {
            StartCoroutine(SpawnBuilding(buildingSpawnTime));
        }

    }

    private IEnumerator SpawnBuilding(float delay)
    {
        yield return new WaitForSeconds(delay);
        BoltNetwork.Instantiate(buildingToSpawn, this.transform.position, this.transform.rotation);
        BoltNetwork.Destroy(this.gameObject);
    }

    public override void MoveToPlace(Vector3 location, int type, float speed)
    {
        return;
    }

    public void SetBuildingToSpawn(GameObject spawn)
    {
        buildingToSpawn =  spawn;
    }

    public void SetSpawnLocation(Vector3 spawn)
    {
        locationToSpawn = spawn;
        if(myAgent == null)
        {
            myAgent = this.GetComponent<NavMeshAgent>();
        }
        myAgent.SetDestination(locationToSpawn);
        myAgent.speed = movementSpeed;
    }
}
