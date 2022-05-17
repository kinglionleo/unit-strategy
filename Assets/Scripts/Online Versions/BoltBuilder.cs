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

    public override BoltStatsManagerScript.UnitType GetUnitType()
    {
        return BoltStatsManagerScript.UnitType.Builder;
    }

    protected override void PullStatsFromManager() 
    {
        BoltStatsManagerScript.UnitType unitType = BoltStatsManagerScript.UnitType.Builder;
        BoltStatsManagerScript.UnitStats unitStats = BoltStatsManagerScript.Instance.GetUnitStats(unitType);
        SetStatsFromManager(unitStats);
    }
}
