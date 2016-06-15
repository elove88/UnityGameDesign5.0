using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// デバッグツール.
//  - P キーでゲームの進行を一時停止する.
// ----------------------------------------------------------------------------
public class DebugToolStopMotion : MonoBehaviour {

	private float origTimeScale;
	
	// ほかのオブジェクトより早くtimeScaleを取得する.
	void Awake()
	{
		origTimeScale = Time.timeScale;
	}
	
	void Update () 
	{
		// クリック.
		if ( Input.GetKeyDown(KeyCode.P) )
		{
			if ( Time.timeScale != 0 )
			{
				Time.timeScale = 0;
			}
			else
			{
				Time.timeScale = origTimeScale;
			}
		}
	}
}
