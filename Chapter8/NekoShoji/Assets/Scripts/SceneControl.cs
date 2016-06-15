using UnityEngine;
using System.Collections;

public class SceneControl : MonoBehaviour {

	public enum STEP {

		NONE = -1,

		GAME = 0,			// ゲーム中.
		GAMEOVER,			// ゲームオーバー.
		CLEAR,				// ゲームクリアー.

		NUM,
	};

	public STEP			step      = STEP.NONE;
	public STEP			next_step = STEP.NONE;
	public float		step_timer = 0.0f;

	// ---------------------------------------------------------------- //

	private int	combo_count = 0;		// コンボカウント　連続して紙をやぶった回数.
		
	// Life Setting
	public Texture2D LifeTexture = null;	
	public static int init_lifecnt = 5;
	public static int lifecnt = init_lifecnt;

	public Texture2D FailedTexture = null;
	public Texture2D Combo01Texture = null;
	public Texture2D Combo02Texture = null;
	public Texture2D EndTexture = null;
	
	private float TextureWidth  = 0.0f;
	private float TextureHeight = 0.0f;
	private float TextureScale = 200.0f;

	private float TextureX  = 10.0f;
	private float TextureY = 480.0f;
	private float TexturePosScale = 600.0f;
	
	// ---------------------------------------------------------------- //

	public bool		clearflg = false;			// 「おしまい」表示中？.
	public float	clear_timer = 0.0f;

	public enum COMBO {

		FAILED = -1,
		NORMAL = 0,
		
		CHAIN01,
		CHAIN02,
	};

	public COMBO combo = COMBO.NORMAL;

	// ---------------------------------------------------------------- //
	// Audio
	public AudioClip COMBO_SOUND_01 = null;
	public AudioClip COMBO_SOUND_02 = null;
	public AudioClip COMBO_SOUND_03 = null;
	
	public AudioClip CLEAR_SOUND = null;
	public AudioClip CLEAR_NEKO_SOUND = null;
	public AudioClip CLEAR_LOOP_SOUND = null;
	public AudioClip GAMEOVER_SOUND = null;
	public AudioClip GAMEOVER_NEKO_SOUND = null;

	public NekoControl	neko_control = null;
	public RoomControl	room_control = null;

	// ---------------------------------------------------------------- //
	// Syouji Setting
	public Transform syouji = null;
	public Transform syouji_paper = null;
	public GameObject syouji_steel = null;
	private int SyoujiCnt = 0;
	
	// ---------------------------------------------------------------- //

	// Use this for initialization
	void Start () {

		this.neko_control = GameObject.FindGameObjectWithTag("NekoPlayer").GetComponent<NekoControl>();

		this.room_control = GameObject.FindGameObjectWithTag("RoomControl").GetComponent<RoomControl>();

		this.clearflg = false;
	}
	
