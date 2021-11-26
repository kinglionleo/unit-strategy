using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Unit : MonoBehaviour
{
    // Used for pathfinding
    NavMeshAgent myAgent;

    public float maxHealth;
    public float currentHealth;

    // How fast this unit attacks measured in seconds
    public float attackSpeed;
    // How long this unit takes to start shooting after coming to a stop. 0 means this unit can attack while moving
    public float acquisitionSpeed;
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
    private bool stationary;
    private bool selected;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    void Start()
    {
        UnitManager.Instance.unitList.Add(this.gameObject);
        myAgent = this.GetComponent<NavMeshAgent>();

        this.transform.Find("HealthBarCanvas").gameObject.SetActive(true);
        this.transform.Find("RangeIndicator").gameObject.SetActive(false);
        attackCooldown = 0;
        timeStationary = 0;
        stationary = true;
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

            float distance = Mathf.Sqrt((unit.transform.position.x - this.transform.position.x) * (unit.transform.position.x - this.transform.position.x) +
                             (unit.transform.position.z - this.transform.position.z) * (unit.transform.position.z - this.transform.position.z));

            if (distance < closestDistance)
            {

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

        if (closestDistance < range) // we need to raycast this instead eventually to allow walls
        {
            
            if (closestEnemy != null && canAttack() && stationary)
            {
                this.transform.LookAt(closestEnemy.transform);
                closestEnemy.transform.GetComponent<Enemy>().TakeDamage(damage);
                cantAttack();
            }
        }

    }
    public void MoveToPlace(Vector3 location)
    {
        myAgent.speed = movementSpeed;
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

    private bool canAttack()
    {
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
            stationary = false;
        }
    }

    




}
