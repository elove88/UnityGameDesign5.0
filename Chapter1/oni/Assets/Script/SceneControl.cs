using UnityEngine;
using System.Collections;

public class SceneControl : MonoBehaviour {

	// -------------------------------------------------------------------------------- //
	// プレハブ.

	public GameObject		OniGroupPrefab = null;
	public GameObject		OniPrefab = null;
	public GameObject		OniEmitterPrefab = null;
	public GameObject[]		OniYamaPrefab;

	// 2D用のテクスチャー
	public Texture	TitleTexture = null;			// 『開始』
	public Texture	StartTexture = null;			// 『はじめ』
	public Texture	ReturnButtonTexture = null;		// 『もどる』ボタン

	// SE
	public AudioClip	GameStart = null;
	public AudioClip	EvalSound = null;			// 評価
	public AudioClip	ReturnSound = null;			// もどる
	// -------------------------------------------------------------------------------- //

	// プレイヤー.
	public PlayerControl	player = null;

	// スコア.
	public ScoreControl		score_control = null;
	
	// カメラ.
	public GameObject	main_camera = null;

	// おにの出現を制御する.
	public LevelControl	level_control = null;
	
	// 得点計算を制御する.
	public ResultControl result_control = null;

	// ゴール時に、上からおにを降らすためのオブジェクト.
	public OniEmitterControl	oni_emitter = null;

	// GUI（２Dの表示） の制御.
	private GUIControl	gui_control = null;
	
	// フェードコントロール
	public FadeControl	fader = null;
	
	// -------------------------------------------------------------------------------- //

	// ゲームの進行状態.
	public enum STEP {

		NONE = -1,

		START,					// 『はじめ！』の文字が出てる間.
		GAME,					// ゲーム中.
		ONI_VANISH_WAIT,		// タイムオーバー後、画面にいるオニがいなくなるのを待つ.
		LAST_RUN,				// オニが出現しなくなってからしばらく進む.
		PLAYER_STOP_WAIT,		// プレイヤーが止まるのを待ってる.

		GOAL,					// ゴール演出.
		ONI_FALL_WAIT,			// 『上からおにが降ってくる』演出が終わるのをまつ.
		RESULT_DEFEAT,			// 倒した数の評価表示.
		RESULT_EVALUATION,		// 倒したタイミングの評価表示.
		RESULT_TOTAL,			// 総合評価.

		GAME_OVER,				// ゲームオーバー.
		GOTO_TITLE,				// タイトルへ移行.

		NUM,
	};

	public STEP	step      = STEP.NONE;		// 現在のゲームの進行状態.
	public STEP	next_step = STEP.NONE;		// 遷移する状態.
	public float	step_timer      = 0.0f;		// 状態が遷移してからの時間.
	public float	step_timer_prev = 0.0f;

	// -------------------------------------------------------------------------------- //

	// ボタンをクリックしてから攻撃が当たるまでの時間（評価に使う）.
	public float		attack_time = 0.0f;


	// 評価.
	// おにを近くで斬るほど高評価.
	public enum EVALUATION {

		NONE = -1,

		OKAY = 0,		// ふつう.
		GOOD,			// まあまあ.
		GREAT,			// すごい.

		MISS,			// ミス（ぶつかった）.

		NUM,
	};
	public static string[] evaluation_str = {

		"okay",
		"good",
		"great",
		"miss",
	};
	
	public EVALUATION	evaluation = EVALUATION.NONE;

	// -------------------------------------------------------------------------------- //

	// ゲーム全体の結果.
	public struct Result {

		public int		oni_defeat_num;			// 倒したオニの数（トータル）.
		public int[]	eval_count;				// 各評価の回数.

		public int		rank;					// ゲーム全体を通しの、結果.
		
		public float	score;					// 現在のスコア
		public float	score_max;				// ゲーム内で取れる得点の最大
		
	};

	public Result	result;

	// -------------------------------------------------------------------------------- //

	// 一度に出現するオニの数の最大値.
	// ミスをしないとどんどん増えていくが、この数以上は増えない.
	public static int	ONI_APPEAR_NUM_MAX = 10;

	// ゲームが終了するオニのグループ数.
	public int				oni_group_appear_max = 50;
	//private int				oni_group_appear_max = 50;
	
	// 失敗の時に減らす出現数.
	public static int		oni_group_penalty = 1;
	
	// スコアを隠す出現数.
	public static float		SCORE_HIDE_NUM = 40;
	
	// グループの出現数.
	public int				oni_group_num = 0;

	// 切った or ぶつかったオニのグループ数.
	public int				oni_group_complite = 0;
	
	// 切ったオニのグループ数.
	public int				oni_group_defeat_num = 0;
	
	// ぶつかったオニのグループ数.
	public int				oni_group_miss_num = 0;
	
