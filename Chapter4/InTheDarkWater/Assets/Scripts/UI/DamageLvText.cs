using UnityEngine;
using System.Collections;

/// <summary>
/// Airgage下のダメージレベル表示
/// </summary>
public class DamageLvText : MonoBehaviour {

    [SerializeField]
    private int disitSize = 1;

    /// <summary>
    /// [SendMessage]表示更新
    /// </summary>
    /// <param name="value"></param>
    void OnDisplayDamageLv(int value)
    {
        GetComponent<GUIText>().text = value.ToString("D" + disitSize);
    }
	
}
