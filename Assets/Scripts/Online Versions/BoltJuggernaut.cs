using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoltJuggernaut : BoltUnit
{
    // Start is called before the first frame update
    protected override void PullStatsFromManager() 
    {
        BoltStatsManagerScript.UnitType unitType = BoltStatsManagerScript.UnitType.Juggernaut;
        BoltStatsManagerScript.UnitStats unitStats = BoltStatsManagerScript.Instance.GetUnitStats(unitType);
        
        SetStatsFromManager(unitStats);
    }
}
