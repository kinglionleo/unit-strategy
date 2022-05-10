using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoltAimingIndicator : MonoBehaviour
{
    public Image aimingIndicator;
    private BoltUnit myUnit;

    void Awake()
    {
        myUnit = this.transform.parent.gameObject.GetComponent<BoltUnit>();
    }
    void Update()
    {
        aimingIndicator.fillAmount = calculateAimPercentage();
    }

    private float calculateAimPercentage() {
        float timeAimed = Time.time - myUnit.getStartAimTime();
        //Debug.Log(timeAimed / myUnit.aimSpeed);
        return timeAimed / myUnit.aimingTime;
    }
} 
