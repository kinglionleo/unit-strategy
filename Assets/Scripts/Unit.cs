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

    private float attackCooldown;
    private float timeStationary;
    private float startStationary;
    private float startAimTime;
    private bool startedAim;
    private bool aimedAtEnemy;
    private bool stationary;
    private bool selected;

    GameObject prevClosestEnemy;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    void Start()
    {
        UnitManager.Instance.unitList.Add(this.gameObject);

        myAgent = this.GetComponent<NavMeshAgent>();
        animator = this.GetComponent<Animator>();

        stationaryIndicator = this.transform.Find("StationaryIndicator").gameObject;

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
        timeStationary = 0;
        startStationary = 0;
        startAimTime = 0;
        aimedAtEnemy = false;
        startedAim = false;
        stationary = true;
        prevClosestEnemy = null;
    }

    void OnDestroy()
    {
        UnitManager.Instance.unitList.Remove(this.gameObject);
    }

    void Update()
    {
        if (acquisitionSpeed != 0)
        {
            checkForMovement();
        }

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

        float closestDistance = float.MaxValue;
        GameObject closestEnemy = null;

        foreach(var unit in UnitManager.Instance.enemyList){

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

                        closestDistance = distance;
                        closestEnemy = unit;

                    }
                }
                
            }
        }

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

                if (startAimTime + aimSpeed <= Time.time) 
                {
                    aimedAtEnemy = true;
                    startedAim = false;

                    if(Time.time > startStationary)
                    {
                        stationaryIndicator.SetActive(true);
                    }
                    else
                    {
                        stationaryIndicator.SetActive(false);
                    }

                    if (isCanAttack() && stationary)
                    {
                        this.transform.LookAt(closestEnemy.transform);
                        closestEnemy.transform.GetComponent<Enemy>().TakeDamage(damage);
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

            stationaryIndicator.SetActive(false);

        }

    }
    public void MoveToPlace(Vector3 location)
    {
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

    public float getTimeStationary()
    {
        return timeStationary;
    }

    public float getStartStationary()
    {
        return startStationary;
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

    private void cantAttack()
    {
        attackCooldown = Time.time + attackSpeed;
    }

    private void checkForMovement()
    {
        if (myAgent.velocity.magnitude <= 0.01)
        {
            if (timeStationary <= Time.time) stationary = true;
            else stationary = false;
        }
        else
        {
            timeStationary = Time.time + acquisitionSpeed;
            startStationary = Time.time;
            stationary = false;
        }
    }
}
