using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UnityEngine.UI;

public class RobotInteractableHandler : NetworkBehaviour
{
    public Transform doorButtonHitbox;
    public Transform door;
    public Transform closedMarker;
    public Transform openMarker;

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
    public override void FixedUpdateNetwork()
    {
        if (doorButtonHitbox.CompareTag("ProcessQueue") && doorClosed)
        {
            OpenDoor();
        } else if (doorButtonHitbox.CompareTag("ProcessQueue") && doorOpen)
        {
            CloseDoor();
        }


        if (doorOpenProcess)
        {
            // Distance moved equals elapsed time times speed..
            var distCovered = (Time.time - startTime) * speed;

            // Fraction of journey completed equals current distance divided by total distance.
            var fractionOfJourney = distCovered / journeyLength;

            // Set our position as a fraction of the distance between the markers.
            door.transform.position = Vector3.Lerp(closedMarker.position, openMarker.position, fractionOfJourney);
            dist = Vector3.Distance(door.position, openMarker.position);
            if ( dist < 0.1f)
            {
                doorButtonHitbox.tag = "Untagged";
                doorOpen = true;
                doorOpenProcess = false;
                
            }
        }
        
        if (doorCloseProcess)
        {
            // Distance moved equals elapsed time times speed..
            var distCovered = (Time.time - startTime) * speed;

            // Fraction of journey completed equals current distance divided by total distance.
            var fractionOfJourney = distCovered / journeyLength;

            // Set our position as a fraction of the distance between the markers.
            door.transform.position = Vector3.Lerp(openMarker.position, closedMarker.position, fractionOfJourney);
            dist = Vector3.Distance(door.position, closedMarker.position);
            if ( dist < 0.1f)
            {
                doorButtonHitbox.tag = "Untagged";
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
        journeyLength = Vector3.Distance(closedMarker.position, openMarker.position);
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
            journeyLength = Vector3.Distance(openMarker.position, closedMarker.position);
            doorCloseProcess = true;
        }
    }
}
