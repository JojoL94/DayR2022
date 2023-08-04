using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class LegTargetHandler : NetworkBehaviour
{
    private float raycastDistance = 15f; // Die maximale Länge des Raycasts
    private float offsetAmount = 0.5f; // Der Offset in Laufrichtung
    private Transform markerNextStep;
    public Transform target;
    private void Start()
    {
        markerNextStep = transform.GetChild(0);
    }

    private void Update()
    {
        //Buffer für raycastDistance ergibt sich je nach schräglage------------------------
        
        raycastDistance = transform.position.y + 1f/*Buffer*/;
        // Richtung des Raycasts (primär nach unten)
        Vector3 raycastDirection = Vector3.down;

        // Prüfen, ob das Objekt sich bewegt
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))
        {
            // Richtung der Bewegung des Objekts
            Vector3 movementDirection = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical")).normalized;

            // Offset in Laufrichtung hinzufügen
            raycastDirection += movementDirection * offsetAmount;
        }

        // Raycast ausführen
        RaycastHit hit;
        if (Physics.Raycast(transform.position, raycastDirection, out hit, raycastDistance))
        {
            markerNextStep.position = hit.point;
            // Der Raycast hat ein Objekt getroffen
            Debug.DrawRay(transform.position, raycastDirection * hit.distance, Color.red);
            //Debug.Log("Getroffen: " + hit.collider.gameObject.name);
        }
        else
        {
            // Der Raycast hat kein Objekt getroffen
            Debug.DrawRay(transform.position, raycastDirection * raycastDistance, Color.green);
            //Debug.Log("Nicht getroffen");
        }

        target.position = markerNextStep.position;
    }
}