using UnityEngine;
using System.Collections;

/// <summary>
/// ステージの各Fieldが共通で持つスクリプト
/// </summary>
public class StageField : MonoBehaviour {


    void Start()
    { 
    }

    // ロード終了時 LoadLevelAdditiveはOnLevelWasLoadedを呼ばないのでAwakeで処理
    //    void OnLevelWasLoaded(int level)
    void Awake()
    {
        Debug.Log("Stage Loaded");
        // ロード完了の通知
        GameObject adapter = GameObject.Find("/Adapter");
        if (adapter) adapter.SendMessage("OnLoadedField");
        else Debug.Log("Adapter is not exist!!!");
    }
}
