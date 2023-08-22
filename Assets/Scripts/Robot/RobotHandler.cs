using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class RobotHandler : NetworkBehaviour
{
    //[Header("Prefabs")] public GameObject explosionParticleSystemPrefab;
    //private Transform door;
    //[Header("Collision detection")] public LayerMask collisionLayers;

    //Thrown by info
    PlayerRef openByPlayerRef;
    string openByPlayerName;

    //Timing
    TickTimer explodeTickTimer = TickTimer.None;

    NetworkRigidbody networkRigidbody;

    [Networked] public bool doorProcess { get; set; }
    [Networked] public bool doorIsOpen { get; set; }
    [SerializeField] private Transform closedMarker;
    [SerializeField] private Transform openedMarker;
    [SerializeField] private Transform doorTargetPosition;

    int doorSpeed = 5;

    public void Start()
    {
        //door = transform.GetChild(0).GetChild(1).GetChild(0).transform;
        closedMarker = transform.GetChild(1).GetChild(0).GetChild(0).transform;
        openedMarker = transform.GetChild(1).GetChild(0).GetChild(1).transform;
        doorTargetPosition = closedMarker;
    }

    public void SetUpRobotHandler()
    {
        //door = transform.GetChild(0).GetChild(1).GetChild(0).transform;
        closedMarker = transform.GetChild(1).GetChild(0).GetChild(0).transform;
        openedMarker = transform.GetChild(1).GetChild(0).GetChild(1).transform;
        doorTargetPosition = closedMarker;
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_DoorOpenRequest(bool newDoorProcess, bool newDoorIsOpen)
    {
        doorProcess = newDoorProcess;
        doorIsOpen = newDoorIsOpen;
    }

    public void OpenCloseDoor()
    {
        Debug.Log("Open Close Methode Triggert");
        Debug.Log("Versuche auf/zu zu machen");
        if (!doorProcess)
        {
            doorProcess = true;
            float dist = Vector3.Distance(transform.GetChild(0).GetChild(1).GetChild(0).transform.position,
                closedMarker.transform.position);
            if (dist < 0.3f)
            {
                RPC_DoorOpenRequest(true, false);
            }
            else
            {
                RPC_DoorOpenRequest(true, true);
            }
        }
        else
        {
            Debug.Log("Door is in Process");
            //Eingabe abgelehnt
        }
    }

    //Network update
    public override void FixedUpdateNetwork()
    {
        if (doorProcess)
        {
            if (doorIsOpen)
            {
                doorTargetPosition = closedMarker;
            }
            else
            {
                doorTargetPosition = openedMarker;
            }

            Vector3 moveDirection =
                (doorTargetPosition.position - transform.GetChild(0).GetChild(1).GetChild(0).transform.position)
                .normalized;
            float distanceToTarget =
                Vector3.Distance(transform.GetChild(0).GetChild(1).GetChild(0).transform.transform.position,
                    doorTargetPosition.position);

            if (distanceToTarget > 0.01f) // Ein kleiner Schwellenwert, um das "Angekommen"-Kriterium zu definieren
            {
                transform.GetChild(0).GetChild(1).GetChild(0).transform.transform.position +=
                    moveDirection * doorSpeed * Runner.DeltaTime;
            }
            else
            {
                RPC_DoorOpenRequest(false, false);
            }
        }
        else
        {
            transform.GetChild(0).GetChild(1).GetChild(0).transform.position =
                doorTargetPosition.transform.position;
        }

        if (Object.HasStateAuthority)
        {
            /*
            if (explodeTickTimer.Expired(Runner))
            {
                int hitCount = Runner.LagCompensation.OverlapSphere(transform.position, 4, thrownByPlayerRef, hits,
                    collisionLayers);

                for (int i = 0; i < hitCount; i++)
                {
                    HPHandler hpHandler = hits[i].Hitbox.transform.root.GetComponent<HPHandler>();

                    if (hpHandler != null)
                        hpHandler.OnTakeDamage(thrownByPlayerName, 100);
                }

                Runner.Despawn(networkObject);

                //Stop the explode timer from being triggered again
                explodeTickTimer = TickTimer.None;
            }*/
        }
    }
}