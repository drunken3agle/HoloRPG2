using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeeleeWeapon : AbstractWeapon {

    private AudioSource attackSound_AudioSource;

    private void Awake()
    {
        attackSound_AudioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        LMGestureManager.Instance.FistClosed += SetVisibility;
    }

    private void SetVisibility(bool isVisible)
    {
        GetComponent<MeshRenderer>().enabled = isVisible;
        GetComponent<BoxCollider>().enabled = isVisible;
    }

    protected override void AttackResponse()
    {
        attackSound_AudioSource.Play();
    }
}
