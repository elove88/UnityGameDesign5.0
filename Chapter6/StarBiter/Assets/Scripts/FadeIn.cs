using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// 画面をフェードイン.
//  - alpha(透過)を 1(透過率0%) -> 0(透過率100%) に変化させてフェードイン効果を再現する.
// ----------------------------------------------------------------------------
public class FadeIn : MonoBehaviour {
	
	private float alphaRate = 1f;			// 透過率.
	private Color textureColor;				// テクスチャの色情報.

	void Start () {
	
		// テクスチャを画面一杯に広げる.
        GetComponent<GUITexture>().pixelInset = new Rect(0, 0, 480, 640);
		
		// FadeOutされた状態.
		textureColor = GetComponent<GUITexture>().color;
		textureColor.a = alphaRate;
		GetComponent<GUITexture>().color = textureColor;
	}

	void Update () {
	
		// 透過が100%に達していない?.
		if ( alphaRate > 0 )
		{
			// FadeIn.
			alphaRate -= 0.007f;
			textureColor.a = alphaRate;
			GetComponent<GUITexture>().color = textureColor;
		}
		
	}
}
