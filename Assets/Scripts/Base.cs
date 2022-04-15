using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Base : Enemy
{
    GameObject prevClosestEnemy;

    public void OnDestroy()
    {
        UnitManager.Instance.enemyList.Remove(this.gameObject);
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

        UnitManager.Instance.enemyList.Add(this.gameObject);
        myAgent = this.GetComponent<NavMeshAgent>();

        maxHealth = 5000;
        currentHealth = maxHealth;
        
        shotLineRenderer = this.transform.Find("ShotLineRenderer").gameObject;
        if (shotLineRenderer != null) {
            shotLineRenderer.SetActive(false);
            shotLineRenderer.gameObject.GetComponent<ShotRendererScript>().shotStartOffset = new Vector3(0, 6, 0);
        }

        lineRenderer = this.GetComponent<LineRenderer>();
        lineRenderer.startWidth = 0.04f;
        lineRenderer.endWidth = 0.04f;
        lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = true;
        lineRenderer.startColor = Color.white;
        lineRenderer.endColor = Color.white;

        this.transform.Find("EnemyHealthBar").gameObject.SetActive(true);
        attackCooldown = 0;
        startAimTime = 0;
        startShootTime = 0;
        aimedAtUnit = false;
        startedAimingPhase = false;
    }

    public void Update()
    {
        // If the unit is in blueprint mode, it shouldn't do anything. TODO: Remove this part later
        if (this.gameObject.tag.Equals("Blueprint"))
        {
            return;
        }
        if (currentHealth <= 0) Destroy(this.gameObject);
        // Loop through all enemies

        // Two variables to store the current closest distance and current closest unit (not necessarily within range)
        float closestUnitDistance = float.MaxValue;
        GameObject closestUnit = null;

        foreach (var unit in UnitManager.Instance.unitList)
        {
            float distance = Mathf.Sqrt((unit.transform.position.x - this.transform.position.x) * (unit.transform.position.x - this.transform.position.x) +
                             (unit.transform.position.z - this.transform.position.z) * (unit.transform.position.z - this.transform.position.z));

            // closestDistance is the distance to the closest unit
            if (distance < closestUnitDistance)
            {
                // This chunk checks to see if the unit is trying to shoot through a physical barrier such as a house by using a raycast
                RaycastHit hit;
                //Debug.Log("Trying to raycast");
                bool didRaycastHit = Physics.Raycast(this.transform.position, (unit.transform.position - this.transform.position), out hit, range);
                if (didRaycastHit)
                {
                    //Debug.Log("raycast hit " + hit);
                    if (hit.transform == unit.transform)
                    {
                        //Debug.Log("Raycasted to a target in range " + unit.gameObject);
                        // If the enemy is going to be the current closest enemy and is also in line-of-sight, we save it to be checked further
                        closestUnitDistance = distance;
                        closestUnit = unit;
                    }
                    else {
                        //Debug.Log("raycast hit itself" + hit);
                    }
                }
            }
        }

        // Now we check if the closest enemy is in range or not
        if (closestUnitDistance < range)
        {
            // really just defensive coding
            // make sure that the enemy is still "alive" aka not set to null
            if (closestUnit != null)
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
                if (!startedAimingPhase && !aimedAtUnit)
                {
                    startAimTime = Time.time;
                    startedAimingPhase = true;
                }

                // This draws a line from the unit's current position to the closest enemy in range.
                lineRenderer.SetPosition(0, this.transform.position);
                lineRenderer.SetPosition(1, closestUnit.transform.position);

                // This checks if the unit has met the aiming time requirement
                if (startAimTime + aimSpeed <= Time.time)
                {
                    aimedAtUnit = true;
                    startedAimingPhase = false;

                    if (isCanAttack())
                    {
                        Debug.Log("enemy attacked!");
                        attackUnit(closestUnit.transform.GetComponent<Unit>());
                        cantAttack();
                    }
                }
            }
            else
            {
                startedAimingPhase = false;
                aimedAtUnit = false;
            }
        }
        else // whatever the closest target is, is not in range of the unit
        {          
            startedAimingPhase = false;
            aimedAtUnit = false;
            // This makes it so no line is drawn since it is the same point
            lineRenderer.SetPosition(0, new Vector3(0, 0, 0));
            lineRenderer.SetPosition(1, new Vector3(0, 0, 0));
        }
    }

    public new void MoveToPlace(Vector3 location, int type)
    {
        
    }

    private bool isCanAttack()
    {
        if (!aimedAtUnit)
        {
            aimedAtUnit = true;
            return false;
        }
        return attackCooldown <= Time.time;
    }

    // SHOULD CALL Enemy's attack unit
    // private void attackUnit(Unit unit)
    // {
    //     Debug.Log("Base attacked " + unit);
    //     shotLineRenderer.SetActive(true);
    //     shotLineRenderer.gameObject.GetComponent<ShotRendererScript>().startShot(unit.transform.position);
    //     unit.TakeDamage()
    //     //canMove = false;
    //     startShootTime = Time.time;
    // }

    private void cantAttack()
    {
        attackCooldown = Time.time + attackSpeed;
    }
}
