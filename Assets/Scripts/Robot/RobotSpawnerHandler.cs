using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class RobotSpawnerHandler : NetworkBehaviour
{
    public NetworkObject robotPrefab;

    [Networked] private NetworkObject robot { get; set; }

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            RPC_SpawnRobot();
        }
    }

    
    [Rpc(RpcSources.StateAuthority, RpcTargets.StateAuthority)]
    void RPC_SpawnRobot()
    {
        if (Object.HasStateAuthority)
        {
            Debug.Log("Roboter soll gespawned werden");
            robot = Runner.Spawn(robotPrefab,
                transform.position,
                Quaternion.identity, null,
                (runner, spawnedRobot) => { });
        }
    }
}