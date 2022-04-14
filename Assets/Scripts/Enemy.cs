using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{

    protected NavMeshAgent myAgent;
    protected LineRenderer lineRenderer;
    // Shows a shot/bullet moving towards towards the target
    protected GameObject shotLineRenderer;
    // The health that is equivalent to the "damage" that has not reached yet (bullets approaching) units will not
    // attack units with trueCurrentHealth <= 0
    protected float trueCurrentHealth;
    // The health that is displayed, not necessarily equivalent to trueCurrentHealth
    public float currentHealth;
    public float maxHealth;
    // How fast this unit attacks measured in seconds
    public float attackSpeed;
    // The distance before this unit can start attacking
    public float range;
    // How much damage an attack does
    public float damage;
    // The splash radius of an attack
    public float damageRadius;

    public Material flashMaterial;
    private bool materialsChanged;
    private Material[][] myMaterials;

    private float damageTakenTime;

    // Denotes the point in time when the unit just shot a bullet
    protected float startShootTime;
    // A base delay for the unit to start shooting after acquiring the target. All units should have this > 0, buildings can have 0.
    protected float aimSpeed;
    // Denotes the point in time when the unit can start attacking
    protected float attackCooldown;
    // Denotes the point in time when the unit just started aiming
    protected float startAimTime;
    // Holds the distance of the closest unit to attack.
    protected float closestUnitDistance;
    // Denotes if the unit is in an aiming state, basically, a NEW target has appeared and it is waiting on its aiming speed
    protected bool startedAimingPhase;
    // Denotes if the unit is currently aimed at an enemy, so it no longer has to wait for its aiming speed
    protected bool aimedAtUnit;

    void Awake() {
        
        currentHealth = maxHealth;
        trueCurrentHealth = currentHealth;

    }

    // Start is called before the first frame update
    void Start()
    {
        UnitManager.Instance.enemyList.Add(this.gameObject);
        myAgent = this.GetComponent<NavMeshAgent>();

        shotLineRenderer = this.transform.Find("ShotLineRenderer").gameObject;
        if (shotLineRenderer != null) {
            shotLineRenderer.SetActive(false);
        }

        lineRenderer = this.GetComponent<LineRenderer>();
        lineRenderer.startWidth = 0.04f;
        lineRenderer.endWidth = 0.04f;
        lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = true;
        lineRenderer.startColor = Color.white;
        lineRenderer.endColor = Color.white;

        this.transform.Find("EnemyHealthBar").gameObject.SetActive(true);

        myMaterials = new Material[this.transform.GetChild(1).childCount][];
        for (int i = 0; i < myMaterials.Length; i++)
        {
            myMaterials[i] = new Material[this.transform.GetChild(1).GetChild(i).GetComponent<Renderer>().materials.Length];
            for (int j = 0; j < myMaterials[i].Length; j++)
            {
                myMaterials[i][j] = this.transform.GetChild(1).GetChild(i).GetComponent<Renderer>().materials[j];
            }
        }

        attackCooldown = 0;
        startAimTime = 0;
        startShootTime = 0;
        aimedAtUnit = false;
        startedAimingPhase = false;
    }

    void Update()
    {
        // If the unit is in blueprint mode, it shouldn't do anything. TODO: Remove this part later
        if (this.gameObject.tag.Equals("Blueprint"))
        {
            return;
        }

        if (materialsChanged && Time.time >= damageTakenTime + 0.1)
        {
            revertToNormalMaterials();
            materialsChanged = false;
        }

        if (currentHealth <= 0) Destroy(this.gameObject);
        // Loop through all enemies

        // Two variables to store the current closest distance and current closest unit (not necessarily within range)
        closestUnitDistance = float.MaxValue;
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
                    Unit unitToAttack = closestUnit.transform.GetComponent<Unit>();
                    if (isCanAttack() && unitToAttack.getTrueCurrentHealth() > 0)
                    {
                        //Debug.Log("enemy attacked!");
                        this.transform.LookAt(closestUnit.transform);
                        attackUnit(unitToAttack);
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

    // Take damage delay is for caller to pass in when the bullet arrives.
    public void TakeDamage(float damage, float damageRadius, float takeDamageDelay)
    {
        trueCurrentHealth -= damage;
        Invoke(nameof(displayDamage), takeDamageDelay);
    }

    // Will be called by TakeDamage after a delay, when the bullet reaches the target.
    private void displayDamage() {
        currentHealth = trueCurrentHealth;
        damageTakenTime = Time.time;
        flashAnimation();

        if (damageRadius != 0) {

            foreach (var unit in UnitManager.Instance.enemyList) {

                if (unit == null) {
                    UnitManager.Instance.enemyList.Remove(unit);
                    continue;
                }
                if (GameObject.ReferenceEquals(unit, this.gameObject)) {
                    continue;
                }
                if(Vector3.Distance(unit.transform.position, this.transform.position) <= damageRadius) {
                    unit.gameObject.GetComponent<Enemy>().TakeDamage(damage, 0, 0);
                }
            }
        }

        if (currentHealth <= 0) {
            Destroy(this.gameObject);
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

    // Units should be checking true current health to determine if they should attack
    public float getTrueCurrentHealth()
    {
        return trueCurrentHealth;
    }

    public float getStartShootTime()
    {
        return startShootTime;
    }

    public float getClosestUnitDistance() {
        return closestUnitDistance;
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

    protected void attackUnit(Unit unit)
    {
        shotLineRenderer.SetActive(true);
        shotLineRenderer.gameObject.GetComponent<ShotRendererScript>().startShot(unit.transform.position);
        float takeDamageDelay = shotLineRenderer.gameObject.GetComponent<ShotRendererScript>().shotTimeLength;
        unit.TakeDamage(damage, damageRadius, takeDamageDelay);

        startShootTime = Time.time;
    }

    private void cantAttack()
    {
        attackCooldown = Time.time + attackSpeed;
    }

    private void flashAnimation()
    {
        int count = this.transform.GetChild(1).childCount;
        for (int i = 0; i < count; i++)
        {
            Material[] m = new Material[this.transform.GetChild(1).GetChild(i).GetComponent<Renderer>().materials.Length];
            for (int j = 0; j < m.Length; j++)
            {
                m[j] = flashMaterial;
            }
            this.transform.GetChild(1).GetChild(i).GetComponent<Renderer>().materials = m;
        }
        materialsChanged = true;

    }

    private void revertToNormalMaterials()
    {
        int count = this.transform.GetChild(1).childCount;
        for (int i = 0; i < count; i++)
        {
            this.transform.GetChild(1).GetChild(i).GetComponent<Renderer>().materials = myMaterials[i];
        }
    }

}
