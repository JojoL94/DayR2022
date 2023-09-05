using Fusion;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ReadyUIHandler : NetworkBehaviour
{
    [Header("UI")] public TextMeshProUGUI buttonReadyText;
    public TextMeshProUGUI buttonRobotSpawnText;
    public TextMeshProUGUI countDownText;
    public NetworkObject robotPrefab;
    public NetworkObject robot;
    public Button robotCustomizButton0;
    public Button robotCustomizButton1;
    public Button robotCustomizButton2;
    public Button robotCustomizButton3;

    bool isReady = false;

    Vector3 desiredCameraPosition = new Vector3(0, 5, 20);

    //Count down
    TickTimer countDownTickTimer = TickTimer.None;

    [Networked(OnChanged = nameof(OnCountdownChanged))]
    byte countDown { get; set; }

    [Networked] public bool robotAllreadySpawned { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        countDownText.text = "";
        robotCustomizButton0.enabled = false;
        robotCustomizButton1.enabled = false;
        robotCustomizButton2.enabled = false;
        robotCustomizButton3.enabled = false;
        if (robotAllreadySpawned)
        {
            robot = GameObject.FindGameObjectWithTag("Robot").GetComponent<NetworkObject>();
        }
    }

    void Update()
    {
        float lerpSpeed = 0.5f;

        if (!isReady)
        {
            desiredCameraPosition = new Vector3(NetworkPlayer.Local.transform.position.x, 0.95f, 5);
            lerpSpeed = 7;
        }
        else
        {
            desiredCameraPosition = new Vector3(14, 3, 30);
            lerpSpeed = 0.5f;
        }

        Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, desiredCameraPosition,
            Time.deltaTime * lerpSpeed);

        if (countDownTickTimer.Expired(Runner))
        {
            StartGame();

            countDownTickTimer = TickTimer.None;
        }
        else if (countDownTickTimer.IsRunning)
        {
            countDown = (byte)countDownTickTimer.RemainingTime(Runner);
        }
    }

    void StartGame()
    {
        //Lock the session, so no other client can join
        Runner.SessionInfo.IsOpen = false;

        GameObject[] gameObjectsToTransfer = GameObject.FindGameObjectsWithTag("Player");
        GameObject robot = GameObject.FindGameObjectWithTag("Robot");
        DontDestroyOnLoad(robot);

        foreach (GameObject gameObjectToTransfer in gameObjectsToTransfer)
        {
            DontDestroyOnLoad(gameObjectToTransfer);
            //Check if the player is ready
            if (!gameObjectToTransfer.GetComponent<CharacterOutfitHandler>().isDoneWithCharacterSelection)
                Runner.Disconnect(gameObjectToTransfer.GetComponent<NetworkObject>().InputAuthority);
        }

        //Update scene for the network
        Runner.SetActiveScene("World1");
    }

    public void OnChangeCharacterHead()
    {
        if (isReady)
            return;

        NetworkPlayer.Local.GetComponent<CharacterOutfitHandler>().OnCycleHead();
    }

    public void OnChangeCharacterBody()
    {
        if (isReady)
            return;

        NetworkPlayer.Local.GetComponent<CharacterOutfitHandler>().OnCycleBody();
    }

    public void OnChangeCharacterLeftArm()
    {
        if (isReady)
            return;

        NetworkPlayer.Local.GetComponent<CharacterOutfitHandler>().OnCycleLeftArm();
    }

    public void OnChangeCharacterRightArm()
    {
        if (isReady)
            return;

        NetworkPlayer.Local.GetComponent<CharacterOutfitHandler>().OnCycleRightArm();
    }

    /**************** Robot ****************/
    public void OnSpawnRobot()
    {
        if (isReady)
            return;

        if (!robotAllreadySpawned)
        {
            Debug.Log("Roboter soll gespawned werden");
            robot = Runner.Spawn(robotPrefab,
                NetworkPlayer.Local.transform.position + NetworkPlayer.Local.transform.forward * -15f,
                Quaternion.identity, null,
                (runner, spawnedRobot) =>
                {
                    /*Hier kann ein Script vor dem Spawn ausgeführt werden.*/
                });

            if (robot != null)
            {
                robotCustomizButton0.enabled = true;
                robotCustomizButton1.enabled = true;
                robotCustomizButton2.enabled = true;
                robotCustomizButton3.enabled = true;
                buttonRobotSpawnText.text = "--";
                robotAllreadySpawned = true;
            }
        }
        else
        {
            robot = GameObject.FindGameObjectWithTag("Robot").GetComponent<NetworkObject>();
            robotCustomizButton0.enabled = true;
            robotCustomizButton1.enabled = true;
            robotCustomizButton2.enabled = true;
            robotCustomizButton3.enabled = true;
            buttonRobotSpawnText.text = "--";
        }
    }

    public void OnChangeRobotKanone()
    {
        if (isReady)
            return;

        robot.GetComponent<RobotCustomizerHandler>().OnCycleKanone();
    }

    public void OnChangeRobotHuelle()
    {
        if (isReady)
            return;

        robot.GetComponent<RobotCustomizerHandler>().OnCycleHuelle();
    }

    public void OnChangeRobotInterior()
    {
        if (isReady)
            return;

        robot.GetComponent<RobotCustomizerHandler>().OnCycleInterior();
    }

    public void OnChangeRobotLeg()
    {
        if (isReady)
            return;

        robot.GetComponent<RobotCustomizerHandler>().OnCycleLeg();
    }

    public void OnReady()
    {
        if (isReady)
            isReady = false;
        else isReady = true;

        if (isReady)
            buttonReadyText.text = "NOT READY";
        else
            buttonReadyText.text = "READY";

        if (Runner.IsServer)
        {
            if (isReady)
                countDownTickTimer = TickTimer.CreateFromSeconds(Runner, 10);
            else
            {
                countDownTickTimer = TickTimer.None;
                countDown = 0;
            }
        }

        NetworkPlayer.Local.GetComponent<CharacterOutfitHandler>().OnReady(isReady);
    }

    static void OnCountdownChanged(Changed<ReadyUIHandler> changed)
    {
        changed.Behaviour.OnCountdownChanged();
    }

    private void OnCountdownChanged()
    {
        if (countDown == 0)
            countDownText.text = $"";
        else countDownText.text = $"Game starts in {countDown}";
    }
}