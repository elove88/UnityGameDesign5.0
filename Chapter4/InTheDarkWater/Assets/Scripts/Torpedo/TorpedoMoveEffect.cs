using UnityEngine;
using System.Collections;

/// <summary>
/// ヒット時にパーティクルを消すだけ
/// </summary>
public class TorpedoMoveEffect : MonoBehaviour {

    void OnHit()
    {
        GetComponent<ParticleSystem>().Stop();
    }
}
