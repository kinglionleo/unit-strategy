using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Unit : MonoBehaviour
{

    NavMeshAgent myAgent;
    public float currentHealth;
    private float maxHealth;
    private bool hasAttacked;
    
    // Start is called before the first frame update
    void Start()
    {
        UnitManager.Instance.unitList.Add(this.gameObject);
        myAgent = this.GetComponent<NavMeshAgent>();

        maxHealth = 100;
        currentHealth = maxHealth;
        this.transform.Find("HealthBarCanvas").gameObject.SetActive(true);
        hasAttacked = false;
    }

    void OnDestroy()
    {
        UnitManager.Instance.unitList.Remove(this.gameObject);
    }

    void Update()
    {
        foreach(var unit in UnitManager.Instance.enemyList){

            float distance = Mathf.Sqrt((unit.transform.position.x - this.transform.position.x) * (unit.transform.position.x - this.transform.position.x) +
                             (unit.transform.position.z - this.transform.position.z) * (unit.transform.position.z - this.transform.position.z));
            if(distance < 2f)
            {
                if (!hasAttacked)
                {
                    unit.transform.GetComponent<Enemy>().TakeDamage(10);
                    hasAttacked = true;
                }
                
            }
        }
    }
    public void MoveToPlace(Vector3 location)
    {
        myAgent.SetDestination(location);
    }

    public void TakeDamage(int damage)
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
