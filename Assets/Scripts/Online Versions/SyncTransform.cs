using UnityEngine;

public class SyncTransform : Photon.Bolt.EntityBehaviour<Photon.Bolt.IUnit>
{
    // Basically Start()
    public override void Attached()
    {
        state.SetTransforms(state.UnitTransform, this.gameObject.transform);
    }
}
