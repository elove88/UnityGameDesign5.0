
using UnityEngine;


/// <summary>hide/show コマンドのイベントアクター</summary>
class EventActorVisibility : EventActor
{
	//==============================================================================================
	// 公開メソッド

	/// <summary>コンストラクタ</summary>
	public EventActorVisibility( BaseObject baseObject, bool doShow )
	{
		m_object = baseObject;
		m_doShow = doShow;
	}

	/// <summary>アクターが生成された際に最初に実行されるメソッド</summary>
	public override void start( EventManager evman )
	{
		ObjectManager om = evman.GetComponent< ObjectManager >();

		// 切り替え時のエフェクトを再生
		om.playSwitchingEffect( m_object );

		// 表示/非表示
		if ( m_doShow )
		{
			om.activate( m_object );
		}
		else
		{
			om.deactivate( m_object );
		}
	}

	/// <summary>実行終了後にクリックを待つかどうかを返す</summary>
	public override bool isWaitClick( EventManager evman )
	{
		// すぐ終了
		return false;
	}


	//==============================================================================================
	// 非公開メンバ変数

	/// <summary>表示・非表示オブジェクト</summary>
	private BaseObject m_object;

	/// <summary>オブジェクトを表示するのかどうか</summary>
	private bool m_doShow;


	//==============================================================================================
	// 静的メソッド

	/// <summary>イベントアクターのインスタンスを生成する</summary>
	public static EventActorVisibility CreateInstance( string[] parameters, bool doShow, GameObject manager )
	{
		if ( parameters.Length >= 1 )
		{
			// 指定されたオブジェクトを探す
			BaseObject bo = manager.GetComponent< ObjectManager >().find( parameters[ 0 ] );
			if ( bo != null )
			{
				// アクターを生成
				EventActorVisibility actor = new EventActorVisibility( bo, doShow );
				return actor;
			}
		}

		Debug.LogError( "Failed to create an actor." );
		return null;
	}
}
