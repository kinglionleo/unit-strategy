using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitClick : MonoBehaviour
{

    private Camera cam;

    public LayerMask clickable;
    public LayerMask ground;
    public float doubleClickSpeed;

    // This stores the period in time when the last click was
    private float lastClickTime;

    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
        lastClickTime = 0;
        ground = LayerMask.NameToLayer("Ground");
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            if(Physics.Raycast(ray, out hit, Mathf.Infinity, clickable))
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    UnitManager.Instance.ShiftClickSelect(hit.collider.gameObject);
                }
                else
                {
                    UnitManager.Instance.ClickSelect(hit.collider.gameObject);
                }
            }
            else
            {
                if (!Input.GetKey(KeyCode.LeftShift))
                {
                    // Deselect everything because we clicked the ground
                    UnitManager.Instance.DeselectAll();
                }
                
            }
        }
        if (!Input.GetKey(KeyCode.LeftControl) && Input.GetMouseButtonDown(1))
        {
            RaycastHit hit;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                if (hit.transform.gameObject.layer == ground) {

                    if(Input.GetKey(KeyCode.LeftShift)){
                        UnitManager.Instance.GroupIgnoreMove(hit.point);
                    }
                    else{
                        UnitManager.Instance.RightClickIgnoreMove(hit.point);
                    }
                    // This checks for double clicking logic;
                    if(Time.time < lastClickTime + doubleClickSpeed)
                    {
                        if(Input.GetKey(KeyCode.LeftShift)){
                            UnitManager.Instance.GroupAttackMove(hit.point);
                        }
                        else {
                            UnitManager.Instance.RightClickAttackMove(hit.point);
                        }
                    }
                    lastClickTime = Time.time;
                    }
                else {
                    // do nothing, raycast hit an obstacle
                }
            }
        }
        
    }
}
