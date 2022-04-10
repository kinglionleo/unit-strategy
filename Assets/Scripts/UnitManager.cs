using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// source: https://www.youtube.com/watch?v=vAVi04mzeKk
public class UnitManager : MonoBehaviour
{

    public List<GameObject> unitList = new List<GameObject>();
    public List<GameObject> enemyList = new List<GameObject>();
    public List<GameObject> unitsSelected = new List<GameObject>();
    public GameObject groupMover;

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
            unit.gameObject.GetComponent<Unit>().MoveToPlace(location, 1);
        }
    }

    public void GroupAttackMove(Vector3 location) {

        if(unitsSelected.Count <= 1){
            RightClickAttackMove(location);
            return;
        }

        SelectUIScript.Instance.setAttackMoveMaterials();
        SelectUIScript.Instance.showAtLocation(location);

        GameObject unitsMover = Instantiate(groupMover);

        // We need to find the average position;
        int count = 0;
        float lowestMovementSpeed = 10000;
        float xPos = 0;
        float yPos = 0;
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
            yPos += unit.transform.position.y;
            zPos += unit.transform.position.z;

            count++;
            
        }

        xPos /= count;
        yPos /= count;
        zPos /= count;

        Vector3 averagePosition = new Vector3(xPos, yPos, zPos);

        unitsMover.gameObject.GetComponent<GroupMoverScript>().SetPosition(averagePosition);

        foreach (var unit in unitsSelected)
        {
            // if unit dies when player still has it selected
            if (unit == null) {
                unitList.Remove(unit);
                continue;
            }

            unit.gameObject.GetComponent<Unit>().SetParent(unitsMover);
            unit.gameObject.GetComponent<Unit>().setIgnoreEnemy(false);
            
        }

        unitsMover.gameObject.GetComponent<GroupMoverScript>().MoveToPlace(location, lowestMovementSpeed);
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
            unit.gameObject.GetComponent<Unit>().MoveToPlace(location, 0);
        }
    }

    public void GroupIgnoreMove(Vector3 location) {

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
