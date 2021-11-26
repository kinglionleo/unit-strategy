using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StationaryIndicatorScript : MonoBehaviour
{

    public Image stationaryIndicator;
    private Unit myUnit;

    void Awake()
    {
        myUnit = this.transform.parent.gameObject.GetComponent<Unit>();
    }
    void Update()
    {
        stationaryIndicator.fillAmount = (Time.time - myUnit.getStartStationary()) / (myUnit.getTimeStationary() - myUnit.getStartStationary());
    }
}
