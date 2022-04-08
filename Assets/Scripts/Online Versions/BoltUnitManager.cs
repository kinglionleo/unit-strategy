using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// source: https://www.youtube.com/watch?v=vAVi04mzeKk
public class BoltUnitManager : MonoBehaviour
{

    public List<GameObject> unitList = new List<GameObject>();
    public List<GameObject> enemyList = new List<GameObject>();
    public List<GameObject> unitsSelected = new List<GameObject>();

    private static BoltUnitManager _instance;

    public static BoltUnitManager Instance
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
        unitToAdd.GetComponent<BoltUnit>().setSelected(true);
    }

    public void ShiftClickSelect(GameObject unitToAdd)
    {
        if (!unitsSelected.Contains(unitToAdd))
        {
            unitsSelected.Add(unitToAdd);
            unitToAdd.GetComponent<BoltUnit>().setSelected(true);
        }
        else
        {
            unitToAdd.GetComponent<BoltUnit>().setSelected(false);
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
            unit.gameObject.GetComponent<BoltUnit>().MoveToPlace(location, 1);
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
            unit.gameObject.GetComponent<BoltUnit>().MoveToPlace(location, 0);
        }
    }

    public void DragSelect(GameObject unitToAdd)
    {
        if (!unitsSelected.Contains(unitToAdd))
        {
            unitsSelected.Add(unitToAdd);
            unitToAdd.GetComponent<BoltUnit>().setSelected(true);
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
            unit.GetComponent<BoltUnit>().setSelected(false);
        }
        unitsSelected.Clear();
    }
    
    public void Deselect(GameObject unitToAdd)
    {

    }
}
