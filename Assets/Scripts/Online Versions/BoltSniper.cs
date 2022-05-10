using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoltSniper : BoltUnit
{
    // Start is called before the first frame update
    protected override void PullStatsFromManager() 
    {
        BoltStatsManagerScript.UnitType unitType = BoltStatsManagerScript.UnitType.Sniper;
        BoltStatsManagerScript.UnitStats unitStats = BoltStatsManagerScript.Instance.GetUnitStats(unitType);
        
        SetStatsFromManager(unitStats);
    }
}
