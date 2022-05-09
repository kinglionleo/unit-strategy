using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.RemoteConfig;

public class BoltStatsManagerScript : MonoBehaviour
{
    private static BoltStatsManagerScript _instance;
    private const String developmentEnvironmentId = "a385e431-ed2e-4229-a974-ea8c14e0e214";
    private const String productionEnvironmentId = "86272771-6842-48f7-91ff-bbfb70ead273";
    private struct userAttributes { }
    private struct appAttributes { }

    public enum UnitTypes
    {
        Basic,
        Sniper,
        Tank,
        Juggenaut,
        SuperSniper,
        Base,
    }

    public enum Stats
    {
        Cost,
        ResearchRequirement,
        Hp,
        Damage,
        DamageRadius,
        ReloadTime,
        // Below are all categorized stats
        AimingTime,
        StationaryDelay,
        MovementSpeed,
        Range,
    }

    private struct UnitStats
    {
        public int cost;
        public int researchRequirement;
        public int hp;
        public int damage;
        public int damageRadius;
        public float reloadTime;
        // TODO add AimingTime, StationaryDelay, MovementSped, and Range as categorizable constants somewhere
    }

    private UnitStats[] unitStatsArray;

    public static BoltStatsManagerScript Instance
    {
        get { return _instance; }
    }
    void Awake()
    {
        // Destroy itself if there exists and instance of this gameobject already
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        
        unitStatsArray = new UnitStats[(Enum.GetNames(typeof(UnitTypes))).Length];
        ConfigManager.SetEnvironmentID(developmentEnvironmentId);
        ConfigManager.FetchCompleted += SetStats;
        ConfigManager.FetchConfigs<userAttributes, appAttributes>(new userAttributes(), new appAttributes());

    }

    void SetStats(ConfigResponse response)
    {
        int unitTypeCount = unitStatsArray.Length;
        //Debug.Log(ConfigManager.appConfig.HasKey("Basic" + "Cost"));
        for (int i = 0; i < unitTypeCount; i++) {
            UnitStats unitStats;
            unitStats.cost = ConfigManager.appConfig.GetInt(((UnitTypes) i).ToString() + "Cost");
            unitStats.researchRequirement = ConfigManager.appConfig.GetInt(((UnitTypes) i).ToString() + "ResearchRequirement");
            unitStats.hp = ConfigManager.appConfig.GetInt(((UnitTypes) i).ToString() + "Hp");
            unitStats.damage = ConfigManager.appConfig.GetInt(((UnitTypes) i).ToString() + "Damage");
            unitStats.damageRadius = ConfigManager.appConfig.GetInt(((UnitTypes) i).ToString() + "DamageRadius");
            unitStats.reloadTime = ConfigManager.appConfig.GetFloat(((UnitTypes) i).ToString() + "ReloadTime");
            unitStatsArray[i] = unitStats;
        }
        DebugStats();
    }

    private void DebugStats()
    {
        int unitTypeCount = unitStatsArray.Length;
        for (int i = 0; i < unitTypeCount; i++) {
            Debug.Log(unitStatsArray[i].cost + ", " + unitStatsArray[i].researchRequirement + ", " + unitStatsArray[i].hp + ", " +
            unitStatsArray[i].damage + ", " + unitStatsArray[i].damageRadius + ", " + unitStatsArray[i].reloadTime);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
