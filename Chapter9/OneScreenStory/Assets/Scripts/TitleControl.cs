
using UnityEngine;


/// <summary>タイトル画面からのゲーム開始用クラス</summary>
class TitleControl : MonoBehaviour
{
	//==============================================================================================
	// 内部データ型

	/// <summary>遷移状態</summary>
	private enum STEP
	{
		NONE = -1,
		SELECT = 0,   // 選択中
		PLAY_JINGLE,  // ジングル再生中
		START_GAME,   // ゲーム開始
		NUM
	}

	/// <summary>ゲームの章</summary>
	private enum CHAPTER
	{
		NONE = -1,
		PROLOGUE = 0,
		C1,
		C2,
		C3_0,
		C3_1,
		C4,
		C5,
		EPILOGUE,
		NUM
	}


	//==============================================================================================
	// MonoBehaviour 関連のメンバ変数・メソッド

	/// <summary>タイトル画面のテクスチャ</summary>
	public Texture2D m_titleTexture = null;

	/// <summary>ジングル音のオーディオクリップ</summary>
	public AudioClip m_startSound = null;

	/// <summary>スタートアップメソッド</summary>
	private void Start()
	{
		m_chapterNames = new string[ ( int ) CHAPTER.NUM ];

		m_chapterNames[ ( int ) CHAPTER.PROLOGUE ] = "プロローグ";
		m_chapterNames[ ( int ) CHAPTER.C1 ]       = "第一章";
		m_chapterNames[ ( int ) CHAPTER.C2 ]       = "第二章";
		m_chapterNames[ ( int ) CHAPTER.C3_0 ]     = "第三章　前半";
		m_chapterNames[ ( int ) CHAPTER.C3_1 ]     = "第三章　後半";
		m_chapterNames[ ( int ) CHAPTER.C4 ]       = "第四章";
		m_chapterNames[ ( int ) CHAPTER.C5 ]       = "第五章";
		m_chapterNames[ ( int ) CHAPTER.EPILOGUE ] = "エピローグ";
	}

	/// <summary>フレーム毎更新メソッド</summary>
	private void Update()
	{
		// ステップ内の遷移チェック
		if ( m_nextStep == STEP.NONE )
		{
			switch ( m_step )
			{
			case STEP.NONE:
				m_nextStep = STEP.SELECT;
				break;

#if !UNITY_EDITOR
			case STEP.SELECT:
				if ( Input.GetMouseButtonDown( 0 ) )
				{
					m_nextStep = STEP.PLAY_JINGLE;
				}
				break;
#endif //!UNITY_EDITOR

			case STEP.PLAY_JINGLE:
				if ( !GetComponent<AudioSource>().isPlaying )
				{
					m_nextStep = STEP.START_GAME;
				}
				break;
			}
		}

		// 状態が遷移したときの初期化
		while ( m_nextStep != STEP.NONE )
		{
			m_step = m_nextStep;
			m_nextStep = STEP.NONE;

			switch ( m_step )
			{
			case STEP.PLAY_JINGLE:
				// ジングル音再生
				GetComponent<AudioSource>().clip = m_startSound;
				GetComponent<AudioSource>().Play();
				break;

			case STEP.START_GAME:
#if !UNITY_EDITOR
				// プロローグから開始
				GlobalParam.getInstance().setStartScriptFiles( "Events/c00_main.txt", "Events/c00_sub.txt" );
#else
				// 選択によって読み込むファイルを変える
				switch ( m_selectedChapter )
				{
				case ( int ) CHAPTER.PROLOGUE:
					GlobalParam.getInstance().setStartScriptFiles( "Events/c00_main.txt", "Events/c00_sub.txt" );
					break;

				case ( int ) CHAPTER.C1:
					GlobalParam.getInstance().setStartScriptFiles( "Events/c01_main.txt", "Events/c01_sub.txt" );
					break;

				case ( int ) CHAPTER.C2:
					GlobalParam.getInstance().setStartScriptFiles( "Events/c02_main.txt", "Events/c02_sub.txt" );
					break;

				case ( int ) CHAPTER.C3_0:
					GlobalParam.getInstance().setStartScriptFiles( "Events/c03_0_main.txt", "Events/c03_0_sub.txt" );
					break;

				case ( int ) CHAPTER.C3_1:
					GlobalParam.getInstance().setStartScriptFiles( "Events/c03_1_main.txt", "Events/c03_1_sub.txt" );
					break;

				case ( int ) CHAPTER.C4:
					GlobalParam.getInstance().setStartScriptFiles( "Events/c04_main.txt", "Events/c04_sub.txt" );
					break;

				case ( int ) CHAPTER.C5:
					GlobalParam.getInstance().setStartScriptFiles( "Events/c05_main.txt", "Events/c05_sub.txt" );
					break;

				case ( int ) CHAPTER.EPILOGUE:
					GlobalParam.getInstance().setStartScriptFiles( "Events/c90_main.txt", "Events/c90_sub.txt" );
					break;
				}
#endif //!UNITY_EDITOR

				// ゲームシーンをロード
				Application.LoadLevel( "GameScene" );

				break;
			}
		}
	}

	/// <summary>GUI ハンドリングメソッド</summary>
	private void OnGUI()
	{
		// タイトル画面
		GUI.DrawTexture( new Rect( 0, 0, m_titleTexture.width, m_titleTexture.height ), m_titleTexture );

#if UNITY_EDITOR
		// デバッグ用の章セレクトボタン
		int x      =  10;
		int y      =  10;
		int width  = 100;
		int height =  20;

		for ( int i = 0; i < ( int ) CHAPTER.NUM; ++i, y += height + 10 )
		{
			if ( GUI.Button( new Rect( x, y, width, height ), m_chapterNames[ i ] ) )
			{
				if ( m_step == STEP.SELECT )
				{
					m_selectedChapter = i;
					m_nextStep        = STEP.PLAY_JINGLE;
				}
			}
		}
#endif //UNITY_EDITOR
	}


	//==============================================================================================
	// 非公開メンバ変数

	/// <summary>現在の状態</summary>
	private STEP m_step = STEP.NONE;

	/// <summary>次に遷移する状態</summary>
	private STEP m_nextStep = STEP.NONE;

	/// <summary>各章の名前</summary>
	private string[] m_chapterNames;

#if UNITY_EDITOR
	/// <summary>デバッグモードで選択した章
	private int m_selectedChapter = 0;
#endif //UNITY_EDITOR
}
