using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.AI;
using Photon.Bolt;

public abstract class BoltUnit : EntityEventListener<IUnit>
{
    // Used for pathfinding
    protected NavMeshAgent myAgent;
    protected LineRenderer lineRenderer;

    // Shows a shot/bullet moving towards towards the target
    public GameObject shotLineRenderer;
    // Shows the radius of splash damage if applicable
    public GameObject splashIndicator;
    // Show a particle effect when dying
    public ParticleSystem deathEffect;

    // The black acquisition circle that appears when a unit becomes stationary
    protected GameObject stationaryIndicator;
    protected GameObject aimingIndicator;
    protected Animator animator;

    protected float maxHp;
    protected float currentHp;

    // How fast this unit attacks measured in seconds
    protected float reloadTime;
    // How long this unit takes to start aiming after coming to a stop. 0 means this unit can attack while moving
    protected float stationaryDelay;
    // A base delay for the unit to start shooting after acquiring the target. All units should have this > 0, buildings can have 0.
    protected float aimingTime;
    // How fast this unit moves
    protected float movementSpeed;
    // The distance before this unit can start attacking
    protected float range;
    // How much damage an attack does
    protected float damage;
    // The radius of this attack, 0 if no splash
    protected float damageRadius;
    // How much it costs to spawn this unit
    protected int cost;
    // The research requirement for this unit
    protected int researchRequirement;
    // How long this unit lasts
    protected int lifetime;

    // If this unit is flying or ground
    protected string unitType;

    // Material related
    public Material flashMaterial;
    protected bool materialsChanged;
    protected Material[][] myMaterials;
    protected Material[][] flashMaterials;

    protected LayerMask ignoreLayer;

    // The remaining two stats "unitCount" and "cost" will be stored in the UnitManager

    // Denotes the point in time when the unit can start attacking
    protected float nextAttackTime;
    // Denotes the point in time when the unit just started aiming
    protected float startAimTime;
    // Denotes the point in time when the unit just shot a bullet
    protected float startShootTime;
    // Denotes the position the unit is going to
    protected Vector3 targetPosition;
    // Denotes the radius of the hitbox so that it is accounted for in distance comparison
    protected float hitboxRadius;
    // Denotes when this unit took damage
    protected float damageTakenTime;
    // Denotes when this unit spawned
    protected float spawnTime;

    // Denotes if the unit is in an aiming state, basically, a NEW target has appeared and it is waiting on its aiming speed
    protected bool startedAimingPhase;
    // Denotes if the unit is currently aimed at an enemy, so it no longer has to wait for its aiming speed
    protected bool aimedAtEnemy;
    // Denotes if the unit can move, which it cannot if it just shot
    protected bool canMove;
    // Denotes if the unit is selected by the user
    protected bool selected;
    // Denotes if the unit can auto attack
    protected bool ignoreEnemy;

    protected GameObject prevClosestEnemy;

