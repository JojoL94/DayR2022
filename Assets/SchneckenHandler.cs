using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class SchneckenHandler : NetworkBehaviour
{
    public LayerMask groundLayer; // Die Layer, auf denen sich der Charakter bewegt
    public List<Transform> measurementPoints = new List<Transform>();
    private Vector3Smoother smootherRotationOffset = new Vector3Smoother(10); // Du kannst die Fenstergröße anpassen.
    private Vector3Smoother smootherNormal = new Vector3Smoother(10); // Du kannst die Fenstergröße anpassen.

    public float rotationThreshold = 0.1f;
    private float smoothingRotationFactor = 0.2f;
    private Vector3 rotationOffset;
    private Vector2 oldPositon;
    private float rotationSpeed = 4f;
    private float targetHeightAboveGround = 1;

    public float moveSpeed = 2.0f; // Die gewünschte Geschwindigkeit
    

    private float desiredHeight = -4f; // Gewünschte Höhe über dem Boden
    private float hoverHeight = 0.5f; // Schwebehöhe über dem Terrain
    private float smoothness = 5f; // Smoothness für die Höhenanpassung
    private TerrainHeightFinder myTerrainHeightFinder;
    public void Start()
    {
        CollectAllMeasurementPoints(measurementPoints[0]);
        myTerrainHeightFinder = GetComponent<TerrainHeightFinder>();
    }

    public override void FixedUpdateNetwork()
    {
        var averageNormal = Vector3.zero;
        var averagePosition = Vector3.zero;

        foreach (var point in measurementPoints)
        {
            RaycastHit hit;
            if (!Physics.Raycast(point.position, Vector3.down, out hit, Mathf.Infinity, groundLayer)) continue;
            averageNormal += hit.normal;
            averagePosition += hit.point;
        }

        averagePosition /= measurementPoints.Count;
        var currentPosition = new Vector2(transform.position.x, transform.position.z);
        if (currentPosition != oldPositon)
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

            if (measurementPoints.Count > 0)
            {
                averageNormal /= measurementPoints.Count;
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
            oldPositon = transform.position;
            
            //NACH VORNE BEWEGEN
            // Berechne die Bewegung in Richtung Z (vorwärts)
            Vector3 movement = Vector3.forward * moveSpeed * Runner.DeltaTime;

            // Setze die Höhe (Y-Achse) des Objekts
            Vector3 newPosition = transform.position;

            // Bewege das Objekt
            transform.position = newPosition + movement;


            // Aktuelle Position des Objekts
            Vector3 objectPosition = transform.position;

            // Berechne die Höhe des Terrains an der aktuellen Position des Objekts
            float terrainHeight = myTerrainHeightFinder.GetTerrainHeightAtPosition(objectPosition);

            // Berechne die Zielhöhe des Objekts über dem Boden
            float targetHeight = terrainHeight + desiredHeight;

            // Interpoliere die Höhe des Objekts, um ein sanftes Anpassen der Höhe zu ermöglichen
            float smoothedHeight = Mathf.Lerp(objectPosition.y, targetHeight, Runner.DeltaTime * smoothness);

            // Setze die Position des Objekts auf die berechnete Höhe
            transform.position = new Vector3(objectPosition.x, smoothedHeight + hoverHeight, objectPosition.z);

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
}