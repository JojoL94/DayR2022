using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class PickUpHandler : NetworkBehaviour
{
    private Transform picker;
    private bool isPickedUp;
    private Rigidbody myRigidbody;
    [SerializeField] private BoxCollider myBoxCollider;
    private float dropDistance = 1f;
    private void Start()
    {
        myRigidbody = transform.GetComponent<Rigidbody>();
        myBoxCollider = GetComponentInChildren<BoxCollider>();
    }

    public override void FixedUpdateNetwork()
    {
        if (isPickedUp)
        {
            transform.position = picker.position;
            transform.rotation = picker.rotation;
        }
        else
        {
            if (!myBoxCollider.enabled)
            {
                if (Vector3.Distance(picker.position, transform.position) > dropDistance)
                {
                    picker = null;
                    myRigidbody.useGravity = true;
                    myBoxCollider.enabled = true;
                }
            }
        }
    }

    public void PickUp(Transform tmpPicker)
    {
        if (!isPickedUp)
        {
            picker = tmpPicker.transform;
            myRigidbody.useGravity = false;
            myBoxCollider.enabled = false;
            isPickedUp = true;
            if (Object.HasStateAuthority)
            {
                Object.ReleaseStateAuthority();
            }
        }
    }

    public void Drop()
    {
        isPickedUp = false;
    }
}
