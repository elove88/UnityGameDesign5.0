using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// 音を再生するオブジェクトを破棄する.
//  - (ノイズが入る為)再生終了後に破棄する.
// ----------------------------------------------------------------------------
public class AudioBreaker : MonoBehaviour {
	
	private bool isDestroy = false;
	
	void Update ()
	{
		// 破壊する時のみ.
		if ( isDestroy )
		{
			// 音が再生終了しているか確認.
			if ( !GetComponent<AudioSource>().isPlaying )
			{
				// 破棄する.
				Destroy( this.gameObject );
			}
		}
	}
	
	// ------------------------------------------------------------------------
	// 破壊開始.
	// ------------------------------------------------------------------------
	public void SetDestroy()
	{
		isDestroy = true;
	}
}
