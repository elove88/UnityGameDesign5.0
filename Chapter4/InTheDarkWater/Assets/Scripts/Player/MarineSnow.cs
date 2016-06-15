using UnityEngine;
using System.Collections;

/// <summary>
/// マリンスノーのエフェクトに対する設定
/// </summary>
public class MarineSnow : MonoBehaviour {

    [SerializeField]
    private float maxSpeed = 30.0f;

    
    //void OnGameOver() 
    //{
        //particleSystem.Pause();
    //}

    public void SetSpeed( float rate ) 
    {
        GetComponent<ParticleSystem>().startSpeed = Mathf.Lerp(1.0f, maxSpeed, rate);
    }
}
