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
        defaultLegMoveSpeed = 15f; 

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
    private float stepDistance = 1.2f;
    private float offsetAmount = 0.7f; // Der Offset in Laufrichtung

    private bool middleStep;
    private bool isInDefaultPosition;
    private Vector3 targetHit;
    //Timing
    private bool isTimerRunning = false;
    private float timerDefaultPositionDuration = 2.5f;
    private float timerStartTime;

    private float maxHeight;
    private float minHeight;
    
    private void Start()
    {
        networkCharacterControllerPrototypeCustom = transform.root.GetComponent<NetworkCharacterControllerPrototypeCustom>();
        robotHandler = transform.root.GetComponent<RobotHandler>();
        maxHeight = robotHandler.maxHeight;
        minHeight = robotHandler.minHeight;
        //defaultMoveSpeed = networkCharacterControllerPrototypeCustom.maxSpeed;

    }
    

    void Update()
    {

        if (isTimerRunning)
        {
            float elapsedTime = Time.time - timerStartTime;

            if (elapsedTime >= timerDefaultPositionDuration)
            {
                if (Vector3.Distance(target.position, new Vector3(transform.position.x, target.position.y, transform.position.z)) > 0.3f)
                {
                    robotHandler.LegOffsetRotate(front,left,true);
                    oldRobotPosition = currentRobotPosition;
                    nextStepMarker = targetHit;
                    middleStep = true;
                    neighborLegHandlerLR.blockedByNeighborLR = true;
                    neighborLegHandlerBF.blockedByNeighborBF = true;
                    makeStep = true;
                    isInDefaultPosition = false;
                }
                // Der Timer ist abgelaufen, setze den Bool-Wert auf false.
                isTimerRunning = false;
            }
        }
        currentRobotPosition = transform.root.position;

        if (Vector3.Distance(currentRobotPosition, oldRobotPosition) > 0.2f)
        {
            if (!isTimerRunning)
            {
                StartTimer();
            }

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
        RaycastHit hit;
        if (isMoving)
        {
            if (Physics.Raycast(transform.position, raycastDirection, out hit, Mathf.Infinity, groundLayer))
            {
                offsetAmount = Mathf.Lerp(offsetAmount, hit.distance, 0.8f);
                offsetAmount = Mathf.Clamp(offsetAmount, minHeight-1f, maxHeight+1f);
                stepDistance = offsetAmount;
                offsetAmount /= 10;
            }
            // Offset in Laufrichtung hinzufügen
            raycastDirection += movementDirection * offsetAmount;
        }
        
        
        if (Physics.Raycast(transform.position, raycastDirection, out hit, Mathf.Infinity, groundLayer))
        {
            // Berechne den Wert zwischen 0 und 1, der die Position des Objekts zwischen minHeight und maxHeight repräsentiert
            float t = Mathf.InverseLerp(minHeight, maxHeight, Vector3.Distance(transform.position, target.position));

            // Verwende Lerp, um die Geschwindigkeit basierend auf t zu interpolieren
            float moveSpeed = Mathf.Lerp(defaultLegMoveSpeed * 0.5f, defaultLegMoveSpeed, t); // Hier 0.5f, um die Geschwindigkeit am Mittelpunkt zu erhöhen
            
            targetHit = Vector3.MoveTowards(targetHit, hit.point, 20f * Time.deltaTime);
            // Der Raycast hat den Boden getroffen
            Debug.DrawRay(transform.position, raycastDirection * raycastDistance, Color.green);
            var distancePointTarget = Vector3.Distance(targetHit, target.position);
            
            var inStepDistance = !(distancePointTarget > stepDistance);
            var makeStepBlocked = blockedByNeighborBF && blockedByNeighborLR;
            var ignoreBlocked = distancePointTarget > stepDistance*1.5f;

            if (makeStep && blockedByNeighborLR && blockedByNeighborBF)
            {
                networkCharacterControllerPrototypeCustom.LegsGrounded = false;
            }

            if ((!inStepDistance && !makeStepBlocked) || ignoreBlocked) //Kontrolle ob sich das Bein zu weit weg befindet und ein Schritt gemacht werden soll
            {
                if (!makeStep) //Kontrolle ob ein Schritt gerade gemacht wird
                {
                    StartTimer();
                    robotHandler.LegOffsetRotate(front,left,true);
                    oldRobotPosition = currentRobotPosition;
                    nextStepMarker = targetHit;
                    middleStep = true;
                    neighborLegHandlerLR.blockedByNeighborLR = true;
                    neighborLegHandlerBF.blockedByNeighborBF = true;
                    makeStep = true;
                    isInDefaultPosition = false;
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
                /*
                if (stepTimer.Expired(Runner))
                {
                    middleStep = false;
                }
*/
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

            //hipTarget.position = target.position;

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
    
    public void StartTimer()
    {
        // Setze den Bool-Wert auf true und starte den Timer.
        isTimerRunning = true;
        timerStartTime = Time.time;
    }
}