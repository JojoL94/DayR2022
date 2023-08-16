using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class RobotHandler : NetworkBehaviour
{
    //[Header("Prefabs")] public GameObject explosionParticleSystemPrefab;
    [SerializeField] public GameObject robot;
    [SerializeField] private Transform door;
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

    public void RoboterEinrichten()
    {
        door = robot.transform.GetChild(0).GetChild(1).GetChild(0).transform;
        closedMarker = robot.transform.GetChild(1).GetChild(0).GetChild(0).transform;
        openedMarker = robot.transform.GetChild(1).GetChild(0).GetChild(1).transform;
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_DoorOpenRequest(bool newDoorProcess, bool newDoorIsOpen)
    {
        doorProcess = newDoorProcess;
        doorIsOpen = newDoorIsOpen;
    }
    public void OpenCloseDoor(Transform tmpRobot)
    {

        Debug.Log("Open Close Methode Triggert");
        if (tmpRobot == robot.transform)
        {
            Debug.Log("Versuche auf/zu zu machen");
            if (!doorProcess)
            {
                doorProcess = true;
                float dist = Vector3.Distance(door.position, closedMarker.transform.position);
                if (dist < 0.3f)
                {
                    RPC_DoorOpenRequest(true, false);
                }
                else
                {
                    RPC_DoorOpenRequest(true, true);
                }
            }
        }
        else
        {
            Debug.Log("Robot Door Access denied");
            //Eintritt abgelehnt
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
            Vector3 moveDirection = (doorTargetPosition.position - door.transform.position).normalized;
            float distanceToTarget = Vector3.Distance(door.transform.position, doorTargetPosition.position);

            if (distanceToTarget > 0.01f) // Ein kleiner Schwellenwert, um das "Angekommen"-Kriterium zu definieren
            {
                door.transform.position += moveDirection * doorSpeed * Runner.DeltaTime;
            }
            else
            {
                RPC_DoorOpenRequest(false, false);
            }
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