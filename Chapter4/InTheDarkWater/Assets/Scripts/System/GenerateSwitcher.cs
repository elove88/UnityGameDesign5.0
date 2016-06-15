using UnityEngine;
using System.Collections;

/// <summary>
/// 生成物の変更。現状、RandomGeneratorによって生成されるものしか考慮していない
/// </summary>
public class GenerateSwitcher : MonoBehaviour {

    enum Type
    {
        None = 0,
        OnlyOne,
        Switch,
//        Random,
//        All,
    };
    [SerializeField]
    private Type type = Type.None;
    [SerializeField]
    private string currentTag = "Enemey";

    public class TargetGenerator
    {
        public bool clearCondition = false;
        public RandomGenerator gen = null;
    };

    private TargetGenerator current = null;
    private Hashtable generators = new Hashtable();

	void Start () 
    {
    }

    private void Init()
    {
        RandomGenerator[] genArr = GetComponentsInChildren<RandomGenerator>();
        foreach( RandomGenerator gen in genArr ){
            AddGenerator(gen);
        } 
    }

    private void AddGenerator( RandomGenerator generater )
    {
        Debug.Log("AddGenerator");
        if (generater == null) return;

        GameObject target = generater.Target();
        Debug.Log("Target:" + target.tag);
        if (target == null) return;
        string tag = target.tag;

        TargetGenerator targetGenerator = new TargetGenerator();
        targetGenerator.clearCondition = false;
        targetGenerator.gen = generater;
        generators.Add(tag, targetGenerator);
    }
	

    void OnSwitchCheck( string key )
    {
		Debug.Log("OnSwitchCheck:" + key);
        if (currentTag.Equals(key))
        {
            switch (type)
            {
                case Type.Switch: Switch(); break;
                // パターンを変えたい場合は追加
                //case Type.Random: SetRandom(); break;
                default: break;
            }
        }
    }

    private void Run()
    {
        if (!generators.ContainsKey(currentTag))
        {
            Debug.Log(currentTag + ": Not Exist!");
            return;
        }
        current = generators[currentTag] as TargetGenerator;
        current.gen.SendMessage("OnGeneratorStart");
    }

    /// <summary>
    /// スイッチする
    /// </summary>
    private void Switch()
    {
        if (generators.Count == 0) return;

        Suspend();
        //current.gen.SendMessage("OnGeneratorSuspend");

        foreach( string key in generators.Keys )
        {
            if (!currentTag.Equals(key))
            {
                currentTag = key;
                Run();
                return;
            }
        }
    }

    // 中断
    private void Suspend()
    {
        if (current == null) return;
        current.gen.SendMessage("OnGeneratorSuspend");
    }


    // ゲーム開始通知
    void OnGameStart()
    {
        Init();
        Run();
    }
    // ゲームオーバー通知
    void OnGameOver()
    {
        Suspend();
    }
    // ゲームクリア通知
    void OnGameClear()
    {
        Suspend();
    }


    void OnClearCondition(string tag)
    {
        // クリア条件
        bool allClear = true;
        foreach (string key in generators.Keys) 
        {
            // 条件を達成していたタグのTargetObjectをtrueにする
            TargetGenerator target = generators[key] as TargetGenerator;
            if (tag.CompareTo(key) == 0) target.clearCondition = true;
            // 全部クリアできてるかチェック
            allClear &= target.clearCondition;
        }

        if (allClear) {
            // ゲーム終了、次のステージへ
            GameObject adapter = GameObject.Find("/Adapter");
            adapter.SendMessage("OnGameEnd", true);
        }
    }
}
