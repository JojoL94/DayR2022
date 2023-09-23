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

    public Button robotCustomizeButton0;
    public Button robotCustomizeButton1;
    public Button robotCustomizeButton2;
    public Button robotCustomizeButton3;

    bool isReady = false;

    Vector3 desiredCameraPosition = new Vector3(0, 5, 20);

    //Count down
    TickTimer countDownTickTimer = TickTimer.None;

    [Networked(OnChanged = nameof(OnCountdownChanged))]
    byte countDown { get; set; }
    
    [Networked] private NetworkObject robot{ get; set; }

    // Start is called before the first frame update
    void Start()
    {
        countDownText.text = "";
    }

    void Update()
    {
        var lerpSpeed = 0.5f;

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

        var gameObjectsToTransfer = GameObject.FindGameObjectsWithTag("Player");
        var robot = GameObject.FindGameObjectWithTag("Robot");
        DontDestroyOnLoad(robot);

        foreach (var gameObjectToTransfer in gameObjectsToTransfer)
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

        if (robot == null)
        {
            SpawnRobot();
        }
        else
        {
            buttonRobotSpawnText.text = "--";
        }
    }

    public void OnChangeRobotKanone()
    {
        if (isReady)
            return;
        SpawnRobot();
        robot.GetComponent<RobotCustomizerHandler>().OnCycleKanone();
    }

    public void OnChangeRobotHuelle()
    {
        if (isReady)
            return;
        SpawnRobot();
        robot.GetComponent<RobotCustomizerHandler>().OnCycleHuelle();
    }

    public void OnChangeRobotInterior()
    {
        if (isReady)
            return;
        SpawnRobot();
        robot.GetComponent<RobotCustomizerHandler>().OnCycleInterior();
    }

    public void OnChangeRobotLeg()
    {
        if (isReady)
            return;
        SpawnRobot();
        robot.GetComponent<RobotCustomizerHandler>().OnCycleLeg();
    }

    public void OnReady()
    {
        SpawnRobot();
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

    void SpawnRobot()
    {
        if (robot == null)
        {
            RPC_SpawnRobot();
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    void RPC_SpawnRobot()
    {
        Debug.Log("Roboter soll gespawned werden");
        robot = Runner.Spawn(robotPrefab,
            NetworkPlayer.Local.transform.position + NetworkPlayer.Local.transform.forward * -15f,
            Quaternion.identity, null,
            (runner, spawnedRobot) => { });
    }
}