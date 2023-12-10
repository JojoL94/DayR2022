using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
public class ArmorSpawner : NetworkBehaviour
{
    public NetworkObject armorPickUpPrefab;

    [Networked] private NetworkObject armorPickUp { get; set; }

    public override void Spawned()
    {
        
        if (Object.HasStateAuthority)
        {
            RPC_SpawnArmor();
        }
    }
    
    [Rpc(RpcSources.StateAuthority, RpcTargets.StateAuthority)]
    void RPC_SpawnArmor()
    {
        if (Object.HasStateAuthority)
        {
            Debug.Log("Armor soll gespawned werden");
            armorPickUp = Runner.Spawn(armorPickUpPrefab,
                transform.position,
                Quaternion.identity, null,
                (runner, spawnedArmor) => { });
        }
    }
}
