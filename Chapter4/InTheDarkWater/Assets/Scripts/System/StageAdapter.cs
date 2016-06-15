using UnityEngine;
using System.Collections;

/// <summary>
/// Stageシーン用Adapter。各StageごとのFieldのローダ的役割。
/// ゲームのスタートと終了は必ずここを通り、シーン切替時にRootに終了を伝える
/// </summary>
public class StageAdapter : MonoBehaviour {

    public enum Type
    {
        None = -1,
        Stage1,
        Stage2,
        Stage3
    };
    [SerializeField]
    private Type currentType = Type.None;   // 現在のステージ
    [SerializeField]
    private string[] fieldSceneName = new string[] { 
        "Field1", "Field2", "Field3" 
    };  // 各種メインシーン名
   
    private GameObject root = null;
    private GameObject ui = null;
    private GameObject field = null;

    [SerializeField]
    private bool playOnAwake = true;
    private bool nextStage = false;
    private bool loaded = false;

    void Awake()
    {
        // LoadLevelでDestory対象からはずす
        // Destoryの判断はSceneSelectorが行う
        DontDestroyOnLoad(gameObject);
    }

    // ロード終了時
//    void OnLevelWasLoaded(int level)
//    {
//        Debug.Log("OnLevelWasLoaded : level=" + level + " - " + Application.loadedLevelName);
    // ロード終了時にすると、単体デバッグができないのでStartにしておく
    void Start()
    {
        root = GameObject.Find("/Root");
        ui = GameObject.Find("/UI");
        // FieldがまだロードされてないのでIntermissionを飛ばして最初のステージをロード
        if (playOnAwake)
        {
            SetNextStage(Type.Stage1);
            OnIntermissionEnd();
        }
    }

    // 各Fieldから呼ばれる
    void OnLoadedField()
    {
        loaded = true;
        // FieldまでLoadできたらインターミッション開始
        Debug.Log("Field Loaded");
        field = GameObject.Find("/Field");

        // ソナーの位置合わせ等。SlideOutする前に設定しておきたいこと
        if (ui) ui.BroadcastMessage("OnAwakeStage", (int)currentType);

        if (root) root.BroadcastMessage("OnSlideOut", gameObject);
        else OnIntermissionEnd();
    }

    // インターミッションの終了受け取り
    void OnIntermissionEnd()
    {
        if (loaded) GameStart();
        else Load();
    }

    // シーン終了時に呼ばれる
    void OnGameEnd(bool nextStage_)
    {
        nextStage = nextStage_;
        if (nextStage) {
            // ステージクリアの挙動指示
            Debug.Log("!!! GameClear !!!");
            if (field) field.BroadcastMessage("OnGameClear", SendMessageOptions.DontRequireReceiver);
            if (ui) ui.BroadcastMessage("OnGameClear", SendMessageOptions.DontRequireReceiver);
        }
		else {
	        // ゲームオーバーの挙動指示
            Debug.Log("!!! GameOver !!!");
            if (field) field.BroadcastMessage("OnGameOver", SendMessageOptions.DontRequireReceiver);
            if (ui) ui.BroadcastMessage("OnGameOver", SendMessageOptions.DontRequireReceiver);
		}
    }

    // TitleSwitcherから必ず呼ばれる
    void OnSceneEnd()
    {
        if (nextStage)
        {
            // 次のStageに切り替え
            SetNextStage();
            // インターミッション開始
            if (root) root.BroadcastMessage("OnSlideIn", gameObject);
            else OnIntermissionEnd();
        }
        else
        {
            // Titleに戻す
            if (root) root.SendMessage("OnStartTitle");
        }
    }
    
    // 主にデバッグ用
    void OnFieldLoad( Type type )
    {
        SetNextStage(type);
        if (root) root.BroadcastMessage("OnSlideIn", gameObject);
        else OnIntermissionEnd();
    }


    // ここからゲームスタート
    private void GameStart()
    {
        Debug.Log("!!! GameStart !!!");
        // ゲーム開始
        if (field) field.BroadcastMessage("OnGameStart", SendMessageOptions.DontRequireReceiver);
        if (ui) ui.BroadcastMessage("OnGameStart", SendMessageOptions.DontRequireReceiver);
    }
    // ロード
    private bool Load()
    {
        if (currentType == Type.None) return false;

        // fieldは削除しておく
        if (field) GameObject.Destroy(field);
        // UIをリセット
        if (ui) ui.BroadcastMessage("OnStageReset", SendMessageOptions.DontRequireReceiver);

        string name = fieldSceneName[(int)currentType];
        Debug.Log("Load : " + name);
        Application.LoadLevelAdditive(name);
        return true;
    }

    private void SetNextStage(Type setType = Type.None)
    {
        loaded = false;

        // currentTypeを設定する
        if (setType == Type.None)
        {
            int current = (int)currentType;
            current++;
            // ステージ数をオーバーしていた場合はTitleへ戻す
            if (current >= fieldSceneName.Length)
            {
                // Titleに戻す
                if (root) root.SendMessage("OnStartTitle");
                return;
            }
            else currentType = (Type)(current);
        }
        else currentType = setType;
    }

}
