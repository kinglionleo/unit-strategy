using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Bolt;

public class BoltUnit : EntityEventListener<IUnit>
{
    // Used for pathfinding
    NavMeshAgent myAgent;
    LineRenderer lineRenderer;
    // The black acquisition circle that appears when a unit becomes stationary
    GameObject stationaryIndicator;
    Animator animator;

    public float maxHealth;
    public float currentHealth;

    // How fast this unit attacks measured in seconds
    public float attackSpeed;
    // How long this unit takes to start aiming after coming to a stop. 0 means this unit can attack while moving
    public float acquisitionSpeed;
    // A base delay for the unit to start shooting after acquiring the target. All units should have this > 0, buildings can have 0.
    public float aimSpeed;
    // How fast this unit moves
    public float movementSpeed;
    // The distance before this unit can start attacking
    public float range;
    // How much damage an attack does
    public float damage;

    // If this unit is flying or ground
    public string unitType;
    // If this unit deals splash or individual damage
    public string damageType;

    // The remaining two stats "unitCount" and "cost" will be stored in the UnitManager

    // Denotes the point in time when the unit can start attacking
    private float attackCooldown;
    // Denotes the point in time when the unit just started aiming
    private float startAimTime;
    // Denotes the point in time when the unit just shot a bullet
    private float startShootTime;
    // Denotes the position the unit is going to
    private Vector3 targetPosition;

    // Denotes if the unit is in an aiming state, basically, a NEW target has appeared and it is waiting on its aiming speed
    private bool startedAimingPhase;
    // Denotes if the unit is currently aimed at an enemy, so it no longer has to wait for its aiming speed
    private bool aimedAtEnemy;
    // Denotes if the unit can move, which it cannot if it just shot
    private bool canMove;
    // Denotes if the unit is selected by the user
    private bool selected;
    // Denotes if the unit can auto attack
    private bool ignoreEnemy;

    GameObject prevClosestEnemy;

    public override void Attached()
    {
        state.Health = maxHealth;
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
    void Awake()
    {
        currentHealth = maxHealth;
    }

    void Start()
    {
        /*
         * This code is just instantiation related
         */

        myAgent = this.GetComponent<NavMeshAgent>();
        animator = this.GetComponent<Animator>();

        stationaryIndicator = this.transform.Find("bolt@StationaryIndicator").gameObject;
        stationaryIndicator.SetActive(false);

        lineRenderer = this.GetComponent<LineRenderer>();
        lineRenderer.startWidth = 0.04f;
        lineRenderer.endWidth = 0.04f;
        lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = true;
        lineRenderer.startColor = Color.white;
        lineRenderer.endColor = Color.white;

        this.transform.Find("bolt@HealthBarCanvas").gameObject.SetActive(true);
        this.transform.Find("RangeIndicator").gameObject.SetActive(false);
        attackCooldown = 0;
        startAimTime = 0;
        startShootTime = 0;
        aimedAtEnemy = false;
        startedAimingPhase = false;
        canMove = true;
        prevClosestEnemy = null;
        targetPosition = this.transform.position;
        ignoreEnemy = false;
    }

    void Update()
    {
        // If the unit is in blueprint mode, it shouldn't do anything. TODO: Remove this part later
        if (this.gameObject.tag.Equals("Blueprint"))
        {
            return;
        }

        // If the unit cannot move, it checks for if it has stayed still for long enough, then allows it to move
        if (!canMove)
        {
            myAgent.SetDestination(this.transform.position);

            // Checks if the current point in time is greater than the time it shot a bullet and the time it must stay still
            if (Time.time >= startShootTime + acquisitionSpeed)
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
            this.transform.Find("RangeIndicator").transform.localScale = new Vector3(range * 4, range * 4, 1);
        }
        else
        {
            this.transform.GetChild(0).gameObject.SetActive(false);
            this.transform.Find("RangeIndicator").gameObject.SetActive(false);
            ignoreEnemy = false;
        }

        // The following section handles attacking logic

        // This checks for if we can interrupt movement to attack an enemy
        if (ignoreEnemy)
        {
            // TODO: make the margin of error dynamic based on the group size as bigger clumps make it impossible to reach the target destination
            if (Mathf.Abs(this.transform.position.x - targetPosition.x) <= 0.6 &&
               Mathf.Abs(this.transform.position.z - targetPosition.z) <= 0.6)
            {
                ignoreEnemy = false;
            }
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
                             (unit.transform.position.z - this.transform.position.z) * (unit.transform.position.z - this.transform.position.z));

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
                        this.transform.LookAt(closestEnemy.transform);
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

    void OnDestroy()
    {
        if(entity.IsOwner)
        {
            BoltUnitManager.Instance.unitList.Remove(this.gameObject);
        }
        else
        {
            BoltUnitManager.Instance.enemyList.Remove(this.gameObject);
        }
    }

    // type = 0: ignore move
    // type = 1: attack move
    public void MoveToPlace(Vector3 location, int type)
    {
        if (type == 0)
        {
            ignoreEnemy = true;
        }
        if (type == 1)
        {
            ignoreEnemy = false;
        }
        // refactored this line to outside the ifs
        targetPosition = location;

        myAgent.speed = movementSpeed;
        myAgent.SetDestination(targetPosition);
        // sets the final destination to the location clicked.
        //this.transform.LookAt(location); Need to lerp this
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
    }

    public override void OnEvent(ReceiveDamage e) {

        state.Health -= e.DamageTaken;
    }

    private void HealthCallback()
    {
        currentHealth = state.Health;
        Debug.Log(currentHealth);
        if (currentHealth <= 0)
        {
            BoltNetwork.Destroy(this.gameObject);
        }
    }

    public float getMaxHealth()
    {
        return maxHealth;
    }

    public float getCurrentHealth()
    {
        return currentHealth;
    }

    public string getUnitType()
    {
        return unitType;
    }

    public string getDamageType()
    {
        return damageType;
    }

    public bool getSelected()
    {
        return selected;
    }

    public void setSelected(bool selected)
    {
        this.selected = selected;
    }

    public float getAcquisitionSpeed()
    {
        return acquisitionSpeed;
    }

    public float getStartShootTime()
    {
        return startShootTime;
    }
    private bool isCanAttack()
    {
        if (!aimedAtEnemy)
        {
            aimedAtEnemy = true;
            return false;
        }
        return attackCooldown <= Time.time;
    }

    private void attackEnemy(BoltUnit enemy)
    {
        enemy.TakeDamage(damage);

        ReceiveDamage e = ReceiveDamage.Create(enemy.gameObject.GetComponent<BoltEntity>(), EntityTargets.Everyone);
        e.DamageTaken = damage;
        e.Send();

        stationaryIndicator.SetActive(true);
        canMove = false;
        startShootTime = Time.time;
    }

    private void cantAttack()
    {
        attackCooldown = Time.time + attackSpeed;
    }
}
