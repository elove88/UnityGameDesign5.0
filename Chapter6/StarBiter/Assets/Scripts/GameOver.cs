using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// ゲームオーバー時のメッセージ表示処理.
// ----------------------------------------------------------------------------
public class GameOver : MonoBehaviour {
	
	public GameObject MessageMission;
	public GameObject MessageAccomplished;
	public GameObject MessageAccomplishedLine1;
	public GameObject MessageAccomplishedLine2;
	public GameObject MessageAccomplishedLine3;
	public GameObject MessageGameOver;
	public GameObject MessageHiScore;
	
	private bool isHiScore = false;		// HISCOREか?
	private bool defeatedBoss = false;	// BOSSを倒したか?
	private bool isEnable = false;		// 本機能を有効にするか?
	
	void Update () {
		
		if ( isEnable )
		{
			// ミッション達成かゲームオーバーのメッセージ表示.
			if ( defeatedBoss )
			{
				// ミッション達成の文字を表示.
				Instantiate( MessageMission, Vector3.zero, new Quaternion(0f, 0f, 0f, 0f) );
				Instantiate( MessageAccomplished, Vector3.zero, new Quaternion(0f, 0f, 0f, 0f) );
				Instantiate( MessageAccomplishedLine1, Vector3.zero, new Quaternion(0f, 0f, 0f, 0f) );
				Instantiate( MessageAccomplishedLine2, Vector3.zero, new Quaternion(0f, 0f, 0f, 0f) );
				Instantiate( MessageAccomplishedLine3, Vector3.zero, new Quaternion(0f, 0f, 0f, 0f) );
			}
			else
			{
				// ゲームオーバーの文字を表示.
				Instantiate( MessageGameOver, Vector3.zero, new Quaternion(0f, 0f, 0f, 0f) );
			}
			
			// HISCORE表示.
			if ( isHiScore )
			{
				// HISCORE表示処理.
				StartCoroutine( WaitAndPrintHiScoreMessage( 0.5f ) );
			}
			
			// 表示処理は一回のみ.
			isEnable = false;
		}
	}
	
	// ------------------------------------------------------------------------
	// HISCOREかどうかの状態を保持.
	// ------------------------------------------------------------------------
	public void SetIsHiScore( bool isHiScore )
	{
		this.isHiScore = isHiScore;
	}
	
	// ------------------------------------------------------------------------
	// BOSSを倒したかどうかの状態を保持.
	// ------------------------------------------------------------------------
	public void SetDefeatedBoss( bool defeatedBoss )
	{
		this.defeatedBoss = defeatedBoss;
	}
	
	// ------------------------------------------------------------------------
	// メッセージを表示するフラグを立てる.
	// ------------------------------------------------------------------------
	public void Show()
	{
		isEnable = true;
	}
	
	// ------------------------------------------------------------------------
	// HISCORE時のメッセージ表示.
	//  - 指定時間表示タイミングを遅らせる.
	// ------------------------------------------------------------------------
	IEnumerator WaitAndPrintHiScoreMessage( float waitForSeconds )
	{
		// 一定時待つ.
		yield return new WaitForSeconds( waitForSeconds );

		// HISCOREのメッセージを表示.
		Instantiate( MessageHiScore, Vector3.zero, new Quaternion(0f, 0f, 0f, 0f) );
	}
}