	// 開始演出（『はじめ！』の文字が出ている）の時間.
	private static float	START_TIME = 2.0f;

	// ゴール演出時に、『オニが積まれているところ』から『プレイヤーが止まる位置』までの距離.
	private static float	GOAL_STOP_DISTANCE = 8.0f;

	// 評価を決めるときの、ボタンをクリックしてから攻撃が当たるまでの経過時間.
	public static float	ATTACK_TIME_GREAT = 0.05f;
	public static float	ATTACK_TIME_GOOD  = 0.10f;

	// -------------------------------------------------------------------------------- //
	// デバッグ用のフラグあれこれ.
	// 適当に変更してゲームがどう変わるか、試してみてください！.

	// true にすると、倒したオニがカメラのローカル座標系で移動するようになります.
	// カメラの動きと連動するので、カメラが急に止まった場合でも、動きが不自然に変化
	// しません.
	//
	public static bool	IS_ONI_BLOWOUT_CAMERA_LOCAL = true;

	// オニグループのコリジョンを表示する（デバッグ用）.
	// オニは何匹かがまとめて出現しますが、グループで共通のコリジョンを使用します。
	//
	// これは
	//
	// ・プレイヤーがオニにあたったときの挙動を調整しやすくするため
	// ・やられたオニがまとめて吹っ飛んだ方がかっこいいため
	//
	// などの理由です.
	//
	public static bool	IS_DRAW_ONI_GROUP_COLLISION = false;

	// プレイヤーの攻撃時に、攻撃判定を表示する.
	public static bool	IS_DRAW_PLAYER_ATTACK_COLLISION = false;

	// デバッグ用全自動機能
	// true にしておくと攻撃判定が出っぱなしになります.
	//
	public static bool	IS_AUTO_ATTACK = false;

	// AUTO_ATTACK のときの評価
	public EVALUATION	evaluation_auto_attack = EVALUATION.GOOD;
	
	// 討伐数が消えた瞬間の討伐数
	private int         backup_oni_defeat_num = -1;
	
	// デバッグ用の背景モデルを表示する（赤、青、緑の色がつくようになる）.
	public static bool	IS_DRAW_DEBUG_FLOOR_MODEL = false;

	public	float		eval_rate_okay  = 1.0f;
	public	float		eval_rate_good  = 2.0f;
	public	float		eval_rate_great = 4.0f;
	public	int			eval_rate		= 1;
	
	// -------------------------------------------------------------------------------- //
	
	void	Start()
	{
		// プレイヤーのインスタンスを探しておく.
		this.player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControl>();

		this.player.scene_control = this;

		// スコアのインスタンスを探しておく.
		this.score_control = GetComponent<ScoreControl>();
		
		// カメラのインスタンスを探しておく.
		this.main_camera = GameObject.FindGameObjectWithTag("MainCamera");

		this.level_control = new LevelControl();
		this.level_control.scene_control = this;
		this.level_control.player = this.player;
		this.level_control.OniGroupPrefab = this.OniGroupPrefab;
		this.level_control.create();
		
		this.result_control = new ResultControl();

		// GUI 制御のスクリプト（コンポーネント）.
		this.gui_control = this.GetComponent<GUIControl>();
		
		// フェードコントロールの追加
		fader = gameObject.AddComponent<FadeControl>();
		
		// ゲームの結果をクリアーしておく.
		this.result.oni_defeat_num = 0;
		this.result.eval_count = new int[(int)EVALUATION.NUM];
		this.result.rank = 0;
		this.result.score = 0;
		this.result.score_max = 0;
		
		for(int i = 0;i < this.result.eval_count.Length;i++) {

			this.result.eval_count[i] = 0;
		}
		
		// フェードインで開始
		this.fader.fade( 3.0f, new Color( 0.0f, 0.0f, 0.0f, 1.0f ), new Color( 0.0f, 0.0f, 0.0f, 0.0f ) );
		
		this.step = STEP.START;
	}

