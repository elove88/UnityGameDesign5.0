using UnityEngine;
using System.Collections;

public class ScoreControl : MonoBehaviour {

	public	Vector3		pos_ui;
	public	Vector3		pos_unit;

	public	Vector3		pos_num;
	public	float		num_digitOffset;
	
	private	float		digitOffset;
	public	int			digitNum;
	public	bool		drawZero;
	
	public	Texture		texBase;
	public	Texture		texUnit;
	public	Texture		texNum0;
	public	Texture		texNum1;
	public	Texture		texNum2;
	public	Texture		texNum3;
	public	Texture		texNum4;
	public	Texture		texNum5;
	public	Texture		texNum6;
	public	Texture		texNum7;
	public	Texture		texNum8;
	public	Texture		texNum9;
	public	Texture		texHighScore;
	
	public	AudioClip	CountUpSound = null;			// カウントアップ.
	public	AudioClip[]	CountUpSounds = null;			// カウントアップ.
	public	AudioSource	count_up_audio;					// カウントアップ音再生用.
	private	int			CountLevel;

	private SimpleSpriteGUI		uiTex;
	private SimpleSpriteGUI		unitTex;
	private SimpleSpriteGUI		highScoreTex;
	private SimpleSpriteGUI[]	numTex;
	
	private	int			targetNum;
	private int			currentNum;
	private float		timer;
	
	// Use this for initialization
	void Start ()
	{
		this.uiTex	= new SimpleSpriteGUI();
		this.uiTex.create();
		this.uiTex.setPosition( pos_ui );
		this.uiTex.setTexture( texBase );
		
		this.unitTex	= new SimpleSpriteGUI();
		this.unitTex.create();
		this.unitTex.setPosition( pos_unit );
		this.unitTex.setTexture( texUnit );
		
		this.numTex		= new SimpleSpriteGUI[10];
		for( int i = 0; i < 10; i++ )
		{
			this.numTex[i]	= new SimpleSpriteGUI();
			this.numTex[i].create();
		}
	
		this.numTex[0].setTexture( texNum0 );
		this.numTex[1].setTexture( texNum1 );
		this.numTex[2].setTexture( texNum2 );
		this.numTex[3].setTexture( texNum3 );
		this.numTex[4].setTexture( texNum4 );
		this.numTex[5].setTexture( texNum5 );
		this.numTex[6].setTexture( texNum6 );
		this.numTex[7].setTexture( texNum7 );
		this.numTex[8].setTexture( texNum8 );
		this.numTex[9].setTexture( texNum9 );
	
		this.count_up_audio = this.gameObject.AddComponent<AudioSource>();
	
		this.timer	= 0.0f;
	}

	public void	Update()
	{
		if( this.targetNum > this.currentNum )
		{
			this.timer += Time.deltaTime;
			
			if( timer > 0.1f )
			{
				// ランダムでSEを鳴らす.
				int	idx = Random.Range(0, this.CountUpSounds.Length);
		
				this.count_up_audio.PlayOneShot( this.CountUpSounds[idx] );

				this.timer	= 0.0f;
				
				// あまりに差があるときは5づつカウントアップする.
				if( this.targetNum - this.currentNum > 10 )
				{
					this.currentNum += 5;
				}
				else
				{
					this.currentNum++;
				}
				CountLevel++;
			}
		}
		else
		{
			CountLevel	= 0;
		}
	}
	
	public void draw()
	{
		//下地の表示
		this.uiTex.setPosition( pos_ui );
		this.uiTex.draw();
		
		//単位の表示
		this.unitTex.setPosition( pos_unit );
		this.unitTex.setScale( new Vector3( 1.0f, 1.0f, 1.0f ) );
		this.unitTex.draw();
		
		// 得点の表示
		digitOffset	= num_digitOffset;
		if( this.targetNum != this.currentNum )
		{
			float	sclae	= 2.5f - 1.5f * ( this.timer * 10.0f );
			drawNum( currentNum, pos_num.x, pos_num.y, pos_num.z, sclae );
		}
		else
		{
			drawNum( currentNum, pos_num.x, pos_num.y, pos_num.z, 1.0f );
		}
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
	
	public bool isActive()
	{
		return ( this.targetNum != this.currentNum ) ? true : false;
	}
}
