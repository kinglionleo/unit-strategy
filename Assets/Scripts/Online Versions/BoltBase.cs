using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Bolt;

public class BoltBase : BoltUnit
{

    // Start Equivalent
    public override void Attached()
    {
        // This ensures that we are only modifying the health variable if we are the owner when the Network instantiates.
        if (entity.IsOwner)
        {
            state.Health = maxHp;
            state.TrueHealth = maxHp;
        }
        state.AddCallback("Health", HealthCallback);

        if (entity.IsOwner)
        {
            this.gameObject.tag = "Player";
            this.gameObject.layer = 7;
            BoltUnitManager.Instance.unitList.Add(this.gameObject);
            // Make ourselves the lower left corner
            CameraController.Instance.SetPositionAndRotation(transform.position, transform.rotation);
            BoltSpawnerScript.Instance.setBase(this.gameObject);
        }
        else
        {
            this.gameObject.tag = "Enemy";
            this.gameObject.layer = 0;
            BoltUnitManager.Instance.enemyList.Add(this.gameObject);
        }
    }

    void Start()
    {
        /*
         * This code is just instantiation related
         */

        myAgent = null;
        animator = this.GetComponent<Animator>();

        stationaryIndicator = this.transform.Find("bolt@StationaryIndicator").gameObject;
        stationaryIndicator.SetActive(false);

        aimingIndicator = this.transform.Find("bolt@AimingIndicator").gameObject;
        aimingIndicator.SetActive(false);

        // shotLineRenderer = this.transform.Find("bolt@ShotLineRenderer").gameObject;
        // shotLineRenderer.SetActive(false);

        lineRenderer = this.GetComponent<LineRenderer>();
        lineRenderer.startWidth = 0.04f;
        lineRenderer.endWidth = 0.04f;
        lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = true;
        lineRenderer.startColor = Color.white;
        lineRenderer.endColor = Color.white;

        myMaterials = new Material[this.transform.GetChild(1).childCount][];
        flashMaterials = new Material[this.transform.GetChild(1).childCount][];
        for (int i = 0; i < myMaterials.Length; i++)
        {
            myMaterials[i] = new Material[this.transform.GetChild(1).GetChild(i).GetComponent<Renderer>().materials.Length];
            flashMaterials[i] = new Material[this.transform.GetChild(1).GetChild(i).GetComponent<Renderer>().materials.Length];
            for (int j = 0; j < myMaterials[i].Length; j++)
            {
                myMaterials[i][j] = this.transform.GetChild(1).GetChild(i).GetComponent<Renderer>().materials[j];
                flashMaterials[i][j] = flashMaterial;
            }
        }

        ignoreLayer = LayerMask.NameToLayer("Clickable");

        this.transform.Find("bolt@HealthBarCanvas").gameObject.SetActive(true);
        this.transform.Find("RangeIndicator").gameObject.SetActive(false);
        this.transform.Find("SpawnIndicator").gameObject.SetActive(false);
        nextAttackTime = 0;
        startAimTime = 0;
        startShootTime = 0;
        aimedAtEnemy = false;
        startedAimingPhase = false;
        canMove = true;
        prevClosestEnemy = null;
        targetPosition = this.transform.position;
        ignoreEnemy = false;
        hitboxRadius = this.gameObject.GetComponent<BoxCollider>().size.x / this.transform.localScale.x;
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

        if (BoltHUDListener.Instance.selected != null)
        {
            this.transform.Find("SpawnIndicator").gameObject.SetActive(true);
            this.transform.Find("SpawnIndicator").transform.localScale = new Vector3(BoltSpawnerScript.Instance.spawnRadius * 2 / this.transform.localScale.x, BoltSpawnerScript.Instance.spawnRadius * 2 / this.transform.localScale.y, 1);
        }
        else
        {
            this.transform.Find("SpawnIndicator").gameObject.SetActive(false);
        }

        // The following section handles attacking logic

        // Two variables to store the current closest distance and current closest enemy (not necessarily within range)
        float closestEnemyDistance = float.MaxValue;
        GameObject closestEnemy = null;

        // Loop through all enemies
        foreach (var unit in BoltUnitManager.Instance.enemyList)
        {

            // Basically handles when an enemy is in blueprint mode (spawning) - this should be removed eventually
            if (!unit.gameObject.tag.Equals("Enemy"))
            {
                continue;
            }

            float distance = Mathf.Sqrt((unit.transform.position.x - this.transform.position.x) * (unit.transform.position.x - this.transform.position.x) +
                             (unit.transform.position.z - this.transform.position.z) * (unit.transform.position.z - this.transform.position.z)) - unit.gameObject.GetComponent<BoltUnit>().getHitboxSize();

            // closestDistance is the distance to the closest enemy
            if (distance < closestEnemyDistance)
            {
                // This chunk checks to see if the unit is trying to shoot through a physical barrier such as a house by using a raycast
                RaycastHit hit;
                if (Physics.Raycast(this.transform.position, (unit.transform.position - this.transform.position), out hit, range, ignoreLayer))
                {
                    if (hit.transform == unit.transform)
                    {
                        // If the enemy is going to be the current closest enemy and is also in line-of-sight, we save it to be checked further
                        closestEnemyDistance = distance;
                        closestEnemy = unit;

                    }
                }

            }
        }

        // Now we check if the closest enemy is in range or not
        if (closestEnemyDistance < range)
        {
            // really just defensive coding
            // make sure that the enemy is still "alive" aka not set to null
            if (closestEnemy != null)
            {
                Debug.Log("In Here 1");
                // This if statement checks if the unit is locking onto a new enemy or not
                // If the previous enemy that it was aiming at is different from the new closest enemy,
                // will reset fields startedAim and aimedAtEnemy to start aiming process again.
                // The purpose of this is to make sure the unit goes through the "aiming phase" before
                // starting to attack a new target.
                // if (closestEnemy != prevClosestEnemy) {
                //     startedAimingPhase = false;
                //     aimedAtEnemy = false;
                //     prevClosestEnemy = closestEnemy;
                // }

                // If the unit is neither already aimed at an enemy or currently in
                // the aimingPhase (startedAimingPhase = true), start the aiming phase
                // by logging the current time and setting startedAimingPhase to true.
                if (!startedAimingPhase && !aimedAtEnemy)
                {
                    startAimTime = Time.time;
                    startedAimingPhase = true;
                    aimingIndicator.SetActive(true);
                }

                // This draws a line from the unit's current position to the closest enemy in range.
                lineRenderer.SetPosition(0, this.transform.position);
                lineRenderer.SetPosition(1, closestEnemy.transform.position);

                // This checks if the unit has met the aiming time requirement
                if (startAimTime + aimingTime <= Time.time)
                {
                    aimedAtEnemy = true;
                    aimingIndicator.SetActive(false);
                    startedAimingPhase = false;

                    if (isCanAttack() && !ignoreEnemy /*&& closestEnemy.GetComponent<BoltUnit>().state.TrueHealth >= 0*/)
                    {
                        // Everything related to the actual attack is in here:

                        // Tell our shotrenderer to start a shot
                        //shotLineRenderer.gameObject.GetComponent<BoltShotLineRenderer>().startShot(closestEnemy);
                        GameObject shotLineRendererClone = Instantiate(shotLineRenderer, this.transform);
                        shotLineRendererClone.gameObject.GetComponent<BoltShotLineRenderer>().startShot(closestEnemy);

                        // Tell the other player that we just fired a shot at them so they can render it on their screen.
                        ShotFired e = ShotFired.Create(entity, EntityTargets.EveryoneExceptOwner);
                        e.Target = closestEnemy.GetComponent<BoltUnit>().entity;
                        e.DamageTaken = damage;
                        e.DamageRadius = damageRadius;
                        e.Send();

                        // Get how long it takes for the shot to arrive at the target
                        float takeDamageDelay = shotLineRendererClone.gameObject.GetComponent<BoltShotLineRenderer>().shotTimeLength;

                        // Actually attack the enemy and tell them when they will be taking the damage
                        attackEnemy(closestEnemy.transform.GetComponent<BoltUnit>(), takeDamageDelay);

                        // Start reloading
                        cantAttack();

                        // This is to prevent stutterstepping
                        if (!selected)
                        {
                            targetPosition = this.transform.position;
                        }
                    }
                }
            }
            else
            {
                startedAimingPhase = false;
                aimingIndicator.SetActive(false);
                aimedAtEnemy = false;
            }
        }
        else // whatever the closest target is, is not in range of the unit
        {
            // if target left the range, the previousClosestEnemy is disregarded, as unit will need to reaim
            // if rengaging witht the same target.
            prevClosestEnemy = null;
            // if there is no enemy in range, the unit must aim again.
            startedAimingPhase = false;
            aimingIndicator.SetActive(false);
            aimedAtEnemy = false;
            // This makes it so no line is drawn since it is the same point
            lineRenderer.SetPosition(0, new Vector3(0, 0, 0));
            lineRenderer.SetPosition(1, new Vector3(0, 0, 0));
        }

    }

    public override void MoveToPlace(Vector3 location, int type, float speed)
    {
        return;
    }

    protected override void PullStatsFromManager() 
    {
        BoltStatsManagerScript.UnitType unitType = BoltStatsManagerScript.UnitType.Base;
        BoltStatsManagerScript.UnitStats unitStats = BoltStatsManagerScript.Instance.GetUnitStats(unitType);
        
        SetStatsFromManager(unitStats);
    }

}
