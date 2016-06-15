using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// 星を動かす.
//  - 仕様.
//    - 星が書かれた一枚絵を3枚重ね、それぞれ指定したスピードでプレイヤーの向きと反対の方向に動かす.
//  - 利用方法.
//    - 本スクリプトを任意のゲームオブジェクトに設定する.
//  - 事前準備.
//    - 星のテクスチャを貼り付けた平面(以下、星板)を用意.
//    - 星板のテクスチャのTilingをx,y両方とも3にする.
//    - 星板に Star1,Star2,Star3 をそれぞれタグ付け.
//    - 星板をX軸方向に動かして移動前と同じ絵になる位置を探し、その値を maxRightPositionX,maxLeftPositionX に設定.
//    - 星板をZ軸方向に動かして移動前と同じ絵になる位置を探し、その値を maxTopPositionZ,maxBottomPositionZ に設定.
//  - 備考.
//    - 永続的に星を動かす為、指定した座標で星板の位置を戻している.
//    - スムーズに切り替える為に、切り替え座標は星板を実際に動かして確認するところがポイント.
// ----------------------------------------------------------------------------
public class StarController : MonoBehaviour {

	public float scrollSpeedStar1 = 0.2f;					// 星(Star1)がスクロールするスピード.
	public float scrollSpeedStar2 = 0.5f;					// 星(Star2)がスクロールするスピード.
	public float scrollSpeedStar3 = 1f;						// 星(Star3)がスクロールするスピード.
	
	private GameObject player;								// プレイヤーのインスタンス.
	
	const int MAX_STARS = 3;								// 星板の数.
	private GameObject[] stars = new GameObject[MAX_STARS];	// 星板のインスタンス.
	private float[] scrollSpeed = new float[MAX_STARS];		// 星板のスクロールスピード.
	
	private float maxRightPositionX = -10f;					// 星板の切り替え位置X.
	private float maxLeftPositionX = 10f;					// 星板の切り替え位置X.
	private float maxTopPositionZ = -10f;					// 星板の切り替え位置Z.
	private float maxBottomPositionZ = 10f;					// 星板の切り替え位置Z.
	
	void Start ()
	{
		// プレイヤーのインスタンスを取得.
		player = GameObject.FindGameObjectWithTag("Player");
		
		// 星板のインスタンスを取得.
		GameObject star1 = GameObject.FindGameObjectWithTag("Star1");
		GameObject star2 = GameObject.FindGameObjectWithTag("Star2");
		GameObject star3 = GameObject.FindGameObjectWithTag("Star3");
		
		// まとめて処理しやすいように、配列にまとめる.
		stars[0] = star1;
		stars[1] = star2;
		stars[2] = star3;
		scrollSpeed[0] = scrollSpeedStar1;
		scrollSpeed[1] = scrollSpeedStar2;
		scrollSpeed[2] = scrollSpeedStar3;
	}
	
	// プレイヤーの進行方向が確定した後に処理する.
	void LateUpdate() 
	{
		// 星をスクロール(プレイヤーの進行方向の逆方向に星を動かす).
		ScrollStars();
	}
	
	// ------------------------------------------------------------------------
	// 星をスクロール(プレイヤーの進行方向の逆方向に星を動かす).
	// ------------------------------------------------------------------------
	private void ScrollStars()
	{
		// Playerが存在しない場合は処理終了.
		if ( !player )
		{
			return;
		}
		
		// プレイヤーの向きを取得.
		Quaternion playerRot = player.transform.rotation;

		// 星をプレイヤーの向きと逆の方向に進める.
		for( int i = 0; i < MAX_STARS; i++ )
		{
			if ( !stars[i] || scrollSpeed[i] == 0 )
			{
				// 星板のインスタンス、又はスクロールするスピードが未設定の場合は処理しない.
				continue;
			}
			
			// 星をスクロール.
			Vector3 additionPos = playerRot * Vector3.forward * scrollSpeed[i] * Time.deltaTime;
			stars[i].transform.position -= additionPos;
			
			// 星のスクロールのループ制御.
			IsOutOfWorld( stars[i] );
		}
	}
	
	// ------------------------------------------------------------------------
	// 星の切り替え位置となった場合のループ制御.
	// ------------------------------------------------------------------------
	private void IsOutOfWorld( GameObject star )
	{
		if ( star.transform.position.x < maxRightPositionX )
		{
			star.transform.position = new Vector3(
				maxLeftPositionX,
				star.transform.position.y,
				star.transform.position.z );
		}
		if ( star.transform.position.x > maxLeftPositionX )
		{
			star.transform.position = new Vector3(
				maxRightPositionX,
				star.transform.position.y,
				star.transform.position.z );
		}
		if ( star.transform.position.z < maxTopPositionZ )
		{
			star.transform.position = new Vector3(
				star.transform.position.x,
				star.transform.position.y,
				maxBottomPositionZ );
		}
		if ( star.transform.position.z > maxBottomPositionZ )
		{
			star.transform.position = new Vector3(
				star.transform.position.x,
				star.transform.position.y,
				maxTopPositionZ );
		}
	}
}
