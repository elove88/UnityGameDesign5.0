
using System;
using UnityEngine;


/// <summary>グローバルパラメータ管理クラス</summary>
class GlobalParam : MonoBehaviour
{
	//==============================================================================================
	// 公開メソッド

	/// <summary>ゲーム開始時に読み込むスクリプトファイルを取得する</summary>
	public string[] getStartScriptFiles()
	{
		return m_startScripts;
	}

	/// <summary>ゲーム開始時に読み込むスクリプトファイルを設定する</summary>
	public void setStartScriptFiles( params string[] files )
	{
		m_startScripts = new string[ files.Length ];
		Array.Copy( files, m_startScripts, files.Length );
	}

	//==============================================================================================
	// 非公開メソッド

	/// <summary>初期化</summary>
	private void create()
	{
		m_startScripts = new string[2];

		// Title 経由じゃない、かつ、インスペクターでスクリプトが指定されて
		// いないときは、ここで指定されたものが使われます。
		//
		m_startScripts[0] = "Events/c00_main.txt";
		m_startScripts[1] = "Events/c00_sub.txt";
	}

	//==============================================================================================
	// 非公開メンバ変数

	/// <summary>ゲーム開始時に読み込むスクリプトファイル</summary>
	private string[] m_startScripts;

	/// <summary>Singleton インスタンス</summary>
	private static GlobalParam m_instance = null;


	//==============================================================================================
	// 静的メソッド

	/// <summary>このクラスのインスタンスを取得する (Singleton)</summary>
	public static GlobalParam getInstance()
	{
		if ( m_instance == null )
		{
			// このクラスをアタッチするオブジェクトを作る
			GameObject go = new GameObject( "GlobalParam" );
			// アタッチ
			m_instance = go.AddComponent< GlobalParam >();

			m_instance.create();

			// シーンを入れ替えてもこのオブジェクトが消えないようにする
			DontDestroyOnLoad( go );
		}

		return m_instance;
	}
}
