using Fusion;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterOutfitHandler : NetworkBehaviour
{
    [Header("Character parts")]
    public GameObject playerHead;
    public GameObject playerBody;
    public GameObject playerRightArm;
    public GameObject playerLeftArm;

    [Header("Ready UI")]
    public Image readyCheckboxImage;

    [Header("Animation")]
    public Animator characterAnimator;

    //List of body part prefabs
    List<GameObject> headPrefabs = new List<GameObject>();
    List<GameObject> bodyPrefabs = new List<GameObject>();
    List<GameObject> leftArmPrefabs = new List<GameObject>();
    List<GameObject> rightArmPrefabs = new List<GameObject>();

    struct NetworkOutfit : INetworkStruct
    {
        public byte headPrefabID;
        public byte bodyPrefabID;
        public byte leftArmPrefabID;
        public byte rightArmPrefabID;
    }

    [Networked(OnChanged = nameof(OnOutfitChanged))]
    NetworkOutfit networkOutfit { get; set; }

    [Networked(OnChanged = nameof(OnIsDoneWithCharacterSelectionChanged))]
    public NetworkBool isDoneWithCharacterSelection { get; set; }

    private void Awake()
    {
        // Load all heads and sort them
        headPrefabs = Resources.LoadAll<GameObject>("Bodyparts/Heads/").ToList();
        headPrefabs = headPrefabs.OrderBy(n => n.name).ToList();

        //Load all bodies and sort them 
        bodyPrefabs = Resources.LoadAll<GameObject>("Bodyparts/Bodies/").ToList();
        bodyPrefabs = bodyPrefabs.OrderBy(n => n.name).ToList();

        //Load all left arms and sort them 
        leftArmPrefabs = Resources.LoadAll<GameObject>("Bodyparts/LeftArms/").ToList();
        leftArmPrefabs = leftArmPrefabs.OrderBy(n => n.name).ToList();

        //Load all right arms and sort them 
        rightArmPrefabs = Resources.LoadAll<GameObject>("Bodyparts/RightArms/").ToList();
        rightArmPrefabs = rightArmPrefabs.OrderBy(n => n.name).ToList();
    }

    // Start is called before the first frame update
    void Start()
    {
        characterAnimator.SetLayerWeight(1, 0.0f);

        if (SceneManager.GetActiveScene().name != "Ready")
            return;

        NetworkOutfit newOutfit = networkOutfit;

        //Pick a random outfit
        newOutfit.headPrefabID = (byte)Random.Range(0, headPrefabs.Count);
        newOutfit.bodyPrefabID = (byte)Random.Range(0, bodyPrefabs.Count);
        newOutfit.leftArmPrefabID = (byte)Random.Range(0, leftArmPrefabs.Count);
        newOutfit.rightArmPrefabID = (byte)Random.Range(0, rightArmPrefabs.Count);

        //Allow ready up animation layer to show
        characterAnimator.SetLayerWeight(1, 1.0f);

        //Request host to change the outfit, if we have input authority over the object.
        if (Object.HasInputAuthority)
            RPC_RequestOutfitChange(newOutfit);
    }

    GameObject ReplaceBodyPart(GameObject currentBodyPart, GameObject prefabNewBodyPart)
    {
        GameObject newPart = Instantiate(prefabNewBodyPart, currentBodyPart.transform.position, currentBodyPart.transform.rotation);
        newPart.transform.parent = currentBodyPart.transform.parent;
        Utils.SetRenderLayerInChildren(newPart.transform, currentBodyPart.layer);
        Destroy(currentBodyPart);

        return newPart;
    }

    void ReplaceBodyParts()
    {
        //Replace head
        playerHead = ReplaceBodyPart(playerHead, headPrefabs[networkOutfit.headPrefabID]);

        //Replace body
        playerBody = ReplaceBodyPart(playerBody, bodyPrefabs[networkOutfit.bodyPrefabID]);

        //Replace left arm
        playerLeftArm = ReplaceBodyPart(playerLeftArm, leftArmPrefabs[networkOutfit.leftArmPrefabID]);

        //Replace right arm
        playerRightArm = ReplaceBodyPart(playerRightArm, rightArmPrefabs[networkOutfit.rightArmPrefabID]);

        GetComponent<HPHandler>().ResetMeshRenderers();
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    void RPC_RequestOutfitChange(NetworkOutfit newNetworkOutfit, RpcInfo info = default)
    {
        Debug.Log($"Received RPC_RequestOutfitChange for player {transform.name}. HeadID {newNetworkOutfit.headPrefabID}");

        networkOutfit = newNetworkOutfit;
    }


    static void OnOutfitChanged(Changed<CharacterOutfitHandler> changed)
    {
        changed.Behaviour.OnOutfitChanged();
    }

    private void OnOutfitChanged()
    {
        ReplaceBodyParts();
    }



    public void OnCycleHead()
    {
        NetworkOutfit newOutfit = networkOutfit;

        //Pick next head
        newOutfit.headPrefabID++;

        if (newOutfit.headPrefabID > headPrefabs.Count - 1)
            newOutfit.headPrefabID = 0;

        //Request host to change the outfit, if we have input authority over the object.
        if (Object.HasInputAuthority)
            RPC_RequestOutfitChange(newOutfit);
    }

    public void OnCycleBody()
    {
        NetworkOutfit newOutfit = networkOutfit;

        //Pick next head
        newOutfit.bodyPrefabID++;

        if (newOutfit.bodyPrefabID > bodyPrefabs.Count - 1)
            newOutfit.bodyPrefabID = 0;

        //Request host to change the outfit, if we have input authority over the object.
        if (Object.HasInputAuthority)
            RPC_RequestOutfitChange(newOutfit);
    }

    public void OnCycleLeftArm()
    {
        NetworkOutfit newOutfit = networkOutfit;

        //Pick next head
        newOutfit.leftArmPrefabID++;

        if (newOutfit.leftArmPrefabID > leftArmPrefabs.Count - 1)
            newOutfit.leftArmPrefabID = 0;

        //Request host to change the outfit, if we have input authority over the object.
        if (Object.HasInputAuthority)
            RPC_RequestOutfitChange(newOutfit);
    }

    public void OnCycleRightArm()
    {
        NetworkOutfit newOutfit = networkOutfit;

        //Pick next head
        newOutfit.rightArmPrefabID++;

        if (newOutfit.rightArmPrefabID > rightArmPrefabs.Count - 1)
            newOutfit.rightArmPrefabID = 0;

        //Request host to change the outfit, if we have input authority over the object.
        if (Object.HasInputAuthority)
            RPC_RequestOutfitChange(newOutfit);
    }

    public void OnReady(bool isReady)
    {
        //Request host to change the outfit, if we have input authority over the object.
        if (Object.HasInputAuthority)
        {
            RPC_SetReady(isReady);
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    void RPC_SetReady(NetworkBool isReady, RpcInfo info = default)
    {
        isDoneWithCharacterSelection = isReady;
    }

    static void OnIsDoneWithCharacterSelectionChanged(Changed<CharacterOutfitHandler> changed)
    {
        changed.Behaviour.IsDoneWithCharacterSelectionChanged();
    }

    private void IsDoneWithCharacterSelectionChanged()
    {
        if (SceneManager.GetActiveScene().name != "Ready")
            return;

        if (isDoneWithCharacterSelection)
        {
            characterAnimator.SetTrigger("Ready");
            readyCheckboxImage.gameObject.SetActive(true);
        }
        else readyCheckboxImage.gameObject.SetActive(false);
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "Ready")
            readyCheckboxImage.gameObject.SetActive(false);
    }
}
