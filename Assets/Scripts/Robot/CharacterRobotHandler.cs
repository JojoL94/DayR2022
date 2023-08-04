using Fusion;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterRobotHandler : NetworkBehaviour
{
   /* [Header("Robot parts")]
    public GameObject playerHead;
    public GameObject playerBody;
    public GameObject playerRightArm;
    public GameObject playerLeftArm;

    [Header("Ready UI")]
    public Image readyCheckboxImage;

    [Header("Animation")]
    public Animator characterAnimator;

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

    [Networked(OnChanged = nameof(OnIsDoneWithCharacterSelectionChanged))]
    public NetworkBool isDoneWithCharacterSelection { get; set; }
    private void Awake()
    {
        // Load all heads and sort them
        kanonePrefabs = Resources.LoadAll<GameObject>("Bodyparts/Heads/").ToList();
        kanonePrefabs = kanonePrefabs.OrderBy(n => n.name).ToList();

        //Load all bodies and sort them 
        huellePrefabs = Resources.LoadAll<GameObject>("Bodyparts/Bodies/").ToList();
        huellePrefabs = huellePrefabs.OrderBy(n => n.name).ToList();

        //Load all left arms and sort them 
        interiorPrefabs = Resources.LoadAll<GameObject>("Bodyparts/LeftArms/").ToList();
        interiorPrefabs = interiorPrefabs.OrderBy(n => n.name).ToList();

        //Load all right arms and sort them 
        legPrefabs = Resources.LoadAll<GameObject>("Bodyparts/RightArms/").ToList();
        legPrefabs = legPrefabs.OrderBy(n => n.name).ToList();
    }
    
    void Start()
    {
        characterAnimator.SetLayerWeight(1, 0.0f);

        if (SceneManager.GetActiveScene().name != "Ready")
            return;

        NetworkRobotParts newRobotParts = networkRobotParts;
        //*************************************************************** Hier muss ein Save and Load System rein ******
        //Pick a random outfit
        newRobotParts.kanonePrefabID = (byte)Random.Range(0, kanonePrefabs.Count);
        newRobotParts.huellePrefabID = (byte)Random.Range(0, huellePrefabs.Count);
        newRobotParts.interiorPrefabID = (byte)Random.Range(0, interiorPrefabs.Count);
        newRobotParts.legPrefabID = (byte)Random.Range(0, legPrefabs.Count);

        //Allow ready up animation layer to show
        characterAnimator.SetLayerWeight(1, 1.0f);

        //Request host to change the outfit, if we have input authority over the object.
        if (Object.HasInputAuthority)
            RPC_RequestRobotPartsChange(newRobotParts);
    }
    */
}
