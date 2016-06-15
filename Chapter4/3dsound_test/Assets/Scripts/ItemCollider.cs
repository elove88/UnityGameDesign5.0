using UnityEngine;
using System.Collections;

/// <summary>
/// アイテムの接触判定
/// </summary>
public class ItemCollider : MonoBehaviour
{

    // [SerializeField]
    // private int type;

    private bool isFinished;

    void Start()
    {
        isFinished = false;
        GetComponent<Collider>().isTrigger = true;  // トリガーをたてておく
    }

    void OnTriggerEnter(Collider collider)
    {
        if (isFinished) return; // 1回だけ衝突をみたいのでその監視用。
                                // isTrigge=falseしても複数回とってしまう。
        GameObject obj = collider.gameObject;
        if (obj.tag.Equals("Player"))   // プレイヤーか判定
        {
            isFinished = true;
            // HitItem通知
            //obj.SendMessage("OnHitItem");
            GameObject ui = GameObject.Find("/UI");
            if (ui) ui.SendMessage("OnHitItem", gameObject.name);
            Note note = GetComponent<Note>();
            if (note) note.SendMessage("OnHitItem");
        }
    }
}
