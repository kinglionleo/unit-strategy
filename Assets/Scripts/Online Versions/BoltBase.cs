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
            state.Health = maxHealth;
        }
        state.AddCallback("Health", HealthCallback);

        if (entity.IsOwner)
        {
            this.gameObject.tag = "Player";
            this.gameObject.layer = 9;
            BoltUnitManager.Instance.unitList.Add(this.gameObject);
            // Make ourselves the lower left corner
            CameraController.Instance.SetPositionAndRotation(transform.position, transform.rotation);
            BoltSpawnerScript.Instance.setBaseLocation(this.gameObject.transform.position);
        }
        else
        {
            this.gameObject.tag = "Enemy";
            this.gameObject.layer = 9;
            BoltUnitManager.Instance.enemyList.Add(this.gameObject);
        }
    }

    // Destroy Equivalent
    public override void Detached()
    {
        if (entity.IsOwner)
        {
            BoltUnitManager.Instance.unitList.Remove(this.gameObject);
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

    void Start()
    {
        /*
         * This code is just instantiation related
         */
        lineRenderer = this.GetComponent<LineRenderer>();
        lineRenderer.startWidth = 0.04f;
        lineRenderer.endWidth = 0.04f;
        lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = true;
        lineRenderer.startColor = Color.white;
        lineRenderer.endColor = Color.white;

        this.transform.Find("bolt@HealthBarCanvas").gameObject.SetActive(true);
        attackCooldown = 0;
        startAimTime = 0;
        startShootTime = 0;
        aimedAtEnemy = false;
        startedAimingPhase = false;
        canMove = true;
        prevClosestEnemy = null;
        targetPosition = this.transform.position;
        ignoreEnemy = false;
        hitboxRadius = this.gameObject.GetComponent<BoxCollider>().size.x * this.transform.localScale.x;
    }

    public override void SimulateOwner()
    {
        // If the unit is in blueprint mode, it shouldn't do anything. TODO: Remove this part later
        if (this.gameObject.tag.Equals("Blueprint"))
        {
            return;
        }

        // This handles if the unit is selected or not. Pretty inefficient, can be a on/off function instead.
        if (selected)
        {
            this.transform.Find("RangeIndicator").gameObject.SetActive(true);
            this.transform.Find("RangeIndicator").transform.localScale = new Vector3(range * 2 / this.transform.localScale.x, range * 2 / this.transform.localScale.x, 1);
        }
        else
        {
            this.transform.Find("RangeIndicator").gameObject.SetActive(false);
            ignoreEnemy = false;
        }

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
                if (Physics.Raycast(this.transform.position, (unit.transform.position - this.transform.position), out hit, range))
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
                }

                // This draws a line from the unit's current position to the closest enemy in range.
                lineRenderer.SetPosition(0, this.transform.position);
                lineRenderer.SetPosition(1, closestEnemy.transform.position);

                // This checks if the unit has met the aiming time requirement
                if (startAimTime + aimSpeed <= Time.time)
                {
                    aimedAtEnemy = true;
                    startedAimingPhase = false;

                    if (isCanAttack() && !ignoreEnemy)
                    {
                        attackEnemy(closestEnemy.transform.GetComponent<BoltUnit>());
                        cantAttack();
                    }
                }
            }
            else
            {
                startedAimingPhase = false;
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
            aimedAtEnemy = false;
            // This makes it so no line is drawn since it is the same point
            lineRenderer.SetPosition(0, new Vector3(0, 0, 0));
            lineRenderer.SetPosition(1, new Vector3(0, 0, 0));
        }

    }

    public new void MoveToPlace(Vector3 location, int type)
    {
        
    }

    public override void OnEvent(ReceiveDamage e)
    {

        state.Health -= e.DamageTaken;
    }

    protected new void attackEnemy(BoltUnit enemy)
    {
        enemy.TakeDamage(damage);
        Debug.Log(enemy.gameObject.tag);
        ReceiveDamage e = ReceiveDamage.Create(enemy.gameObject.GetComponent<BoltEntity>(), EntityTargets.OnlyOwner);
        e.DamageTaken = damage;
        e.DamageRadius = damageRadius;
        e.Send();
        startShootTime = Time.time;
    }
}
