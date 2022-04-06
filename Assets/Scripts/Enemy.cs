using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{

    NavMeshAgent myAgent;
    LineRenderer lineRenderer;

    public float currentHealth;
    public float maxHealth;
    // How fast this unit attacks measured in seconds
    public float attackSpeed;
    // Denotes the point in time when the unit just shot a bullet
    private float startShootTime;
    // A base delay for the unit to start shooting after acquiring the target. All units should have this > 0, buildings can have 0.
    public float aimSpeed;
    // The distance before this unit can start attacking
    public float range;
    // How much damage an attack does
    public float damage;
    // Denotes the point in time when the unit can start attacking
    private float attackCooldown;
    // Denotes the point in time when the unit just started aiming
    private float startAimTime;
    // Denotes if the unit is in an aiming state, basically, a NEW target has appeared and it is waiting on its aiming speed
    private bool startedAimingPhase;
    // Denotes if the unit is currently aimed at an enemy, so it no longer has to wait for its aiming speed
    private bool aimedAtUnit;

    // Start is called before the first frame update
    void Start()
    {
        UnitManager.Instance.enemyList.Add(this.gameObject);
        myAgent = this.GetComponent<NavMeshAgent>();

        maxHealth = 100;
        currentHealth = maxHealth;

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

    void Update()
    {
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
                if (Physics.Raycast(this.transform.position, (unit.transform.position - this.transform.position), out hit, range))
                {
                    if (hit.transform == unit.transform)
                    {
                        // If the enemy is going to be the current closest enemy and is also in line-of-sight, we save it to be checked further
                        closestUnitDistance = distance;
                        closestUnit = unit;

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
                        this.transform.LookAt(closestUnit.transform);
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

    void OnDestroy()
    {
        UnitManager.Instance.enemyList.Remove(this.gameObject);
    }

    public void MoveToPlace(Vector3 location)
    {
        myAgent.SetDestination(location);
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0) Destroy(this.gameObject);
    }

    public float getMaxHealth()
    {
        return maxHealth;
    }

    public float getCurrentHealth()
    {
        return currentHealth;
    }

    public float getStartShootTime()
    {
        return startShootTime;
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

    private void attackUnit(Unit unit)
    {
        unit.TakeDamage(damage);
        startShootTime = Time.time;
    }
    private void cantAttack()
    {
        attackCooldown = Time.time + attackSpeed;
    }

}
