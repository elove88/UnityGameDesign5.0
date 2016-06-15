using UnityEngine;
using System.Collections;

/// <summary>
/// シーンを切り替える
/// </summary>
public class SceneSelector : MonoBehaviour {

    public enum Type {
        None = -1,
        Title = 0,
        Stage
    };
    [SerializeField]
    private string titleSceneName = "Title";
    [SerializeField]
    private string stageSceneName = "Stage";
    [SerializeField]
    private Type type = Type.None;   // 現在のステージ

    [SerializeField]
    private bool playOnAwake = true;    // 即時スタートするか（Release時はOnにすること）

    [SerializeField]    // debug
    private int highScore = 0;        // ハイスコアの記録

	void Awake()
	{
		// LoadLevelでDestoryされる対象からはずす
        DontDestroyOnLoad(gameObject);
	}
	
    void Start() 
    {
        // 即時Titleロード(ロードの関係から念のため1.0f間を置いてから)
        if (playOnAwake) StartCoroutine("Wait", 1.0f);
    }
    private IEnumerator Wait(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        LoadScene();
    }

    private bool LoadScene()
    {
        switch (type)
        { 
            default:    return false;
            case Type.Title:
                GameObject ui = GameObject.Find("/UI");
                if(ui) {
                    // ハイスコア更新
                    UpdateHiScore( ui );
                    // UI強制削除
                    Destroy(ui);
                    ui = null;
                }
                // 参照を保存
                GameObject adapter = GameObject.Find("/Adapter");
                if (adapter)
                {
                    // Adapterも強制削除
                    Destroy(adapter);
                    adapter = null;
                }
                Application.LoadLevel(titleSceneName);
                break;
            case Type.Stage:
                // TitleはDontDestory指定してないのでLoadLevelでOK
                Application.LoadLevel(stageSceneName);
                break;
        }
        
        return true;
    }

    private void UpdateHiScore( GameObject ui )
    {
        StageUI stageUI = ui.GetComponent<StageUI>();
        int newScore = 0;
        if (stageUI) newScore = stageUI.Score();
        if (highScore < newScore) highScore = newScore;
    }

    // タイトルをロード
    void OnStartTitle()
    {
        // タイトルを設定
        type = Type.Title;
        // インターミッション開始
        BroadcastMessage("OnFadeIn", gameObject);
    }

    // 次のステージをロード
    void OnStartStage( )
    {
        // ステージを設定
        type = Type.Stage;
        // インターミッション開始
        BroadcastMessage("OnFadeIn", gameObject);
    }

    // インターミッションの終了受け取り
    void OnIntermissionEnd()
    {
        // ロードできてないならロード開始
        LoadScene(); 
    }

    public int HighScore()
    {
        return highScore;
    }
}
