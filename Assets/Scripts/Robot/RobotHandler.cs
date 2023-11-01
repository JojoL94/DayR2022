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
    private Vector3Smoother smootherRotationOffset = new Vector3Smoother(10); // Du kannst die Fenstergröße anpassen.

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
    [Networked] public bool robotInGround { get; set; }
    [Networked] public float targetHeightValue { get; set; }


    [SerializeField] private Transform closedMarker;
    [SerializeField] private Transform openedMarker;
    [SerializeField] private Transform doorTargetPosition;

    private float doorSpeed = 5f;
    private float moveSpeed = 10f;

    private float smoothingRotationFactor = 0.2f;
    private Vector3 rotationOffset;
    private float rotationSpeed = 4f;
    private float rotationOffsetValue = 20f;


    public float robotYOffset;
    private bool movingUp;
    public float minHeight = 2f;
    public float maxHeight = 3.2f;
    private float heightValue;
    private Vector3 targetHeight;
    private Vector2 oldPositon;
    private Vector3 targetOnGround;

    private float smoothingHeightFactor = 0.8f; // Anpassbare Glättungsfaktor

    private NetworkCharacterControllerPrototypeCustom networkCharacterControllerPrototypeCustom;

    private Terrain terrain;
    private TerrainData terrainData;
    private float hoverHeight = 0.5f; // Schwebehöhe über dem Terrain
    private float smoothness = 5f; // Smoothness für die Höhenanpassung

    public void Start()
    {
        closedMarker = transform.GetChild(1).GetChild(0).GetChild(0).transform;
        openedMarker = transform.GetChild(1).GetChild(0).GetChild(1).transform;
        doorTargetPosition = closedMarker;
        CollectAllMeasurementPoints(measurementPoints[0]);
        oldPositon = new Vector2(transform.position.x, transform.position.z);
        rotationOffset = Vector3.zero;
        robotYOffset = GetComponent<CharacterController>().center.y * 2;
        networkCharacterControllerPrototypeCustom = GetComponent<NetworkCharacterControllerPrototypeCustom>();
        terrain = Terrain.activeTerrain;
        terrainData = terrain.terrainData;
    }


    //Network update
    public override void FixedUpdateNetwork()
    {
        DoorProcess();

        //float hitDistance = 50;

        if (Object.HasStateAuthority)
        {
            var currentPosition = new Vector2(transform.position.x, transform.position.z);
            if (currentPosition != oldPositon)
            {
                oldPositon = new Vector2(transform.position.x, transform.position.z);
            }


            if (!engineOn)
            {
                GetComponent<NetworkCharacterControllerPrototypeCustom>().Move(Vector3.zero, false);
                var averageNormal = Vector3.zero;

                foreach (var point in measurementPoints)
                {
                    RaycastHit hit;
                    if (!Physics.Raycast(point.position, Vector3.down, out hit, Mathf.Infinity, groundLayer)) continue;
                    averageNormal += hit.normal;
                }
            }
            else
            {
                var averageNormal = Vector3.zero;
                float averageTerrainHeight = 0;
                int counterPoint = 0;
                foreach (var point in measurementPoints)
                {
                    RaycastHit hit;
                    if (Physics.Raycast(point.position, Vector3.down, out hit, Mathf.Infinity, groundLayer))
                    {
                        averageNormal += hit.normal;
                        counterPoint++;
                    }
                    // Berechne die Höhe des Terrains an der aktuellen Position des Objekts
                    averageTerrainHeight += terrain.SampleHeight(point.position);
                }

                averageTerrainHeight /= measurementPoints.Count;
                averageNormal /= counterPoint;
                // Begrenze den Float-Wert innerhalb des angegebenen Bereichs
                targetHeightValue = Mathf.Clamp(targetHeightValue, minHeight, maxHeight);
                heightValue = Mathf.Lerp(heightValue, targetHeightValue, smoothingHeightFactor);
                heightValue = Mathf.Clamp(heightValue, minHeight, maxHeight);

                RotateRobotMotion(averageNormal);
                HeightRobotMotion(averageTerrainHeight, heightValue - robotYOffset);
            }
        }
    }

    public void HeightRobotMotion(float averageTerrainHeight, float targetHeightAboveGround)
    {
        /*// Stelle sicher, dass das Terrain vorhanden ist
        if (terrain == null)
        {
            Debug.LogError("Terrain not found!");
            if (!robotInGround)
            {
                RaycastHit hit;
                if (Physics.Raycast(
                        new Vector3(transform.position.x, averageYPosition + targetHeightAboveGround,
                            transform.position.z),
                        Vector3.down, out hit, groundLayer))
                {
                    if (Vector3.Distance(hit.point, measurementPoints[0].position) > maxHeight + 0.3f)
                    {
                        isRobotFalling = true;
                    }
                    else
                    {
                        isRobotFalling = false;

                        var newYPosition = hit.point.y + targetHeightAboveGround;

                        // Bewege das Objekt zur vorgegebenen Höhe über dem Boden (nur Y-Komponente ändern)
                        var newPosition = new Vector3(transform.position.x, newYPosition, transform.position.z);
                        var smoothedNewPosition = smootherHeight.Smooth(newPosition);
                        transform.position = Vector3.MoveTowards(transform.position, smoothedNewPosition,
                            Runner.DeltaTime * moveSpeed);
                    }
                }
                else // Ist unter der Erde
                {
                    robotInGround = true;
                }
            }
            else
            {
                RaycastHit hit;
                if (Physics.Raycast(measurementPoints[0].position + Vector3.up * 20f, Vector3.down, out hit,
                        Mathf.Infinity,
                        groundLayer))
                {
                    if (Vector3.Distance(transform.position,
                            new Vector3(transform.position.x, hit.point.y - robotYOffset / 2,
                                transform.position.z)) < 0.3f)
                    {
                        robotInGround = false;
                    }
                    else
                    {
                        transform.position =
                            Vector3.MoveTowards(transform.position,
                                new Vector3(transform.position.x, hit.point.y - robotYOffset / 2,
                                    transform.position.z),
                                Runner.DeltaTime * moveSpeed / 2);
                    }
                }
            }
        }
        else
        {
            // Aktuelle Position des Objekts
            Vector3 objectPosition = transform.position;

            // Berechne die Höhe des Terrains an der aktuellen Position des Objekts
            float terrainHeight = terrain.SampleHeight(objectPosition);

            // Berechne die Zielhöhe des Objekts über dem Boden
            float targetHeight = terrainHeight + targetHeightAboveGround - 4f;

            // Interpoliere die Höhe des Objekts, um ein sanftes Anpassen der Höhe zu ermöglichen
            float smoothedHeight = Mathf.Lerp(objectPosition.y, targetHeight, Runner.DeltaTime * smoothness);

            // Setze die Position des Objekts auf die berechnete Höhe
            transform.position = new Vector3(objectPosition.x, smoothedHeight + hoverHeight, objectPosition.z);
        }*/
        float tmpTerrainHeight = terrain.SampleHeight(transform.position);

        // Berechne die Zielhöhe des Objekts über dem Boden
        float targetHeight = tmpTerrainHeight + averageTerrainHeight + targetHeightAboveGround;

        transform.position = Vector3.MoveTowards(transform.position,
            new Vector3(transform.position.x, targetHeight, transform.position.z),
            Runner.DeltaTime * 10);
    }

    private void RotateRobotMotion(Vector3 averageNormal)
    {
        if (engineOn)
        {
            var smoothedRotationOffset = smootherRotationOffset.Smooth(rotationOffset);
            var targetRotationOffset =
                Quaternion.FromToRotation(transform.up, smoothedRotationOffset) * transform.rotation;

            // Überprüfen, ob die Differenz zwischen aktueller Rotation und Zielrotation den Schwellenwert überschreitet
            if (Quaternion.Angle(transform.rotation, targetRotationOffset) > rotationThreshold)
            {
                // Objekt so ausrichten, dass die Up-Richtung der durchschnittlichen Oberflächennormalen entspricht
                transform.rotation =
                    Quaternion.Lerp(transform.rotation, targetRotationOffset,
                        Runner.DeltaTime * smoothingRotationFactor);
            }
        }

        if (measurementPoints.Count > 0)
        {
            averageNormal.Normalize();
            var smoothedAverageNormal = smootherNormal.Smooth(averageNormal);
            var targetRotation =
                Quaternion.FromToRotation(transform.up, smoothedAverageNormal) * transform.rotation;
            // Überprüfen, ob die Differenz zwischen aktueller Rotation und Zielrotation den Schwellenwert überschreitet
            if (Quaternion.Angle(transform.rotation, targetRotation) > rotationThreshold)
            {
                // Objekt so ausrichten, dass die Up-Richtung der durchschnittlichen Oberflächennormalen entspricht
                transform.rotation =
                    Quaternion.Lerp(transform.rotation, targetRotation,
                        Runner.DeltaTime * rotationSpeed);
            }
        }
    }

    private void DoorProcess()
    {
        var doorPosition = transform.GetChild(0).GetChild(1).GetChild(0).transform.position;
        var distClosedMarker = Vector3.Distance(doorPosition,
            closedMarker.transform.position);
        var distOpenMarker = Vector3.Distance(doorPosition,
            openedMarker.transform.position);

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

            var distanceToTarget =
                Vector3.Distance(doorPosition,
                    doorTargetPosition.position);

            if (distanceToTarget > 0.01f) // Ein kleiner Schwellenwert, um das "Angekommen"-Kriterium zu definieren
            {
                transform.GetChild(0).GetChild(1).GetChild(0).transform.position = Vector3.MoveTowards(doorPosition,
                    doorTargetPosition.position,
                    doorSpeed * Runner.DeltaTime);
            }
            else
            {
                if (distClosedMarker > distOpenMarker)
                {
                    doorIsOpen = true;
                }
                else
                {
                    doorIsOpen = false;
                }

                doorProcess = false;
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

            var distanceToTarget =
                Vector3.Distance(doorPosition,
                    doorTargetPosition.position);

            if (distanceToTarget > 0.01f) // Ein kleiner Schwellenwert, um das "Angekommen"-Kriterium zu definieren
            {
                transform.GetChild(0).GetChild(1).GetChild(0).transform.position = Vector3.MoveTowards(doorPosition,
                    doorTargetPosition.position,
                    doorSpeed * Runner.DeltaTime);
            }
        }
    }

    // Diese Methode sammelt rekursiv alle Child-Objekte.
    private void CollectAllMeasurementPoints(Transform parent)
    {
        foreach (Transform child in parent)
        {
            // Füge das Child-Objekt zur Liste hinzu.
            measurementPoints.Add(child);

            // Rufe die Methode rekursiv für das Child-Objekt auf, um seine Kinder zu sammeln.
            CollectAllMeasurementPoints(child);
        }
    }

    public void OpenCloseDoor()
    {
        if (!doorProcess)
        {
            doorProcess = true;
        }
    }

    public void LegOffsetRotate(bool legFront, bool legLeft, bool inStep)
    {
        if (inStep)
        {
            if (legFront)
            {
                rotationOffset = rotationOffset + new Vector3(rotationOffsetValue, 0, 0);
            }
            else
            {
                rotationOffset = rotationOffset + new Vector3(-rotationOffsetValue, 0, 0);

                //Rotate X -
            }

            if (legLeft)
            {
                rotationOffset = rotationOffset + new Vector3(0, 0, rotationOffsetValue);

                //Rotate z +
            }
            else
            {
                rotationOffset = rotationOffset + new Vector3(0, 0, -rotationOffsetValue);

                //Rotate z -
            }
        }
        else
        {
            if (legFront)
            {
                rotationOffset = rotationOffset + new Vector3(-rotationOffsetValue, 0, 0);
                //Rotate X -
            }
            else
            {
                rotationOffset = rotationOffset + new Vector3(rotationOffsetValue, 0, 0);

                //Rotate X +
            }

            if (legLeft)
            {
                rotationOffset = rotationOffset + new Vector3(0, 0, -rotationOffsetValue);

                //Rotate z -
            }
            else
            {
                rotationOffset = rotationOffset + new Vector3(0, 0, rotationOffsetValue);

                //Rotate z +
            }
        }
    }


    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_EngineRequest(bool newEngineOn)
    {
        if (newEngineOn)
        {
            GetComponent<CharacterController>().center = new Vector3(0, 0, 0);
        }
        else
        {
            GetComponent<CharacterController>().center = new Vector3(0, 5, 0);
        }

        engineOn = newEngineOn;
    }

    public void SetUpDriver(bool engineOn)
    {
        RPC_EngineRequest(engineOn);
    }
}