	// Update is called once per frame
	void Update () {

		this.step_timer += Time.deltaTime;

		// ---------------------------------------------------------------- //
		// 次の状態に移るかどうかを、チェックする.

		if(this.next_step == STEP.NONE) {

			switch(this.step) {
	
				case STEP.NONE:
				{
					this.next_step = STEP.GAME;
				}
				break;

				case STEP.GAME:
				{
					int		shoji_num = this.getPaperNum();

					if(shoji_num == 0) {

						this.next_step = STEP.CLEAR;
					}

					if(this.getLifeCount() <= 0) {

						this.next_step = STEP.GAMEOVER;
					}
				}
				break;
			}
		}

		// ---------------------------------------------------------------- //
		// 状態が遷移したときの初期化.

		if(this.next_step != STEP.NONE) {

			switch(this.next_step) {
	
				case STEP.GAME:
				{
					this.clearComboCount();
					this.clear_timer = 0.0f;
					lifecnt = init_lifecnt;
				}
				break;

				case STEP.CLEAR:
				{
					// 自動運転開始.
					this.neko_control.beginAutoDrive();
					StartCoroutine("gameClear");
				}
				break;

				case STEP.GAMEOVER:
				{
					// 「おしまい」をすぐに表示.
					this.clearflg = true;
				}
				break;
			}

			this.step      = this.next_step;
			this.next_step = STEP.NONE;

			this.step_timer = 0.0f;
		}

		// ---------------------------------------------------------------- //
		// 各状態での実行処理.

		switch(this.step) {

			case STEP.CLEAR:
			{
				if(!this.clearflg) {

					// 一定時間たったら、「おしまい」を自動的に表示.
					if(this.step_timer > CLEAR_NEKO_SOUND.length + CLEAR_SOUND.length + CLEAR_LOOP_SOUND.length) {

						this.clearflg = true;
					}
				}
			}
			break;
			
		}

		// ---------------------------------------------------------------- //
	}

	
	void OnGUI () {

		if(!LifeTexture) {
			
			Debug.LogError("A Texture is not assigned.");
			return;
		}

		for ( int i = 0; i < lifecnt - 1; i++ ) {
		
			GUI.Label(new Rect(10, 4*Screen.height/5-25*i, LifeTexture.width/2, LifeTexture.height/2), LifeTexture);
			
		}
		
		if ( lifecnt > 0 ) {
			
			switch(this.combo) {
	
				case COMBO.FAILED:
				{
					this.TextureWidth  -= this.TextureScale*Time.deltaTime;
					this.TextureWidth   = Mathf.Clamp(this.TextureWidth, FailedTexture.width/8, Combo02Texture.width/6);
					this.TextureHeight -= this.TextureScale*Time.deltaTime;
					this.TextureHeight  = Mathf.Clamp(this.TextureHeight, FailedTexture.height/8, Combo02Texture.height/6);

					this.TextureX += this.TexturePosScale*Time.deltaTime;
					this.TextureX  = Mathf.Clamp(this.TextureX, 0, 10);
					this.TextureY += this.TexturePosScale*Time.deltaTime;
					this.TextureY  = Mathf.Clamp(this.TextureY, 4*Screen.height/5-35*(lifecnt - 1), 4*Screen.height/5-25*(lifecnt - 1));
				
					GUI.Label(new Rect(this.TextureX, this.TextureY, this.TextureWidth, this.TextureHeight), FailedTexture);
				}
				break;

				case COMBO.NORMAL:
				{
					this.TextureWidth  -= this.TextureScale*Time.deltaTime;
					this.TextureWidth   = Mathf.Clamp(this.TextureWidth, LifeTexture.width/2, Combo02Texture.width/6);
					this.TextureHeight -= this.TextureScale*Time.deltaTime;
					this.TextureHeight  = Mathf.Clamp(this.TextureHeight, LifeTexture.height/2, Combo02Texture.height/6);

					this.TextureX += this.TexturePosScale*Time.deltaTime;
					this.TextureX  = Mathf.Clamp(this.TextureX, 0, 10);
					this.TextureY += this.TexturePosScale*Time.deltaTime;
					this.TextureY  = Mathf.Clamp(this.TextureY, 4*Screen.height/5-35*(lifecnt - 1), 4*Screen.height/5-25*(lifecnt - 1));
				
					GUI.Label(new Rect(this.TextureX, this.TextureY, this.TextureWidth, this.TextureHeight), LifeTexture);
				}
				break;

				case COMBO.CHAIN01:
				{
					this.TextureWidth  += this.TextureScale*Time.deltaTime;
					this.TextureWidth   = Mathf.Clamp(this.TextureWidth, LifeTexture.width/2, Combo01Texture.width/6);
					this.TextureHeight += this.TextureScale*Time.deltaTime;
					this.TextureHeight  = Mathf.Clamp(this.TextureHeight, LifeTexture.height/2, Combo01Texture.height/6);

					this.TextureX -= this.TexturePosScale*Time.deltaTime;
					this.TextureX  = Mathf.Clamp(this.TextureX, 7, 10);
					this.TextureY -= this.TexturePosScale*Time.deltaTime;
					this.TextureY  = Mathf.Clamp(this.TextureY, 4*Screen.height/5-21*lifecnt, 4*Screen.height/5-14*lifecnt);
				
					GUI.Label(new Rect(this.TextureX, this.TextureY, this.TextureWidth, this.TextureHeight), Combo01Texture);
				}
				break;

				case COMBO.CHAIN02:
				{
					this.TextureWidth  += this.TextureScale*Time.deltaTime;
					this.TextureWidth   = Mathf.Clamp(this.TextureWidth, Combo01Texture.width/6, Combo02Texture.width/4);
					this.TextureHeight += this.TextureScale*Time.deltaTime;
					this.TextureHeight  = Mathf.Clamp(this.TextureHeight, Combo01Texture.height/6, Combo02Texture.height/4);

					this.TextureX -= this.TexturePosScale*Time.deltaTime;
					this.TextureX  = Mathf.Clamp(this.TextureX, 0, 7);
					this.TextureY -= this.TexturePosScale*Time.deltaTime;
					this.TextureY  = Mathf.Clamp(this.TextureY, 4*Screen.height/5-28*lifecnt, 4*Screen.height/5-24*lifecnt);

					GUI.Label(new Rect(this.TextureX, this.TextureY, this.TextureWidth, this.TextureHeight), Combo02Texture);
				}
				break;
			}
		}

		// Count SyoujiPaper from Tags
		SyoujiCnt = this.getPaperNum();
		
		if((SyoujiCnt <= 0) || (lifecnt <=0)) {

			if ( Input.GetMouseButton(0) ) {
	
				this.clearflg = true;

			}

			if (this.clearflg) {
	
				this.clear_timer += Time.deltaTime;
	
				GUI.Label(new Rect(64, 112, EndTexture.width, EndTexture.height), EndTexture);

				if ( (Input.GetMouseButton(0)) && (clear_timer > 1.0f)) {

					Application.LoadLevel("TitleScene");
				}
			}
		}

		//
	}
	
