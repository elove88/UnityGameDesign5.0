using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// StoneController
//  - 岩石「M12_asteroid」の動きを制御する.
//  - 使い方.
//    - 本スクリプトが付いた岩石を配置する.
//  - 動き仕様.
//    - ランダムな方向にランダムなスピードで回転する.
// ---------------------------------------------------------------------------
public class StoneController : MonoBehaviour {
	
	private float rotateSpeed = 0;			// 岩石の回転スピード.
	
	void Start () 
	{
	
		// 岩石の向きをランダムに設定.
		transform.rotation = new Quaternion(
			Random.Range( 0, 360 ),
			Random.Range( 0, 360 ),
			Random.Range( 0, 360 ),
			Random.Range( 0, 360 ));
		
		// 岩石の回転スピードをランダムに設定.
		rotateSpeed = Random.Range( 0.01f, 3f );
		
		// 岩石を攻撃対象とする.
		SendMessage( "SetIsAttack", true );
	}
	
	void Update ()
	{
	
		// 岩石を回転させる.
		transform.Rotate( new Vector3( 0, rotateSpeed, 0 ) );
		
	}
}
