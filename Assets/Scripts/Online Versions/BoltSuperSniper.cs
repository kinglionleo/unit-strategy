using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoltSuperSniper : BoltUnit
{
    // Start is called before the first frame update
    protected override void PullStatsFromManager() 
    {
        BoltStatsManagerScript.UnitType unitType = BoltStatsManagerScript.UnitType.SuperSniper;
        BoltStatsManagerScript.UnitStats unitStats = BoltStatsManagerScript.Instance.GetUnitStats(unitType);
        
        SetStatsFromManager(unitStats);
    }

    public override BoltStatsManagerScript.UnitType GetUnitType()
    {
        return BoltStatsManagerScript.UnitType.SuperSniper;
    }
}
