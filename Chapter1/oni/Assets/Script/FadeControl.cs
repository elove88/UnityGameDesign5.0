using UnityEngine;
using System.Collections;

public class FadeControl : MonoBehaviour
{
    private	Texture2D texture;			// フェード処理に使うテクスチャ

    private float	timer;				// 現在の時間
	private float	fadeTime;			// フェードにかかる時間
	private	Color	colorStart;			// フェード開始時の色
	private	Color	colorTarget;		// フェード終了時の色
	
    void Awake()
    {
		this.texture		= new Texture2D(1, 1);
		this.timer			= 0.0f;
		this.fadeTime		= 0.0f;
		this.colorStart		= new Color( 0.0f, 0.0f, 0.0f, 0.0f );
		this.colorTarget	= new Color( 0.0f, 0.0f, 0.0f, 0.0f );
    }
    
 	void	Update()
	{
		this.timer += Time.deltaTime;
	}
	
	void OnGUI()
    {
		float	rate;
		
		if( fadeTime == 0.0f )
		{
			rate = 1.0f;
		}
		else
		{
			rate = this.timer / this.fadeTime;
		}
		Color	color = Color.Lerp( this.colorStart, this.colorTarget, rate );
		
		if( color.a <= 0.0f )
		{
			return;
		}
		
		GUI.depth = 0;
		
        this.texture.SetPixel(0, 0, color);
        this.texture.Apply();
        
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), this.texture);
    }
    
    void OnApplicationQuit()
    {
        Destroy(texture);
    }
	
	public void fade( float time, Color start, Color target )
	{
		this.fadeTime		= time;
		this.timer			= 0.0f;
		this.colorStart		= start;
		this.colorTarget	= target;
	}
	
	public bool isActive()
	{
		return ( this.timer > this.fadeTime ) ? false : true;
	}
}