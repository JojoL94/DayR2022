using Fusion;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterRobotHandler : NetworkBehaviour
{
    [Header("Robot parts")] public NetworkObject robot;
    GameObject robotKanone;
    GameObject robotHuelle;
    GameObject robotInterior;
    GameObject robotLeg;

    //List of Robot part prefabs
    List<GameObject> kanonePrefabs = new List<GameObject>();
    List<GameObject> huellePrefabs = new List<GameObject>();
    List<GameObject> interiorPrefabs = new List<GameObject>();
    List<GameObject> legPrefabs = new List<GameObject>();

    struct NetworkRobotParts : INetworkStruct
    {
        public byte kanonePrefabID;
        public byte huellePrefabID;
        public byte interiorPrefabID;
        public byte legPrefabID;
    }

    [Networked(OnChanged = nameof(OnRobotPartsChanged))]
    NetworkRobotParts networkRobotParts { get; set; }

    private void Awake()
    {
        // Load all heads and sort them
        kanonePrefabs = Resources.LoadAll<GameObject>("Robotparts/Kanonen/").ToList();
        kanonePrefabs = kanonePrefabs.OrderBy(n => n.name).ToList();

        //Load all bodies and sort them 
        huellePrefabs = Resources.LoadAll<GameObject>("Robotparts/Huellen/").ToList();
        huellePrefabs = huellePrefabs.OrderBy(n => n.name).ToList();

        //Load all left arms and sort them 
        interiorPrefabs = Resources.LoadAll<GameObject>("Robotparts/Interiors/").ToList();
        interiorPrefabs = interiorPrefabs.OrderBy(n => n.name).ToList();

        //Load all right arms and sort them 
        legPrefabs = Resources.LoadAll<GameObject>("Robotparts/Legs/").ToList();
        legPrefabs = legPrefabs.OrderBy(n => n.name).ToList();
    }

    public void RoboterCustomizerEinrichten()
    {
        if (SceneManager.GetActiveScene().name != "Ready")
            return;
        robotKanone = robot.transform.GetChild(0).transform.GetChild(0).gameObject;
        robotHuelle = robot.transform.GetChild(0).transform.GetChild(1).gameObject;
        robotInterior = robot.transform.GetChild(0).transform.GetChild(2).gameObject;
        robotLeg = robot.transform.GetChild(0).transform.GetChild(3).gameObject;

        NetworkRobotParts newRobotParts = networkRobotParts;
        //******************************** Hier muss ein Save and Load System rein anstatt dieses Random Zeug******
        //Pick a random outfit
        newRobotParts.kanonePrefabID = (byte)Random.Range(0, kanonePrefabs.Count);
        newRobotParts.huellePrefabID = (byte)Random.Range(0, huellePrefabs.Count);
        newRobotParts.interiorPrefabID = (byte)Random.Range(0, interiorPrefabs.Count);
        newRobotParts.legPrefabID = (byte)Random.Range(0, legPrefabs.Count);


        //Request host to change the outfit, if we have input authority over the object.
        //if (Object.HasInputAuthority)
            RPC_RequestRobotPartsChange(newRobotParts);
    }

    GameObject ReplaceRobotPart(GameObject currentRobotPart, GameObject prefabNewRobotPart)
    {
        GameObject newPart = Instantiate(prefabNewRobotPart, currentRobotPart.transform.position,
            currentRobotPart.transform.rotation);
        newPart.transform.parent = currentRobotPart.transform.parent;
        Utils.SetRenderLayerInChildren(newPart.transform, currentRobotPart.layer);
        Destroy(currentRobotPart);

        return newPart;
    }

    void ReplaceBodyParts()
    {
        //Replace Kanone
        robotKanone = ReplaceRobotPart(robotKanone, kanonePrefabs[networkRobotParts.kanonePrefabID]);

        //Replace Huelle
        robotHuelle = ReplaceRobotPart(robotHuelle, huellePrefabs[networkRobotParts.huellePrefabID]);

        //Replace Interior
        robotInterior = ReplaceRobotPart(robotInterior, interiorPrefabs[networkRobotParts.interiorPrefabID]);

        //Replace Leg
        robotLeg = ReplaceRobotPart(robotLeg, legPrefabs[networkRobotParts.legPrefabID]);

        //GetComponent<HPHandler>().ResetMeshRenderers();
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    void RPC_RequestRobotPartsChange(NetworkRobotParts newNetworkRobotParts, RpcInfo info = default)
    {
        Debug.Log(
            $"Received RPC_RequestRobotPartsChange for robot {transform.name}. KanoneID {newNetworkRobotParts.kanonePrefabID}");

        networkRobotParts = newNetworkRobotParts;
    }


    static void OnRobotPartsChanged(Changed<CharacterRobotHandler> changed)
    {
        changed.Behaviour.OnRobotPartsChanged();
    }

    private void OnRobotPartsChanged()
    {
        ReplaceBodyParts();
    }


    public void OnCycleKanone()
    {
        NetworkRobotParts newRobotPart = networkRobotParts;

        //Pick next head
        newRobotPart.kanonePrefabID++;

        if (newRobotPart.kanonePrefabID > kanonePrefabs.Count - 1)
            newRobotPart.kanonePrefabID = 0;

        //Request host to change the outfit, if we have input authority over the object.
        //if (Object.HasInputAuthority)
            RPC_RequestRobotPartsChange(newRobotPart);
    }

    public void OnCycleHuelle()
    {
        NetworkRobotParts newRobotPart = networkRobotParts;

        //Pick next head
        newRobotPart.huellePrefabID++;

        if (newRobotPart.huellePrefabID > huellePrefabs.Count - 1)
            newRobotPart.huellePrefabID = 0;

        //Request host to change the outfit, if we have input authority over the object.
        //if (Object.HasInputAuthority)
            RPC_RequestRobotPartsChange(newRobotPart);
    }

    public void OnCycleInterior()
    {
        NetworkRobotParts newRobotPart = networkRobotParts;

        //Pick next head
        newRobotPart.interiorPrefabID++;

        if (newRobotPart.interiorPrefabID > interiorPrefabs.Count - 1)
            newRobotPart.interiorPrefabID = 0;

        //Request host to change the outfit, if we have input authority over the object.
        //if (Object.HasInputAuthority)
            RPC_RequestRobotPartsChange(newRobotPart);
    }

    public void OnCycleLeg()
    {
        NetworkRobotParts newRobotPart = networkRobotParts;

        //Pick next head
        newRobotPart.legPrefabID++;

        if (newRobotPart.legPrefabID > legPrefabs.Count - 1)
            newRobotPart.legPrefabID = 0;

        //Request host to change the outfit, if we have input authority over the object.
        //if (Object.HasInputAuthority)
            RPC_RequestRobotPartsChange(newRobotPart);
    }
}