using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SoundManager : NetworkBehaviour
{
    public AudioSource hitSound;

    [ClientRpc]
    public void RpcMakeHitSound()
    {
        hitSound.Play();
    }
}
