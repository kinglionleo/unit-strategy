using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Unit : MonoBehaviour
{

    NavMeshAgent myAgent;
    public float currentHealth;
    private float maxHealth;
    
    // Start is called before the first frame update
    void Start()
    {
        UnitSelections.Instance.unitList.Add(this.gameObject);
        myAgent = this.GetComponent<NavMeshAgent>();

        maxHealth = 100;
        currentHealth = maxHealth;
        this.transform.Find("HealthBarCanvas").gameObject.SetActive(true);
    }

    void OnDestroy()
    {
        UnitSelections.Instance.unitList.Remove(this.gameObject);
    }

    public void MoveToPlace(Vector3 location)
    {
        myAgent.SetDestination(location);
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
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
