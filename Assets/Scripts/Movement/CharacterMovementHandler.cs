using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class CharacterMovementHandler : NetworkBehaviour
{
    [Header("Animation")] public Animator characterAnimator;

    bool isRespawnRequested = false;
    public bool inDrivingMode;
    private Transform driverSeat;
    float walkSpeed = 0;
    public List<LegTargetHandler> legTargetHandlers = new List<LegTargetHandler>();
    NetworkCharacterControllerPrototypeCustom networkCharacterControllerPrototypeCustom { get; set; }
    HPHandler hpHandler;
    LegTargetHandler legtargetHandler;
    NetworkInGameMessages networkInGameMessages;
    NetworkPlayer networkPlayer;

    private void Awake()
    {
        networkCharacterControllerPrototypeCustom = GetComponent<NetworkCharacterControllerPrototypeCustom>();
        hpHandler = GetComponent<HPHandler>();
        networkInGameMessages = GetComponent<NetworkInGameMessages>();
        networkPlayer = GetComponent<NetworkPlayer>();
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    public override void FixedUpdateNetwork()
    {
        if (Object.HasStateAuthority)
        {
            if (isRespawnRequested)
            {
                Respawn();
                return;
            }

            //Don't update the clients position when they are dead
            if (hpHandler.isDead)
                return;
        }

        //Get the input from the network
        if (GetInput(out NetworkInputData networkInputData))
        {
            //Rotate the transform according to the client aim vector
            transform.forward = networkInputData.aimForwardVector;

            //Cancel out rotation on X axis as we don't want our character to tilt
            Quaternion rotation = transform.rotation;
            rotation.eulerAngles = new Vector3(0, rotation.eulerAngles.y, rotation.eulerAngles.z);
            transform.rotation = rotation;
            if (inDrivingMode)
            {
                transform.position = driverSeat.position;
                //Move
                Vector3 moveDirection = driverSeat.transform.forward * networkInputData.movementInput.y +
                                        transform.right * networkInputData.movementInput.x;
                moveDirection.Normalize();

                Vector3 animateMoveDirection = moveDirection;
                networkCharacterControllerPrototypeCustom.Move(moveDirection, true);

                // Rufe die Methode "AnimateRobotLeg()" in jedem der gesammelten Skripte auf
                if (networkInputData.isRotateLeftPressed)
                {
                    networkCharacterControllerPrototypeCustom.Rotate(-1);
                    animateMoveDirection = Vector3.zero;
                }

                if (networkInputData.isRotateRightPressed)
                {
                    networkCharacterControllerPrototypeCustom.Rotate(1);
                    animateMoveDirection = Vector3.zero;
                }
                foreach (LegTargetHandler legTargetHandler in legTargetHandlers)
                {
                    legTargetHandler.AnimateRobotLeg(animateMoveDirection);
                }

                //Jump
                if (networkInputData.isJumpPressed)
                    networkCharacterControllerPrototypeCustom.Jump();

                Vector2 walkVector = new Vector2(networkCharacterControllerPrototypeCustom.Velocity.x,
                    networkCharacterControllerPrototypeCustom.Velocity.z);
                walkVector.Normalize();

                walkSpeed = Mathf.Lerp(walkSpeed, Mathf.Clamp01(walkVector.magnitude), Runner.DeltaTime * 5);

                if (characterAnimator != null)
                {
                    characterAnimator.SetFloat("walkSpeed", walkSpeed);
                }
            }
            else
            {
                //Move
                Vector3 moveDirection = transform.forward * networkInputData.movementInput.y +
                                        transform.right * networkInputData.movementInput.x;
                moveDirection.Normalize();

                networkCharacterControllerPrototypeCustom.Move(moveDirection, false);

                //Jump
                if (networkInputData.isJumpPressed)
                    networkCharacterControllerPrototypeCustom.Jump();

                Vector2 walkVector = new Vector2(networkCharacterControllerPrototypeCustom.Velocity.x,
                    networkCharacterControllerPrototypeCustom.Velocity.z);
                walkVector.Normalize();

                walkSpeed = Mathf.Lerp(walkSpeed, Mathf.Clamp01(walkVector.magnitude), Runner.DeltaTime * 5);

                if (characterAnimator != null)
                {
                    characterAnimator.SetFloat("walkSpeed", walkSpeed);
                }
            }


            //Check if we've fallen off the world.
            CheckFallRespawn();
        }
    }

    void CheckFallRespawn()
    {
        if (transform.position.y < -12)
        {
            if (Object.HasStateAuthority)
            {
                Debug.Log($"{Time.time} Respawn due to fall outside of map at position {transform.position}");

                networkInGameMessages.SendInGameRPCMessage(networkPlayer.nickName.ToString(), "fell off the world");

                Respawn();
            }
        }
    }

    public void RequestRespawn()
    {
        isRespawnRequested = true;
    }

    void Respawn()
    {
        networkCharacterControllerPrototypeCustom.TeleportToPosition(Utils.GetRandomSpawnPoint());

        hpHandler.OnRespawned();

        isRespawnRequested = false;
    }

    public void SetCharacterControllerEnabled(bool isEnabled)
    {
        networkCharacterControllerPrototypeCustom.Controller.enabled = isEnabled;
    }

    public void SetNetworkCharacterPrototypeCustom(
        NetworkCharacterControllerPrototypeCustom newNetworkCharacterControllerPrototypeCustom, Transform newDriverSeat)
    {
        networkCharacterControllerPrototypeCustom = newNetworkCharacterControllerPrototypeCustom;
        driverSeat = newDriverSeat;
        legTargetHandlers = driverSeat.root.GetComponent<RobotHandler>().legTargetHandlers;
    }
}