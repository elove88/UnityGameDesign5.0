
using UnityEngine;


/// <summary>fadein/fadeout コマンドのイベントアクター</summary>
class EventActorFading : EventActor
{
	//==============================================================================================
	// 公開メソッド

	/// <summary>コンストラクタ</summary>
	public EventActorFading( FadeInOutManager manager, bool isFadeIn, float duration )
	{
		m_manager  = manager;
		m_isFadeIn = isFadeIn;
		m_duration = duration;
	}

	/// <summary>アクターが生成された際に最初に実行されるメソッド</summary>
	public override void start( EventManager evman )
	{
		if ( m_isFadeIn )
		{
			m_manager.fadeIn( m_duration );
		}
		else
		{
			m_manager.fadeOut( m_duration );
		}
	}

	/// <summary>アクターで行うべき処理が終わったかどうかを返す</summary>
	public override bool isDone()
	{
		return !m_manager.isFading();
	}

	/// <summary>実行終了後にクリックを待つかどうかを返す</summary>
	public override bool isWaitClick( EventManager evman )
	{
		return false;
	}


	//==============================================================================================
	// 非公開メンバ変数

	/// <summary>画面の暗転を管理するオブジェクト</summary>
	private FadeInOutManager m_manager;

	/// <summary>フェードインするかどうか</summary>
	private bool m_isFadeIn;

	/// <summary>フェードイン/アウトにかかる秒数</summary>
	private float m_duration;


	//==============================================================================================
	// 静的メソッド

	/// <summary>イベントアクターのインスタンスを生成する</summary>
	public static EventActorFading CreateInstance( string[] parameters, bool isFadeIn, GameObject manager )
	{
		if ( parameters.Length >= 1 )
		{
			FadeInOutManager fiom = manager.GetComponent< FadeInOutManager >();
			float duration;

			if ( fiom != null && float.TryParse( parameters[ 0 ], out duration ) )
			{
				// アクターを生成
				EventActorFading actor = new EventActorFading( fiom, isFadeIn, duration );
				return actor;
			}
		}

		Debug.LogError( "Failed to create an actor." );
		return null;
	}
}
