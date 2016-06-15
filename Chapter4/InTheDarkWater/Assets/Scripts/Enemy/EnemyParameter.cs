using UnityEngine;

/// <summary>
/// 敵のパラメータ
/// </summary>
[System.Serializable]
public class EnemyParameter 
{
    public int scoreMax = 1000; // 敵を倒すと取得できるスコア最大値
    public int scoreMin = 100;   // 敵を倒すと取得できるスコア最小値

    public float cautionWaitMax = 1.0f;    // Caution値更新間隔最大値
    public float cautionWaitMin = 0.01f;   // Caution値更新間隔最小値

    public float cautionWaitLimit = 10.0f;  // sneaking時のCautionの更新間隔限界値

    public float sneaking = 0.5f;  // プレイヤーが速度を落とすことに対するCaution値更新間隔への干渉度合い


    public int sonarHitAddCaution = 10;  // 出現時間(この時間を過ぎると自動消失)

    public EnemyParameter(){ }
    public EnemyParameter(EnemyParameter param_)
    {
        scoreMax = param_.scoreMax;
        scoreMin = param_.scoreMin;
        cautionWaitMax = param_.cautionWaitMax;
        cautionWaitMin = param_.cautionWaitMin;
        cautionWaitLimit = param_.cautionWaitLimit;
        sonarHitAddCaution = param_.sonarHitAddCaution;
        sneaking = param_.sneaking;
    }
}
