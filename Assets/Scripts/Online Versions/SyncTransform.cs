using UnityEngine;
using Photon.Bolt;

public class SyncTransform : EntityBehaviour<IUnit>
{
    // Basically Start()
    public override void Attached()
    {
        state.SetTransforms(state.UnitTransform, this.gameObject.transform);
    }
}
