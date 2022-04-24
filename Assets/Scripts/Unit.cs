using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Unit : MonoBehaviour
{
    // Used for pathfinding
    NavMeshAgent myAgent;
    LineRenderer lineRenderer;
    // Shows a shot/bullet moving towards towards the target
    public GameObject shotLineRenderer;
    // Shows the radius of splash damage if applicable
    public GameObject splashIndicator;
    // The black acquisition circle that appears when a unit becomes stationary
    GameObject stationaryIndicator;
    // White circle that appears when a unit is aiming
    GameObject aimingIndicator;
    Animator animator;

    public float maxHealth;
    // The health that is equivalent to the "damage" that has not reached yet (bullets approaching) units will not
    // attack units with trueCurrentHealth <= 0
    protected float trueCurrentHealth;
    // The health that is displayed, not necessarily equivalent to trueCurrentHealth
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
    // The splash radius of an attack
    public float damageRadius;

    // If this unit is flying or ground
    public string unitType;
    // If this unit deals splash or individual damage
    public string damageType;

    public Material flashMaterial;
    private bool materialsChanged;
    private Material[][] myMaterials;
    private Material[][] flashMaterials;

    // Denotes the point in time when the unit can start attacking
    private float attackCooldown;
    // Denotes the point in time when the unit just started aiming
    private float startAimTime;
    // Denotes the point in time when the unit just shot a bullet
    private float startShootTime;
    // Denotes the position the unit is going to
    private Vector3 targetPosition;
    // The time when this unit took damage
    private float damageTakenTime;

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

    //The distance to the closest enemy.
    private float closestEnemyDistance;

    GameObject closestEnemy;

    GameObject prevClosestEnemy;

    void Awake()
    {
        currentHealth = maxHealth;
        trueCurrentHealth = currentHealth;
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

        aimingIndicator = this.transform.Find("AimingIndicator").gameObject;
        aimingIndicator.SetActive(false);

        // shotLineRenderer = this.transform.Find("ShotLineRenderer").gameObject;
        // if (shotLineRenderer != null) {
        //     shotLineRenderer.SetActive(false);
        // }

        lineRenderer = this.GetComponent<LineRenderer>();
        lineRenderer.startWidth = 0.04f;
        lineRenderer.endWidth = 0.04f;
        lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = true;
        lineRenderer.startColor = Color.white;
        lineRenderer.endColor = Color.white;

        this.transform.Find("HealthBarCanvas").gameObject.SetActive(true);
        this.transform.Find("RangeIndicator").gameObject.SetActive(false);

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
        // if (currentHealth <= 0) {
        //     Destroy(this.gameObject);
        //     return;
        // }
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
            this.transform.Find("RangeIndicator").transform.localScale = new Vector3(range * 2 / this.transform.localScale.x, range * 2 / this.transform.localScale.y, 1);
        }
        else
        {
            this.transform.GetChild(0).gameObject.SetActive(false);
            this.transform.Find("RangeIndicator").gameObject.SetActive(false);
            ignoreEnemy = false;
        }

        // The following section handles attacking logic

        // This checks for if we can interrupt movement to attack an enemy
        if(ignoreEnemy)
        {
            // TODO: make the margin of error dynamic based on the group size as bigger clumps make it impossible to reach the target destination
            if(Mathf.Abs(this.transform.position.x - targetPosition.x) <= 0.6 &&
               Mathf.Abs(this.transform.position.z - targetPosition.z) <= 0.6 )
            {
                ignoreEnemy = false;
            }
        }

        // Two variables to store the current closest distance and current closest enemy (not necessarily within range)
        closestEnemyDistance = float.MaxValue;
        closestEnemy = null;

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
            if (distance < closestEnemyDistance)
            {
                // This chunk checks to see if the unit is trying to shoot through a physical barrier such as a house by using a raycast
                RaycastHit hit;
                // Layer to ignore layer is 7, so we need to bit shift and negate
                // so the layer mask is something like 1101111111 (7th bit from the right is 0, the ignored)
                LayerMask clickableLayer = ~(1 << 7);
                Debug.Log(clickableLayer.value);
                bool cast = Physics.Raycast(this.transform.position, (unit.transform.position - this.transform.position), out hit, range, clickableLayer);
                if (cast)
                {
                    // Debugging layermask
                    // if (maxHealth >= 500) {
                    //     Debug.Log("hit " + hit.transform.name + " at " + hit.distance);
                    // }
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
                if (!startedAimingPhase && !aimedAtEnemy) {
                    startAimTime = Time.time;
                    startedAimingPhase = true;
                    aimingIndicator.SetActive(true);
                }

                // This draws a line from the unit's current position to the closest enemy in range.
                lineRenderer.SetPosition(0, this.transform.position);
                lineRenderer.SetPosition(1, closestEnemy.transform.position);

                // This checks if the unit has met the aiming time requirement
                if (startAimTime + aimSpeed <= Time.time) 
                {
                    aimedAtEnemy = true;
                    aimingIndicator.SetActive(false);
                    startedAimingPhase = false;
                    Enemy enemyToAttack = closestEnemy.transform.GetComponent<Enemy>();
                    if (isCanAttack() && !ignoreEnemy && enemyToAttack.getTrueCurrentHealth() > 0)
                    {
                        this.transform.LookAt(closestEnemy.transform);
                        StartCoroutine(attackEnemy(enemyToAttack));
                        cantAttack();
                        if (!selected) {
                            targetPosition = this.transform.position;
                        }
                    }
                }
            }
            else
            {
                startedAimingPhase = false;
                aimedAtEnemy = false;
                aimingIndicator.SetActive(false);
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
            aimingIndicator.SetActive(false);
            // This makes it so no line is drawn since it is the same point
            lineRenderer.SetPosition(0, new Vector3(0,0,0));
            lineRenderer.SetPosition(1, new Vector3(0,0,0));
        }

    }

    void OnDestroy()
    {
        UnitManager.Instance.unitList.Remove(this.gameObject);
        // To make sure that units that die get deselected
        UnitManager.Instance.unitsSelected.Remove(this.gameObject);
    }

    // type = 0: ignore move
    // type = 1: attack move

    // type = 2: ignore group move
    // type = 3: ignore attack move
    public void MoveToPlace(Vector3 location, int type, float speed)
    {
        if (type == 0 || type == 2)
        {
            ignoreEnemy = true;
        }
        if(type == 1 || type == 3)
        {
            ignoreEnemy = false;
        }
        // refactored this line to outside the ifs
        targetPosition = location;

        if(type == 2 || type == 3)
        {
            myAgent.speed = speed;
        }
        else
        {
            myAgent.speed = movementSpeed;
        }
        myAgent.SetDestination(targetPosition);
        
        // sets the final destination to the location clicked.
        //this.transform.LookAt(location); Need to lerp this
    }

    public void CancelCommand()
    {
        targetPosition = this.transform.position;
        myAgent.SetDestination(targetPosition);
        ignoreEnemy = false;
    }

    public void TakeDamage(float damage, float damageRadius, float scaledDamageRadius)
    {
        trueCurrentHealth -= damage;
        displayDamage();
        if(damageRadius != 0) {
            // clone splashRenderer at the origin of the unit that got hit.
            GameObject splashIndicatorClone = Instantiate(splashIndicator, this.transform);
            splashIndicatorClone.gameObject.GetComponent<SplashIndicatorScript>().startSplash(scaledDamageRadius);
            foreach (var unit in UnitManager.Instance.unitList) {

                if (unit == null) {
                    UnitManager.Instance.unitList.Remove(unit);
                    continue;
                }
                if (GameObject.ReferenceEquals(unit, this.gameObject)) {
                    continue;
                }
                
                if(Vector3.Distance(unit.transform.position, this.transform.position) <= damageRadius) {
                    unit.gameObject.GetComponent<Unit>().TakeDamage(damage, 0, 0);
                }
            }
        }
    }

    private void displayDamage() {
        currentHealth = trueCurrentHealth;
        damageTakenTime = Time.time;
        flashAnimation();

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

    public float getTrueCurrentHealth()
    {
        return trueCurrentHealth;
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

    public GameObject getClosestEnemy() {
        return closestEnemy;
    }

    public float getClosestEnemyDistance() {
        return closestEnemyDistance;
    }

    public float getStartAimTime() {
        return startAimTime;
    }

    public void setIgnoreEnemy(bool ignore) {
        ignoreEnemy = ignore;
    }

    public void setTargetPosition(Vector3 position)
    {
        targetPosition = position;
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

    protected virtual IEnumerator attackEnemy(Enemy enemy)
    {   
        if (enemy == null) {
            yield break;
        }
        GameObject shotLineRendererClone = Instantiate(shotLineRenderer, this.transform);
        shotLineRendererClone.gameObject.GetComponent<ShotRendererScript>().startShot(enemy.gameObject);
        float takeDamageDelay = shotLineRendererClone.gameObject.GetComponent<ShotRendererScript>().getShotTimeLength();
        
        float scaledDamageRadius = damageRadius * 2 / this.transform.localScale.x;
        
        stationaryIndicator.SetActive(true);
        canMove = false;
        startShootTime = Time.time;

        // Wait for the delay (travel time of "projectile")
        yield return new WaitForSeconds(takeDamageDelay);

        // need to call TakeDamage after we know how long the shot will take to arrive at enemy
        enemy.TakeDamage(damage, damageRadius, scaledDamageRadius);

        
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
            this.transform.GetChild(1).GetChild(i).GetComponent<Renderer>().materials = flashMaterials[i];
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
