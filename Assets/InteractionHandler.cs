using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using TMPro;

public class InteractionHandler : NetworkBehaviour
{
    [Header("Aim")]
    public Transform aimPoint;
    [Header("Collision")]
    public LayerMask collisionLayers;

    //Other components
    HPHandler hpHandler;
    NetworkPlayer networkPlayer;
    NetworkObject networkObject;
    
    [Networked(OnChanged = nameof(OnInteractChanged))]
    public bool isInteracting { get; set; }
    // Start is called before the first frame update
    private void Awake()
    {
        hpHandler = GetComponent<HPHandler>();
        networkPlayer = GetBehaviour<NetworkPlayer>();
        networkObject = GetComponent<NetworkObject>();
    }

    public override void FixedUpdateNetwork()
    {
        if (hpHandler.isDead)
            return;

        //Get the input from the network
        if (GetInput(out NetworkInputData networkInputData))
        {
            if (networkInputData.isInteractModePressed && networkInputData.isFireButtonPressed)
                Interact(networkInputData.aimForwardVector);
        }
    }
    
    void Interact(Vector3 aimForwardVector)
    {
        Debug.Log("Try Interact");
        float hitDistance = 5;
        Runner.LagCompensation.Raycast(aimPoint.position, aimForwardVector, hitDistance, Object.InputAuthority, out var hitinfo, collisionLayers, HitOptions.IgnoreInputAuthority);

        bool isInteractableObject = false;

        if (hitinfo.Distance > 0)
            hitDistance = hitinfo.Distance;
        if (hitinfo.Hitbox != null)
        {
            Debug.Log($"{Time.time} {transform.name} hit hitbox {hitinfo.Hitbox.transform.root.name}");

            if (Object.HasStateAuthority)
                isInteractableObject = true;
            hitinfo.Hitbox.tag = "ProcessQueue";

        }
        if (hitinfo.Collider != null)
        {
            if (Object.HasStateAuthority)
            {
                isInteractableObject = true;
            }
        }

        //Debug
        if (isInteractableObject)
            Debug.DrawRay(aimPoint.position, aimForwardVector * hitDistance, Color.red, 1);
        else Debug.DrawRay(aimPoint.position, aimForwardVector * hitDistance, Color.green, 1);

    }
    
    IEnumerator InteractingEffectCO()
    {
        isInteracting = true;

        //Particle gedöns oder irgendein Effekt

        yield return new WaitForSeconds(0.02f);

        isInteracting = false;
    }
    
    static void OnInteractChanged(Changed<InteractionHandler> changed)
    {
        //Debug.Log($"{Time.time} OnFireChanged value {changed.Behaviour.isFiring}");

        bool isInteractingCurrent = changed.Behaviour.isInteracting;

        //Load the old value
        changed.LoadOld();

        bool isInteractingOld = changed.Behaviour.isInteracting;

        if (isInteractingCurrent && !isInteractingOld)
            changed.Behaviour.OnInteractRemote();

    }
    
    void OnInteractRemote()
    {
        //Mach gedöns... Animation zeug oder so
    }
}
