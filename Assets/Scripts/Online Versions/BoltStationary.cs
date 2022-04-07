using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoltStationary : MonoBehaviour
{

    public Image stationaryIndicator;
    private BoltUnit myUnit;

    void Awake()
    {
        myUnit = this.transform.parent.gameObject.GetComponent<BoltUnit>();
    }
    void Update()
    {
        stationaryIndicator.fillAmount = Mathf.Min((Time.time - myUnit.getStartShootTime()) / myUnit.getAcquisitionSpeed(), 1);
    }
}
