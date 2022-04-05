using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{

    NavMeshAgent myAgent;
    public float currentHealth;
    private float maxHealth;

    // Start is called before the first frame update
    void Start()
    {
        UnitManager.Instance.enemyList.Add(this.gameObject);
        myAgent = this.GetComponent<NavMeshAgent>();

        maxHealth = 100;
        currentHealth = maxHealth;
        this.transform.Find("EnemyHealthBar").gameObject.SetActive(true);
    }

    void Update()
    {
        if (currentHealth <= 0) Destroy(this.gameObject);
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

}
