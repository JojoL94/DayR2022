using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterInputHandler : MonoBehaviour
{
    Vector2 moveInputVector = Vector2.zero;
    Vector2 viewInputVector = Vector2.zero;

    bool isJumpButtonPressed = false;
    bool isFireButtonPressed = false;
    bool isGrenadeFireButtonPressed = false;
    bool isRocketLauncherFireButtonPressed = false;
    bool isInteractModePressed = false;
    bool isRotateLeftPressed = false;
    bool isRotateRightPressed = false;
    bool isScrollUp;
    bool isScrollDown;

//Other components
    LocalCameraHandler localCameraHandler;
    CharacterMovementHandler characterMovementHandler;

    private void Awake()
    {
        localCameraHandler = GetComponentInChildren<LocalCameraHandler>();
        characterMovementHandler = GetComponent<CharacterMovementHandler>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!characterMovementHandler.Object.HasInputAuthority)
            return;

        if (SceneManager.GetActiveScene().name == "Ready")
            return;

        //View input
        viewInputVector.x = Input.GetAxis("Mouse X");
        viewInputVector.y = Input.GetAxis("Mouse Y") * -1; //Invert the mouse look

        //Move input
        moveInputVector.x = Input.GetAxis("Horizontal");
        moveInputVector.y = Input.GetAxis("Vertical");
        
        //Scroll input
        if (Input.GetAxis("Mouse ScrollWheel") > 0f ) // forward
        {
            isScrollUp = true;
            isScrollDown = false;
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0f ) // backwards
        {
            isScrollUp = false;
            isScrollDown = true;
        }
        else if (Input.GetAxis("Mouse ScrollWheel") == 0f )
        {
            isScrollUp = false;
            isScrollDown = false;
        }

        //Jump
        if (Input.GetButtonDown("Jump"))
            isJumpButtonPressed = true;

        //Fire
        if (Input.GetButtonDown("Fire1"))
            isFireButtonPressed = true;

        //Fire
        if (Input.GetButtonDown("Fire2"))
            isRocketLauncherFireButtonPressed = true;

        //Throw grenade
        if (Input.GetKeyDown(KeyCode.G))
            isGrenadeFireButtonPressed = true;

        //Interact Mode ON
        if (Input.GetKeyDown(KeyCode.F))
        {
            isInteractModePressed = true;
        }

        //Interact Mode OFF
        if (Input.GetKeyUp(KeyCode.F))
        {
            isInteractModePressed = false;
        }
        
        //Rotate Left
        if (Input.GetKeyDown(KeyCode.Q))
        {
            isRotateLeftPressed = true;
        }
        //Rotate Right
        if (Input.GetKeyDown(KeyCode.E))
        {
            isRotateRightPressed = true;
        }
        
        //Stop Rotate Left
        if (Input.GetKeyUp(KeyCode.Q))
        {
            isRotateLeftPressed = false;
        }
        //Stop Rotate Right
        if (Input.GetKeyUp(KeyCode.E))
        {
            isRotateRightPressed = false;
        }
        
        //Set view
        localCameraHandler.SetViewInputVector(viewInputVector);
    }

    public NetworkInputData GetNetworkInput()
    {
        NetworkInputData networkInputData = new NetworkInputData();

        //Aim data
        networkInputData.aimForwardVector = localCameraHandler.transform.forward;

        //Move data
        networkInputData.movementInput = moveInputVector;
        
        //Scroll data
        networkInputData.isScrollUp = isScrollUp;
        networkInputData.isScrollDown = isScrollDown;

        //Jump data
        networkInputData.isJumpPressed = isJumpButtonPressed;

        //Fire data
        networkInputData.isFireButtonPressed = isFireButtonPressed;

        //Rocket data
        networkInputData.isRocketLauncherFireButtonPressed = isRocketLauncherFireButtonPressed;

        //Grenade fire data
        networkInputData.isGrenadeFireButtonPressed = isGrenadeFireButtonPressed;

        //InteractMode data
        networkInputData.isInteractModePressed = isInteractModePressed;
        
        //Rotate Right data
        networkInputData.isRotateRightPressed = isRotateRightPressed;
        
        //Rotate Left data
        networkInputData.isRotateLeftPressed = isRotateLeftPressed;
        
        //Reset variables now that we have read their states
        isJumpButtonPressed = false;
        isFireButtonPressed = false;
        isGrenadeFireButtonPressed = false;
        isRocketLauncherFireButtonPressed = false;

        return networkInputData;
    }
}