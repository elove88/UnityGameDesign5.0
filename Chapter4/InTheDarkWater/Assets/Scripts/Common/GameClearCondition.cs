using UnityEngine;
using System.Collections;

/// <summary>
/// クリア条件
/// </summary>
public class GameClearCondition : MonoBehaviour
{
    [SerializeField]
    private bool valid = false;
    [SerializeField]
    private int destoryNorma = 0;
    [SerializeField]
    private int hitNorma = 0;

    private GameObject field = null;


    void Start()
    {
        field = GameObject.Find("/Field");
    }

    /// <summary>
    /// 生成したタイミング
    /// </summary>
    /// <param name="target"></param>
    void OnInstantiatedChild(GameObject target)
    {
        // 1つでも生成されば許可
    }

    /// <summary>
    /// 破棄されるタイミング
    /// </summary>
    /// <param name="target"></param>
    void OnDestroyObject(GameObject target)
    {
        if (!valid) return;
        if (destoryNorma == 0) return;

        // 
        destoryNorma--;
        if (destoryNorma <= 0) Clear(target.tag);
    }

    /// <summary>
    /// ヒットタイミング
    /// </summary>
    /// <param name="tag"></param>
    void OnHitObject(string tag)
    {
        if (!valid) return;
        if (hitNorma == 0) return;

        // ヒットしたタイミング
        hitNorma--;
        if (hitNorma <= 0) Clear(tag);
    }

    /// <summary>
    /// ロストしたタイミング
    /// </summary>
    /// <param name="tag"></param>
    void OnLostObject(string tag)
    {
    }

    private void Clear( string tag )
    {
        if (field) field.SendMessage("OnClearCondition", tag);
        valid = false;
    }
}
