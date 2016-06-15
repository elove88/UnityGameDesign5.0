using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// ゲーム内情報保持(GlobalParamはUnity実行開始後に保持し続ける領域).
//  - HISCOREを保持する/返す.
// ----------------------------------------------------------------------------
public class GlobalParam : MonoBehaviour {
	
	private int hiScore = 0;
	private static GlobalParam instance = null;
	
	// GlobalParamは一度だけ作成してインスタンスを返す.
	// (作成後は作成済みのインスタンスを返す).
	public static GlobalParam GetInstance()
	{
		if ( instance == null )
		{
			GameObject globalParam = new GameObject("GlobalParam");
			instance = globalParam.AddComponent<GlobalParam>();
			DontDestroyOnLoad( globalParam );
		}
		return instance;
	}
	
	// HISCOREを返す.
	public int GetHiScore()
	{
		return hiScore;
	}
	
	// HISCOREを保持する.
	public void SetHiScore( int hiScore )
	{
		this.hiScore = hiScore;
	}
}
