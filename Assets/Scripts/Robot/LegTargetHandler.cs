using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using Unity.Mathematics;
using UnityEngine;

public class LegTargetHandler : NetworkBehaviour
{
    public bool front;
    public bool left;
    public float raycastDistance = 20f; // Die maximale Länge des Raycasts
    public LayerMask groundLayer;
    public Transform target;
    public Transform hipTarget;
    public bool makeStep;

    private float
        defaultMoveSpeed = 13f; //HIER UNBEDINGT SPEED ABGREIFEN VON CC ODER SO****************************************

    public NetworkCharacterControllerPrototypeCustom networkCharacterControllerPrototypeCustom;
    public RobotHandler robotHandler;
    public LegTargetHandler neighborLegHandlerLR;
    public LegTargetHandler neighborLegHandlerBF;
    private bool blockedByNeighborLR;
    private bool blockedByNeighborBF;

    private float moveSpeedModifier;
    private float moveSpeed;

    private Vector3 nextStepMarker;
    private Vector3 middleStepMarker;
    private Vector3 oldStepMarker;
    private bool isMoving;
    private Vector3 movementDirection;
    private Vector3 raycastDirection = Vector3.down;
    private Vector3 currentRobotPosition;
    private Vector3 oldRobotPosition;

    private float hipTargetValue = 2.3f;
    private float stepDistance = 3.3f;
    private float offsetAmount = 0.7f; // Der Offset in Laufrichtung

    private bool middleStep;
    private bool isInDefaultPosition;
    private Vector3 targetHit;
    private TickTimer stepTimer = TickTimer.None;

    private void Start()
    {
        networkCharacterControllerPrototypeCustom = transform.root.GetComponent<NetworkCharacterControllerPrototypeCustom>();
        robotHandler = transform.root.GetComponent<RobotHandler>();
        defaultMoveSpeed = networkCharacterControllerPrototypeCustom.maxSpeed;

    }

    void Update()
    {
        currentRobotPosition = transform.root.position;

        if (Vector3.Distance(currentRobotPosition, oldRobotPosition) > 0.2f)
        {
            isMoving = true;

            movementDirection = new Vector3(currentRobotPosition.x, oldRobotPosition.y, currentRobotPosition.z) -
                                oldRobotPosition;
            movementDirection.Normalize();
        }
        else
        {
            isMoving = false;
        }

        raycastDirection = Vector3.down;


        raycastDirection.Normalize();

        if (isMoving)
        {
            // Offset in Laufrichtung hinzufügen
            raycastDirection += movementDirection * offsetAmount;
        }

        RaycastHit hit;
        if (Physics.Raycast(transform.position, raycastDirection, out hit, Mathf.Infinity, groundLayer))
        {
            targetHit = Vector3.MoveTowards(targetHit, hit.point, 20f * Time.deltaTime);
            // Der Raycast hat den Boden getroffen
            Debug.DrawRay(transform.position, raycastDirection * raycastDistance, Color.green);
            float distancePointTarget = Vector3.Distance(targetHit, target.position);
            
            bool inStepDistance = !(distancePointTarget > stepDistance);
            bool makeStepBlocked = blockedByNeighborBF && blockedByNeighborLR;
            bool ignoreBlocked = distancePointTarget > stepDistance*1.5f;

            if (makeStep && blockedByNeighborLR && blockedByNeighborBF)
            {
                networkCharacterControllerPrototypeCustom.LegsGrounded = false;
            }

            if ((!inStepDistance && !makeStepBlocked) || ignoreBlocked) //Kontrolle ob sich das Bein zu weit weg befindet und ein Schritt gemacht werden soll
            {
                if (!makeStep) //Kontrolle ob ein Schritt gerade gemacht wird
                {
                    robotHandler.LegOffsetRotate(front,left,true);
                    oldRobotPosition = currentRobotPosition;
                    nextStepMarker = targetHit;
                    middleStep = true;
                    neighborLegHandlerLR.blockedByNeighborLR = true;
                    neighborLegHandlerBF.blockedByNeighborBF = true;
                    makeStep = true;
                    isInDefaultPosition = false;
                    moveSpeed = defaultMoveSpeed;
                    stepTimer = TickTimer.CreateFromSeconds(Runner, 2);
                }
                else if
                    (distancePointTarget > stepDistance * 4) //Kontrolle ob sich das Target am Arsch der Welt befindet
                {
                    targetHit = transform.position;
                    target.position = transform.position;
                }
            }

            if (makeStep)
            {
                if (stepTimer.Expired(Runner))
                {
                    middleStep = false;
                }

                if (middleStep)
                {
                    middleStepMarker = new Vector3(transform.position.x, (transform.position.y + targetHit.y) / 2,
                        transform.position.z);
                    target.position =
                        Vector3.MoveTowards(target.position, middleStepMarker, moveSpeed * Time.deltaTime);

                    if (Vector3.Distance(target.position, middleStepMarker) < 0.2f)
                    {
                        middleStep = false;
                    }
                }
                else
                {
                    nextStepMarker = targetHit;
                    target.position =
                        Vector3.MoveTowards(target.position, nextStepMarker, moveSpeed * Time.deltaTime);
                }

                if (Vector3.Distance(nextStepMarker, target.position) < 0.1f)
                {
                    oldStepMarker = nextStepMarker;
                    neighborLegHandlerLR.blockedByNeighborLR = false;
                    neighborLegHandlerBF.blockedByNeighborBF = false;
                    networkCharacterControllerPrototypeCustom.LegsGrounded = true;
                    robotHandler.LegOffsetRotate(front,left,false);
                    makeStep = false;
                }
            }
            else
            {
                target.position = oldStepMarker;
            }

            if (Physics.Raycast(target.position + new Vector3(0, 10, 0), Vector3.down, out hit, Mathf.Infinity,
                    groundLayer))
            {
                hipTarget.position = target.position +
                                     new Vector3(0, hipTargetValue - Vector3.Distance(target.position, hit.point), 0);
                Debug.DrawRay(transform.position,
                    (hipTarget.position - transform.position) *
                    Vector3.Distance(hipTarget.position, transform.position), Color.cyan);
            }
            else
            {
                hipTarget.position = target.position + new Vector3(0, hipTargetValue, 0);
            }
        }
        else
        {
            // Der Raycast hat nichts getroffen
            Debug.DrawRay(transform.position, raycastDirection * raycastDistance, Color.red);

            // Hier kannst du weitere Aktionen ausführen, wenn der Raycast nichts trifft
        }
    }
}