using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.RemoteConfig;

public class BoltStatsManagerScript : MonoBehaviour
{
    private static BoltStatsManagerScript _instance;
    private const String DevelopmentEnvironmentId = "a385e431-ed2e-4229-a974-ea8c14e0e214";
    private const String ProductionEnvironmentId = "86272771-6842-48f7-91ff-bbfb70ead273";
    private struct userAttributes { }
    private struct appAttributes { }

    public enum UnitType
    {
        Basic,
        Sniper,
        Tank,
        Juggernaut,
        SuperSniper,
        Base,
    }

    public enum Stats
    {
        Cost,
        ResearchRequirement,
        Lifetime,
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

    [System.Serializable]
    public class UnitStats
    {
        public int cost;
        public int researchRequirement;
        public int lifetime;
        public int hp;
        public int damage;
        public int damageRadius;
        public float reloadTime;
        // categorizble:
        public float aimingTime;
        public float stationaryDelay;
        public float movementSpeed;
        public int range;

        public static UnitStats CreateFromJSON(string jsonString)
        {
            return JsonUtility.FromJson<UnitStats>(jsonString);
        }
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
        else
        {
            _instance = this;
        }
        DontDestroyOnLoad(this.gameObject);
        unitStatsArray = new UnitStats[(Enum.GetNames(typeof(UnitType))).Length];
        ConfigManager.SetEnvironmentID(DevelopmentEnvironmentId);
        ConfigManager.FetchCompleted += GetStatsFromRemote;
        ConfigManager.FetchConfigs<userAttributes, appAttributes>(new userAttributes(), new appAttributes());

    }

    void GetStatsFromRemote(ConfigResponse response)
    {
        int unitTypeCount = unitStatsArray.Length;
        for (int i = 0; i < unitTypeCount; i++) {
            unitStatsArray[i] = UnitStats.CreateFromJSON(ConfigManager.appConfig.GetJson(((UnitType) i).ToString() + "Stats"));
        }
        DebugStats();
    }

    private void DebugStats()
    {
        int unitTypeCount = unitStatsArray.Length;
        for (int i = 0; i < unitTypeCount; i++) {
            if (unitStatsArray[i] == null) {
                Debug.Log("unitStatsArray @i is null");
            }
            Debug.Log(unitStatsArray[i].cost + ", " + unitStatsArray[i].researchRequirement + ", " + unitStatsArray[i].hp + ", " +
            unitStatsArray[i].damage + ", " + unitStatsArray[i].damageRadius + ", " + unitStatsArray[i].reloadTime);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public UnitStats GetUnitStats(UnitType unitType)
    {
        return unitStatsArray[(int) unitType];
    }
}
