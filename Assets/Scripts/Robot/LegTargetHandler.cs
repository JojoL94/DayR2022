using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using Unity.Mathematics;
using UnityEngine;

public class LegTargetHandler : NetworkBehaviour
{
    public float raycastDistance = 20f; // Die maximale Länge des Raycasts
    public float offsetAmount = 1f; // Der Offset in Laufrichtung
    public LayerMask groundLayer;
    public Transform target;
    public Transform hipTarget;
    public bool makeStep;

    public float
        defaultMoveSpeed = 10f; //HIER UNBEDINGT SPEED ABGREIFEN VON CC ODER SO****************************************

    private float moveSpeedModifier;
    private float moveSpeed;

    private Vector3 nextStepMarker;
    private Vector3 middleStepMarker;
    private Vector3 oldStepMarker;
    private bool isMoving;
    private Vector3 movementDirection;
    private float stepDistance = 5f;
    private bool middleStep;
    private bool isInDefaultPosition;
    private Vector3 targetHit;

    //Timing
    TickTimer defaultPositionTickTimer = TickTimer.None;
    private TickTimer stepTimer = TickTimer.None;

    public override void FixedUpdateNetwork()
    {
        // Offset für den Raycast je nach Bewegungsrichtung
        Vector3 raycastDirection = Vector3.down;
        if (isMoving)
        {
            // Offset in Laufrichtung hinzufügen
            raycastDirection += movementDirection * offsetAmount;
        }

        RaycastHit hit;
        if (Physics.Raycast(transform.position, raycastDirection, out hit, Mathf.Infinity, groundLayer))
        {
            targetHit = Vector3.MoveTowards(targetHit, hit.point, 30f * Runner.DeltaTime);
            // Der Raycast hat den Boden getroffen
            Debug.DrawRay(transform.position, raycastDirection * raycastDistance, Color.green);
            float distancePointTarget = Vector3.Distance(hit.point, target.position);
            if (distancePointTarget > stepDistance)
            {
                if (!makeStep)
                {
                    nextStepMarker = targetHit;
                    middleStepMarker = targetHit + new Vector3(0, transform.position.y / 2, 0);
                    middleStep = true;
                    makeStep = true;
                    isInDefaultPosition = false;
                    moveSpeed = defaultMoveSpeed;
                    stepTimer = TickTimer.CreateFromSeconds(Runner, 2);
                    defaultPositionTickTimer = TickTimer.CreateFromSeconds(Runner, 4);
                }
                else if (distancePointTarget > stepDistance * 2)
                {
                    targetHit = transform.position;
                    target.position = transform.position;
                }
                else
                {
                    middleStep = false;
                }
            }

            if (distancePointTarget > 0.2f && defaultPositionTickTimer.Expired(Runner) && !makeStep)
            {
                nextStepMarker = targetHit;
                middleStepMarker = targetHit + new Vector3(0, transform.position.y / 2, 0);
                middleStep = true;
                makeStep = true;
                moveSpeed = defaultMoveSpeed;
                isInDefaultPosition = true;
                stepTimer = TickTimer.CreateFromSeconds(Runner, 2);
                defaultPositionTickTimer = TickTimer.CreateFromSeconds(Runner, 2);
            }

            if (makeStep)
            {
                if (stepTimer.Expired(Runner))
                {
                    middleStep = false;
                }

                if (middleStep)
                {
                    target.position =
                        Vector3.MoveTowards(target.position, middleStepMarker, moveSpeed * Runner.DeltaTime);

                    if (Vector3.Distance(target.position, middleStepMarker) < 1f)
                    {
                        middleStep = false;
                    }
                }
                else
                {
                    nextStepMarker = targetHit;
                    target.position =
                        Vector3.MoveTowards(target.position, nextStepMarker, moveSpeed * Runner.DeltaTime);
                }

                if (Vector3.Distance(nextStepMarker, target.position) < 0.1f)
                {
                    oldStepMarker = nextStepMarker;
                    makeStep = false;
                }
            }
            else
            {
                target.position = oldStepMarker;
            }

            hipTarget.position = target.position + new Vector3(0, 4f, 0);
        }
        else
        {
            // Der Raycast hat nichts getroffen
            Debug.DrawRay(transform.position, raycastDirection * raycastDistance, Color.red);

            // Hier kannst du weitere Aktionen ausführen, wenn der Raycast nichts trifft
        }
    }

    public void AnimateRobotLeg(Vector3 newMovementDirection)
    {
        if (newMovementDirection != Vector3.zero)
        {
            movementDirection = newMovementDirection;
            isMoving = true;
            isInDefaultPosition = false;
        }
        else
        {
            isMoving = false;
        }
    }
}