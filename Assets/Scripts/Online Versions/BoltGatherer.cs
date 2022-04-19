using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Bolt;

public class BoltGatherer : BoltUnit
{
    // How much the unit generates per gather
    public int generationAmount;

    protected Vector3 resourceLocation;

    private bool holdingResource;

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
        if(collision.gameObject.name.Equals("Resource"))
        {
            holdingResource = true;
            resourceLocation = collision.transform.position;
            MoveToPlace(BoltSpawnerScript.Instance.getBase().transform.position, 0, 0);
            
        }
        else if(GameObject.ReferenceEquals(collision.gameObject, BoltSpawnerScript.Instance.getBase())) {

            if(holdingResource)
            {
                BoltSpawnerScript.Instance.addResource(generationAmount);
                holdingResource = false;
                if(resourceLocation != null)
                {
                    MoveToPlace(resourceLocation, 0, 0);
                }
            }
            
        }
    }

    public override void SimulateOwner()
    {

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
}
