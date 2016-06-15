using UnityEngine;
using System.Collections;

/// <summary>
/// ステージ終わりの切り替わりで表示するテキスト
/// </summary>
public class StageEndText : MonoBehaviour {

    [SerializeField]
    private float backtitleDelay = 3.0f;
    [SerializeField]
    private string gameclearText = "STAGE CLEAR";
    [SerializeField]
    private string gameoverText = "GAME OVER";

    void Start()
    {
    }

    // ゲームクリア通知
    void OnGameClear()
    {
        GetComponent<GUIText>().text = gameclearText;
        StartCoroutine("Wait");
    }
    // ゲームオーバー通知
    void OnGameOver()
    {
        GetComponent<GUIText>().text = gameoverText;
        StartCoroutine("Wait");
    }
    // ステージリセット
    void OnStageReset()
    {
        GetComponent<GUIText>().enabled = false;
    }
    
    private IEnumerator Wait()
    {
        yield return new WaitForSeconds(backtitleDelay);
        GetComponent<GUIText>().enabled = true;
        BroadcastMessage("OnStartSwitcher");
    }

}
