
using UnityEngine;


/// <summary>マウスによるドラッグが可能なオブジェクト</summary>
class DraggableObject : BaseObject
{
	//==============================================================================================
	// MonoBehaviour 関連のメンバ変数・メソッド

	/// <summary>マウスドラッグの際にオブジェクトを持ち上げる高さ</summary>
	public float m_pickupHeight = 150.0f;

	/// <summary>オブジェクトとの衝突</summary>
	private void OnCollisionEnter( Collision collision )
	{
		if ( !m_isDragging && collision.gameObject.layer == m_terrainLayerIndex )
		{
			m_isLanding = true;
		}
	}


	//==============================================================================================
	// 公開メソッド

	/// <summary>マウスドラッグを開始したフレームの更新メソッド</summary>
	public void onDragBegin( RaycastHit hit )
	{
		// 重力を無効にする
		GetComponent<Rigidbody>().useGravity = false;

		// オブジェクトを持ち上げる (オブジェクト位置の更新)
		updateDragPosition();

		// 着地フラグをクリア
		m_isLanding = false;

		// ドラッグ開始
		m_isDragging = true;
	}

	/// <summary>マウスドラッグ中のフレームの更新メソッド</summary>
	public void onDragUpdate()
	{
		// オブジェクト位置の更新
		updateDragPosition();
	}

	/// <summary>マウスドラッグを終了したフレームの更新メソッド</summary>
	public void onDragEnd()
	{
		// 落下速度を初期化 (接地する前にドラッグするのを繰り返して落下速度が蓄積されるのを防ぐ)
		GetComponent<Rigidbody>().velocity = Vector3.zero;

		// 重力を有効にする
		GetComponent<Rigidbody>().useGravity = true;

		// ドラッグ終了
		m_isDragging = false;
	}


	//==============================================================================================
	// 非公開メソッド

	/// <summary>マウスドラッグ中のオブジェクト位置を更新する</summary>
	private void updateDragPosition()
	{
		// オブジェクトの移動先
		Vector3 moveTo = Vector3.zero;

		// マウスカーソル位置にある Terrain の座標を求める
		Vector3 mousePosition = Input.mousePosition;
		Ray rayFromMouse = Camera.main.ScreenPointToRay( mousePosition );
		RaycastHit hitFromMouse;
		if ( mousePosition.x >= 0.0f && mousePosition.x <= Screen.width  &&
		     mousePosition.y >= 0.0f && mousePosition.y <= Screen.height &&
		     Physics.Raycast( rayFromMouse, out hitFromMouse, float.PositiveInfinity, 1 << m_terrainLayerIndex ) )
		{
			// Terrain の座標を基準に移動先を決める
			moveTo = hitFromMouse.point + m_pickupHeight * Vector3.up;
		}
		else
		{
			// マウスカーソルが画面外にいる or マウスカーソル位置に Terrain がない

			// 現在位置を維持
			moveTo = transform.position;
		}

		// 補間を入れてスムーズに持ち上げる
		moveTo.y = Mathf.Lerp( transform.position.y, moveTo.y, 0.3f );

		transform.position = moveTo;
	}


	//==============================================================================================
	// 非公開メンバ変数

	/// <summary>ドラッグ中かどうか</summary>
	private bool m_isDragging = false;
}
