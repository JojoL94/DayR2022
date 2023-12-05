using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon.StructWrapping;
using UnityEngine;
using Fusion;
using TMPro;
using Unity.VisualScripting;

public class InteractionHandler : NetworkBehaviour
{
    [Header("Aim")] public Transform aimPoint;
    [Header("Collision")] public LayerMask collisionLayers;

    //Other components
    LocalCameraHandler localCameraHandler;
    HPHandler hpHandler;
    NetworkPlayer networkPlayer;

    NetworkObject networkObject;

    //Values for Object Carry
    private bool interactionBlocked;
    private GameObject objectToCarry;
    [SerializeField] private Transform pickPosition;
    
    [Networked(OnChanged = nameof(OnInteractChanged))]
    public bool isInteracting { get; set; }

    // Start is called before the first frame update
    private void Awake()
    {
        hpHandler = GetComponent<HPHandler>();
        networkPlayer = GetBehaviour<NetworkPlayer>();
        networkObject = GetComponent<NetworkObject>();
        localCameraHandler = GetComponentInChildren<LocalCameraHandler>();
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
        if (!interactionBlocked)
        {
            Debug.Log("Try Interact");
            float hitDistance = 5;
            Runner.LagCompensation.Raycast(aimPoint.position, aimForwardVector, hitDistance, Object.InputAuthority,
                out var hitinfo, collisionLayers, HitOptions.IgnoreInputAuthority);

            var isInteractableObject = false;

            if (hitinfo.Distance > 0)
                hitDistance = hitinfo.Distance;
            if (hitinfo.Hitbox != null)
            {
                Debug.Log($"{Time.time} {transform.name} hit hitbox {hitinfo.Hitbox.transform.root.name}");
                isInteractableObject = true;
                if (hitinfo.Hitbox.tag == "Door")
                {
                    hitinfo.Hitbox.transform.root.GetComponent<RobotHandler>().OpenCloseDoor();
                }
                else if (hitinfo.Hitbox.tag == "RobotDriver")
                {
                    GetComponent<CharacterController>().enabled = false;
                    localCameraHandler.SetNetworkCharacterPrototypeCustom(
                        hitinfo.Hitbox.Root.GetComponent<NetworkCharacterControllerPrototypeCustom>(), true,
                        hitinfo.Hitbox.gameObject.transform.GetChild(0));

                    GetComponent<CharacterMovementHandler>().SetCharacterMode(
                        hitinfo.Hitbox.Root.GetComponent<NetworkCharacterControllerPrototypeCustom>(),
                        hitinfo.Hitbox.gameObject.transform.GetChild(0), null);
                }
                else if (hitinfo.Hitbox.tag == "RobotGunner")
                {
                    GetComponent<CharacterController>().enabled = false;

                    localCameraHandler.SetNetworkCharacterPrototypeCustom(
                        hitinfo.Hitbox.Root.GetComponent<NetworkCharacterControllerPrototypeCustom>(), true,
                        hitinfo.Hitbox.gameObject.transform.GetChild(0));

                    GetComponent<CharacterMovementHandler>()
                        .SetCharacterMode(null, hitinfo.Hitbox.gameObject.transform.GetChild(0), null);
                }
                else if (hitinfo.Hitbox.tag == "RobotExit")
                {
                    GetComponent<CharacterController>().enabled = true;

                    localCameraHandler.SetNetworkCharacterPrototypeCustom(
                        null, false,
                        null);
                    GetComponent<CharacterMovementHandler>().SetCharacterMode(
                        null,
                        null, hitinfo.Hitbox.gameObject.transform);

                    //Position ändern
                }

                if (Object.HasStateAuthority)
                {
                }
            }
            else
            {
                RaycastHit hit;
                // Erstelle einen Ray vom aktuellen Objekt in die angegebene Richtung
                Ray ray = new Ray(transform.position, aimForwardVector);

                // Führe den Raycast durch und überprüfe, ob etwas getroffen wurde
                if (Physics.Raycast(ray, out hit, hitDistance))
                {
                    if (hit.transform.tag == "RobotArmor")
                    {
                        objectToCarry = hit.transform.GameObject();
                        interactionBlocked = true;
                        objectToCarry.GetComponent<PickUpHandler>().PickUp(pickPosition);
                        Debug.Log("Object to Carry" + objectToCarry);
                    }
                }
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
        else
        {
            interactionBlocked = false;
            objectToCarry.GetComponent<PickUpHandler>().Drop();
            objectToCarry = null;
        }
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

        var isInteractingCurrent = changed.Behaviour.isInteracting;

        //Load the old value
        changed.LoadOld();

        var isInteractingOld = changed.Behaviour.isInteracting;

        if (isInteractingCurrent && !isInteractingOld)
            changed.Behaviour.OnInteractRemote();
    }

    void OnInteractRemote()
    {
        //Mach gedöns... Animation zeug oder so
        //Idee Rotes licht am Kopf oder so geht an
    }
}