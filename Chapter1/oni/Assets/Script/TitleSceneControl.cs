using UnityEngine;
using System.Collections;

public class TitleSceneControl : MonoBehaviour {

	// 進行状態.
	public enum STEP {

		NONE = -1,

		TITLE = 0,				// タイトル表示（ボタン押し待ち）.
		WAIT_SE_END,			// スタートのSEが終わるのを待ってる.
		FADE_WAIT,				// フェード終了待ち.

		NUM,
	};

	private STEP	step = STEP.NONE;
	private STEP	next_step = STEP.NONE;
	private float	step_timer = 0.0f;

	public Texture	TitleTexture = null;			// 『はじめ』のテクスチャー
	public Texture	StartButtonTexture = null;			// 『はじめ』のテクスチャー
	private FadeControl	fader = null;					// フェードコントロール	
	
	// タイトルの画像
	public float	title_texture_x		=    0.0f;
	public float	title_texture_y		=  100.0f;
	
	// 『開始』の文字.
	public float	start_texture_x		=    0.0f;
	public float	start_texture_y		= -100.0f;
	
	// 始めが押された時にアニメーションをする時間
	private static float	TITLE_ANIME_TIME = 0.1f;
	private static float	FADE_TIME = 1.0f;
	
	// -------------------------------------------------------------------------------- //

	void Start () {
		// プレイヤーを操作不能にする.
		PlayerControl	player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControl>();
		player.UnPlayable();
		
		// フェードコントロールの追加
		this.fader = gameObject.AddComponent<FadeControl>();
		//fader	= gameObject.AddComponent();
		this.fader.fade( 1.0f, new Color( 0.0f, 0.0f, 0.0f, 1.0f ), new Color( 0.0f, 0.0f, 0.0f, 0.0f) );
		
		this.next_step = STEP.TITLE;
	}

	void Update ()
	{
		this.step_timer += Time.deltaTime;

		// 次の状態に移るかどうかを、チェックする.
		switch(this.step) {

			case STEP.TITLE:
			{
				// マウスがクリックされた
				//
				if(Input.GetMouseButtonDown(0)) {

					this.next_step = STEP.WAIT_SE_END;
				}
			}
			break;

			case STEP.WAIT_SE_END:
			{
				// SE の再生が終わったらフードアウト
			
				bool	to_finish = true;

				do {

					if(!this.GetComponent<AudioSource>().isPlaying) {

						break;
					}

					if(this.GetComponent<AudioSource>().time >= this.GetComponent<AudioSource>().clip.length) {

						break;
					}

					to_finish = false;

				} while(false);

				if(to_finish) {

					this.fader.fade( FADE_TIME, new Color( 0.0f, 0.0f, 0.0f, 0.0f ), new Color( 0.0f, 0.0f, 0.0f, 1.0f) );
				
					this.next_step = STEP.FADE_WAIT;
				}
			}
			break;
			
			case STEP.FADE_WAIT:
			{
				// フェードが終了したら、ゲームシーンをロードして終了.
				if(!this.fader.isActive()) 
				{
					Application.LoadLevel("GameScene");
				}
			}
			
			break;
		}

		// 状態がかわったときの初期化処理.

		if(this.next_step != STEP.NONE) {

			switch(this.next_step) {

				case STEP.WAIT_SE_END:
				{
					// 開始のSEを鳴らす.
					this.GetComponent<AudioSource>().Play();
				}
				break;
			}

			this.step = this.next_step;
			this.next_step = STEP.NONE;

			this.step_timer = 0.0f;
		}

		// 各状態での実行処理.

		/*switch(this.step) {

			case STEP.TITLE:
			{
			}
			break;
		}*/

	}

	void OnGUI()
	{	
		GUI.depth = 1;
		
		float	scale	= 1.0f;
		
		if( this.step == STEP.WAIT_SE_END )
		{
			float	rate = this.step_timer / TITLE_ANIME_TIME;
			
			scale = Mathf.Lerp( 2.0f, 1.0f, rate );
		}
		
		TitleSceneControl.drawTexture(this.StartButtonTexture, start_texture_x, start_texture_y, scale, scale, 0.0f, 1.0f);		
		TitleSceneControl.drawTexture(this.TitleTexture, title_texture_x, title_texture_y, 1.0f, 1.0f, 0.0f, 1.0f);		
	}

	public static void drawTexture(Texture texture, float x, float y, float scale_x = 1.0f, float scale_y = 1.0f, float angle = 0.0f, float alpha = 1.0f)
	{
		Vector3		position;
		Vector3		scale;
		Vector3		center;

		position.x =  x + Screen.width/2.0f;
		position.y = -y + Screen.height/2.0f;
		position.z = 0.5f;

		scale.x = scale_x;
		scale.y = scale_y;
		scale.z = 1.0f;

		center.x = texture.width/2.0f;
		center.y = texture.height/2.0f;
		center.z = 0.0f;

		Matrix4x4	matrix = Matrix4x4.identity;

		matrix *= Matrix4x4.TRS(position - center, Quaternion.identity, Vector3.one);

		// テクスチャーの中心を基準に、回転とスケールをかける.
		//
		matrix *= Matrix4x4.TRS( center,           Quaternion.identity, Vector3.one);
		matrix *= Matrix4x4.TRS(Vector3.zero,      Quaternion.AngleAxis(angle, Vector3.forward), scale);
		matrix *= Matrix4x4.TRS(-center,           Quaternion.identity, Vector3.one);

		GUI.matrix = matrix;
		GUI.color  = new Color(1.0f, 1.0f, 1.0f, alpha);

		Rect	rect = new Rect(0.0f, 0.0f, texture.width, texture.height);

		GUI.DrawTexture(rect, texture);
	}
}
