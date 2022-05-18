using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.AI;
using Photon.Bolt;

public class BoltBomber : BoltUnit
{
    protected override void PullStatsFromManager() 
    {
        BoltStatsManagerScript.UnitType unitType = BoltStatsManagerScript.UnitType.Bomber;
        BoltStatsManagerScript.UnitStats unitStats = BoltStatsManagerScript.Instance.GetUnitStats(unitType);
        
        SetStatsFromManager(unitStats);
    }

    public override BoltStatsManagerScript.UnitType GetUnitType()
    {
        return BoltStatsManagerScript.UnitType.Bomber;
    }

    public override void Detached()
    {
        Debug.Log("Detached");

        float scaledDamageRadius = damageRadius * 2;
        GameObject splashIndicatorClone = Instantiate(splashIndicator);
        splashIndicatorClone.gameObject.GetComponent<BoltSplashIndicatorScript>().startSplash(scaledDamageRadius, this);

        if (entity.IsOwner)
        {
            BoltUnitManager.Instance.unitList.Remove(this.gameObject);
            // To make sure that units that die get deselected
            BoltUnitManager.Instance.unitsSelected.Remove(this.gameObject);

            foreach(var enemy in BoltUnitManager.Instance.enemyList) {

                if(Vector3.Distance(this.transform.position, enemy.transform.position) <= damageRadius) {
                    // This tells the local enemy to flash
                    enemy.gameObject.GetComponent<BoltUnit>().TakeDamage(damage);

                    // This tells the online enemy to take damage
                    ReceiveDamage e = ReceiveDamage.Create(enemy.gameObject.GetComponent<BoltEntity>(), EntityTargets.OnlyOwner);
                    e.DamageTaken = damage;
                    e.DamageRadius = 0;
                    e.DamageDealer = this.GetComponent<BoltEntity>();
                    e.DelayUntilDamageIsTaken = 0;
                    e.Send();
                }
            }
        }
        else
        {
            BoltUnitManager.Instance.enemyList.Remove(this.gameObject);
        }
        Instantiate(deathEffect, transform.position, Quaternion.identity);

    }

    public override void SimulateOwner()
    {
        if (lifetime != 0 && BoltNetwork.ServerTime >= spawnTime + lifetime)
        {
            BoltNetwork.Destroy(this.gameObject);
        }
        // If the unit cannot move, it checks for if it has stayed still for long enough, then allows it to move
        if (!canMove)
        {
            if (myAgent != null) {
                myAgent.SetDestination(this.transform.position);
            }
            // Checks if the current point in time is greater than the time it shot a bullet and the time it must stay still
            if (Time.time >= startShootTime + stationaryDelay)
            {
                canMove = true;
                stationaryIndicator.SetActive(false);
                // continues moving towards the last clicked place for this unit.
                myAgent.SetDestination(targetPosition);
            }
        }

        // This handles if the unit is selected or not. Pretty inefficient, can be a on/off function instead.
        if (selected)
        {
            this.transform.GetChild(0).gameObject.SetActive(true);
            this.transform.Find("RangeIndicator").gameObject.SetActive(true);
            this.transform.Find("RangeIndicator").transform.localScale = new Vector3(damageRadius * 2 / this.transform.localScale.x, damageRadius * 2 / this.transform.localScale.y, 1);
        }
        else
        {
            this.transform.GetChild(0).gameObject.SetActive(false);
            this.transform.Find("RangeIndicator").gameObject.SetActive(false);
            ignoreEnemy = false;
        }

    }
}
