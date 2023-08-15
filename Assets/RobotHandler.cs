using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class RobotHandler : NetworkBehaviour
{
    //[Header("Prefabs")] public GameObject explosionParticleSystemPrefab;
    [SerializeField] public NetworkObject robot;

    [SerializeField] private Transform door;
    //[Header("Collision detection")] public LayerMask collisionLayers;

    //Thrown by info
    PlayerRef openByPlayerRef;
    string openByPlayerName;

    //Timing
    TickTimer explodeTickTimer = TickTimer.None;

    //Other components
    NetworkObject networkObject;
    NetworkRigidbody networkRigidbody;


    [SerializeField] private bool doorProcess = false;
    [SerializeField] private bool doorIsOpen = false;
    [SerializeField] private Transform closedMarker;
    [SerializeField] private Transform openedMarker;
    [SerializeField] private Transform doorTargetPosition;

    int doorSpeed = 20;

    public void RoboterEinrichten()
    {
        door = robot.transform.GetChild(0).GetChild(1).GetChild(0);
        closedMarker = robot.transform.GetChild(1).GetChild(0).GetChild(0);
        openedMarker = robot.transform.GetChild(1).GetChild(0).GetChild(1);
    }

    public void OpenCloseDoor(Transform tmpRobot)
    {

        Debug.Log("Open Close Methode Triggert");
        if (tmpRobot == robot)
        {
            if (!doorProcess)
            {
                doorProcess = true;
                if (doorIsOpen)
                {
                    doorTargetPosition = closedMarker;
                }
                else
                {
                    doorTargetPosition = openedMarker;
                }
                Debug.Log("door process: " + doorProcess + "door is open?: " + doorIsOpen);
                Debug.Log("door target position: " + doorTargetPosition);
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

        if (Object.HasStateAuthority)
        {
            if (doorProcess)
            {
                Vector3 moveDirection = (doorTargetPosition.position - door.transform.position).normalized;
                float distanceToTarget = Vector3.Distance(door.transform.position, doorTargetPosition.position);

                if (distanceToTarget > 0.01f) // Ein kleiner Schwellenwert, um das "Angekommen"-Kriterium zu definieren
                {
                    transform.position += moveDirection * doorSpeed * Runner.DeltaTime;
                }
                else
                {
                    if (doorIsOpen)
                    {
                        doorIsOpen = false;
                    }
                    else
                    {
                        doorIsOpen = true;
                    }
                    doorProcess = false;
                }
            }
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