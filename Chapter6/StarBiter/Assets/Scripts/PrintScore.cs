using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// SCOREの表示.
// ----------------------------------------------------------------------------
public class PrintScore : MonoBehaviour {
	
	private int score = 0;
	private GUIText textScore;
	
	void Start () 
	{
		// scoreのインスタンスを取得.
		textScore = GetComponent<GUIText>();
		
		// 初期値表示.
		textScore.text = score.ToString();
	}
	
	// ------------------------------------------------------------------------
	// SCOREに加算.
	// ------------------------------------------------------------------------
	public void AddScore( int score )
	{
		// 加算
		this.score += score;
		
		// 表示
		textScore.text = this.score.ToString();
	}
	
	// ------------------------------------------------------------------------
	// SCOREを返す.
	// ------------------------------------------------------------------------
	public int GetScore()
	{
		return score;
	}
		
}
