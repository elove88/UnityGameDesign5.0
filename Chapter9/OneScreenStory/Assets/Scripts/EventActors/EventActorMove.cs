
using UnityEngine;


/// <summary>move コマンドのイベントアクター</summary>
class EventActorMove : EventActor
{
	//==============================================================================================
	// 公開メソッド

	/// <summary>コンストラクタ</summary>
	public EventActorMove( BaseObject target, BaseObject to, float duration )
	{
		m_target         = target;
		m_targetPosition = m_target.transform.position;  // いまの座標を覚えておく
		m_to             = to;
		m_beginTime      = Time.time;
		m_endTime        = m_beginTime + duration;
	}

	/// <summary>アクターが破棄されるまで毎フレーム実行されるメソッド</summary>
	public override void execute( EventManager evman )
	{
		Vector3 presentPosition;

		if ( Time.time >= m_endTime )
		{
			presentPosition = m_to.transform.position;
			m_isDone = true;
		}
		else
		{
			// 進行度 (0.0～1.0)
			float ratio = Mathf.InverseLerp( m_beginTime, m_endTime, Time.time );

			presentPosition = Vector3.Lerp( m_targetPosition, m_to.transform.position, ratio );
		}

		// y 座標の調整 (Terrain との衝突点に配置する)
		RaycastHit hit;
		if ( Physics.Raycast( presentPosition + 10000.0f * Vector3.up,
		                      -Vector3.up,
		                      out hit,
		                      float.PositiveInfinity,
		                      1 << LayerMask.NameToLayer( "Terrain" ) ) )
		{
			m_target.transform.position = hit.point;
		}
		else
		{
			m_target.transform.position = new Vector3( presentPosition.x, 0.0f, presentPosition.z );
		}
	}

	/// <summary>アクターで行うべき処理が終わったかどうかを返す</summary>
	public override bool isDone()
	{
		return m_isDone;
	}

	/// <summary>実行終了後にクリックを待つかどうかを返す</summary>
	public override bool isWaitClick( EventManager evman )
	{
		return false;
	}


	//==============================================================================================
	// 非公開メンバ変数

	/// <summary>移動対象のオブジェクト</summary>
	private BaseObject m_target;

	/// <summary>移動前の座標</summary>
	private Vector3 m_targetPosition;

	/// <summary>移動先となるオブジェクト</summary>
	private BaseObject m_to;

	/// <summary>移動の開始時間</summary>
	private float m_beginTime;

	/// <summary>移動の終了時間</summary>
	private float m_endTime;

	/// <summary>アクターの処理が終了しているかどうか</summary>
	private bool m_isDone = false;


	//==============================================================================================
	// 静的メソッド

	/// <summary>イベントアクターのインスタンスを生成する</summary>
	public static EventActorMove CreateInstance( string[] parameters, GameObject manager )
	{
		if ( parameters.Length >= 3 )
		{
			ObjectManager om = manager.GetComponent< ObjectManager >();
			BaseObject target = om.find( parameters[ 0 ] );
			BaseObject to     = om.find( parameters[ 1 ] );
			float duration;

			if ( target != null && to != null && float.TryParse( parameters[ 2 ], out duration ) )
			{
				// アクターを生成
				EventActorMove actor = new EventActorMove( target, to, duration );
				return actor;
			}
		}

		Debug.LogError( "Failed to create an actor." );
		return null;
	}
}
