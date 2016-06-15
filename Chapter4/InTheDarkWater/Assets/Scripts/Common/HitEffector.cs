using UnityEngine;
using System.Collections;

/// <summary>
/// ヒットエフェクト専用。
/// OnHitが伝わったときに、パーティクルとサウンドを再生する。
/// </summary>
public class HitEffector : MonoBehaviour {

    [SerializeField]
    private bool valid = true;

    void Start()
    { 
    }

    // 無効なら事前に呼んでおく
    void OnInvalidEffect()
    {
        Debug.Log("HitEffector.OnInvalid");
        valid = false;
    }

    // ヒット時の挙動管理と終了タイミング
    void OnHit()
    {

        Debug.Log("HitEffector.OnHit:" + transform.parent.gameObject.transform.parent.tag);
        if (valid)
        {
            if (GetComponent<ParticleSystem>())
            {
                Debug.Log("HitEffector => particle.Play");
                GetComponent<ParticleSystem>().Play();
            }
            if (GetComponent<AudioSource>())
            {
                Debug.Log("HitEffector => audio.Play");
                GetComponent<AudioSource>().Play();
            }
        }
        else Debug.Log("HitEffector.OnHit: Invalid");
    }

    public bool IsFinished()
    {
        bool result = true;
        if (GetComponent<ParticleSystem>()) result = result && !GetComponent<ParticleSystem>().isPlaying;
        if (GetComponent<AudioSource>()) result = result && !GetComponent<AudioSource>().isPlaying;
        return result;
    }
    public bool IsPlaying()
    {
        bool result = false;
        if (GetComponent<ParticleSystem>()) result = result || GetComponent<ParticleSystem>().isPlaying;
        if (GetComponent<AudioSource>()) result = result || GetComponent<AudioSource>().isPlaying;
        return result;
    }
}
