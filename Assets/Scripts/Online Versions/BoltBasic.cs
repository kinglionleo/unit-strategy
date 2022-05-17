using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoltBasic : BoltUnit
{
    protected override void PullStatsFromManager() 
    {
        BoltStatsManagerScript.UnitType unitType = BoltStatsManagerScript.UnitType.Basic;
        BoltStatsManagerScript.UnitStats unitStats = BoltStatsManagerScript.Instance.GetUnitStats(unitType);
        
        SetStatsFromManager(unitStats);
    }

    public override int getUnitId()
    {
        return (int) BoltStatsManagerScript.UnitType.Basic;
    }
}
