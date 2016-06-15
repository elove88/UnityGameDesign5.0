
using UnityEngine;


/// <summary>画面の暗転を管理するクラス</summary>
class FadeInOutManager : MonoBehaviour
{
	//==============================================================================================
	// MonoBehaviour 関連のメンバ変数・メソッド

	/// <summary>暗転役のテクスチャ</summary>
	public GUITexture m_fadeInOutObject = null;

	/// <summary>スタートアップメソッド</summary>
	private void Start()
	{
		m_fadeInOutObject.color = new Color( 0.0f, 0.0f, 0.0f, m_currentAlpha );
		m_fadeInOutObject.enabled = true;
	}

	/// <summary>フレーム毎更新メソッド</summary>
	private void Update()
	{
		if ( m_isFading )
		{
			if ( Time.time >= m_endTime )
			{
				// フェード時間経過後初めての Update

				// アルファ値を目的値に変更
				m_fadeInOutObject.color = new Color( 0.0f, 0.0f, 0.0f, m_currentAlpha = m_destinationAlpha );

				// フェードインが完了したときは GUITexture を無効にする
				if ( m_destinationAlpha < 0.25f )	// float なので == 0.0f にはしないでおく
				{
					m_fadeInOutObject.enabled = false;
				}

				// フェード終了
				m_isFading = false;
			}
			else
			{
				// 進行度 (0.0～1.0)
				float ratio = Mathf.InverseLerp( m_beginTime, m_endTime, Time.time );

				// アルファ値変更
				m_fadeInOutObject.color = new Color( 0.0f, 0.0f, 0.0f,
				                                     Mathf.Lerp( m_currentAlpha, m_destinationAlpha, ratio ) );
			}
		}
	}


	//==============================================================================================
	// 公開メソッド

	/// <summary>フェードを行う</summary>
	public void fadeTo( float destinationAlpha, float duration )
	{
		m_destinationAlpha = destinationAlpha;
		m_beginTime        = Time.time;
		m_endTime          = m_beginTime + duration;
		m_isFading         = true;

		// GUITexture を有効にする
		m_fadeInOutObject.enabled = true;
	}

	/// <summary>フェードアウトを行う</summary>
	public void fadeOut( float duration )
	{
		fadeTo( 1.0f, duration );
	}

	/// <summary>フェードインを行う</summary>
	public void fadeIn( float duration )
	{
		fadeTo( 0.0f, duration );
	}

	/// <summary>フェード中かどうかを返す</summary>
	public bool isFading()
	{
		return m_isFading;
	}


	//==============================================================================================
	// 非公開メンバ変数

	/// <summary>現在のアルファ値</summary>
	private float m_currentAlpha = 1.0f;

	/// <summary>目標のアルファ値</summary>
	private float m_destinationAlpha = 0.0f;

	/// <summary>フェードの開始時間</summary>
	private float m_beginTime = 0.0f;

	/// <summary>フェードの終了時間</summary>
	private float m_endTime = 0.0f;

	/// <summary>フェード中かどうか</summary>
	private bool m_isFading = false;
}
