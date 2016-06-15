
using System;
using UnityEngine;


/// <summary>choice コマンドのイベントアクター</summary>
class EventActorChoice : EventActor
{
	//==============================================================================================
	// 公開メソッド

	/// <summary>コンストラクタ</summary>
	public EventActorChoice( BaseObject baseObject, string name, string[] choices )
	{
		m_object  = baseObject;
		m_name    = name;
		m_choices = choices;
	}

	/// <summary>アクターが破棄されるまで GUI の描画タイミングで実行されるメソッド</summary>
	public override void onGUI( EventManager evman )
	{
		// フォントとパディングを設定
		GUIStyle style = new GUIStyle( "button" );  // button はボタンのデフォルトスタイル
		style.font = evman.gameObject.GetComponent< TextManager >().m_text.font;
		style.padding = new RectOffset( 50, 50, 8, 8 );

		// 画面中心にボタンを表示
		GUILayout.BeginArea( new Rect( 0, 0, Screen.width, Screen.height ) );
			GUILayout.FlexibleSpace();	// 上
			GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();	// 中左
				GUILayout.BeginVertical();

					for ( int i = 0; i < m_choices.Length; ++i )
					{
						// 選択肢表示
						if ( GUILayout.Button( m_choices[ i ], style ) )
						{
							// クリックされた選択肢のインデックスをゲーム内変数に設定して終了
							// (最初の選択肢は 1 になる)
							m_object.setVariable( m_name, ( i + 1 ).ToString() );
							m_isDone = true;
						}
					}

				GUILayout.EndVertical();
				GUILayout.FlexibleSpace();	// 中右
			GUILayout.EndHorizontal();
			GUILayout.FlexibleSpace();	// 下
		GUILayout.EndArea();
	}

	/// <summary>アクターで行うべき処理が終わったかどうかを返す</summary>
	public override bool isDone()
	{
		return m_isDone;
	}

	/// <summary>実行終了後にクリックを待つかどうかを返す</summary>
	public override bool isWaitClick( EventManager evman )
	{
		// 選択肢でクリック待ちが入るのでこっちでは待たない
		return false;
	}


	//==============================================================================================
	// 非公開メンバ変数

	/// <summary>ゲーム内変数を操作するオブジェクト</summary>
	private BaseObject m_object;

	/// <summary>ゲーム内変数名</summary>
	private string m_name;

	/// <summary>選択肢の一覧</summary>
	private string[] m_choices;

	/// <summary>アクターの処理が終了しているかどうか</summary>
	private bool m_isDone = false;


	//==============================================================================================
	// 静的メソッド

	/// <summary>イベントアクターのインスタンスを生成する</summary>
	public static EventActorChoice CreateInstance( string[] parameters, GameObject manager )
	{
		if ( parameters.Length >= 3 )
		{
			// 指定されたオブジェクトを探す
			BaseObject bo = manager.GetComponent< ObjectManager >().find( parameters[ 0 ] );
			if ( bo != null )
			{
				string[] choices = new string[ parameters.Length - 2 ];
				Array.Copy( parameters, 2, choices, 0, choices.Length );

				// アクターを生成.
				EventActorChoice actor = new EventActorChoice( bo, parameters[ 1 ], choices );
				return actor;
			}
		}

		Debug.LogError( "Failed to create an actor." );
		return null;
	}
}
