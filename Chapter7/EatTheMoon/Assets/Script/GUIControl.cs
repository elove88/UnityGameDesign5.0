using UnityEngine;
using System.Collections;

public class GUIControl : MonoBehaviour {

	public Texture	MaskTexture = null;					//!< マスク画像（画面の両サイド）.

	public Texture	StomachRedTexture = null;			//!< 胃袋（赤）.
	public Texture	StomachGreenTexture = null;			//!< 胃袋（緑）.
	public Texture	StomachFrameTexture = null;			//!< 胃袋（わく）.
	public Texture	playerTexture = null;
	
	public SimpleSpriteGUI	player_stock = null;
	public SimpleSpriteGUI	mask;
	public SimpleSpriteGUI	stomach_red;
	public SimpleSpriteGUI	stomach_green;
	public SimpleSpriteGUI	stomach_frame;

	public float	stomach_rate = 1.0f;				//!< 胃袋の緑の部分の比率.

	public Vector3	stomach_position;					//!< 胃袋の表示位置.

	public bool		is_disp_goal = false;				//!< 「GOAL」表示する？.
	public bool		is_disp_gameover = false;			//!< 「GAME OVER」表示する？.
	
	public float	num_offset = 32.0f;
	public Vector3	stock_position;
	public Vector3	score_position;
	public Vector3	height_position;
	
	private SceneControl		scene_control = null;
	private ScoreControl		score;						//!< スコア表示用
	private ScoreControl		height;						//!< 登った段数表示用
	
	public	Texture[]	texNum;
	
	
	// ---------------------------------------------------------------- //

	// Use this for initialization
	void Start () {

		this.scene_control = GameObject.FindGameObjectWithTag("SceneControl").GetComponent<SceneControl>();
		
		this.mask = new SimpleSpriteGUI();

		this.stomach_red = new SimpleSpriteGUI();
		this.stomach_green = new SimpleSpriteGUI();
		this.stomach_frame = new SimpleSpriteGUI();
		this.player_stock = new SimpleSpriteGUI();
		
		this.mask.create();
		this.mask.setTexture(this.MaskTexture);

		//

		this.stomach_position.x = 320.0f - 56.0f;
		this.stomach_position.y = 2.0f;
		this.stomach_position.z = 0.0f;

		this.stomach_red.create();
		this.stomach_red.setPosition(this.stomach_position);
		this.stomach_red.setTexture(this.StomachRedTexture);

		this.stomach_green.create();
		this.stomach_green.setPosition(this.stomach_position);
		this.stomach_green.setTexture(this.StomachGreenTexture);

		this.stomach_frame.create();
		this.stomach_frame.setPosition(this.stomach_position);
		this.stomach_frame.setTexture(this.StomachFrameTexture);
		
		this.player_stock.create();
		this.player_stock.setTexture(this.playerTexture);		
		
		this.score				= gameObject.AddComponent<ScoreControl>();
		this.score.texture		= texNum;
		this.score.digitOffset	= num_offset;
		this.score.digitNum		= 4;
		this.score.drawZero		= true;
		
		this.height				= gameObject.AddComponent<ScoreControl>();
		this.height.texture		= texNum;
		this.height.digitOffset	= 32;
		this.height.digitNum	= 4;
		this.height.drawZero	= true;
		//

		this.is_disp_goal = false;
		this.is_disp_gameover = false;
	}
	
	// Update is called once per frame
	void Update ()
	{
		this.score.position		= score_position;
		this.score.digitOffset	= num_offset;
		this.score.setNum( this.scene_control.stack_control.score );
	
		this.height.position	= height_position;
		this.height.digitOffset	= num_offset;
		this.height.setNum( this.scene_control.height_level );
		if(this.stomach_rate > 0.0f) {

			Vector3		p;
	
			p = this.stomach_position;
			p.y -= (1.0f - this.stomach_rate)*this.stomach_green.texture.height/2.0f;
	
			this.stomach_green.setPosition(p);
			this.stomach_green.setScale(new Vector3(1.0f, this.stomach_rate, 1.0f));
			this.stomach_green.is_visible = true;

		} else {

			this.stomach_green.is_visible = false;
		}
	}

	void OnGUI()
	{
		this.mask.draw();

		this.stomach_red.draw();
		this.stomach_green.draw();
		this.stomach_frame.draw();
		
		for( int i = 0; i < this.scene_control.player_stock; i++ )
		{
			Vector3	pos = this.stock_position;
			
			pos.x	+= i * 40;
			this.player_stock.setPosition( pos );
			this.player_stock.draw();
		}
		
		if(this.is_disp_goal) {

			GUI.contentColor = Color.black;

			GUI.Label(new Rect(320.0f - 10.0f*2.0f, 100.0f, 100.0f, 20.0f), "GOAL");

			GUI.contentColor = Color.white;
		}

		if(this.is_disp_gameover) {

			GUI.contentColor = Color.black;

			GUI.Label(new Rect(320.0f - 10.0f*4.5f, 100.0f, 100.0f, 20.0f), "GAME OVER");

			GUI.contentColor = Color.white;
		}
	}
}
