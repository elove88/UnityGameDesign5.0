using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// ダメージのエフェクトと音の再生.
// ----------------------------------------------------------------------------
public class EffectDamage : MonoBehaviour {

	private ParticleSystem[] effects = new ParticleSystem[2];	// 再生するエフェクト.
	private int endOfPlayingCount = 0;					// 再生終了した数.
	private int playingCount = 0;						// 再生開始した数.
	
	private AudioMaker damageAudioMaker;				// ダメージ音メーカー.
	
	void Start ()
	{	
		// ダメージ音メーカーのインスタンスを取得.
		damageAudioMaker =
			GameObject.FindGameObjectWithTag("EffectDamageAudioMaker")
				.GetComponent<AudioMaker>();
		
		// 音の再生.
		if ( damageAudioMaker )
		{
			damageAudioMaker.Play( this.gameObject );
		}
		
		// 子オブジェクトにある全てのエフェクトのオブジェクトを取得.
		effects = GetComponentsInChildren<ParticleSystem>();
		
		// 全てのエフェクトを再生.
		for( int i = 0; i < effects.Length; i++ )
		{
			if ( effects[i] )
			{
				effects[i].Play();
				playingCount++;
			}
		}
	}
	
	void Update () 
	{
		// 再生終了したエフェクトをカウント.
		for( int i = 0; i < effects.Length; i++ )
		{
			if ( !effects[i].isPlaying )
			{
				endOfPlayingCount++;
			}
		}
				
		// 全て再生が終了した場合、オブジェクトを破棄する.
		if ( endOfPlayingCount >= playingCount )
		{
			Destroy( this.gameObject );
		}
		
	}
}
