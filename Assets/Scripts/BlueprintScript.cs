using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlueprintScript : MonoBehaviour
{

    RaycastHit hit;
    void Start()
    {

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if(Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            transform.position = hit.point;
        }
    }

    // Update is called once per frame
    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            transform.position = hit.point;
        }

        if (Input.GetMouseButton(0))
        {
            // this enables the regular behavior of a unit and disables the blueprint mechanic once a click is registered.
            this.gameObject.GetComponent<Unit>().enabled = true;
            this.enabled = false;
        }
    }
}
