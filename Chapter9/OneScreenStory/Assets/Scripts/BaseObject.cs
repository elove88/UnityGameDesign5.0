
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/// <summary>このゲームにおける「オブジェクト」の基本単位</summary>
class BaseObject : MonoBehaviour
{
	//==============================================================================================
	// MonoBehaviour 関連のメンバ変数・メソッド

	/// <summary>イベントマネージャオブジェクト</summary>
	protected EventManager m_eventManager = null;

	/// <summary>会話文の背景色</summary>
	/// 会話文を表示しないオブジェクトの場合は設定不要
	public Color m_dialogBackground = new Color32( 0, 128, 255, 160 );

	/// <summary>y 座標の上端値</summary>
	/// 会話文の吹き出しや DraggableObject のマジックハンドなどの表示位置の調整に使われる値
	public float m_yTop = 0.0f;

	/// <summary>y 座標の下端値</summary>
	/// 会話文の吹き出しや DraggableObject のマジックハンドなどの表示位置の調整に使われる値
	public float m_yBottom = 0.0f;

#if UNITY_EDITOR
	/// <summary>インスペクタでゲーム内変数の内容を確認するためのメンバ変数</summary>
	public string[] m_debug_variables;
#endif //UNITY_EDITOR

	/// <summary>スタートアップメソッド</summary>
	private void Awake()
	{
		m_eventManager = EventManager.get();

		// Terrain レイヤーインデックスを検索
		m_terrainLayerIndex = LayerMask.NameToLayer( "Terrain" );
	}

	/// <summary>フレーム毎更新メソッド</summary>
	private void Update()
	{
		if ( !m_isLandingPrevious && m_isLanding )  // 着地した瞬間
		{
			foreach ( GameObject co in m_contactingObjects )
			{
				BaseObject bo = co.GetComponent< BaseObject >();
				if ( bo == null ) continue;

				// イベントマネージャへ接触したオブジェクトの情報を通知
				m_eventManager.addContactingObject( bo );
			}

			// イベントマネージャへ自分の情報を通知
			m_eventManager.addContactingObject( this );
		}

		m_contactingObjects.Clear();
		m_isLandingPrevious = m_isLanding;
	}

	/// <summary>オブジェクトとの接触</summary>
	private void OnTriggerStay( Collider collider )
	{
		// 接触してきた相手を覚えておく
		m_contactingObjects.Add( collider.gameObject );
	}

	/// <summary>オブジェクトとの衝突</summary>
	private void OnCollisionEnter( Collision collision )
	{
		if ( collision.gameObject.layer == m_terrainLayerIndex )
		{
			m_isLanding = true;
		}
	}


	//==============================================================================================
	// 公開メソッド

	/// <summary>ゲーム内変数を取得する</summary>
	/// 指定された名前のゲーム内変数が存在しない場合は null を返す
	public string getVariable( string name )
	{
		// 辞書から該当する名前の要素を取り出す
		string value;
		bool hasData = m_variables.TryGetValue( name, out value );

		return hasData ? value : null;
	}

	/// <summary>ゲーム内変数を設定する</summary>
	public void setVariable( string name, string value )
	{
		if ( m_variables.ContainsKey( name ) )
		{
			// 既に同じ名前の変数がある →置き換え
			m_variables[ name ] = value;
		}
		else
		{
			// 同じ名前の変数がない →新しく追加する
			m_variables.Add( name, value );
		}

#if UNITY_EDITOR
		// エディタで動作させているときはインスペクタ表示を更新する
		m_debug_variables = new string[ m_variables.Count ];

		int i = 0;
		foreach ( KeyValuePair< string, string > pair in m_variables )
		{
			m_debug_variables[ i++ ] = pair.Key + " = " + pair.Value;
		}
#endif //UNITY_EDITOR
	}

	/// <summary>ゲーム内変数をクリアする</summary>
	public bool clearVariable( string name )
	{
		return m_variables.Remove( name );
	}

	/// <summary>すべてのゲーム内変数をクリアする</summary>
	public void clearAllVariables( bool alsoGlobal )
	{
		if ( alsoGlobal )
		{
			m_variables.Clear();
		}
		else
		{
			// グローバルスコーププリフィックスを持たないキーを取り出して削除
			IEnumerable< string > localKeys = from key in m_variables.Keys
			                                  where key.IndexOf( m_globalScopePrefix ) != 0
			                                  select key;
			foreach ( string key in localKeys )
			{
				clearVariable( key );
			}
		}
	}

	/// <summary>会話文の背景色を返す</summary>
	public Color getDialogBackgroundColor()
	{
		return m_dialogBackground;
	}

	/// <summary>y 座標の上端値を返す</summary>
	public float getYTop()
	{
		return m_yTop;
	}

	/// <summary>y 座標の下端値を返す</summary>
	public float getYBottom()
	{
		return m_yBottom;
	}

	/// <summary>アクターから来たメッセージをフレーム毎に処理する</summary>
	/// 派生クラスでこのメソッドをオーバーライドして記述する
	/// 戻り値は現在のメッセージを次のフレームでも処理するかどうか
	public virtual bool updateMessage( string message, string[] parameters )
	{
		// デフォルトでは何もせず即終了
		return false;
	}


	//==============================================================================================
	// 非公開メンバ変数

	/// <summary>Terrain となるオブジェクトのレイヤーインデックス</summary>
	protected int m_terrainLayerIndex;

	/// <summary>着地しているかどうか</summary>
	protected bool m_isLanding = true;

	/// <summary>前のフレームで着地しているかどうか</summary>
	private bool m_isLandingPrevious = true;

	/// <summary>接触しているオブジェクト</summary>
	private List< GameObject > m_contactingObjects = new List< GameObject >();

	/// <summary>ゲーム内変数を保存するための辞書</summary>
	private Dictionary< string, string > m_variables = new Dictionary< string, string >();

	/// <summary>グローバルスコープのゲーム内変数を示すプリフィックス</summary>
	private const string m_globalScopePrefix = "_global_";
}