    // Start Equivalent
    public override void Attached()
    {
        // This ensures that we are only modifying the health variable if we are the owner when the Network instantiates.
        if(entity.IsOwner)
        {
            state.Health = maxHp;
            state.TrueHealth = maxHp;
        }
        spawnTime = BoltNetwork.ServerTime;
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

    // Destroy Equivalent
    public override void Detached()
    {
        Debug.Log("Detached");
        if (entity.IsOwner)
        {
            BoltUnitManager.Instance.unitList.Remove(this.gameObject);
            // To make sure that units that die get deselected
            BoltUnitManager.Instance.unitsSelected.Remove(this.gameObject);
        }
        else
        {
            BoltUnitManager.Instance.enemyList.Remove(this.gameObject);
        }
        Instantiate(deathEffect, transform.position, Quaternion.identity);

    }
    void Awake()
    {
         // Gets the stats and sets them from StatsManager that this unit is supposed to have upon spawn.
        PullStatsFromManager();
        currentHp = maxHp;
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
        nextAttackTime = 0;
        startAimTime = 0;
        startShootTime = 0;
        aimedAtEnemy = false;
        startedAimingPhase = false;
        canMove = true;
        prevClosestEnemy = null;
        targetPosition = this.transform.position;
        ignoreEnemy = false;
        hitboxRadius = this.gameObject.GetComponent<CapsuleCollider>().radius * this.transform.localScale.x;
    }

    void Update()
    {
        if (materialsChanged && Time.time >= damageTakenTime + 0.1)
        {
            revertToNormalMaterials();
            materialsChanged = false;
        }
    }

    public override void SimulateOwner()
    {
        if (lifetime != 0 && BoltNetwork.ServerTime >= spawnTime + lifetime)
        {
            BoltNetwork.Destroy(this.gameObject);
        }
        // If the unit cannot move, it checks for if it has stayed still for long enough, then allows it to move
        if (!canMove)
        {
            if (myAgent != null) {
                myAgent.SetDestination(this.transform.position);
            }
            // Checks if the current point in time is greater than the time it shot a bullet and the time it must stay still
            if (Time.time >= startShootTime + stationaryDelay)
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
                        GameObject shotLineRendererClone = Instantiate(shotLineRenderer, this.transform);
                        shotLineRendererClone.gameObject.GetComponent<BoltShotLineRenderer>().startShot(closestEnemy);
                        // float takeDamageDelay = shotLineRendererClone.gameObject.GetComponent<ShotRendererScript>().getShotTimeLength();
                        // shotLineRenderer.gameObject.GetComponent<BoltShotLineRenderer>().startShot(closestEnemy);

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

                        // Now we can't move
                        stationaryIndicator.SetActive(true);
                        canMove = false;
                        startShootTime = Time.time;

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

    void OnDestroy()
    {
        
    }

    protected virtual void PullStatsFromManager() 
    {
        BoltStatsManagerScript.UnitType unitType = BoltStatsManagerScript.UnitType.Basic;
        BoltStatsManagerScript.UnitStats unitStats = BoltStatsManagerScript.Instance.GetUnitStats(unitType);
        SetStatsFromManager(unitStats);
    }

    protected void SetStatsFromManager(BoltStatsManagerScript.UnitStats unitStats)
    {
        this.cost = unitStats.cost;
        this.researchRequirement = unitStats.researchRequirement;
        this.lifetime = unitStats.lifetime;
        this.maxHp = unitStats.hp;
        this.damage = unitStats.damage;
        this.damageRadius = unitStats.damageRadius;
        this.reloadTime = unitStats.reloadTime;
        this.aimingTime = unitStats.aimingTime;
        this.stationaryDelay = unitStats.stationaryDelay;
        this.movementSpeed = unitStats.movementSpeed;
        this.range = unitStats.range;
    }

    // type = 0: ignore move
    // type = 1: attack move
    public virtual void MoveToPlace(Vector3 location, int type, float speed)
    {
        if (myAgent == null)
        {
            return;
        }

        if (type == 0 || type == 2)
        {
            ignoreEnemy = true;
        }
        if (type == 1 || type == 3)
        {
            ignoreEnemy = false;
        }
        // refactored this line to outside the ifs
        targetPosition = location;

        if (type == 2 || type == 3)
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

    // It's important to note that this is for a LOCAL ENEMY to have their health updated without needing to access server health.
    // This is for pure, instantaneous feedback and has nothing to do with an enemy's actual health.
    public void TakeDamage(float damage)
    {
        damageTakenTime = Time.time;
        
        flashAnimation();
    }

    // This event will only be recieved by OWNERS of the entity.
    public override void OnEvent(ReceiveDamage e) {
        StartCoroutine(ReceiveDamageEvent(e.DamageTaken, e.DamageRadius, e.DamageDealer, e.DelayUntilDamageIsTaken));
    }

    protected IEnumerator ReceiveDamageEvent(float damageTaken, float damageRadius, BoltEntity damageDealer, float delay)
    {
        yield return new WaitForSeconds(delay);

        damageTakenTime = Time.time;
        flashAnimation();

        state.Health -= damageTaken;

        if (damageRadius != 0)
        {
            float scaledDamageRadius = damageRadius * 2;
            GameObject splashIndicatorClone = Instantiate(splashIndicator);
            splashIndicatorClone.gameObject.GetComponent<BoltSplashIndicatorScript>().startSplash(scaledDamageRadius, this);

            foreach (var unit in BoltUnitManager.Instance.unitList)
            {

                if (unit == null)
                {
                    BoltUnitManager.Instance.unitList.Remove(unit);
                    continue;
                }
                if (GameObject.ReferenceEquals(unit, this.gameObject))
                {
                    continue;
                }
                if (Vector3.Distance(unit.transform.position, this.transform.position) <= damageRadius)
                {
                    unit.gameObject.GetComponent<BoltUnit>().state.Health -= damageTaken;
                    unit.gameObject.GetComponent<BoltUnit>().state.TrueHealth -= damageTaken;

                    Debug.Log("H: " + unit.gameObject.GetComponent<BoltUnit>().state.Health + ", TH:" + unit.gameObject.GetComponent<BoltUnit>().state.TrueHealth);
                }
            }
        }

    }

    // It's important to note that this is for a LOCAL ENEMY to start the shot animation. No health is modified.
    // If we are the owner, we will NEVER receive this event.
    public override void OnEvent(ShotFired e)
    {
        if (e != null && e.Target != null && shotLineRenderer != null && shotLineRenderer.GetComponent<BoltShotLineRenderer>() != null) {
            e.Target.gameObject.GetComponent<BoltUnit>().state.TrueHealth -= e.DamageTaken;
            Debug.Log("TH: " + e.Target.gameObject.GetComponent<BoltUnit>().state.TrueHealth);
            GameObject shotLineRendererClone = Instantiate(shotLineRenderer, this.transform);
            shotLineRendererClone.GetComponent<BoltShotLineRenderer>().startShot(e.Target.gameObject);
        }
    }

    protected void HealthCallback()
    {
        currentHp = state.Health;
        Debug.Log(currentHp);
        if (currentHp <= 0)
        {
            BoltNetwork.Destroy(this.gameObject);
        }
    }

    public abstract BoltStatsManagerScript.UnitType GetUnitType();

    public float getMaxHp()
    {
        return maxHp;
    }

    public float getCurrentHp()
    {
        return currentHp;
    }

    public string getUnitType()
    {
        return unitType;
    }

    public bool getSelected()
    {
        return selected;
    }

    public void setSelected(bool selected)
    {
        this.selected = selected;
    }

    public float getStationaryDelay()
    {
        return stationaryDelay;
    }

    public float getStartShootTime()
    {
        return startShootTime;
    }

    public float getStartAimTime()
    {
        return startAimTime;
    }

    public float getHitboxSize()
    {
        return hitboxRadius;
    }

    public int getCost()
    {
        return cost;
    }

    public float getAimingTime() 
    {
        return aimingTime;
    }

    public float getLifetime()
    {
        return lifetime;
    }

    public int getResearchRequirement()
    {
        return researchRequirement;
    }

    public float getSpawnTime()
    {
        return spawnTime;
    }

    public float getMovementSpeed()
    {
        return movementSpeed;
    }

    protected bool isCanAttack()
    {
        if (!aimedAtEnemy)
        {
            aimedAtEnemy = true;
            return false;
        }
        return nextAttackTime <= Time.time;
    }

    protected virtual void attackEnemy(BoltUnit enemy, float delay)
    {
        // Defensive programming: ensure that our enemy still exists
        if(enemy != null)
        {
            // This tells the local enemy to flash
            enemy.TakeDamage(damage);

            // This tells the online enemy to take damage
            ReceiveDamage e = ReceiveDamage.Create(enemy.gameObject.GetComponent<BoltEntity>(), EntityTargets.OnlyOwner);
            e.DamageTaken = damage;
            e.DamageRadius = damageRadius;
            e.DamageDealer = this.GetComponent<BoltEntity>();
            e.DelayUntilDamageIsTaken = delay;
            e.Send();
            
            if (damageRadius > 0) {
                float scaledDamageRadius = damageRadius * 2;
                GameObject splashIndicatorClone = Instantiate(splashIndicator);
                splashIndicatorClone.gameObject.GetComponent<BoltSplashIndicatorScript>().startSplash(scaledDamageRadius, enemy);
            }
        }
    }

    protected void cantAttack()
    {
        nextAttackTime = Time.time + reloadTime;
    }

    protected void flashAnimation()
    {
        if(this == null)
        {
            Debug.Log("Null Child");
            return;
        }
        int count = this.transform.GetChild(1).childCount;
        for (int i = 0; i < count; i++)
        {
            this.transform.GetChild(1).GetChild(i).GetComponent<Renderer>().materials = flashMaterials[i];
        }
        materialsChanged = true;

    }

    protected void revertToNormalMaterials()
    {
        int count = this.transform.GetChild(1).childCount;
        for (int i = 0; i < count; i++)
        {
            this.transform.GetChild(1).GetChild(i).GetComponent<Renderer>().materials = myMaterials[i];
        }
    }
}
