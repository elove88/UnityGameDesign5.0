using UnityEngine;
using System.Collections;

/// <summary>
/// ソナーの下に出てくるメッセージ
/// </summary>
public class SonarMessage : MonoBehaviour {

    [SerializeField]
    private string enemyDestroyed = "The enemy is destroyed!";
    [SerializeField]
    private string itemFound = "You found the item!";
    [SerializeField]
    private string itemLost = "The item was lost...";

    void Start() 
    {
        GetComponent<GUIText>().text = "";
        GetComponent<GUIText>().enabled = false;
    }

    /// <summary>
    /// オブジェクトのヒット
    /// </summary>
    /// <param name="tag"></param>
    void OnHitObject( string tag )
    {
        if (tag.Equals("Enemy")) GetComponent<GUIText>().text = enemyDestroyed;
        else if (tag.Equals("Item")) GetComponent<GUIText>().text = itemFound;
        // 点滅開始
        SendMessage("OnStartTextBlink");
    }

    /// <summary>
    /// オブジェクトのロスト
    /// </summary>
    /// <param name="tag"></param>
    void OnLostObject( string tag )
    {
        if (tag.Equals("Item"))
        {
            GetComponent<GUIText>().text = itemLost;
        }
        // 点滅開始
        SendMessage("OnStartTextBlink");
    }


    void OnEndTextBlink()
    {
        GetComponent<GUIText>().enabled = false;
    }
}
