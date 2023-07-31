using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RobotInteractableHandler : MonoBehaviour
{
    public Transform doorHitbox;
    public Transform door;
    public Transform startMarker;
    public Transform endMarker;

    private float dist;
    private bool doorOpenProcess = false;
    private bool doorCloseProcess = false;
    private bool doorOpen = false;
    private bool doorClosed = true;
    // Movement speed in units per second.
    public float speed = 1.0F;
    // Time when the movement started.
    private float startTime;

    // Total distance between the markers.
    private float journeyLength;

    // Update is called once per frame
    void Update()
    {
        if (doorHitbox.CompareTag("ProcessQueue") && doorClosed)
        {
            OpenDoor();
        } else if (doorHitbox.CompareTag("ProcessQueue") && doorOpen)
        {
            CloseDoor();
        }


        if (doorOpenProcess)
        {
            // Distance moved equals elapsed time times speed..
            float distCovered = (Time.time - startTime) * speed;

            // Fraction of journey completed equals current distance divided by total distance.
            float fractionOfJourney = distCovered / journeyLength;

            // Set our position as a fraction of the distance between the markers.
            door.transform.position = Vector3.Lerp(startMarker.position, endMarker.position, fractionOfJourney);
            dist = Vector3.Distance(door.position, endMarker.position);
            if ( dist < 0.1f)
            {
                doorHitbox.tag = "Untagged";
                doorOpen = true;
                doorOpenProcess = false;
                
            }
        }
        
        if (doorCloseProcess)
        {
            // Distance moved equals elapsed time times speed..
            float distCovered = (Time.time - startTime) * speed;

            // Fraction of journey completed equals current distance divided by total distance.
            float fractionOfJourney = distCovered / journeyLength;

            // Set our position as a fraction of the distance between the markers.
            door.transform.position = Vector3.Lerp(endMarker.position, startMarker.position, fractionOfJourney);
            dist = Vector3.Distance(door.position, startMarker.position);
            if ( dist < 0.1f)
            {
                doorHitbox.tag = "Untagged";
                doorClosed = true;
                doorCloseProcess = false;
            }
        }
 
    }

    void OpenDoor()
    {
        if (!doorOpenProcess && !doorCloseProcess)
        {
            doorClosed = false;
            
        // Keep a note of the time the movement started.
        startTime = Time.time;

        // Calculate the journey length.
        journeyLength = Vector3.Distance(startMarker.position, endMarker.position);
        doorOpenProcess = true;
        }
    }
    
    void CloseDoor()
    {
        if (!doorOpenProcess && !doorCloseProcess)
        {
            doorOpen = false;
            // Keep a note of the time the movement started.
            startTime = Time.time;

            // Calculate the journey length.
            journeyLength = Vector3.Distance(endMarker.position, startMarker.position);
            doorCloseProcess = true;
        }
    }
}
