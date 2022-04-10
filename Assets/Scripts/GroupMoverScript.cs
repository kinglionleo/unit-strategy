using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GroupMoverScript : MonoBehaviour
{

    private NavMeshAgent myAgent;
    private bool moving;
    private Vector3 targetPosition;
    // Start is called before the first frame update

    void Awake()
    {
        myAgent = this.gameObject.GetComponent<NavMeshAgent>();
    }
    void Start()
    {
        moving = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(moving) {
            if(this.transform.childCount == 1) {
                Destroy(this);
            }
        }

        if(Mathf.Abs(this.transform.position.x - targetPosition.x) <= 0.2 &&
           Mathf.Abs(this.transform.position.z - targetPosition.z) <= 0.2 )
        {
            this.transform.DetachChildren();
            Destroy(this);
        }
    }

    // type = 0: ignore move
    // type = 1: attack move
    public void MoveToPlace(Vector3 location, float movementSpeed)
    {
        myAgent.speed = movementSpeed;
        targetPosition = location;
        moving = true;
        
        myAgent.SetDestination(targetPosition);
        // sets the final destination to the location clicked.
        //this.transform.LookAt(location); Need to lerp this
    }

    public void SetPosition(Vector3 location) {
        this.transform.position = location;
    }

}