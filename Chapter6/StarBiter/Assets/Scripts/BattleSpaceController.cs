using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// BattleSpaceController
//  - 戦闘空間(プレイヤーが飛び回る空間)のスクロールを制御する.
//  - 使い方.
//    - 空のゲームオブジェクトに本スクリプトを付ける.
//    - 上記ゲームオブジェクトの子として岩石「M12_asteroid」、
//      クロスハッチ「T06_crossharch」を配置する. 
//  - 動き仕様.
//    - プレイヤーの進行方向と逆方向に戦闘空間を動かす.
//    - 動かした移動量は、他から参照できるようにする.
//  - 注意点.
//    - 戦闘空間の境目付近には岩石は置かないこと.
//      (境界で反対側に移動した瞬間に岩石が消えたように見える為)
// ----------------------------------------------------------------------------
public class BattleSpaceController : MonoBehaviour {
	
	public float scrollSpeed = 3f;					// プレイヤーの進行方向に合わせて.
													// 戦闘空間がスクロールするスピード.
	
	private Vector3 additionPosition;				// 戦闘空間が移動した移動量.
	private GameObject player;						// プレイヤーのインスタンス.
	
	private float bgX1 = -40f;						// 戦闘空間の境界(左端)
	private float bgX2 = 40f;						// 戦闘空間の境界(右端)
	private float bgZ1 = -40f;						// 戦闘空間の境界(上端)
	private float bgZ2 = 40f;						// 戦闘空間の境界(下端)
	
	void Start () {
	
		// プレイヤーのインスタンスを取得.
		player = GameObject.FindGameObjectWithTag("Player");
		
	}
	
	void LateUpdate() {
		
		// 戦闘空間をスクロールする.
		ScrollBattleSpace();
		
	}
	
	// ------------------------------------------------------------------------
	// 戦闘空間をスクロールする.
	// ------------------------------------------------------------------------
	private void ScrollBattleSpace()
	{
		
		// プレイヤーの向きを取得.
		Quaternion playerRotation = player.transform.rotation;

		// 戦闘空間をスクロールする(プレイヤーの向きと逆の方向に進める).
		additionPosition = playerRotation * Vector3.forward * scrollSpeed * Time.deltaTime;
		transform.position -= additionPosition;
		
		// 戦闘空間のループ制御.
		IsOutOfWorld();
		
	}
	
	// ------------------------------------------------------------------------
	// 戦闘空間のループ制御.
	// ------------------------------------------------------------------------
	private void IsOutOfWorld()
	{
		// 戦闘空間の右端を出た.
		if ( transform.position.x < bgX1 )
		{
			// 戦闘空間を左端に移動.
			transform.position = new Vector3(
				bgX2,
				transform.position.y,
				transform.position.z );
		}
		
		// 戦闘空間の左端を出た.
		if ( transform.position.x > bgX2 )
		{
			// 戦闘空間を右端に移動.
			transform.position = new Vector3(
				bgX1,
				transform.position.y,
				transform.position.z );
		}
		
		// 戦闘空間の上端を出た.
		if ( transform.position.z < bgZ1 )
		{
			// 戦闘空間を下端に移動.
			transform.position = new Vector3(
				transform.position.x,
				transform.position.y,
				bgZ2 );
		}
		
		// 戦闘空間の下端を出た.
		if ( transform.position.z > bgZ2 )
		{
			// 戦闘空間の上端に移動.
			transform.position = new Vector3(
				transform.position.x,
				transform.position.y,
				bgZ1 );
		}
	}
	
	// ------------------------------------------------------------------------
	// 戦闘空間が移動した移動量を返す.
	// ------------------------------------------------------------------------
	public Vector3 GetAdditionPos()
	{
		return additionPosition;
	}

}
