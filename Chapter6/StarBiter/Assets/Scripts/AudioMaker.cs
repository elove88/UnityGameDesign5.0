using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// 音の同時発生をコントロールする.
//  - 同時発生数に上限を設ける.
//  - 音の停止時に発生するノイズを抑制する.
// ----------------------------------------------------------------------------
public class AudioMaker : MonoBehaviour {

	public int maxPlayingCount = 0;						// 最大同時再生数.
	public GameObject audioObject;						// 再生する音.
	
	private bool initialized = false;					// 初期化済み.
	private GameObject[] AudioInstances;				// 再生する音のインスタンス.
	
	void Start() 
	{
		// 音がない場合は終了.
		if ( !audioObject )
		{
			return;
		}
			
		// 再生する音の準備(最大同時発生数の数だけ準備).
		AudioInstances = new GameObject[maxPlayingCount];
		for( int i = 0; i < maxPlayingCount; i++ )
		{
			GameObject audioInstance = Instantiate(
				audioObject,
				Vector3.zero,
				new Quaternion( 0, 0, 0, 0 ) ) as GameObject;
			AudioInstances[i] = audioInstance;
		}
		
		// 初期化完了の状態を保持する.
		if ( AudioInstances.Length > 0 )
		{
			initialized = true;
		}
	}
	
	// 音の再生.
	public void Play( GameObject target )
	{
		if ( initialized && target )
		{
			bool canPlay = false;
			for( int i = 0; i < maxPlayingCount; i++ )
			{
				AudioSource audioSource = AudioInstances[i].GetComponent<AudioSource>();
				
				// 空き(再生していない音)を探す.
				if ( !audioSource.isPlaying )
				{
					// 音の再生.
					canPlay = true;
					audioSource.Play();
					break;
				}
			}
			// 同時発生数に達していた為に再生できなかった.
			if ( !canPlay )
			{
				// 再生中の音の一つを停止し、新たに再生する
				
				// ------------------------------------------------------------
				// ノイズ対策.
				//  - AudioをStop(又は再生中に再度Play)するとノイズが入る為、以下の対策をする.
				//    1. 一つの音をMuteする.
				//    2. 新たにオブジェクトを作成する.
				//    3. 再生する.
				// ------------------------------------------------------------
				
				// Mute、削除
				AudioInstances[0].GetComponent<AudioSource>().mute = true;
				AudioInstances[0].GetComponent<AudioBreaker>().SetDestroy();
				
				// 先頭要素を削除した後に、配列を前につめる
				for( int i = 0; i < maxPlayingCount - 1; i++ )
				{
					AudioInstances[i] = AudioInstances[i + 1];
				}
				
				// 新たにオブジェクトを作成し、要素の最後に追加後、再生する.
				GameObject audioInstance = Instantiate(
					audioObject,
					Vector3.zero,
					new Quaternion( 0, 0, 0, 0 ) ) as GameObject;
				AudioInstances[maxPlayingCount - 1] = audioInstance;
				AudioInstances[maxPlayingCount - 1].GetComponent<AudioSource>().Play();
			}
		}
	}
}
