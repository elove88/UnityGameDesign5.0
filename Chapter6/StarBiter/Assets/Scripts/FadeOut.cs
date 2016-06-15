using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// 画面をフェードアウト.
//  - alpha(透過)を 0(透過率100%) -> 1(透過率0%) に変化させてフェードアウト効果を再現する.
// ----------------------------------------------------------------------------
public class FadeOut : MonoBehaviour 
{
	private float alphaRate = 0f;			// 透過率.
	private Color textureColor;				// テクスチャの色情報.
	private bool isEnabled = false;
	private string openingSceneName = "opening";	// オープニングシーン名.

	void Start () 
	{
		// テクスチャを画面一杯に広げる.
        GetComponent<GUITexture>().pixelInset = new Rect(0, 0, 480, 640);
		
		// FadeOutされた状態.
		textureColor = GetComponent<GUITexture>().color;
		textureColor.a = alphaRate;
		GetComponent<GUITexture>().color = textureColor;
	}

	void Update () 
	{
		if ( isEnabled )
		{
			// 透過が100%に達していない?.
			if ( alphaRate < 1 )
			{
				// FadeOut.
				alphaRate += 0.007f;
				textureColor.a = alphaRate;
				GetComponent<GUITexture>().color = textureColor;
			}
			else
			{
				// ゲームシーンを呼び出す.
				Application.LoadLevel( openingSceneName );
			}
		}
	}
	
	public void SetEnable()
	{
		StartCoroutine( WaitAndEnable( 8f ) );
	}
	
	IEnumerator WaitAndEnable( float waitForSeconds )
	{
		// 指定した時間を待つ.
		yield return new WaitForSeconds( waitForSeconds );
		
		isEnabled = true;
	}
}
