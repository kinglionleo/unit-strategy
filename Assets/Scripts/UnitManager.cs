using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// source: https://www.youtube.com/watch?v=vAVi04mzeKk
public class UnitManager : MonoBehaviour
{

    public List<GameObject> unitList = new List<GameObject>();
    public List<GameObject> enemyList = new List<GameObject>();
    public List<GameObject> unitsSelected = new List<GameObject>();

    private static UnitManager _instance;

    public static UnitManager Instance
    {
        get { return _instance; }
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    public void ClickSelect(GameObject unitToAdd)
    {
        DeselectAll();
        unitsSelected.Add(unitToAdd);
        unitToAdd.GetComponent<Unit>().setSelected(true);
    }
    public void ShiftClickSelect(GameObject unitToAdd)
    {
        if (!unitsSelected.Contains(unitToAdd))
        {
            unitsSelected.Add(unitToAdd);
            unitToAdd.GetComponent<Unit>().setSelected(true);
        }
        else
        {
            unitToAdd.GetComponent<Unit>().setSelected(false);
            unitsSelected.Remove(unitToAdd);
        }
    }

    public void RightClickAttackMove(Vector3 location)
    {
        Debug.Log("Attack");
        if (unitsSelected.Count > 0)
        {
            SelectUIScript.Instance.setAttackMoveMaterials();
            SelectUIScript.Instance.showAtLocation(location);
        }

        foreach (var unit in unitsSelected)
        {
            // if unit dies when player still has it selected
            if (unit == null) {
                unitList.Remove(unit);
                continue;
            }
            unit.gameObject.GetComponent<Unit>().MoveToPlace(location, 1, 0);
        }
    }

    public void GroupAttackMove(Vector3 location) {

        if(unitsSelected.Count <= 1){
            RightClickAttackMove(location);
            return;
        }

        SelectUIScript.Instance.setAttackMoveMaterials();
        SelectUIScript.Instance.showAtLocation(location);

        // We need to find the average position;
        int count = 0;
        float lowestMovementSpeed = 10000;
        float xPos = 0;
        float zPos = 0;

        foreach (var unit in unitsSelected)
        {
            // if unit dies when player still has it selected
            if (unit == null) {
                unitList.Remove(unit);
                continue;
            }

            if(unit.gameObject.GetComponent<Unit>().movementSpeed < lowestMovementSpeed) {
                lowestMovementSpeed = unit.gameObject.GetComponent<Unit>().movementSpeed;
            }
            xPos += unit.transform.position.x;
            zPos += unit.transform.position.z;

            count++;
            
        }

        xPos /= count;
        zPos /= count;

        Vector3 averagePosition = new Vector3(xPos, 0, zPos);


        foreach (var unit in unitsSelected)
        {
            // if unit dies when player still has it selected
            if (unit == null) {
                unitList.Remove(unit);
                continue;
            }

            Vector3 offsetPosition = new Vector3(averagePosition.x - unit.transform.position.x,
                                                 0,
                                                 averagePosition.z - unit.transform.position.z);

            unit.gameObject.GetComponent<Unit>().MoveToPlace(new Vector3(location.x - offsetPosition.x, location.y, location.z - offsetPosition.z), 3, lowestMovementSpeed);

        }

    }

    public void RightClickIgnoreMove(Vector3 location)
    {

        Debug.Log("Ignore");
        if (unitsSelected.Count > 0)
        {
            SelectUIScript.Instance.setIgnoreMoveMaterials();
            SelectUIScript.Instance.showAtLocation(location);
        }

        foreach (var unit in unitsSelected)
        {
            // if unit dies when player still has it selected
            if (unit == null) {
                unitList.Remove(unit);
                continue;
            }
            unit.gameObject.GetComponent<Unit>().MoveToPlace(location, 0, 0);
        }
    }

    public void GroupIgnoreMove(Vector3 location) {

        if (unitsSelected.Count <= 1)
        {
            RightClickIgnoreMove(location);
            return;
        }

        SelectUIScript.Instance.setIgnoreMoveMaterials();
        SelectUIScript.Instance.showAtLocation(location);

        // We need to find the average position;
        int count = 0;
        float lowestMovementSpeed = 10000;
        float xPos = 0;
        float zPos = 0;

        foreach (var unit in unitsSelected)
        {
            // if unit dies when player still has it selected
            if (unit == null)
            {
                unitList.Remove(unit);
                continue;
            }

            if (unit.gameObject.GetComponent<Unit>().movementSpeed < lowestMovementSpeed)
            {
                lowestMovementSpeed = unit.gameObject.GetComponent<Unit>().movementSpeed;
            }
            xPos += unit.transform.position.x;
            zPos += unit.transform.position.z;

            count++;

        }

        xPos /= count;
        zPos /= count;

        Vector3 averagePosition = new Vector3(xPos, 0, zPos);


        foreach (var unit in unitsSelected)
        {
            // if unit dies when player still has it selected
            if (unit == null)
            {
                unitList.Remove(unit);
                continue;
            }

            Vector3 offsetPosition = new Vector3(averagePosition.x - unit.transform.position.x,
                                                 0,
                                                 averagePosition.z - unit.transform.position.z);

            unit.gameObject.GetComponent<Unit>().MoveToPlace(new Vector3(location.x - offsetPosition.x, location.y, location.z - offsetPosition.z), 2, lowestMovementSpeed);

        }
    }

    public void DragSelect(GameObject unitToAdd)
    {
        if (!unitsSelected.Contains(unitToAdd))
        {
            unitsSelected.Add(unitToAdd);
            unitToAdd.GetComponent<Unit>().setSelected(true);
        }
    }

    public void DeselectAll()
    {
        foreach (var unit in unitsSelected)
        {
            if (unit == null) {
                unitList.Remove(unit);
                continue;
            }
            unit.GetComponent<Unit>().setSelected(false);
        }
        unitsSelected.Clear();
    }
    public void Deselect(GameObject unitToAdd)
    {

    }
}
