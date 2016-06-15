/// <summary>
/// アイテムのパラメータ
/// </summary>
[System.Serializable]
public class ItemParameter 
{
    public int scoreMax = 1000; // スコア最大値
    public int scoreMin = 100;   // スコア最小値

    public int recoveryMax = 100;    // 回復量最大値
    public int recoveryMin = 10;     // 回復量最小値

    public float lifeTime = 60.0f;  // 出現時間(この時間を過ぎると自動消失)


    public ItemParameter() { }
    public ItemParameter(ItemParameter param_)
    {
        scoreMax = param_.scoreMax;
        scoreMin = param_.scoreMin;  
        recoveryMax = param_.recoveryMax;   
        recoveryMin = param_.recoveryMin;
        lifeTime = param_.lifeTime;  

    }

}
