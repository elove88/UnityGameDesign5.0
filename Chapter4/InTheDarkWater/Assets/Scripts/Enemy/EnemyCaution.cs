using UnityEngine;
using System.Collections;

/// <summary>
/// 滴によるCaution値の設定。
/// </summary>
public class EnemyCaution : MonoBehaviour {

    [SerializeField]
    private float waitForce = 0.02f;

    [SerializeField]
    private int step = 1;

    [SerializeField]   // Debug閲覧用
    private EnemyParameter param = null;
    
    [SerializeField]    // Debug閲覧用
    private int cautionValue = 0;

    private int currentStep = 1;

    [SerializeField]    // Debug閲覧用
    private float waitTime = 1.0f;

    private float count = 0.0f;

    private bool counting = false;
    private bool emergency = false;
    private bool countup = true;
    private CautionUpdater updater = null;
    private PlayerController controller = null;

	void Start () 
    {
        GameObject enemy = GameObject.Find("/Field/Enemies");
        if (enemy) updater = enemy.GetComponent<CautionUpdater>();
        GameObject player = GameObject.Find("/Field/Player");
        if (player) controller = player.GetComponent<PlayerController>();
	}

    void Update()
    {
        if (param == null || !counting) return;

        count += Time.deltaTime;
        if (count >= waitTime) {
            count = 0.0f;
            counting = UpdateCaution();
        }
    }

    void OnStayPlayer( float distRate )
    {
        if (param==null) return;

        if (countup)
        {
            // Playerとの距離が近いほど、Caution値は上昇しやすい
            if (!emergency)
            {
                waitTime = Mathf.Lerp(param.cautionWaitMin, param.cautionWaitMax, distRate);
                // Playerの速度が遅いほど、Caution値が上昇しづらい
                float speedRate = controller.SpeedRate();
                // 通常はwaitTimeをLerpさせる
                float sneakingRate = (1.0f - speedRate) * param.sneaking;
                waitTime = Mathf.Lerp(waitTime, param.cautionWaitLimit, sneakingRate);
            }
        }
        else 
        {
            // カウントダウンしてる場合はアップに切り替え
            StartCount(true);
        }
    }

      // 離れた時に何かするならここ
    void OnExitPlayer()
    {
        if (param == null || counting) return;
        // カウントしてないで離れる
        waitTime = waitForce;
        emergency = true;
        StartCount(false);
    }

    void OnStartCautionCount(EnemyParameter param_)
    {
        // パラメータセットと、カウンタ開始
        Debug.Log("OnStartCautionCount");
        param = param_;
        waitTime = param.cautionWaitMax;
        StartCount(true);
    }

    void OnAddScore()
    {
        if (param == null) return;

        // スコア値を送る
        GameObject ui = GameObject.Find("/UI");
        if (ui)
        {
            // オブジェクトのヒット
            ui.BroadcastMessage("OnHitObject", tag, SendMessageOptions.DontRequireReceiver);
            // 得点追加
            float time = 1.0f - Mathf.InverseLerp(0, 100, cautionValue);
            int scoreValue = (int)Mathf.Lerp(param.scoreMin, param.scoreMax, time);
            ui.BroadcastMessage("OnAddScore", scoreValue);
        }
        GameObject parent = gameObject.transform.parent.gameObject;
        if (parent)
        {
            // 親にもヒット通知
            parent.SendMessage("OnHitObject", tag, SendMessageOptions.DontRequireReceiver);
        }
        // 自分にヒット判定
        BroadcastMessage("OnHit");
    }

    void OnActiveSonar()
    {
        if (param == null) return;
        Debug.Log("EnemyCaution.OnActiveSonar");
        // ソナーがヒットするたびに、Cautionが上昇
        cautionValue = Mathf.Clamp(cautionValue + param.sonarHitAddCaution, 0, 100);
        updater.DisplayValue(gameObject, cautionValue);
    }

    private void StartCount(bool countup_)
    {
        count = 0;
        counting = true;
        countup = countup_;
        currentStep = (countup) ? step : (-step);
    }

    private bool UpdateCaution()
    {
        cautionValue = Mathf.Clamp(cautionValue + currentStep, 0, 100);
        // 表示更新
        updater.DisplayValue(gameObject, cautionValue);
        // 条件チェック
        if (cautionValue >= 100)
        {
            // Playerを発見 
            SendMessage("OnEmergency");
            return false;
        }
        else if (cautionValue <= 0)
        {
            // Playerを見失う
            SendMessage("OnUsual");
            emergency = false;
            StartCount(true);   // カウントしなおし
        }

        return true;
    }

    void OnEmergency()
    {
        emergency = true;
        if (cautionValue < 100)
        {
            // まだcuation値が満たされていない場合
            waitTime = waitForce;
        }
    }

    public int Value(){ return cautionValue; }
}
