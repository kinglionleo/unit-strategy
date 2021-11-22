using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Unit : MonoBehaviour
{

    NavMeshAgent myAgent;
    
    // Start is called before the first frame update
    void Start()
    {
        UnitSelections.Instance.unitList.Add(this.gameObject);
        myAgent = this.GetComponent<NavMeshAgent>();

    }

    void OnDestroy()
    {
        UnitSelections.Instance.unitList.Remove(this.gameObject);
    }

    public void MoveToPlace(Vector3 location)
    {
        myAgent.SetDestination(location);
    }

}
