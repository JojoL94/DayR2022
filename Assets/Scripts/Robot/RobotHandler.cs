using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class RobotHandler : NetworkBehaviour
{
    public LayerMask groundLayer; // Die Layer, auf denen sich der Charakter bewegt
    public float rotationThreshold = 0.1f;
    public List<Transform> measurementPoints = new List<Transform>();
    private Vector3Smoother smootherHeight = new Vector3Smoother(10); // Du kannst die Fenstergröße anpassen.
    private Vector3Smoother smootherNormal = new Vector3Smoother(10); // Du kannst die Fenstergröße anpassen.

    //Driver info
    PlayerRef driverIsPlayerRef;
    string driverIsPlayerName;

    //Gunner info
    PlayerRef gunnerIsPlayerRef;
    string gunnerIsPlayerName;
    [Networked] public bool doorProcess { get; set; }
    [Networked] public bool doorIsOpen { get; set; }
    [Networked] public bool engineOn { get; set; }
    [Networked] public bool isRobotFalling { get; set; }
    [Networked] public float targetHeightValue { get; set; }


    [SerializeField] private Transform closedMarker;
    [SerializeField] private Transform openedMarker;
    [SerializeField] private Transform doorTargetPosition;
    int doorSpeed = 5;
    private float moveSpeed = 5f;
    private bool movingUp;
    private float minHeight = 2f;
    private float maxHeight = 4.3f;
    private Vector3 targetHeight;
    private Vector2 oldPositon;
    private float heightValue;
    private float smoothingHeightFactor = 0.7f; // Anpassbare Glättungsfaktor

    public void Start()
    {
        closedMarker = transform.GetChild(1).GetChild(0).GetChild(0).transform;
        openedMarker = transform.GetChild(1).GetChild(0).GetChild(1).transform;
        doorTargetPosition = closedMarker;
        heightValue = (minHeight + maxHeight) / 2;
        CollectAllMeasurementPoints(measurementPoints[0]);
        oldPositon = new Vector2(transform.position.x, transform.position.z);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_DoorOpenRequest(bool newDoorProcess, bool newDoorIsOpen)
    {
        doorProcess = newDoorProcess;
        doorIsOpen = newDoorIsOpen;
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_EngineRequest(bool newEngineOn)
    {
        engineOn = newEngineOn;
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_DriverChangeRequest(PlayerRef newDriverPlayerRef, String newDriverPlayerName)
    {
        driverIsPlayerRef = newDriverPlayerRef;
        driverIsPlayerName = newDriverPlayerName;
    }

    public void OpenCloseDoor()
    {
        if (!doorProcess)
        {
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

            doorProcess = true;
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
        DoorProcess();
        Vector2 currentPosition = new Vector2(transform.position.x, transform.position.z);
        if (currentPosition != oldPositon)
        {
            RotateRobotMotion();
            oldPositon = transform.position;
        }


        if (!engineOn)
        {
            GetComponent<NetworkCharacterControllerPrototypeCustom>().Move(Vector3.zero, false);
        }
        else
        {
            HeightRobotMotion();
        }

        if (Object.HasStateAuthority)
        {
        }
    }

    public void HeightRobotMotion()
    {
        RaycastHit hit;
        if (Physics.Raycast(measurementPoints[0].position, Vector3.down, out hit,
                Mathf.Infinity, groundLayer))
        {
            if (Vector3.Distance(hit.point, measurementPoints[0].position) < maxHeight + 1f)
            {
                isRobotFalling = false;
                // Begrenze den Float-Wert innerhalb des angegebenen Bereichs
                targetHeightValue = Mathf.Clamp(targetHeightValue, minHeight, maxHeight);
                float hitYValue = 0;
                if (hit.point.y < transform.position.y + 4f)
                {
                    hitYValue = hit.point.y;
                }

                // Glätte die Änderung des Float-Werts
                heightValue = Mathf.Lerp(heightValue, targetHeightValue, smoothingHeightFactor);
                heightValue = Mathf.Clamp(heightValue, minHeight - 1f, maxHeight + 1f);
                targetHeight = new Vector3(transform.position.x, heightValue + hitYValue - 4f, transform.position.z);
                Vector3 smoothedTargetHeight = smootherHeight.Smooth(targetHeight);
                if (Vector3.Distance(transform.position, targetHeight) > 0.1f)
                {
                    transform.position = Vector3.MoveTowards(transform.position, smoothedTargetHeight,
                        moveSpeed / 2 * Runner.DeltaTime);
                }
            }
            else
            {
                isRobotFalling = true;
                GetComponent<NetworkCharacterControllerPrototypeCustom>().Move(Vector3.zero, false);
            }

            Debug.DrawRay(measurementPoints[0].position,
                Vector3.down * Vector3.Distance(measurementPoints[0].position, hit.point), Color.green);
        }
    }

    private void RotateRobotMotion()
    {
        float hitDistance = 50;
        Vector3 averageNormal = Vector3.zero;
        float averageHeight = 0;

        foreach (Transform point in measurementPoints)
        {
            RaycastHit hit;
            if (Physics.Raycast(point.position, Vector3.down, out hit, Mathf.Infinity, groundLayer))
            {
                averageNormal += hit.normal;
                averageHeight += hit.distance;
            }
        }

        if (measurementPoints.Count > 0)
        {
            averageNormal /= measurementPoints.Count;
            averageNormal.Normalize();
            Vector3 smoothedAverageNormal = smootherNormal.Smooth(averageNormal);
            Quaternion targetRotation =
                Quaternion.FromToRotation(transform.up, smoothedAverageNormal) * transform.rotation;

            // Überprüfen, ob die Differenz zwischen aktueller Rotation und Zielrotation den Schwellenwert überschreitet
            if (Quaternion.Angle(transform.rotation, targetRotation) > rotationThreshold)
            {
                // Objekt so ausrichten, dass die Up-Richtung der durchschnittlichen Oberflächennormalen entspricht
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Runner.DeltaTime * 5f);
            }
        }
    }

    private void DoorProcess()
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
                float dist = Vector3.Distance(transform.GetChild(0).GetChild(1).GetChild(0).transform.position,
                    closedMarker.transform.position);
                if (dist < 0.3f)
                {
                    RPC_DoorOpenRequest(false, false);
                }
                else
                {
                    RPC_DoorOpenRequest(false, true);
                }
            }
        }
        else
        {
            if (doorIsOpen)
            {
                doorTargetPosition = openedMarker;
            }
            else
            {
                doorTargetPosition = closedMarker;
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

            /*transform.GetChild(0).GetChild(1).GetChild(0).transform.position =
                doorTargetPosition.transform.position;*/
        }
    }

    // Diese Methode sammelt rekursiv alle Child-Objekte.
    public void CollectAllMeasurementPoints(Transform parent)
    {
        foreach (Transform child in parent)
        {
            // Füge das Child-Objekt zur Liste hinzu.
            measurementPoints.Add(child);

            // Rufe die Methode rekursiv für das Child-Objekt auf, um seine Kinder zu sammeln.
            CollectAllMeasurementPoints(child);
        }
    }

    public void SetUpDriver(bool engineOn)
    {
        RPC_EngineRequest(engineOn);
    }
}