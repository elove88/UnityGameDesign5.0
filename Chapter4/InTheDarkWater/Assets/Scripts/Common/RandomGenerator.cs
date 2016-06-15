using UnityEngine;
using System.Collections;

/// <summary>
/// ランダムな位置にインスタンスを生成
/// </summary>
public class RandomGenerator : MonoBehaviour {

    [SerializeField]
    private GameObject target = null;  // 生成対象
    [SerializeField]
    private GenerateParameter param = new GenerateParameter();
    [SerializeField]
    private bool relative = false;
    [SerializeField]
    private Rect runningArea = new Rect(-700.0f, -700.0f, 1400.0f, 1400.0f);

    private int counter = 0;

    private bool clear = false;
    private bool limitCheck = false;
    private bool ready = false;

    private ArrayList childrenArray = new ArrayList();
    private ArrayList sonarArray = new ArrayList();

    private GameObject field = null;
    private GameObject player = null;
   
    void Start()
    {
        // 初期配置分がある場合はここで登録しておく（主にデバッグ用）
        GameObject[] children = GameObject.FindGameObjectsWithTag(target.tag);
        for (int i = 0; i < children.Length; i++ )
        {
            childrenArray.Add(children[i]);
            sonarArray.Add(children[i]);
        }

        field = GameObject.Find("/Field");
        player = GameObject.Find("/Field/Player");
    }

    void Update()
    {
        if (TimingCheck())
        {
            ready = false;
            StartCoroutine("Delay");
//            Generate();
        }
    }

    private bool TimingCheck()
    {
        // 準備できてない
        if (!ready) return false;
        // 1度リミットに到達していて、エンドレスフラグが立っていないときは追加しない
        if (!param.endless && limitCheck) return false;
        // 個数チェック
        return (ChildrenNum() < param.limitNum) ? true : false;
    }

    private IEnumerator Delay()
    {
        yield return new WaitForSeconds(param.delayTime);

        Generate();
        ready = true;
    }

    void OnGeneratorStart()
    {
        counter = 0;
        ready = true;
        limitCheck = false;
    }
    void OnGeneratorSuspend()
    {
        ready = false;
    }
    void OnGeneratorResume()
    {
        ready = true;
    }

    /// <summary>
    /// [ Message ] オブジェクト破壊
    /// </summary>
    void OnDestroyObject( GameObject target )
    {
        // 配列に残っていれば削除
        childrenArray.Remove(target);
        sonarArray.Remove(target);
        // 子供が減った通知
        SendMessage("OnDestroyChild", target, SendMessageOptions.DontRequireReceiver);
        if (field) field.SendMessage("OnSwitchCheck", target.tag);
        Destroy(target);
    }

    // オブジェクト生成
    public void Generate()
    {
        Rect rect = param.posXZ;
        float offsetX = 0.0f;
        float offsetZ = 0.0f;
        if (relative)
        {
            offsetX = player.transform.position.x;
            offsetZ = player.transform.position.z;
        }

        Vector3 pos = new Vector3(rect.xMin + offsetX, 0, rect.yMin + offsetZ);
        if (param.fill)
        {
            // posRange内にランダムに位置を決める
            pos.x += rect.width * Random.value;
            pos.z += rect.height * Random.value;
        }
        else {
            // posRange外周上にランダムに位置を決める
            if (Random.Range(0, 2) == 1)
            {
                pos.x += rect.width * Random.value;
                if (Random.Range(0, 2) == 1) pos.z = rect.yMax;
            }
            else
            {
                if (Random.Range(0, 2) == 1) pos.x = rect.xMax;
                pos.z += rect.height * Random.value;
            }
        }

        // 範囲外だったらClampして補正
        pos.x = Mathf.Clamp(pos.x, runningArea.xMin, runningArea.xMax);
        pos.z = Mathf.Clamp(pos.z, runningArea.yMin, runningArea.yMax);

        // インスタンス生成
        GameObject newChild = Object.Instantiate(target, pos, Quaternion.identity) as GameObject;
        // 自分を親にする
        newChild.transform.parent = transform;
        Debug.Log("generated[" + ChildrenNum() + "]=" + newChild.name);

        // 配列更新
        childrenArray.Add(newChild);
        sonarArray.Add(newChild);

        // 子供を増やした通知
        SendMessage("OnInstantiatedChild", newChild, SendMessageOptions.DontRequireReceiver);

        counter++;
        if (counter >= param.limitNum)
        {
            limitCheck = true;  // 一度リミットに到達したらチェックを入れる
        }
    }

    public int ChildrenNum()
    {
        if (childrenArray != null) return childrenArray.Count;
        return 0;
    }

    public GameObject Target() { return target; }
    public bool Clear() { return clear; }

    // 管理している子の参照
    public ArrayList Children() { return childrenArray; }
    // ソナーにあたった分をとっておく
    public ArrayList SonarChildren() { return sonarArray; }
    // 生成パラメータセット
    public void SetParam(GenerateParameter param_) {  param = param_; }

}
