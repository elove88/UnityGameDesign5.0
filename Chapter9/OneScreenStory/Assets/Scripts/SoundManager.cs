
using UnityEngine;


/// <summary>サウンド再生管理クラス</summary>
class SoundManager : MonoBehaviour
{
	//==============================================================================================
	// MonoBehaviour 関連のメンバ変数・メソッド

	/// <summary>再生対象のオーディオクリップ</summary>
	public AudioClip[] m_audioClips = null;

	/// <summary>スタートアップメソッド</summary>
	private void Start()
	{
		// SE と BGM 用の音源を作成
		audioSE  = gameObject.AddComponent< AudioSource >();
		audioBGM = gameObject.AddComponent< AudioSource >();

		// BGM を再生
		audioBGM.clip = getAudioClip( "rpg_ambience01" );
		audioBGM.loop = true;
		audioBGM.Play();
	}


	//==============================================================================================
	// 公開メソッド

	/// <summary>SE を再生する</summary>
	public void playSE( AudioClip clip, bool isLoop = false )
	{
		if ( clip != null )
		{
			audioSE.clip = clip;
			audioSE.loop = isLoop;
			audioSE.Play();
		}
	}

	/// <summary>名前を指定して SE を再生する</summary>
	public void playSE( string name, bool isLoop = false )
	{
		AudioClip clip = getAudioClip( name );
		if ( clip != null )
		{
			playSE( clip, isLoop );
		}
	}

	/// <summary>SE を停止する</summary>
	public void stopSE()
	{
		audioSE.Stop();
	}

	/// <summary>名前からオーディオクリップを取得する</summary>
	public AudioClip getAudioClip( string name )
	{
		AudioClip audioClip = null;
		foreach ( AudioClip clip in m_audioClips )
		{
			if ( name == clip.name )
			{
				audioClip = clip;
				break;
			}
		}

		return audioClip;
	}

	public static SoundManager	get()
	{
		if(instance == null) {
			
			GameObject	go = GameObject.Find("SoundManager");
			
			if(go == null) {
				
				Debug.Log("Can't find \"SoundManager\" GameObject.");
				
			} else {
				
				instance = go.GetComponent<SoundManager>();
			}
		}
		return(instance);
	}
	protected static SoundManager	instance = null;

	//==============================================================================================
	// 非公開メンバ変数

	/// <summary>SE 用の音源オブジェクト</summary>
	private AudioSource audioSE;

	/// <summary>BGM 用の音源オブジェクト</summary>
	private AudioSource audioBGM;
}