	void	Update()
	{
		// ゲームの現在の状態を管理する
		this.step_timer_prev = this.step_timer;
		this.step_timer += Time.deltaTime;

		// 次の状態に移るかどうかを、チェックする.
		switch(this.step) {
		
			case STEP.START:
			{
				if(this.step_timer > SceneControl.START_TIME) {
					next_step = STEP.GAME;
				}
			}
			break;

			case STEP.GAME:
			{
				// 出現最大数を過ぎたら、オニの発生を止める.
				if(this.oni_group_complite >= this.oni_group_appear_max )
				{
					next_step = STEP.ONI_VANISH_WAIT;
				}
			
				if(this.oni_group_complite >= SCORE_HIDE_NUM && this.backup_oni_defeat_num == -1)
				{
					this.backup_oni_defeat_num = this.result.oni_defeat_num;
				}
			}
			break;

			case STEP.ONI_VANISH_WAIT:
			{
				do {

					// おに（やられる前）をすべて倒すまで待つ.
					if(GameObject.FindGameObjectsWithTag("OniGroup").Length > 0) {

						break;
					}

					// プレイヤーが加速するまで待つ.
					// おに山を画面外で出現させるため、最後のおにを倒してから一定距離
					// 走るようにしたい.
					if(this.player.GetSpeedRate() < 0.5f) {

						break;
					}

					//

					next_step = STEP.LAST_RUN;

				} while(false);
			}
			break;

			case STEP.LAST_RUN:
			{
				if(this.step_timer > 2.0f) {

					// プレイヤーを止める.
					next_step = STEP.PLAYER_STOP_WAIT;
				}
			}
			break;

			case STEP.PLAYER_STOP_WAIT:
			{
				// プレイヤーが止まったら、ゴール演出開始.
				if(this.player.IsStopped()) {
				
					this.gui_control.score_control.setNumForce(this.backup_oni_defeat_num);
					this.gui_control.score_control.setNum(this.result.oni_defeat_num);
					next_step = STEP.GOAL;
				}
			}
			break;

			case STEP.GOAL:
			{
				// おにが全部画面に出きるまでまつ.
				if(this.oni_emitter.oni_num == 0) {

					this.next_step = STEP.ONI_FALL_WAIT;
				}
			}
			break;

			case STEP.ONI_FALL_WAIT:
			{
				if(!this.score_control.isActive() && this.step_timer > 1.5f) {
					this.next_step = STEP.RESULT_DEFEAT;
				}
			}
			break;

			case STEP.RESULT_DEFEAT:
			{
				if(this.step_timer >= 0.4f && this.step_timer_prev < 0.4f )
				{
					// SE（『どどんっ』）.
					this.GetComponent<AudioSource>().PlayOneShot(this.EvalSound);
				}
				// 評価の表示が終わるまで待つ.
				//
				if(this.step_timer > 0.5f) {

					this.next_step = STEP.RESULT_EVALUATION;
				}
			}
			break;
			
			case STEP.RESULT_EVALUATION:
			{
				if(this.step_timer >= 0.4f && this.step_timer_prev < 0.4f )
				{
					// SE（『どどんっ』）.
					this.GetComponent<AudioSource>().PlayOneShot(this.EvalSound);
				}
				// 評価の表示が終わるまで待つ.
				//
				if(this.step_timer > 2.0f) {

					this.next_step = STEP.RESULT_TOTAL;
				}
			}
			break;
			
			case STEP.RESULT_TOTAL:
			{
				if(this.step_timer >= 0.4f && this.step_timer_prev < 0.4f )
				{
					// SE（『どどんっ』）.
					this.GetComponent<AudioSource>().PlayOneShot(this.EvalSound);
				}
				// 評価の表示が終わるまで待つ.
				//
				if(this.step_timer > 2.0f) {

					this.next_step = STEP.GAME_OVER;
				}
			}
			break;

			case STEP.GAME_OVER:
			{
				// マウスがクリックされたらフェードアウトしてタイトル画面に戻る.
				//
				if(Input.GetMouseButtonDown(0)) {
				
					// フェードアウトさせる
					this.fader.fade( 1.0f, new Color( 0.0f, 0.0f, 0.0f, 0.0f ), new Color( 0.0f, 0.0f, 0.0f, 1.0f ) );
					this.GetComponent<AudioSource>().PlayOneShot(this.ReturnSound);
					
					this.next_step = STEP.GOTO_TITLE;
				}
			}
			break;
			
			case STEP.GOTO_TITLE:
			{
				// フェードが終わったらタイトル画面に戻る.
				//
				if(!this.fader.isActive()) { 
					Application.LoadLevel("TitleScene");
				}
			}
			break;
		}

		// 状態がかわったときの初期化処理.

		if(this.next_step != STEP.NONE) {

			switch(this.next_step) {
			
				case STEP.PLAYER_STOP_WAIT:
				{
					// プレイヤーを止める.
					this.player.StopRequest();

					// -------------------------------------------------------- //
					// 『オニが積まれている山』を生成する.
					
					if( this.result_control.getTotalRank() > 0 ) {
						GameObject	oni_yama = Instantiate(this.OniYamaPrefab[this.result_control.getTotalRank() - 1]) as GameObject;
				
						Vector3		oni_yama_position = this.player.transform.position;
				
						oni_yama_position.x += this.player.CalcDistanceToStop();
						oni_yama_position.x += SceneControl.GOAL_STOP_DISTANCE;
	
						oni_yama_position.y = 0.0f;
				
						oni_yama.transform.position = oni_yama_position;
					}
					else{
						
					}

					// -------------------------------------------------------- //
				}
				break;

				case STEP.GOAL:
				{
					// ゴール演出開始.

					// 『おにが画面の上から飛んでくる』用のエミッターを生成する.

					GameObject	go = Instantiate(this.OniEmitterPrefab) as GameObject;
	
					this.oni_emitter = go.GetComponent<OniEmitterControl>();

					Vector3		emitter_position = oni_emitter.transform.position;

					// おに山の真上に置く.

					emitter_position.x  = this.player.transform.position.x;
					emitter_position.x += this.player.CalcDistanceToStop();
					emitter_position.x += SceneControl.GOAL_STOP_DISTANCE;
	
					this.oni_emitter.transform.position = emitter_position;

					// 最終評価で、ふってくるおにの数を変える.

					int		oni_num = 0;

					switch(this.result_control.getTotalRank()) {
						case 0:		oni_num = Mathf.Min( this.result.oni_defeat_num, 10 );	break;
						case 1:		oni_num = 6;	break;
						case 2:		oni_num = 10;	break;
						case 3:		oni_num = 20;	break;
					}
				
					this.oni_emitter.oni_num = oni_num;
					if( oni_num == 0 )
					{
						this.oni_emitter.is_enable_hit_sound = false;
					}
				}
				break;

				case STEP.RESULT_DEFEAT:
				{
					// 評価が出た後は、おにの落下音を鳴らなくする.
					this.oni_emitter.is_enable_hit_sound = false;
				}
				break;
			}

			this.step = this.next_step;
			this.next_step = STEP.NONE;

			this.step_timer = 0.0f;
			this.step_timer_prev = -1.0f;
		}

		// 各状態での実行処理.

		switch(this.step) {

			case STEP.GAME:
			{
				// おにの出現の制御.
				this.level_control.oniAppearControl();
			}
			break;

			case STEP.RESULT_DEFEAT:
			{
				// 評価の文字.
				this.gui_control.updateEval(this.step_timer);
			}
			break;
			
			case STEP.RESULT_EVALUATION:
			{
				// 評価の文字.
				this.gui_control.updateEval(this.step_timer);
			}
			break;
			
			case STEP.RESULT_TOTAL:
			{
				// 評価の文字.
				this.gui_control.updateEval(this.step_timer);
			}
			break;
		}

	}

