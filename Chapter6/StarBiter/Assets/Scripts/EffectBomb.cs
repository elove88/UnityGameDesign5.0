using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// 爆発のエフェクトと音の再生.
// ----------------------------------------------------------------------------
public class EffectBomb : MonoBehaviour {
	
	private ParticleSystem[] effects = new ParticleSystem[2];	// 再生するエフェクト.
	private int endOfPlayingCount = 0;					// 再生終了した数.
	private int playingCount = 0;						// 再生開始した数.
	
	private AudioMaker bombAudioMaker;					// 爆発音メーカー.
	private BattleSpaceController battleSpaceContoller;	// 戦闘空間.
	
	private float speed = 0f;
	private bool isMoving = false;
	
	void Start () {
		
		// 戦闘空間のインスタンスを取得.
		battleSpaceContoller =
			GameObject.FindGameObjectWithTag("BattleSpace")
				.GetComponent<BattleSpaceController>();
		
		// 爆発音メーカーのインスタンスを取得.
		bombAudioMaker = 
			GameObject.FindGameObjectWithTag("EffectBombAudioMaker")
				.GetComponent<AudioMaker>();
		
		// 音の再生.
		if ( bombAudioMaker )
		{
			bombAudioMaker.Play( this.gameObject );
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
		// 破壊前の動きを加算.
		if ( isMoving )
		{
			transform.Translate( transform.forward * speed * Time.deltaTime );
			// 徐々にスピードを減衰.
			if ( speed > 0 )
			{
				speed -= 0.1f;
			}
		}
		
		// 戦闘空間のスクロール方向を加える.
		transform.position -= battleSpaceContoller.GetAdditionPos();
		
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
	
	// ------------------------------------------------------------------------
	// エフェクトが動くスピードを設定する.
	//  - 破壊前の敵機のスピードを利用する.
	// ------------------------------------------------------------------------
	public void SetIsMoving( float speed )
	{
		this.speed = speed * 40f;
		if ( this.speed > 5f )
		{
			this.speed = 5f;
		}
		this.isMoving = true;
	}

}
