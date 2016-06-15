using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// HISCOREの表示.
// ----------------------------------------------------------------------------
public class PrintHiScore : MonoBehaviour {

	private int hiScore = 0;
	private GUIText textHiScore;
	
	void Start () 
	{
		// hi-scoreのインスタンスを取得.
		textHiScore = GetComponent<GUIText>();
		
		// グローバルパラメーターからhi-scoreを取得.
		hiScore = GlobalParam.GetInstance().GetHiScore();		
		
		// 初期値表示.
		textHiScore.text = hiScore.ToString();
	}
	
	// ------------------------------------------------------------------------
	// HISCOREの設定.
	// ------------------------------------------------------------------------
	public void SetHiScore( int hiScore )
	{
		// 保持
		this.hiScore = hiScore;
	
		// 表示
		textHiScore.text = this.hiScore.ToString();
	}

}
