using Fusion;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RobotCustomizerHandler : NetworkBehaviour
{
    [Header("Robot parts")] [SerializeField]
    GameObject robotKanone;

    [SerializeField] GameObject robotHuelle;
    [SerializeField] GameObject robotInterior;
    [SerializeField] GameObject robotLeg;

    //List of Robot part prefabs
    [SerializeField] List<GameObject> kanonePrefabs = new List<GameObject>();
    [SerializeField] List<GameObject> huellePrefabs = new List<GameObject>();
    [SerializeField] List<GameObject> interiorPrefabs = new List<GameObject>();
    [SerializeField] List<GameObject> legPrefabs = new List<GameObject>();
    [Networked(OnChanged = nameof(OnRobotPartsChanged))]
    NetworkRobotParts networkRobotParts { get; set; }
    
    struct NetworkRobotParts : INetworkStruct
    {
        public byte kanonePrefabID;
        public byte huellePrefabID;
        public byte interiorPrefabID;
        public byte legPrefabID;
    }
    
    private void Awake()
    {
        // Load all kanonen and sort them
        kanonePrefabs = Resources.LoadAll<GameObject>("Robotparts/Kanonen/").ToList();
        kanonePrefabs = kanonePrefabs.OrderBy(n => n.name).ToList();

        //Load all huellen and sort them 
        huellePrefabs = Resources.LoadAll<GameObject>("Robotparts/Huellen/").ToList();
        huellePrefabs = huellePrefabs.OrderBy(n => n.name).ToList();

        //Load all interiors and sort them 
        interiorPrefabs = Resources.LoadAll<GameObject>("Robotparts/Interiors/").ToList();
        interiorPrefabs = interiorPrefabs.OrderBy(n => n.name).ToList();

        //Load all legs and sort them 
        legPrefabs = Resources.LoadAll<GameObject>("Robotparts/Legs/").ToList();
        legPrefabs = legPrefabs.OrderBy(n => n.name).ToList();
    }


    public void Start()
    {
        if (SceneManager.GetActiveScene().name != "Ready")
            return;

        var newRobotParts = networkRobotParts;
        newRobotParts.kanonePrefabID = (byte)Random.Range(0, kanonePrefabs.Count);
        newRobotParts.huellePrefabID = (byte)Random.Range(0, huellePrefabs.Count);
        newRobotParts.interiorPrefabID = (byte)Random.Range(0, interiorPrefabs.Count);
        newRobotParts.legPrefabID = (byte)Random.Range(0, legPrefabs.Count);

        RPC_RequestRobotPartsChange(newRobotParts);

    }


    GameObject ReplaceRobotPart(GameObject currentRobotPart, GameObject prefabNewRobotPart)
    {
        var newPart = Instantiate(prefabNewRobotPart, currentRobotPart.transform.position,
            currentRobotPart.transform.rotation);
        newPart.transform.parent = currentRobotPart.transform.parent;
        Utils.SetRenderLayerInChildren(newPart.transform, currentRobotPart.layer);
        Destroy(currentRobotPart);
        return newPart;
    }

    void ReplaceRobotParts()
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


    static void OnRobotPartsChanged(Changed<RobotCustomizerHandler> changed)
    {
        changed.Behaviour.OnRobotPartsChanged();
    }

    private void OnRobotPartsChanged()
    {
        ReplaceRobotParts();
    }


    public void OnCycleKanone()
    {
        var newRobotPart = networkRobotParts;

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
        var newRobotPart = networkRobotParts;

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
        var newRobotPart = networkRobotParts;

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
        var newRobotPart = networkRobotParts;

        //Pick next head
        newRobotPart.legPrefabID++;

        if (newRobotPart.legPrefabID > legPrefabs.Count - 1)
            newRobotPart.legPrefabID = 0;

        //Request host to change the outfit, if we have input authority over the object.
        //if (Object.HasInputAuthority)
        RPC_RequestRobotPartsChange(newRobotPart);
    }
}