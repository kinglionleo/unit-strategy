using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Bolt;

public class BoltGatherer : BoltUnit
{
    // How much the unit generates per gather
    public int generationAmount;
    public int capIncreaseAmount;

    protected Vector3 resourceLocation;
    protected Vector3 researchLocation;

    private bool holdingResource;
    private bool holdingResearch;

    // Start Equivalent
    public override void Attached()
    {
        base.Attached();
        if(entity.IsOwner)
        {
            BoltSpawnerScript.Instance.AddGatherer(1);
        }
    }

    // Destroy Equivalent
    public override void Detached()
    {
        base.Detached();
        if (entity.IsOwner)
        {
            BoltSpawnerScript.Instance.AddGatherer(-1);
        }

    }
    void OnCollisionEnter(Collision collision)
    {
        if(!entity.IsOwner)
        {
            return;
        }
        if(collision.gameObject.tag.Equals("Resource"))
        {
            holdingResource = true;
            holdingResearch = false;
            resourceLocation = collision.transform.position;
            MoveToPlace(BoltSpawnerScript.Instance.getBase().transform.position, 0, 0);
            
        }
        if (collision.gameObject.tag.Equals("Research"))
        {
            holdingResearch = true;
            holdingResource = false;
            researchLocation = collision.transform.position;
            MoveToPlace(BoltSpawnerScript.Instance.getBase().transform.position, 0, 0);

        }
        else if(GameObject.ReferenceEquals(collision.gameObject, BoltSpawnerScript.Instance.getBase())) {

            if(holdingResource)
            {
                BoltSpawnerScript.Instance.addResourceCap(capIncreaseAmount);
                BoltSpawnerScript.Instance.addResource(generationAmount);
                holdingResource = false;
                if(resourceLocation != null)
                {
                    MoveToPlace(resourceLocation, 0, 0);
                }
            }
            if (holdingResearch)
            {
                BoltSpawnerScript.Instance.addResearch(generationAmount);
                holdingResearch = false;
                if (researchLocation != null)
                {
                    MoveToPlace(researchLocation, 0, 0);
                }
            }

        }
    }

    public override void SimulateOwner()
    {
        if (lifetime != 0 && BoltNetwork.ServerTime >= spawnTime + lifetime)
        {
            BoltNetwork.Destroy(this.gameObject);
        }

        // This handles if the unit is selected or not. Pretty inefficient, can be a on/off function instead.
        if (selected)
        {
            this.transform.GetChild(0).gameObject.SetActive(true);
            this.transform.Find("RangeIndicator").gameObject.SetActive(true);
            this.transform.Find("RangeIndicator").transform.localScale = new Vector3(range * 2 / this.transform.localScale.x, range * 2 / this.transform.localScale.y, 1);
        }
        else
        {
            this.transform.GetChild(0).gameObject.SetActive(false);
            this.transform.Find("RangeIndicator").gameObject.SetActive(false);
        }

    }

    public override BoltStatsManagerScript.UnitType GetUnitType()
    {
        return BoltStatsManagerScript.UnitType.Gatherer;
    }

    protected override void PullStatsFromManager() 
    {
        BoltStatsManagerScript.UnitType unitType = BoltStatsManagerScript.UnitType.Gatherer;
        BoltStatsManagerScript.UnitStats unitStats = BoltStatsManagerScript.Instance.GetUnitStats(unitType);
        SetStatsFromManager(unitStats);
    }
}
