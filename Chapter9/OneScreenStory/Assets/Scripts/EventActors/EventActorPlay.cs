
using UnityEngine;


/// <summary>play コマンドのイベントアクター</summary>
class EventActorPlay : EventActor
{
	//==============================================================================================
	// 公開メソッド

	/// <summary>コンストラクタ</summary>
	public EventActorPlay( AudioClip clip, bool isLoop )
	{
		m_clip   = clip;
		m_isLoop = isLoop;
	}

	/// <summary>アクターが生成された際に最初に実行されるメソッド</summary>
	public override void start( EventManager evman )
	{
		// オーディオクリップを再生
		evman.getSoundManager().playSE( m_clip, m_isLoop );
	}

	/// <summary>実行終了後にクリックを待つかどうかを返す</summary>
	public override bool isWaitClick( EventManager evman )
	{
		// すぐ終了
		return false;
	}


	//==============================================================================================
	// 非公開メンバ変数

	/// <summary>再生するオーディオクリップ</summary>
	private AudioClip m_clip;

	/// <summary>ループ再生をするかどうか</summary>
	private bool m_isLoop;


	//==============================================================================================
	// 静的メソッド

	/// <summary>イベントアクターのインスタンスを生成する</summary>
	public static EventActorPlay CreateInstance( string[] parameters, GameObject manager, Event ev )
	{
		bool      isValid = false;
		bool      isLoop  = false;
		AudioClip clip    = null;

		// これから作るアクターの名前をセットする (エラーメッセージ用)
		ev.setCurrentActorName( "EventActorPlay" );

		if ( parameters.Length >= 1 )
		{
			isValid = true;

			if ( parameters.Length >= 2 && parameters[ 1 ].ToLower() == "loop" )
			{
				isLoop = true;
			}

			// サウンドマネージャからオーディオクリップを取得
			clip = manager.GetComponent< EventManager >().getSoundManager().getAudioClip( parameters[ 0 ] );
			if ( clip == null )
			{
				ev.debugLogError( "Can't find audio clip \"" + parameters[ 0 ] + "\"" );
				isValid = false;
			}
		}

		if ( isValid )
		{
			// アクターを生成
			EventActorPlay actor = new EventActorPlay( clip, isLoop );
			return actor;
		}

		Debug.LogError( "Failed to create an actor." );
		return null;
	}
}
