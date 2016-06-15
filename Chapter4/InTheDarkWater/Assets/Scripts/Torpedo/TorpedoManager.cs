using UnityEngine;
using System.Collections;

/// <summary>
/// 魚雷の全般管理。魚雷の自動削除。
/// </summary>
public class TorpedoManager : MonoBehaviour {

    [SerializeField]
    private bool check = true;
    [SerializeField]
    private Rect runningArea = new Rect(-950, -950, 1900, 1900);   // 有効範囲（ワールド座標）
    [SerializeField]
    private bool relative = false;
    [SerializeField]
    private float delayTime = 2.0f;

    private Rect rect;

    private ArrayList childrenArray = new ArrayList();
    private ArrayList sonarArray = new ArrayList();

    void Start()
    {
        // スタートしておく
        if (check) StartCoroutine("CheckDelay");
    }

    /// <summary>
    /// インスタンス生成タイミング
    /// </summary>
    /// <param name="target">生成されたインスタンス</param>
    void OnInstantiatedChild(GameObject target)
    {
        childrenArray.Add(target);
        sonarArray.Add(target);
    }
    /// <summary>
    /// インスタンス削除タイミング
    /// </summary>
    /// <param name="target">削除対象</param>
    void OnDestroyChild(GameObject target)
    {
        // リストに入っていれば削除しておく
        Debug.Log("TorpedManager.OnDestroyChild");
        childrenArray.Remove(target);
        sonarArray.Remove(target);

        Destroy(target);
    }

    /// <summary>
    /// ゲームオーバー時
    /// </summary>
    void OnGameOver()
    {
        StopAllCoroutines();
    }
    /// <summary>
    /// ゲームクリア時
    /// </summary>
    void OnGameClear()
    {
        StopAllCoroutines();
    }

    /// <summary>
    /// 魚雷が有効領域外に出ていないか定期的にチェックする
    /// </summary>
    /// <returns></returns>
    private IEnumerator CheckDelay()
    {
        yield return new WaitForSeconds(delayTime);

        if (relative)
        {
            GameObject player = GameObject.Find("/Field/Player");
            if (player) {
                Vector3 pos = player.transform.position;
                rect = new Rect(runningArea.xMin + pos.x, runningArea.yMin + pos.z, runningArea.width, runningArea.height);
            }
        }
        else rect = new Rect(runningArea);

        int i = 0;
        while (i < childrenArray.Count)
        {
            GameObject target = childrenArray[i] as GameObject;
            if (target == null)
            {
                i++;
                continue;
            }

            Vector3 pos = target.transform.position;
            if (rect.Contains(new Vector2(pos.x, pos.z)))
            {
                i++;
            }
            else
            {
                childrenArray.RemoveAt(i);  // 対象を削除
                sonarArray.Remove(target);  // もしsonarにも残ってたら削除
                Destroy(target);
            }
        }

        // 次のDelay
        StartCoroutine("CheckDelay");
    }

    public int ChildrenNum()
    {
        if (childrenArray != null) return childrenArray.Count;
        return 0;
    }

    // 管理している子の参照
    public ArrayList Children() { return childrenArray; }
    // ソナーにあたった分をとっておく
    public ArrayList SonarChildren() { return sonarArray; }
}
