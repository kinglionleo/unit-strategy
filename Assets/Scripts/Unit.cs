using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Unit : MonoBehaviour
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
    // Denotes if the unit is in an aiming state, basically, a NEW target has appeared and it is waiting on its aiming speed
    private bool startedAim;
    // Denotes if the unit is currently aimed at an enemy, so it no longer has to wait for its aiming speed
    private bool aimedAtEnemy;
    // Denotes if the unit can move, which it cannot if it just shot
    private bool canMove;
    // Denotes if the unit is selected by the user
    private bool selected;

    GameObject prevClosestEnemy;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    void Start()
    {
        /*
         * This code is just instantiation related
         */
        UnitManager.Instance.unitList.Add(this.gameObject);

        myAgent = this.GetComponent<NavMeshAgent>();
        animator = this.GetComponent<Animator>();

        stationaryIndicator = this.transform.Find("StationaryIndicator").gameObject;
        stationaryIndicator.SetActive(false);

        lineRenderer = this.GetComponent<LineRenderer>();
        lineRenderer.startWidth = 0.04f;
        lineRenderer.endWidth = 0.04f;
        lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = true;
        lineRenderer.startColor = Color.white;
        lineRenderer.endColor = Color.white;

        this.transform.Find("HealthBarCanvas").gameObject.SetActive(true);
        this.transform.Find("RangeIndicator").gameObject.SetActive(false);
        attackCooldown = 0;
        startAimTime = 0;
        startShootTime = 0;
        aimedAtEnemy = false;
        startedAim = false;
        canMove = true;
        prevClosestEnemy = null;
    }

    void OnDestroy()
    {
        UnitManager.Instance.unitList.Remove(this.gameObject);
    }

    void Update()
    {
        // If the unit cannot move, it checks for if it has stayed still for long enough, then allows it to move
        if (!canMove)
        {
            // Checks if the current point in time is greater than the time it shot a bullet and the time it must stay still
            if (Time.time >= startShootTime + acquisitionSpeed)
            {
                canMove = true;
                stationaryIndicator.SetActive(false);
            }
        }

        // This handles if the unit is selected or not. Pretty inefficient, can be a on/off function instead.
        if (selected)
        {
            this.transform.GetChild(0).gameObject.SetActive(true);
            this.transform.Find("RangeIndicator").gameObject.SetActive(true);
            this.transform.Find("RangeIndicator").transform.localScale = new Vector3(range*4, range*4, 1);
        }
        else
        {
            this.transform.GetChild(0).gameObject.SetActive(false);
            this.transform.Find("RangeIndicator").gameObject.SetActive(false);
        }

        // The following section handles attacking logic

        // Two variables to store the current closest distance and current closest enemy (not necessarily within range)
        float closestDistance = float.MaxValue;
        GameObject closestEnemy = null;

        // Loop through all enemies
        foreach(var unit in UnitManager.Instance.enemyList){

            // Basically handles when an enemy is in blueprint mode (spawning) - this should be removed eventually
            if (!unit.gameObject.tag.Equals("Enemy"))
            {
                continue;
            }

            float distance = Mathf.Sqrt((unit.transform.position.x - this.transform.position.x) * (unit.transform.position.x - this.transform.position.x) +
                             (unit.transform.position.z - this.transform.position.z) * (unit.transform.position.z - this.transform.position.z));

            // closestDistance is the distance to the closest enemy
            if (distance < closestDistance)
            {
                // This chunk checks to see if the unit is trying to shoot through a physical barrier such as a house by using a raycast
                RaycastHit hit;
                if (Physics.Raycast(this.transform.position, (unit.transform.position - this.transform.position), out hit, range))
                {
                    if (hit.transform == unit.transform)
                    {
                        // If the enemy is going to be the current closest enemy and is also in line-of-sight, we save it to be checked further
                        closestDistance = distance;
                        closestEnemy = unit;

                    }
                }
                
            }
        }

        // Now we check if the closest enemy is in range or not
        if (closestDistance < range)
        {
                  
            if (closestEnemy != null)
            { 
                if (closestEnemy != prevClosestEnemy) {
                    startedAim = false;
                    aimedAtEnemy = false;
                    prevClosestEnemy = closestEnemy;
                }
                
                if (!startedAim && !aimedAtEnemy) {
                    startAimTime = Time.time;
                    startedAim = true;
                }

                // This draws a line from the unit's current position to the closest enemy in range.
                lineRenderer.SetPosition(0, this.transform.position);
                lineRenderer.SetPosition(1, closestEnemy.transform.position);

                // This checks if the unit has met the aiming time requirement
                if (startAimTime + aimSpeed <= Time.time) 
                {
                    aimedAtEnemy = true;
                    startedAim = false;

                    if (isCanAttack())
                    {
                        this.transform.LookAt(closestEnemy.transform);
                        attackEnemy(closestEnemy.transform.GetComponent<Enemy>());
                        cantAttack();
                    }
                }
            }
            else
            {
                startedAim = false;
                aimedAtEnemy = false;
            }
        }
        else
        {
            // This makes it so no line is drawn since it is the same point.
            lineRenderer.SetPosition(0, new Vector3(0,0,0));
            lineRenderer.SetPosition(1, new Vector3(0,0,0));
        }

    }
    public void MoveToPlace(Vector3 location)
    {
        if(!canMove)
        {
            myAgent.speed = 0;
            return;
        }

        myAgent.speed = movementSpeed;
        myAgent.SetDestination(location);
        //this.transform.LookAt(location); Need to lerp this
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

    private void attackEnemy(Enemy enemy)
    {
        enemy.TakeDamage(damage);
        stationaryIndicator.SetActive(true);
        canMove = false;
        startShootTime = Time.time;
    }

    private void cantAttack()
    {
        attackCooldown = Time.time + attackSpeed;
    }
}
