using UnityEngine;
using System.Collections;

/// <summary>
/// 空気残量の泡が出てるエフェクト
/// </summary>
public class AirgageBubble : MonoBehaviour {

    void OnDisplayDamageLv(int value)
    {
        GetComponent<ParticleSystem>().emissionRate = 5 + 10 * (float)(value);
    }

    void OnGameOver()
    {
        GetComponent<ParticleSystem>().Stop();
    }
}