	// プレイヤーがミスした時の処理.
	public void	OnPlayerMissed()
	{
		this.oni_group_miss_num++;
		this.oni_group_complite++;
		this.oni_group_appear_max -= oni_group_penalty;
		
		this.level_control.OnPlayerMissed();

		this.evaluation = EVALUATION.MISS;

		this.result.eval_count[(int)this.evaluation]++;

		// 画面上のグループすべてを退場させる.

		GameObject[] oni_groups = GameObject.FindGameObjectsWithTag("OniGroup");

		foreach(var oni_group in oni_groups) {
			this.oni_group_num--;
			oni_group.GetComponent<OniGroupControl>().beginLeave();
		}
	}

	// 倒したオニの数を追加.
	public void	AddDefeatNum(int num)
	{
		this.oni_group_defeat_num++;
		this.oni_group_complite++;
		this.result.oni_defeat_num += num;
		
		// ボタンをクリックしてからの時間で評価を決める
		// （クリックしてから攻撃が当たるまでの時間が短い＝ぎりぎりまでひきつけてから斬った）.

		this.attack_time = this.player.GetComponent<PlayerControl>().GetAttackTimer();

		if(this.evaluation == EVALUATION.MISS) {

			// ミスした直後（のろのろ運転中）は、OKAY しか出ない.
			this.evaluation = EVALUATION.OKAY;

		} else {

			if(this.attack_time < ATTACK_TIME_GREAT) {
	
				this.evaluation = EVALUATION.GREAT;
	
			} else if(this.attack_time < ATTACK_TIME_GOOD) {
	
				this.evaluation = EVALUATION.GOOD;
	
			} else {
	
				this.evaluation = EVALUATION.OKAY;
			}
		}

		if(SceneControl.IS_AUTO_ATTACK) {

			this.evaluation = this.evaluation_auto_attack;
		}

		this.result.eval_count[(int)this.evaluation] += num;
		
		// 得点計算
		float[] score_list = { this.eval_rate_okay, this.eval_rate_good, this.eval_rate_great, 0 };
		this.result.score_max += num * this.eval_rate_great;
		this.result.score += num * score_list[(int)this.evaluation];
		
		this.result_control.addOniDefeatScore(num);
		this.result_control.addEvaluationScore((int)this.evaluation);
	}
	
	//スコアを表示していいかどうか
	public bool IsDrawScore()
	{
		if( this.step >= STEP.GOAL )
		{
			return true;
		}
		
		if(this.oni_group_complite >= SCORE_HIDE_NUM )
		{
			return false;
		}
		
		return true;
	}

	// -------------------------------------------------------------------------------- //

}
