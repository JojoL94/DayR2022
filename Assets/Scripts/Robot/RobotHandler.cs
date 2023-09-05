using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class RobotHandler : NetworkBehaviour
{
    public LayerMask groundLayer; // Die Layer, auf denen sich der Charakter bewegt
    public Transform[] measurementPoints; // Die Punkte am Objekt, von denen aus gemessen wird
    public float rotationThreshold = 0.1f;
    public List<LegTargetHandler> legTargetHandlers = new List<LegTargetHandler>();

    //Driver info
    PlayerRef driverIsPlayerRef;
    string driverIsPlayerName;

    //Gunner info
    PlayerRef gunnerIsPlayerRef;
    string gunnerIsPlayerName;
    [Networked] public bool doorProcess { get; set; }
    [Networked] public bool doorIsOpen { get; set; }
    [Networked] public bool engineOn { get; set; }


    [SerializeField] private Transform closedMarker;
    [SerializeField] private Transform openedMarker;
    [SerializeField] private Transform doorTargetPosition;
    int doorSpeed = 5;
    private float moveSpeed = 5f;
    private bool movingUp;
    private float heightValue = 3f;
    private Vector3 targetHeight;

    public void Start()
    {
        closedMarker = transform.GetChild(1).GetChild(0).GetChild(0).transform;
        openedMarker = transform.GetChild(1).GetChild(0).GetChild(1).transform;
        doorTargetPosition = closedMarker;
        SetUpLegs();
        // Du kannst jetzt auf die legTargetHandlers-Liste zugreifen, um auf die gesammelten Skripte zuzugreifen
        // Zum Beispiel: legTargetHandlers[0].DoSomething();
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


        float hitDistance = 50;
        Vector3 averageNormal = Vector3.zero;
        float averageHeight = 0;

        foreach (Transform point in measurementPoints)
        {
            RaycastHit hit;
            if (Physics.Raycast(point.position, Vector3.down, out hit, hitDistance, groundLayer))
            {
                averageNormal += hit.normal;
                averageHeight += hit.distance;
            }
        }

        if (measurementPoints.Length > 0)
        {
            averageNormal /= measurementPoints.Length;
            averageNormal.Normalize();
            averageHeight /= measurementPoints.Length;

            Quaternion targetRotation = Quaternion.FromToRotation(transform.up, averageNormal) * transform.rotation;

            // Überprüfen, ob die Differenz zwischen aktueller Rotation und Zielrotation den Schwellenwert überschreitet
            if (Quaternion.Angle(transform.rotation, targetRotation) > rotationThreshold)
            {
                // Objekt so ausrichten, dass die Up-Richtung der durchschnittlichen Oberflächennormalen entspricht
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Runner.DeltaTime * 5f);
            }
        }

        if (!engineOn)
        {
            GetComponent<NetworkCharacterControllerPrototypeCustom>().Move(Vector3.zero, false);
            // Rufe die Methode "AnimateRobotLeg()" in jedem der gesammelten Skripte auf
            foreach (LegTargetHandler legTargetHandler in legTargetHandlers)
            {
                legTargetHandler.AnimateRobotLeg(Vector3.zero);
            }
        }
        else
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position + new Vector3(0, 20f, 0), Vector3.down, out hit, Mathf.Infinity, groundLayer))
            {
                targetHeight = hit.point + new Vector3(0, heightValue - Vector3.Distance(transform.position, measurementPoints[0].position), 0);
                if (Vector3.Distance(transform.position, targetHeight) > 0.2f)
                {
                    transform.position = Vector3.MoveTowards(transform.position, targetHeight, moveSpeed * Runner.DeltaTime);
                }
            }
        }


        if (Object.HasStateAuthority)
        {
        }
    }

    public void SetUpDriver(PlayerRef newDriverPlayerRef, string newDriverPlayerName)
    {
        RPC_DriverChangeRequest(newDriverPlayerRef, newDriverPlayerName);
        RPC_EngineRequest(true);
    }

    public void SetUpLegs()
    {
        legTargetHandlers.Clear();
        // Finde alle Kindobjekte dieses GameObjects
        Transform[] children = GetComponentsInChildren<Transform>();


        foreach (Transform child in children)
        {
            // Überprüfe, ob das Kindobjekt ein Skript mit dem Namen "LegTargetHandler" hat
            LegTargetHandler legTargetHandler = child.GetComponent<LegTargetHandler>();

            if (legTargetHandler != null)
            {
                // Füge das gefundene Skript zur Liste hinzu
                legTargetHandlers.Add(legTargetHandler);
            }
        }

        // Überprüfe, ob die Liste mindestens 4 Elemente hat, bevor du sie kürzt
        if (legTargetHandlers.Count >= 5)
        {
            // Entferne die Elemente von Position [0] bis [3]
            legTargetHandlers.RemoveRange(0, 4);
        }
    }

}