using UnityEngine;
using System.Collections;

/// <summary>
/// 残りの空気の残量表示
/// </summary>
public class Airgage : MonoBehaviour {

    [SerializeField]
    private float offsetGageSize = 120.0f;  // ゼロで画面端
    [SerializeField]
    private Vector2 offsetPixelGage = Vector2.zero;  // ゼロで画面端
    [SerializeField]
    private Vector2 offsetPixelText = Vector2.zero;  // ゼロで画面端
    
    [SerializeField]
    private float[] airUpdateTime = new float[] {
        8.0f, 5.0f, 3.0f, 2.0f, 1.0f, 0.5f
    };  // 酸素が減る更新頻度

    [SerializeField]
    private float airMax = 1000.0f;     // airの最大値
    [SerializeField]
    private float step = 1.0f;          // 一度の更新に減る量

    [SerializeField]    // debug
    private float air = 0;              // 現在のair値

    private int damageLv = 0;           // ダメージレベル
    private float counter = 0;

    private bool gameover = false;

    private GameObject meterObj;
    private GameObject damageLvObj;


    void Start()
    {
        meterObj = GameObject.Find("AirgageMeter");
        damageLvObj = GameObject.Find("DamageLvText");

        // 位置調整
        float w = (float)Screen.width;
        float h = (float)Screen.height;

        float aspect = w / h;
        offsetPixelGage.x += offsetGageSize;
        offsetPixelGage.y += offsetGageSize;
        float posX = aspect * (1.0f - offsetPixelGage.x / w);
        float posY = 1.0f - offsetPixelGage.y / h;
        meterObj.transform.position = new Vector3(posX, posY, 0.0f);

        posX = 1.0f - offsetPixelText.x / w;
        posY = 1.0f - offsetPixelText.y / h;
        damageLvObj.transform.position = new Vector3(posX, posY, 0.0f);

        OnStageReset();
    }

    void Update()
    {
        // カウンタによる更新
        if (!gameover)
        {
            counter += Time.deltaTime;
            if (counter > airUpdateTime[damageLv])
            {
                Deflate();
                counter = 0;
            }
        }
    }

    /// <summary>
    /// [BroadcastMessage]
    /// ダメージを受けた
    /// </summary>
    /// <param name="value">ダメージ量。通常1</param>
    void OnDamage(int value)
    {
        // ダメージレベル加算
        damageLv += value;
        UpdateDamageLv();
    }

    void OnAddAir(int value )
    {
        air += value;
        if (airMax < air) air = airMax;
    }

    void OnStageReset()
    {
        air = airMax;
        damageLv = 0;
        UpdateAirgage();
        UpdateDamageLv();
        gameover = false;
    }

    /// <summary>
    /// air更新
    /// </summary>
    private void Deflate()
    {
        // 値更新
        air -= step;
        if( air <= 0.0f ) {
            air = 0.0f;
            gameover = true;
        }
        // メーターに値を伝える
        UpdateAirgage();

        if (gameover)
        {
            // 酸素切れ。ゲームオーバー(falseをわたす)
            GameObject adapter = GameObject.Find("/Adapter");
            adapter.SendMessage("OnGameEnd", false);
        }
    }

    private void UpdateDamageLv()
    {
        damageLv = Mathf.Clamp(damageLv, 0, airUpdateTime.Length - 1);
        BroadcastMessage("OnDisplayDamageLv", damageLv);
    }
    private void UpdateAirgage()
    {
        float threshold = Mathf.InverseLerp(0, airMax, air);
        meterObj.SendMessage("OnDisplayAirgage", threshold);
    }

    public float Air() { return air; }
    public int DamageLevel() { return damageLv; }
}
