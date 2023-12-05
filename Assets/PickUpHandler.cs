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

    private void Start()
    {
        myRigidbody = transform.GetComponent<Rigidbody>();
    }

    public override void FixedUpdateNetwork()
    {
        if (isPickedUp)
        {
            transform.position = picker.position;
            transform.rotation = picker.rotation;
        }
    }

    public void PickUp(Transform tmpPicker)
    {
        if (!isPickedUp)
        {
            myRigidbody.useGravity = false;
            picker = tmpPicker;
            isPickedUp = true;
        }
    }

    public void Drop()
    {
        myRigidbody.useGravity = true;
        isPickedUp = false;
        picker = null;
    }
}
