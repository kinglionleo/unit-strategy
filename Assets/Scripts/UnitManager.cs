using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public void RightClickMove(Vector3 location)
    {
        if (unitsSelected.Count > 0)
        {
            SelectUIScript.Instance.showAtLocation(location);
        }

        foreach (var unit in unitsSelected)
        {
            unit.gameObject.GetComponent<Unit>().MoveToPlace(location);
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
            unit.GetComponent<Unit>().setSelected(false);
        }
        unitsSelected.Clear();
    }
    public void Deselect(GameObject unitToAdd)
    {

    }
}
