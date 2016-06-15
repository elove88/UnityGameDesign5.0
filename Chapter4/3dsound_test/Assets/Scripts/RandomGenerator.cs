using UnityEngine;
using System.Collections;

public class RandomGenerator : MonoBehaviour {

    [SerializeField]
    private GameObject target;  // 生成対象
    [SerializeField]
    private GenerateParameter param = new GenerateParameter();
    [SerializeField]
    private float posY = 1.0f;

    private int counter = 0;
    private bool limitCheck = false;
    private bool ready = false;

    private ArrayList childrenArray = new ArrayList();
//    private ArrayList sonarArray = new ArrayList();

    private GameObject field = null;
   
    void Start()
    {
        // 初期配置分がある場合はここで登録しておく（主にデバッグ用）
        GameObject[] children = GameObject.FindGameObjectsWithTag(target.tag);
        for (int i = 0; i < children.Length; i++ )
        {
            childrenArray.Add(children[i]);
//            sonarArray.Add(children[i]);
        }

        //field = GameObject.Find("/Field");
        OnGeneratorStart();
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
//        sonarArray.Remove(target);
        // 子供が減った通知
        SendMessage("OnDestroyChild", target, SendMessageOptions.DontRequireReceiver);
//        if (field) field.SendMessage("OnSwitchCheck", target.tag);
        Destroy(target);
    }

    // オブジェクト生成
    public void Generate()
    {
        Rect rect = param.posXZ;
        Vector3 pos = new Vector3(rect.xMin, posY, rect.yMin);
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

        // インスタンス生成
        GameObject newChild = Object.Instantiate(target, pos, Quaternion.identity) as GameObject;
        // 自分を親にする
        newChild.transform.parent = transform;
        Debug.Log("generated[" + ChildrenNum() + "]=" + newChild.name);

        // 配列更新
        childrenArray.Add(newChild);
//        sonarArray.Add(newChild);
        // 子供を増やした通知
        SendMessage("OnInstantiatedChild", newChild, SendMessageOptions.DontRequireReceiver);

        counter++;
        if (counter >= param.limitNum)
        {
            limitCheck = true;  // 一度リミットに到達したらチェックを入れる
        }
    }

    /*
    // 手抜き収集
    private void UpdateArray()
    {
        childrenArray = GameObject.FindGameObjectsWithTag(target.tag);
        // OnUpdateArrayがあれば通知
        SendMessage("OnUpdateArray", childrenArray, SendMessageOptions.DontRequireReceiver);
    }
    */

    public int ChildrenNum()
    {
        if (childrenArray != null) return childrenArray.Count;
        return 0;
    }
    public GameObject Target() { return target; }

    // 管理している子の参照
    public ArrayList Children() { return childrenArray; }
    // ソナーにあたった分をとっておく
//    public ArrayList SonarChildren() { return sonarArray; }
    // 生成パラメータセット
    public void SetParam(GenerateParameter param_) {  param = param_; }

}
