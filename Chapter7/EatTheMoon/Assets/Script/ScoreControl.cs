using UnityEngine;
using System.Collections;

public class ScoreControl : MonoBehaviour {

	public	Vector3		position;
	public	float		digitOffset;
	public	int			digitNum;
	public	bool		drawZero;

	private SimpleSpriteGUI[]	numTex;

	private	int			targetNum;
	private int			currentNum;
	private float		timer;
	public	Texture[] 	texture;
	public	int			layer	= 0;
	
	public delegate void CallBack();
	public CallBack	CallBackCountUp = null;
	
	// Use this for initialization
	void Start ()
	{
		this.numTex		= new SimpleSpriteGUI[10];
		for( int i = 0; i < 10; i++ )
		{
			this.numTex[i]	= new SimpleSpriteGUI();
			this.numTex[i].create();
			this.numTex[i].setTexture( texture[i] );
		}
		
		this.timer	= 0.0f;
	}

	public void	Update()
	{
		if( this.targetNum > this.currentNum )
		{
			this.timer += Time.deltaTime;
			
			if( timer > 0.05f )
			{
				this.timer	= 0.0f;
				
				// あまりに差があるときは5づつカウントアップする
				if( this.targetNum - this.currentNum > 10 )
				{
					this.currentNum += 5;
				}
				else
				{
					this.currentNum++;
				}
				if( CallBackCountUp != null )
				{
					CallBackCountUp();
				}
			}
		}
	}
	
	public void OnGUI()
	{
		GUI.depth = layer;
		
		drawNum( currentNum, position.x, position.y, position.z, 1.0f );
	}
	
	private void drawNum( int num, float x, float y, float z, float scale )
	{
		//数字の表示
		string	numStr	= num.ToString();
		float	ofs		= 0.0f;
			
		for( int i = 0; i < this.digitNum - numStr.Length; i++ )
		{
			if( this.drawZero )
			{
				this.numTex[ 0 ].setPosition( new Vector3(x + ofs, y, z) );
				this.numTex[ 0 ].setScale( new Vector3( scale, scale, scale ) );
				this.numTex[ 0 ].draw();
			}
			ofs	+= digitOffset;
		}	
			
		for( int i = 0; i < numStr.Length; i++ )
		{
			int	idx = int.Parse( numStr[i].ToString() );
			this.numTex[ idx ].setPosition( new Vector3(x + ofs, y, z) );
			this.numTex[ idx ].setScale( new Vector3( scale, scale, scale ) );
			this.numTex[ idx ].draw();
			ofs += digitOffset;
		}
	}
	
	//表示する数字を設定
	public void setNum( int num )
	{
		if( this.targetNum == this.currentNum )
		{
			this.timer	= 0.0f;
		}
		this.targetNum	= num;
	}
	
	//表示する数字を即時で設定
	public void setNumForce( int num )
	{
		this.targetNum		= num;
		this.currentNum		= num;
	}
}
