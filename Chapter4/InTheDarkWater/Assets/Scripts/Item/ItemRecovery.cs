using UnityEngine;
using System.Collections;

/// <summary>
/// アイテムによる回復量の設定。
/// </summary>
public class ItemRecovery : MonoBehaviour
{
    [SerializeField]
    private ItemParameter param;

    private float timeStamp = 0.0f; // 経過時間を測るためのタイムスタンプ

    void Start()
    {
    }

    void OnStartLifeTimer(ItemParameter param_)
    {
        Debug.Log("OnStartLifeTimer");
        param = param_;
        // タイムスタンプをとっておく。
        timeStamp = Time.time;
        StartCoroutine("WaitLifeTimeEnd");
    }

    void OnDestroyObject()
    {
        // 念のためコルーチンは切る
        StopAllCoroutines();
    }

    private IEnumerator WaitLifeTimeEnd()
    {
        yield return new WaitForSeconds(param.lifeTime);
        // 寿命まで待ってから消失する
        Disappear();
    }

    // 寿命が来たので自分で自分をDestoryする
    private void Disappear()
    {
        Debug.Log("Disappear");

        GameObject ui = GameObject.Find("/UI");
        if (ui)
        {
            // ロスト通知
            ui.BroadcastMessage("OnLostObject", tag, SendMessageOptions.DontRequireReceiver);
        }
        GameObject parent = gameObject.transform.parent.gameObject;
        if (parent)
        {
            // 親にもロスト通知
            parent.SendMessage("OnLostObject", tag, SendMessageOptions.DontRequireReceiver);
        }

        // 強制削除
        BroadcastMessage("OnInvalidEffect"); // ヒットエフェクトだけ無効化
        BroadcastMessage("OnHit");  // ヒット通知
    }

    /// <summary>
    /// プレイヤーに取得されたら回復
    /// </summary>
    void OnRecovery()
    {
        GameObject ui = GameObject.Find("/UI");
        if (ui)
        {
            // ヒット通知
            ui.BroadcastMessage("OnHitObject", tag, SendMessageOptions.DontRequireReceiver);

            float t = (Time.time - timeStamp) / param.lifeTime;
            // スコア通知
            int score = (int)Mathf.Lerp(param.scoreMax, param.scoreMin, t);
            Debug.Log(t + ":ItemScore=" + score);
            ui.BroadcastMessage("OnAddScore",  score );
            // Airの回復
            int recoveryValue = (int)Mathf.Lerp(param.recoveryMax, param.recoveryMin, t);
            Debug.Log(t + ":ItemAir=" + recoveryValue);
            ui.BroadcastMessage("OnAddAir", recoveryValue);
        }

        GameObject parent = gameObject.transform.parent.gameObject;
        if (parent)
        {
            // 親にもヒット通知
            parent.SendMessage("OnHitObject", tag, SendMessageOptions.DontRequireReceiver);
        }

        // ヒット後の自分の処理
        BroadcastMessage("OnHit");  // ヒット通知
    }

}
