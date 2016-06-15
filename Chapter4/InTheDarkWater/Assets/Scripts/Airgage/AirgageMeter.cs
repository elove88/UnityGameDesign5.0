using UnityEngine;
using System.Collections;

/// <summary>
/// 空気残量の表示メーター。Cutoutのシェーダ利用。
/// </summary>
public class AirgageMeter : MonoBehaviour {

    /// <summary>
    /// [SendMessage]値更新
    /// </summary>
    /// <param name="value">更新値[0,1]</param>
    void OnDisplayAirgage(float value)
    {
        //Debug.Log("OnDeflate: Air=" + value);
        // シェーダのアルファcutoffの値を変更して表示更新
        GetComponent<Renderer>().material.SetFloat("_Cutoff", value);
    }
}
