using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitClick : MonoBehaviour
{

    private Camera cam;

    public LayerMask clickable;
    public LayerMask ground;
    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;   
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
                    UnitSelections.Instance.ShiftClickSelect(hit.collider.gameObject);
                }
                else
                {
                    UnitSelections.Instance.ClickSelect(hit.collider.gameObject);
                }
            }
            else
            {
                if (!Input.GetKey(KeyCode.LeftShift))
                {
                    // Deselect everything because we clicked the ground
                    UnitSelections.Instance.DeselectAll();
                }
                
            }
        }
        if (!Input.GetKey(KeyCode.LeftControl) && Input.GetMouseButtonDown(1))
        {
            RaycastHit hit;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, ground))
            {           
                UnitSelections.Instance.RightClickMove(hit.point);
            }
        }
        
    }
}
