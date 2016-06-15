using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// プレイヤーの操作/制御.
// ----------------------------------------------------------------------------
public class PlayerController : MonoBehaviour {

	private	GameObject mainCamera;					// メインカメラ.
	private	GameObject scoutingLaser;				// 索敵レーザー.
	private bool isScanOn = false;					// 索敵モード.
	private bool isAlive = false;					// プレイヤー生存.
	
	private PlayerStatus playerStatus;				// PlayerStatusインスタンス.
	
	void Start () 
	{
		// メインカメラのインスタンスを取得.
		mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
		
		// 索敵レーザーのインスタンスを取得.
		scoutingLaser = GameObject.FindGameObjectWithTag("ScoutingLaser");
		
		// PlayerStatusインスタンスを取得.
		playerStatus = this.gameObject.GetComponent<PlayerStatus>();

	}
	
	void Update ()
	{
		// プレイヤーは生存しているか?.
		if ( isAlive )
		{
			// プレイヤーステータスのインスタンスは存在しているか?.
			if ( playerStatus )
			{
				// プレイヤーは操作可能か?.
				if ( playerStatus.GetCanPlay() == true )
				{
					// マウスカーソルの方向にプレイヤーを向ける.
					SetPlayerDirection();
					
					// 索敵モード切替.
					ChangeScanMode();
				}
			}
		}

	}
	
	// ------------------------------------------------------------------------
	// マウスカーソルの方向にプレイヤーを向ける.
	// ------------------------------------------------------------------------
	private void SetPlayerDirection()
	{
		// マウスポインタの方向に向く角度を求める.
		Vector3 mousePos = GetWorldPotitionFromMouse();
		Vector3 relativePos = mousePos - transform.position;
		Quaternion tmpRotation = Quaternion.LookRotation( relativePos );
		
		// プレイヤーの角度を変更
		transform.rotation = tmpRotation;
		
	}

	// ------------------------------------------------------------------------
	// 索敵モードの切り替え.
	// ------------------------------------------------------------------------
	private void ChangeScanMode()
	{
		// マウスの左クリックが押された時は索敵モードをONにする.
		if ( isScanOn == false ) {
			if ( Input.GetButtonDown("Fire1") ) {
				isScanOn = true;
				scoutingLaser.SendMessage( "SetIsScanOn", isScanOn );
				SendMessage( "SetFireOrder" );
			}
		}
		
		// マウスの左クリックが離された時は索敵モードをOFFにする.
		if ( isScanOn == true ) {
			if ( Input.GetButtonUp("Fire1") ) {
				isScanOn = false;
				scoutingLaser.SendMessage( "SetIsScanOn", isScanOn );
			}
		}
	}

	// ------------------------------------------------------------------------
	// プレイヤーの生存設定.
	// ------------------------------------------------------------------------
	public void SetIsAlive( bool isAlive )
	{
		this.isAlive = isAlive;
	}
	
	// ------------------------------------------------------------------------
	// マウスの位置を、３D空間のワールド座標に変換する.
	//   - 次の二つが交わるところを求める.
	//     1. マウスカーソルとカメラの位置を通る直線
	//     2. ピースの中心を通る、水平な面
	// ------------------------------------------------------------------------
	private Vector3	GetWorldPotitionFromMouse()
	{
		Vector3	mousePosition = Input.mousePosition;

		// ピースの中心を通る、水平（法線がY軸。XZ平面）な面.
		// 中心はプレイヤーとする.
		Plane plane = new Plane( Vector3.up, new Vector3( 0f, 0f, 0f ) );
		
		// カメラ位置とマウスカーソルの位置を通る直線.
		Ray ray = mainCamera.GetComponent<Camera>().ScreenPointToRay( mousePosition );

		// 上の二つが交わるところを求める.
		float depth;
		
		plane.Raycast( ray, out depth );
		
		Vector3	worldPosition;
		
		worldPosition = ray.origin + ray.direction * depth;
		
		// Y座標はプレイヤーとあわせておく.
		worldPosition.y = 0;
		
		return worldPosition;
	}
	
	// ------------------------------------------------------------------------
	// 全ての値をリセットする.
	// ------------------------------------------------------------------------
	public void Reset()
	{
		// 索敵レーザーをOFFにする.
		isScanOn = false;
	}
	
}