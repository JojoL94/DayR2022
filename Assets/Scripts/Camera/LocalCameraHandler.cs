using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalCameraHandler : MonoBehaviour
{
    public Transform cameraAnchorPoint;

    //Input
    Vector2 viewInput;

    //Rotation
    float cameraRotationX = 0;
    float cameraRotationY = 0;

    private bool inDrivingMode;
    public float initialRotationSpeed; // Die anfängliche Rotationsgeschwindigkeit
    public float accelerationFactor; // Der Beschleunigungsfaktor für die Geschwindigkeitszunahme
    public float rotationDelay;
    
    private Quaternion initialRotation; // Die anfängliche Rotation des eigenen Objekts
    private float currentRotationSpeed; // Die aktuelle Rotationsgeschwindigkeit

    [SerializeField] private Transform driverSeat;

    //Other components
    NetworkCharacterControllerPrototypeCustom networkCharacterControllerPrototypeCustom;
    NetworkCharacterControllerPrototypeCustom localNetworkCharacterControllerPrototypeCustom;
    public Camera localCamera;

    private void Awake()
    {
        localCamera = GetComponent<Camera>();
        localNetworkCharacterControllerPrototypeCustom = GetComponentInParent<NetworkCharacterControllerPrototypeCustom>();
        networkCharacterControllerPrototypeCustom = localNetworkCharacterControllerPrototypeCustom;
    }

    // Start is called before the first frame update
    void Start()
    {
        initialRotationSpeed = 1.0f;
        accelerationFactor = 0.1f;
        rotationDelay = 5f;
        cameraRotationX = GameManager.instance.cameraViewRotation.x;
        cameraRotationY = GameManager.instance.cameraViewRotation.y;
        initialRotation = transform.rotation;
        currentRotationSpeed = initialRotationSpeed;
    }

    void LateUpdate()
    {
        if (cameraAnchorPoint == null)
            return;

        if (!localCamera.enabled)
            return;

        //Move the camera to the position of the player
        localCamera.transform.position = cameraAnchorPoint.position;

        //Calculate rotation
        cameraRotationX += viewInput.y * Time.deltaTime *
                           localNetworkCharacterControllerPrototypeCustom.viewUpDownRotationSpeed;
        cameraRotationX = Mathf.Clamp(cameraRotationX, -90, 90);
        if (inDrivingMode)
        {
            Quaternion targetRotation = driverSeat.root.rotation;
            Quaternion currentRotation = transform.rotation;

            // Berechne die Differenz zwischen den Rotationen
            Quaternion rotationDifference = Quaternion.Inverse(currentRotation) * targetRotation;

            // Berechne den Winkel der Differenz
            float angleDifference = Quaternion.Angle(Quaternion.identity, rotationDifference);

            // Erhöhe die Rotationsgeschwindigkeit basierend auf der Differenz
            currentRotationSpeed =
                Mathf.Min(currentRotationSpeed + angleDifference * accelerationFactor, localNetworkCharacterControllerPrototypeCustom.rotationSpeed - rotationDelay);

            // Maus-Input für die zusätzliche Rotation
            cameraRotationY += viewInput.x * Time.deltaTime * localNetworkCharacterControllerPrototypeCustom.rotationSpeed;
            Quaternion mouseRotation = Quaternion.Euler(cameraRotationX, cameraRotationY, 0f);

            // Kombiniere die Rotationen (Zielrotation, Mausrotation)
            Quaternion newRotation = targetRotation * mouseRotation;

            // Drehe das Objekt
            transform.rotation =
                Quaternion.RotateTowards(currentRotation, newRotation, currentRotationSpeed * Time.deltaTime);
        }
        else
        {
            cameraRotationY += viewInput.x * Time.deltaTime * localNetworkCharacterControllerPrototypeCustom.rotationSpeed;
            //Apply rotation
            localCamera.transform.rotation = Quaternion.Euler(cameraRotationX, cameraRotationY, 0);
        }
    }

    public void SetViewInputVector(Vector2 viewInput)
    {
        this.viewInput = viewInput;
    }

    private void OnDestroy()
    {
        if (cameraRotationX != 0 && cameraRotationY != 0)
        {
            GameManager.instance.cameraViewRotation.x = cameraRotationX;
            GameManager.instance.cameraViewRotation.y = cameraRotationY;
        }
    }

    public void SetNetworkCharacterPrototypeCustom(
        NetworkCharacterControllerPrototypeCustom newNetworkCharacterControllerPrototypeCustom, bool newInDrivingMode,
        Transform newDriverSeat)
    {
        networkCharacterControllerPrototypeCustom = newNetworkCharacterControllerPrototypeCustom;
        driverSeat = newDriverSeat;
        inDrivingMode = newInDrivingMode;
    }
}