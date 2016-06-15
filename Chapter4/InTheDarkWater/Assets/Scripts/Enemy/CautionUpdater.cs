using UnityEngine;
using System.Collections;

/// <summary>
/// 常に最新の最大値Cautionを表示するためのスクリプト
/// </summary>
public class CautionUpdater : MonoBehaviour
{
    [SerializeField] // debug
    private int instantiatedCount = 0;

    private GameObject ui = null;
    private GameObject maxCautionEnemy = null;

    void Start()
    {
    }

    void OnGameStart()
    {
        // シーンを分けたので、念のためOnGameStartでつなげる
        ui = GameObject.Find("/UI");
    }

    void OnInstantiatedChild(GameObject target)
    {
        instantiatedCount++;
        //EnemyCaution enemyCaution = target.GetComponent<EnemyCaution>();
        //float waitTime = 0.0f;
        //if (instantiatedCount>0) waitTime = maxWaitTime / (float)instantiatedCount;
        //enemyCaution.SetCountUp(waitTime);

        // 通常ゼロになっているはずだが、念のためUpdate
        DisplayValue(target, GetCautionValue(target));
    }

    void OnDestroyChild(GameObject target)
    {
        if (target.Equals(maxCautionEnemy))
        {
            maxCautionEnemy = null;
            if(ui)ui.BroadcastMessage("OnUpdateCaution", 0, SendMessageOptions.DontRequireReceiver);
        }
    }

    public void DisplayValue(GameObject updateEnemy, int newValue)
    {
        //Debug.Log(updateEnemy.name + ".cation=" + newValue);
        int maxValue = 0;
        if (!updateEnemy.Equals(maxCautionEnemy))
        {
            // 同一でないなら現状のMax値を持つ敵の現在の値と比較
            maxValue = GetCautionValue(maxCautionEnemy);
            if (newValue > maxValue)
            {
                maxValue = newValue;
                maxCautionEnemy = updateEnemy;
            }
        }
        else
        {
            // 同一ならそのまま更新
            maxValue = newValue;
        }
        // 最大値を表示用に通知
        if(ui)ui.BroadcastMessage("OnUpdateCaution", maxValue, SendMessageOptions.DontRequireReceiver);
    }

    private int GetCautionValue(GameObject enemyObj)
    {
        if(enemyObj == null ) return 0;
        EnemyCaution enemyCauiton = enemyObj.GetComponent<EnemyCaution>();
        if (enemyCauiton) return enemyCauiton.Value();
        return 0;
    }
}
