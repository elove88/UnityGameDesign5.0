
using System.Collections.Generic;
using UnityEngine;


/// <summary>デバッグテキスト表示クラス</summary>
class DebugPrint : MonoBehaviour
{
	//==============================================================================================
	// 内部データ型

	/// <summary>テキストの表示情報</summary>
	private struct TextItem
	{
		public int x;
		public int y;
		public string text;
		public float lifetime;
	}


	//==============================================================================================
	// MonoBehaviour 関連のメンバ変数・メソッド

	/// <summary>スタートアップメソッド</summary>
	private void Start()
	{
		// バッファをクリア
		clear();
	}

	/// <summary>GUI ハンドリングメソッド</summary>
	private void OnGUI()
	{
		// バッファにたまっているテキストを表示する
		foreach ( TextItem item in m_textItems )
		{
			int x = item.x * CHARA_W;
			int y = item.y * CHARA_H;

			GUI.Label( new Rect( x, y, item.text.Length * CHARA_W, CHARA_H ), item.text );
		}

		// バッファをクリア
		if ( UnityEngine.Event.current.type == EventType.Repaint )
		{
			clear();
		}
	}


	//==============================================================================================
	// 非公開メソッド

	/// <summary>バッファをクリアする</summary>
	private void clear()
	{
		m_locateX = m_locateY = 0;

		for ( int i = 0; i < m_textItems.Count; ++i )
		{
			TextItem item = m_textItems[ i ];

			if ( item.lifetime >= 0.0f )
			{
				item.lifetime -= Time.deltaTime;
				m_textItems[ i ] = item;  // 書き戻す

				if ( item.lifetime <= 0.0f )
				{
					m_textItems.Remove( m_textItems[ i ] );
				}
			}
		}
	}

	/// <summary>表示位置をセットする</summary>
	private void setLocatePrivate( int x, int y )
	{
		m_locateX = x;
		m_locateY = y;
	}

	/// <summary>テキストを追加する</summary>
	private void addText( string text, float lifetime = 0.0f )
	{
		TextItem item;
		item.x        = m_locateX;
		item.y        = m_locateY++;
		item.text     = text;
		item.lifetime = lifetime;

		m_textItems.Add( item );
	}


	//==============================================================================================
	// 非公開メンバ変数

	/// <summary>横表示位置</summary>
	private int m_locateX;

	/// <summary>縦表示位置</summary>
	private int m_locateY;

	/// <summary>テキスト表示情報のリスト</summary>
	private List< TextItem > m_textItems = new List< TextItem >();

	/// <summary>横表示位置の単位ピクセル</summary>
	private const int CHARA_W = 20;

	/// <summary>縦表示位置の単位ピクセル</summary>
	private const int CHARA_H = 20;

	/// <summary>Singleton インスタンス</summary>
	private static DebugPrint m_instance = null;


	//==============================================================================================
	// 静的メソッド

	/// <summary>このクラスのインスタンスを取得する (Singleton)</summary>
	public static DebugPrint getInstance()
	{
		if ( m_instance == null )
		{
			// このクラスをアタッチするオブジェクトを作る
			GameObject go = new GameObject( "DebugPrint" );
			// アタッチ
			m_instance = go.AddComponent< DebugPrint >();

			// シーンを入れ替えてもこのオブジェクトが消えないようにする
			DontDestroyOnLoad( go );
		}

		return m_instance;
	}

	/// <summary>テキストを表示する</summary>
	public static void print( string text, float lifetime = 0.0f )
	{
		DebugPrint.getInstance().addText( text, lifetime );
	}

	/// <summary>表示位置をセットする</summary>
	public static void setLocate( int x, int y )
	{
		DebugPrint.getInstance().setLocatePrivate( x, y );
	}
}
