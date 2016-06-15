
using UnityEngine;


/// <summary>カメラの位置・回転角度・平行投影サイズを管理するクラス</summary>
class CameraManager : MonoBehaviour
{
	//==============================================================================================
	// MonoBehaviour 関連のメンバ変数・メソッド

	/// <summary>Terrain の左端</summary>
	public float m_terrainEndLeft = 0.0f;
	/// <summary>Terrain の右端</summary>
	public float m_terrainEndRight = 0.0f;
	/// <summary>Terrain の手前端</summary>
	public float m_terrainEndFront = 0.0f;
	/// <summary>Terrain の奥端</summary>
	public float m_terrainEndBack = 0.0f;
	/// <summary>Terrain 奥の背景上端</summary>
	public float m_backgroundTop = 0.0f;

	/// <summary>スタートアップメソッド</summary>
	private void Start()
	{
		// 初期位置 (＝開始時点での現在位置) を覚えておく
		m_originalPosition  = m_currentPosition  = transform.position;
		m_originalRotationX = m_currentRotationX = transform.eulerAngles.x;
		m_originalSize      = m_currentSize      = GetComponent<Camera>().orthographicSize;
	}

	/// <summary>フレーム毎更新メソッド</summary>
	private void Update()
	{
		if ( m_isMoving )
		{
			if ( Time.time >= m_endTime )
			{
				// カメラ移動時間経過後初めての Update

				// 現在位置＝目標位置に移動
				transform.position      = m_currentPosition = m_destinationPosition;
				GetComponent<Camera>().orthographicSize = m_currentSize     = m_destinationSize;
				m_currentRotationX = m_destinationRotationX;
				transform.eulerAngles = new Vector3( m_currentRotationX, transform.eulerAngles.y, transform.eulerAngles.z );

				// 移動終了
				m_isMoving = false;
			}
			else
			{
				// 進行度 (0.0～1.0)
				float ratio = Mathf.InverseLerp( m_beginTime, m_endTime, Time.time );

				// 角度と投影サイズは補間
				transform.eulerAngles = new Vector3( Mathf.Lerp( m_currentRotationX, m_destinationRotationX, ratio ),
				                                     transform.eulerAngles.y, transform.eulerAngles.z );
				GetComponent<Camera>().orthographicSize = Mathf.Lerp( m_currentSize, m_destinationSize, ratio );

				// 位置は補間中に Terrain の端が見えないよう調整
				transform.position = fixPosition( Vector3.Lerp( m_currentPosition, m_destinationPosition, ratio ), transform.eulerAngles.x, GetComponent<Camera>().orthographicSize );
			}
		}
	}


	//==============================================================================================
	// 公開メソッド

	/// <summary>指定位置に移動する</summary>
	public void moveTo( Vector3 destinationPosition, float destinationRotationX, float destinationSize, float duration )
	{
		// 座標調整
		destinationPosition = fixPosition( destinationPosition, destinationRotationX, destinationSize );

		m_destinationPosition  = destinationPosition;
		m_destinationRotationX = destinationRotationX;
		m_destinationSize      = destinationSize;

		m_beginTime = Time.time;
		m_endTime   = m_beginTime + duration;
		m_isMoving  = true;
	}

	/// <summary>初期位置を取得する</summary>
	public Vector3 getOriginalPosition()
	{
		return m_originalPosition;
	}

	/// <summary>初期 x 軸回転角度を取得する</summary>
	public float getOriginalRotationX()
	{
		return m_originalRotationX;
	}

	/// <summary>初期平行投影サイズを取得する</summary>
	public float getOriginalSize()
	{
		return m_originalSize;
	}

	/// <summary>現在位置を取得する</summary>
	/// カメラが移動中の場合は移動を始める前の位置
	public Vector3 getCurrentPosition()
	{
		return m_currentPosition;
	}

	/// <summary>現在の x 軸回転角度を取得する</summary>
	/// カメラが移動中の場合は移動を始める前の回転角度
	public float getCurrentRotationX()
	{
		return m_currentRotationX;
	}

	/// <summary>現在の平行投影サイズを取得する</summary>
	/// カメラが移動中の場合は移動を始める前のサイズ
	public float getCurrentSize()
	{
		return m_currentSize;
	}

	/// <summary>カメラが移動中かどうかを返す</summary>
	public bool isMoving()
	{
		return m_isMoving;
	}


	public static CameraManager	get()
	{
		if(instance == null) {
			
			GameObject	go = GameObject.FindGameObjectWithTag("MainCamera");
			
			if(go == null) {
				
				Debug.Log("Can't find \"MainCamera\" GameObject.");
				
			} else {
				
				instance = go.GetComponent<CameraManager>();
			}
		}
		return(instance);
	}
	protected static CameraManager	instance = null;

	//==============================================================================================
	// 非公開メソッド

	/// <summary>Terrain の端が見えないように座標を調整する</summary>
	private Vector3 fixPosition( Vector3 position, float rotationX, float size )
	{
		Vector3 newPosition = new Vector3( position.x, position.y, position.z );
		float horizontalSize = size * Screen.width / Screen.height;

		// 左端
		if ( position.x - horizontalSize < m_terrainEndLeft )
		{
			newPosition.x = m_terrainEndLeft + horizontalSize;
		}

		// 右端
		if ( position.x + horizontalSize > m_terrainEndRight )
		{
			newPosition.x = m_terrainEndRight - horizontalSize;
		}

		// 手前端
		float terrainZOfBottom = position.z
		                       + position.y / Mathf.Tan( rotationX * Mathf.Deg2Rad )
		                       - size / Mathf.Sin( rotationX * Mathf.Deg2Rad );
		if ( terrainZOfBottom < m_terrainEndFront )
		{
			newPosition.z = position.z + m_terrainEndFront - terrainZOfBottom;
		}

		// 奥端
		float terrainYOfTop = position.y
		                    - ( m_terrainEndBack - position.z ) * Mathf.Tan( rotationX * Mathf.Deg2Rad )
		                    + size / Mathf.Cos( rotationX * Mathf.Deg2Rad );
		if ( terrainYOfTop > m_backgroundTop )
		{
			newPosition.z = position.z - ( terrainYOfTop - m_backgroundTop ) / Mathf.Tan( rotationX * Mathf.Deg2Rad );
		}

		return newPosition;
	}


	//==============================================================================================
	// 非公開メンバ変数

	/// <summary>初期位置</summary>
	private Vector3 m_originalPosition;

	/// <summary>初期 x 軸回転角度</summary>
	private float m_originalRotationX;

	/// <summary>初期平行投影サイズ</summary>
	private float m_originalSize;

	/// <summary>現在位置</summary>
	private Vector3 m_currentPosition;

	/// <summary>現在の x 軸回転角度</summary>
	private float m_currentRotationX;

	/// <summary>現在の平行投影サイズ</summary>
	private float m_currentSize;

	/// <summary>目標位置</summary>
	private Vector3 m_destinationPosition;

	/// <summary>目標の x 軸回転角度</summary>
	private float m_destinationRotationX;

	/// <summary>目標の平行投影サイズ</summary>
	private float m_destinationSize;

	/// <summary>カメラ移動の開始時間</summary>
	private float m_beginTime = 0.0f;

	/// <summary>カメラ移動の終了時間</summary>
	private float m_endTime = 0.0f;

	/// <summary>カメラが移動中かどうか</summary>
	private bool m_isMoving = false;
}