	void applyDamage(int damage) {

		this.clearComboCount();
		lifecnt -= damage;
		
		if ( lifecnt <= 0 ) {

			StartCoroutine("gameOver");
		}
    }
	
	void nekoFailed() {

		this.combo = COMBO.FAILED;
	}

	public int getLifeCount()
	{
		return(SceneControl.lifecnt);
	}

	// 紙の残り枚数を取得する.
	public int getPaperNum()
	{
		return(this.room_control.getPaperNum());
	}

	// コンボカウントプラス.
	public void	addComboCount()
	{
		this.combo_count++;
		
		switch(this.combo_count) {
			
			case 0:
			{
				GetComponent<AudioSource>().clip = COMBO_SOUND_01;
			}
			break;

			case 1:
			{
				GetComponent<AudioSource>().clip = COMBO_SOUND_01;
			}
			break;

			case 2:
			{
				GetComponent<AudioSource>().clip = COMBO_SOUND_02;
				this.combo = COMBO.CHAIN01;
			}
			break;

			default:
			{
				GetComponent<AudioSource>().clip = COMBO_SOUND_03;
				this.combo = COMBO.CHAIN02;
			}
			break;

		}
		
		GetComponent<AudioSource>().Play();
	}

	// コンボカウントを０にする.
	public void	clearComboCount()
	{
		this.combo_count = 0;
		this.combo = COMBO.NORMAL;
	}
	
	private IEnumerator gameClear()
	{
		GetComponent<AudioSource>().clip = CLEAR_NEKO_SOUND;
		GetComponent<AudioSource>().Play();

		yield return new WaitForSeconds(CLEAR_NEKO_SOUND.length);
		GetComponent<AudioSource>().clip = CLEAR_SOUND;
		GetComponent<AudioSource>().Play();

		yield return new WaitForSeconds(CLEAR_SOUND.length);
		GetComponent<AudioSource>().clip = CLEAR_LOOP_SOUND;
		GetComponent<AudioSource>().loop = true;
		GetComponent<AudioSource>().Play();
	}
	
	private IEnumerator gameOver()
	{
		GetComponent<AudioSource>().clip = GAMEOVER_NEKO_SOUND;
		GetComponent<AudioSource>().Play();

		yield return new WaitForSeconds(GAMEOVER_NEKO_SOUND.length);
		GetComponent<AudioSource>().clip = GAMEOVER_SOUND;
		GetComponent<AudioSource>().Play();
	}
	
}
