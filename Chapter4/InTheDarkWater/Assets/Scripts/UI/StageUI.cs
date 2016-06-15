using UnityEngine;
using System.Collections;

/// <summary>
/// Stageシーン用
/// </summary>
public class StageUI : MonoBehaviour {


	void Awake()
	{
		// LoadLevelでDestory対象からはずす
        // Destoryの判断はSceneSelectorが行う
        DontDestroyOnLoad(gameObject);
	}

    // スコアを返す
    public int Score()
    {
        GameObject scoreDisp = GameObject.Find("ScoreDisplay");
        if (scoreDisp)
        {
            ScoreDisplay disp = scoreDisp.GetComponent<ScoreDisplay>();
            if (disp) return disp.Score();
        }
        return 0;
    }
}
