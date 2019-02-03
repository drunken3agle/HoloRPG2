using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeeleeWeapon : AbstractWeapon {
    

    private void Start()
    {
        LMGestureManager.Instance.FistClosed += SetVisibility;
    }

    private void SetVisibility(bool isVisible)
    {
        GetComponent<MeshRenderer>().enabled = isVisible;
        GetComponent<MeshCollider>().enabled = isVisible;
    }

    protected override void AttackResponse()
    {

    }
}
