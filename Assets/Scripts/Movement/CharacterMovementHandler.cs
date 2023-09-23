using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class CharacterMovementHandler : NetworkBehaviour
{
    [Header("Animation")] public Animator characterAnimator;

    bool isRespawnRequested = false;

    private bool exitSeat;
    private bool entrySeat;
    private bool exitDriveMode;
    private Transform exitPoint;
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
        networkCharacterControllerPrototypeCustom.LegsGrounded = true;
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
            var rotation = transform.rotation;
            rotation.eulerAngles = new Vector3(0, rotation.eulerAngles.y, rotation.eulerAngles.z);
            transform.rotation = rotation;
            if (inDrivingMode)
            {
                DriveRobot(networkInputData);
            }
            else if (inGunnerMode)
            {
                if (entrySeat)
                {
                    transform.position = Vector3.MoveTowards(transform.position, robotSeat.position, Runner.DeltaTime * 3f);
                    if (Vector3.Distance(transform.position, robotSeat.position) < 0.2f)
                    {
                        if (inDrivingMode)
                        {
                            robotHandler.SetUpDriver(true);
                        }
                        entrySeat = false;
                    }
                }
                else
                {
                    // Berechne die gewünschte Position basierend auf dem Zielobjekt und dem Offset
                    var desiredPosition = robotSeat.position;

                    // Verwende Lerp, um die Position allmählich an die gewünschte Position anzunähern
                    var smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSeatSpeed);

                    // Aktualisiere die Position des Objekts
                    transform.position = smoothedPosition;
                }

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
        if (exitSeat)
        {
            transform.position = Vector3.MoveTowards(transform.position, exitPoint.position, Runner.DeltaTime * 3f);
            if (Vector3.Distance(transform.position, exitPoint.position) < 0.2f)
            {
                exitSeat = false;
                if (exitDriveMode)
                {
                    robotHandler.SetUpDriver(false);
                    robot = null;
                    robotHandler = null;
                    exitDriveMode = false;
                }
            }
        }
        else
        {
            //Move
            var moveDirection = transform.forward * networkInputData.movementInput.y +
                                transform.right * networkInputData.movementInput.x;
            moveDirection.Normalize();

            networkCharacterControllerPrototypeCustom.Move(moveDirection, false);

            //Jump
            if (networkInputData.isJumpPressed)
                networkCharacterControllerPrototypeCustom.Jump();

            var walkVector = new Vector2(networkCharacterControllerPrototypeCustom.Velocity.x,
                networkCharacterControllerPrototypeCustom.Velocity.z);
            walkVector.Normalize();

            walkSpeed = Mathf.Lerp(walkSpeed, Mathf.Clamp01(walkVector.magnitude), Runner.DeltaTime * 5);

            if (characterAnimator != null)
            {
                characterAnimator.SetFloat("walkSpeed", walkSpeed);
            }
        }
    }

    private void DriveRobot(NetworkInputData networkInputData)
    {
        if (entrySeat)
        {
            transform.position = Vector3.MoveTowards(transform.position, robotSeat.position, Runner.DeltaTime * 3f);
            if (Vector3.Distance(transform.position, robotSeat.position) < 0.2f)
            {
                if (inDrivingMode)
                {
                    robotHandler.SetUpDriver(true);
                }
                entrySeat = false;
            }
        }
        else
        {
            // Berechne die gewünschte Position basierend auf dem Zielobjekt und dem Offset
            var desiredPosition = robotSeat.position;

            // Verwende Lerp, um die Position allmählich an die gewünschte Position anzunähern
            var smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSeatSpeed);

            // Aktualisiere die Position des Objekts
            transform.position = smoothedPosition;
        }

        if (!robotHandler.isRobotFalling)
        {
            if (!robotHandler.robotInGround && !entrySeat)
            {
                //Move
                var moveDirection = robotSeat.transform.forward * networkInputData.movementInput.y +
                                    robotSeat.transform.right * networkInputData.movementInput.x;
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


                var walkVector = new Vector2(networkCharacterControllerPrototypeCustom.Velocity.x,
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
                networkCharacterControllerPrototypeCustom.Move(Vector3.zero, true);
            }
        }
        else
        {
            networkCharacterControllerPrototypeCustom.Move(Vector3.zero, false);
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
        NetworkCharacterControllerPrototypeCustom newNetworkCharacterControllerPrototypeCustom, Transform newRobotSeat,
        Transform newExitPoint)
    {
        if (newNetworkCharacterControllerPrototypeCustom == null)
        {
            if (newRobotSeat == null) //ExitMode!
            {
                if (inDrivingMode)
                {
                    exitDriveMode = true;
                }
                else
                {
                    robot = null;
                    robotHandler = null;
                }
                inDrivingMode = false;
                inGunnerMode = false;
                networkCharacterControllerPrototypeCustom =
                    transform.GetComponent<NetworkCharacterControllerPrototypeCustom>();

                exitSeat = true;
                entrySeat = false;
                exitPoint = newExitPoint;
            }
            else //GunnerMode!
            {
                robotSeat = newRobotSeat;
                entrySeat = true;
                inDrivingMode = false;
                inGunnerMode = true;
            }
        }
        else //DriveMode
        {
            inDrivingMode = true;
            entrySeat = true;
            networkCharacterControllerPrototypeCustom = newNetworkCharacterControllerPrototypeCustom;
            robotSeat = newRobotSeat;
            robot = robotSeat.root.transform;
            robotHandler = robot.GetComponent<RobotHandler>();
        }
    }

}