using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class CharacterMovementHandler : NetworkBehaviour
{
    [Header("Animation")] public Animator characterAnimator;

    bool isRespawnRequested = false;


    private bool inDrivingMode;
    private bool inGunnerMode;
    private Transform robotSeat;
    private Transform robot;
    private RobotHandler robotHandler;
    float walkSpeed = 0;
    private float scrollSpeed = 0.3f;
    private float smoothSeatSpeed = 0.9f; // Anpassbare Glättungsgeschwindigkeit

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
                DriveRobot(networkInputData);
            }
            else if (inGunnerMode)
            {
                // Berechne die gewünschte Position basierend auf dem Zielobjekt und dem Offset
                Vector3 desiredPosition = robotSeat.position;

                // Verwende Lerp, um die Position allmählich an die gewünschte Position anzunähern
                Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSeatSpeed);

                // Aktualisiere die Position des Objekts
                transform.position = smoothedPosition;
            }
            else
            {
                MoveCharacter(networkInputData);
            }


            //Check if we've fallen off the world.
            CheckFallRespawn();
        }
    }

    private void MoveCharacter(NetworkInputData networkInputData)
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

    private void DriveRobot(NetworkInputData networkInputData)
    {
        // Berechne die gewünschte Position basierend auf dem Zielobjekt und dem Offset
        Vector3 desiredPosition = robotSeat.position;

        // Verwende Lerp, um die Position allmählich an die gewünschte Position anzunähern
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSeatSpeed);

        // Aktualisiere die Position des Objekts
        transform.position = smoothedPosition;

        if (!robotHandler.isRobotFalling)
        {
            //Move
            Vector3 moveDirection = robotSeat.transform.forward * networkInputData.movementInput.y +
                                    transform.right * networkInputData.movementInput.x;
            moveDirection.Normalize();

            networkCharacterControllerPrototypeCustom.Move(moveDirection, true);
            //Rotate Robot
            if (networkInputData.isRotateLeftPressed)
            {
                networkCharacterControllerPrototypeCustom.Rotate(-1);
            }

            if (networkInputData.isRotateRightPressed)
            {
                networkCharacterControllerPrototypeCustom.Rotate(1);
            }

            if (networkInputData.isScrollUp)
            {
                robotHandler.targetHeightValue += scrollSpeed;
            }
            else if (networkInputData.isScrollDown)
            {
                robotHandler.targetHeightValue -= scrollSpeed;
            }

            //Jump **HIER MUSS EINE BEDINGUNG REIN - DAS ALLE FÜßE UNTEN SIND**
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
            Vector3 moveDirection = robotSeat.transform.forward * networkInputData.movementInput.y +
                                    transform.right * networkInputData.movementInput.x;
            moveDirection.Normalize();

            networkCharacterControllerPrototypeCustom.Move(moveDirection, false);
            //Rotate Robot
            if (networkInputData.isRotateLeftPressed)
            {
                networkCharacterControllerPrototypeCustom.Rotate(-1);
            }

            if (networkInputData.isRotateRightPressed)
            {
                networkCharacterControllerPrototypeCustom.Rotate(1);
            }

            if (networkInputData.isScrollUp)
            {
                robotHandler.targetHeightValue += scrollSpeed;
            }
            else if (networkInputData.isScrollDown)
            {
                robotHandler.targetHeightValue -= scrollSpeed;
            }

            //Jump **HIER MUSS EINE BEDINGUNG REIN - DAS ALLE FÜßE UNTEN SIND**
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

    public void SetCharacterMode(
        NetworkCharacterControllerPrototypeCustom newNetworkCharacterControllerPrototypeCustom, Transform newRobotSeat)
    {
        if (newNetworkCharacterControllerPrototypeCustom == null) //DriveMode
        {
            if (newRobotSeat == null) //GunnerMode?
            {
                inDrivingMode = false;
                networkCharacterControllerPrototypeCustom = transform.GetComponent<NetworkCharacterControllerPrototypeCustom>();
                robot = null;
                robotHandler = null;
            }
            else //GunnerMode!
            {
                robotSeat = newRobotSeat;
                inDrivingMode = false;
                inGunnerMode = true;
            }
        }
        else //DriveMode
        {
            inDrivingMode = true;
            networkCharacterControllerPrototypeCustom = newNetworkCharacterControllerPrototypeCustom;
            robotSeat = newRobotSeat;
            robot = robotSeat.root.transform;
            robotHandler = robot.GetComponent<RobotHandler>();
        }

    }
}