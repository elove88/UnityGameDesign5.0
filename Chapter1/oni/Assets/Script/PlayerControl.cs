using UnityEngine;
using System.Collections;

// メモ
//
// 回転しないようにするには、
// rigidbody -> constraint -> freeze rotation
// にチェックを入れる
//
// プレハブの複製
// × Ctrl-C Ctrl-V
// ○ Ctrl-D
//
// 敵のコリジョンまとめる
//
// 無限に繰り返す背景の作り方
//
// GameObject からスクリプトの変数やメソッドを使用したいときは、
// GetComponent<クラス名>() を使う
//
// 不要になったインスタンスがちゃんと削除されているかは、
// ゲームをポーズしてHierarchy ビューを見ればチェックしやすい
//
// 生成したインスタンスを GameObject 型として受け取りたいときは、
// as GameObject とする
//
// インスタンスを削除するときは、Destory(this) ではなく、Destory(this.gameObject)
// とすること
//
// OnBecameVisible/Invisible() が呼ばれない
// MeshRender が無効になっている（Inspector でチェックボックスが外れている）
// と呼ばれません
//
// On*() が呼ばれない
// メソッドの名前があっていても、引数の型が違うと呼ばれない
// × void OnCollisionEnter(Collider other)
// ○ void OnCollisionEnter(Collision other)
//.

public class PlayerControl : MonoBehaviour {

	// -------------------------------------------------------------------------------- //

	// サウンド
	public AudioClip[]	AttackSound;				// 攻撃するときのかけ声.
	public AudioClip	SwordSound;					// 剣をふる音.
	public AudioClip	SwordHitSound;				// ヒット音（剣がおにに当たったときの音）.
	public AudioClip	MissSound;					// ミスした時の音.
	public AudioClip	runSound;
	
	public AudioSource	attack_voice_audio;			// 攻撃音.
	public AudioSource	sword_audio;				// 剣の音（ふる音、ヒット音）.
	public AudioSource	miss_audio;					// ミスした時の音.
	public AudioSource	run_audio;
	
	public int			attack_sound_index = 0;		// 次に鳴らす AttakSound.

	// -------------------------------------------------------------------------------- //

	// 移動スピード.
	private	float	run_speed = 5.0f;

	// 移動スピードの最大値 [m/sec].
	public static float	RUN_SPEED_MAX = 20.0f;

	// 移動スピードの加速値 [m/sec^2].
	private const float	run_speed_add = 5.0f;

	// 移動スピードの減速値 [m/sec^2].
	private const float	run_speed_sub = 5.0f*4.0f;

	// 攻撃判定用コライダー.
	private	AttackColliderControl	attack_collider = null;

	public SceneControl				scene_control = null;

	// 攻撃判定が発生中タイマー.
	// attack_timer > 0.0f なら攻撃中.
	private float	attack_timer = 0.0f;

	// 空振り後の攻撃できない中タイマー.
	// attack_disable_timer > 0.0f なら攻撃できない.
	private float	attack_disable_timer = 0.0f;

	// 攻撃判定が継続する時間 [sec].
	private static float	ATTACK_TIME = 0.3f;

	// 攻撃判定が継続する時間 [sec].
	private static float	ATTACK_DISABLE_TIME = 1.0f;

	private bool	is_running = true;

	private bool	is_contact_floor = false;

	private bool	is_playable		= true;
	
	// 停止目標位置.
	// （SceneControl.cs が決めた、ここで止まってほしいという位置）.
	public float	stop_position = -1.0f;

	// 攻撃モーションの種類.
	public enum ATTACK_MOTION {

		NONE = -1,

		RIGHT = 0,
		LEFT,

		NUM,
	};

	public ATTACK_MOTION	attack_motion = ATTACK_MOTION.LEFT;


	// 剣の軌跡エフェクト.
	public AnimatedTextureExtendedUV	kiseki_left = null;
	public AnimatedTextureExtendedUV	kiseki_right = null;

	// ヒットエフェクト.
	public ParticleSystem				fx_hit = null;
	
	// 走っている時のエフェクト.
	public ParticleSystem				fx_run = null;

	// 
	public	float	min_rate = 0.0f;
	public	float	max_rate = 3.0f;
	
	// -------------------------------------------------------------------------------- //

	public enum STEP {

		NONE = -1,

		RUN = 0,		// 走る　　ゲーム中.
		STOP,			// 止まる　ゴール演出時.
		MISS,			// ミス　　おににぶつかったしたとき.
		NUM,
	};

	public STEP		step			= STEP.NONE;
	public STEP		next_step    	= STEP.NONE;

	// -------------------------------------------------------------------------------- //

	void	Start()
	{
		// 攻撃判定用コライダーを探しておく.
		this.attack_collider = GameObject.FindGameObjectWithTag("AttackCollider").GetComponent<AttackColliderControl>();

		// 攻撃判定用コライダーにプレイヤーのインスタンスをセットしておく.
		this.attack_collider.player = this;

		// 剣の軌跡エフェクト.

		this.kiseki_left = GameObject.FindGameObjectWithTag("FX_Kiseki_L").GetComponent<AnimatedTextureExtendedUV>();
		this.kiseki_left.stopPlay();

		this.kiseki_right = GameObject.FindGameObjectWithTag("FX_Kiseki_R").GetComponent<AnimatedTextureExtendedUV>();
		this.kiseki_right.stopPlay();

		// ヒットエフェクト.

		this.fx_hit = GameObject.FindGameObjectWithTag("FX_Hit").GetComponent<ParticleSystem>();
		
		this.fx_run = GameObject.FindGameObjectWithTag("FX_Run").GetComponent<ParticleSystem>();
		//

		this.run_speed = 0.0f;

		this.next_step = STEP.RUN;

		this.attack_voice_audio = this.gameObject.AddComponent<AudioSource>();
		this.sword_audio        = this.gameObject.AddComponent<AudioSource>();
		this.miss_audio         = this.gameObject.AddComponent<AudioSource>();
		
		this.run_audio         	= this.gameObject.AddComponent<AudioSource>();
		this.run_audio.clip		= this.runSound;
		this.run_audio.loop		= true;
		this.run_audio.Play();
	}

	void	Update()
	{
#if false
		if(Input.GetKey(KeyCode.Keypad1)) {
			min_rate -= 0.1f;
		}
		if(Input.GetKey(KeyCode.Keypad2)) {
			min_rate += 0.1f;
		}
		if(Input.GetKey(KeyCode.Keypad4)) {
			max_rate -= 0.1f;
		}
		if(Input.GetKey(KeyCode.Keypad5)) {
			max_rate += 0.1f;
		}
#endif		
		min_rate = Mathf.Clamp( min_rate, 0.0f, max_rate );
		max_rate = Mathf.Clamp( max_rate, min_rate, 5.0f );
		
		// 次の状態に移るかどうかを、チェックする.
		if(this.next_step == STEP.NONE) {

			switch(this.step) {
	
				case STEP.RUN:
				{
					if(!this.is_running) {
	
						if(this.run_speed <= 0.0f) {
						
							// 走行音、走行エフェクトを停止する.
							this.fx_run.Stop();
						
							this.next_step = STEP.STOP;
						}
					}
				}
				break;

				case STEP.MISS:
				{
					if(this.GetComponent<Rigidbody>().velocity.y < 0.0f) {

						if(this.is_contact_floor) {
						
							// 走行エフェクトを再開.
							this.fx_run.Play();
						
							this.GetComponent<Rigidbody>().useGravity = true;
							this.next_step = STEP.RUN;
						}
					}
				}
				break;
			}
		}
			
		// 状態の遷移時の初期化.
		if(this.next_step != STEP.NONE) {

			switch(this.next_step) {

				case STEP.STOP:
				{
					Animation	animation = this.transform.GetComponentInChildren<Animation>();

					animation.Play("P_stop");
				}
				break;

				case STEP.MISS:
				{
					// 斜め上に跳ね返る.

					Vector3	velocity = this.GetComponent<Rigidbody>().velocity;

					float	jump_height = 1.0f;

					velocity.x = -2.5f;
					velocity.y = Mathf.Sqrt(2.0f*9.8f*jump_height);
					velocity.z = 0.0f;

					this.GetComponent<Rigidbody>().velocity = velocity;
					this.GetComponent<Rigidbody>().useGravity = false;

					this.run_speed = 0.0f;

					Animation	animation = this.transform.GetComponentInChildren<Animation>();

					animation.Play("P_yarare");				
					animation.CrossFadeQueued("P_run");

					//

					this.miss_audio.PlayOneShot(this.MissSound);
				
					// 走行エフェクトを止める.
					this.fx_run.Stop();
				}
				break;
			}

			this.step = this.next_step;

			this.next_step = STEP.NONE;
		}
		
		// 走行音のボリューム制御
		if( this.is_running ){
			this.run_audio.volume = 1.0f;
		}else{
			this.run_audio.volume = Mathf.Max(0.0f, this.run_audio.volume - 0.05f );
		}
		
		// 各状態の実行

		// ---------------------------------------------------- //
		// ポジション.

		switch(this.step) {

			case STEP.RUN:
			{
				// ---------------------------------------------------- //
				// 速度.
		
				if(this.is_running) {
		
					this.run_speed += PlayerControl.run_speed_add*Time.deltaTime;
		
				} else {
		
					this.run_speed -= PlayerControl.run_speed_sub*Time.deltaTime;
				}
		
				this.run_speed = Mathf.Clamp(this.run_speed, 0.0f, PlayerControl.RUN_SPEED_MAX);
		
				Vector3	new_velocity = this.GetComponent<Rigidbody>().velocity;
		
				new_velocity.x = run_speed;
		
				if(new_velocity.y > 0.0f) {
		
					new_velocity.y = 0.0f;
				}
		
				this.GetComponent<Rigidbody>().velocity = new_velocity;
		
				float	rate;
			
				rate	= this.run_speed / PlayerControl.RUN_SPEED_MAX;
				this.run_audio.pitch	= Mathf.Lerp( min_rate, max_rate, rate);

				// ---------------------------------------------------- //
				// 攻撃.
		
				this.attack_control();

				this.sword_fx_control();

				// ---------------------------------------------------- //
				// 攻撃可能かどうかで色を変える（デバッグ用）
		
				if(this.attack_disable_timer > 0.0f) {
		
					this.GetComponent<Renderer>().material.color = Color.gray;
		
				} else {
		
					this.GetComponent<Renderer>().material.color = Color.Lerp(Color.white, Color.blue, 0.5f);
				}
		
				// ---------------------------------------------------- //
				// "W" キーで前方に大きく移動（デバッグ用）.
#if UNITY_EDITOR
				if(Input.GetKeyDown(KeyCode.W)) {
		
					Vector3		position = this.transform.position;
		
					position.x += 100.0f*FloorControl.WIDTH*FloorControl.MODEL_NUM;
		
					this.transform.position = position;
				}
#endif
			}
			break;

			case STEP.MISS:
			{
				this.GetComponent<Rigidbody>().velocity += Vector3.down*9.8f*2.0f*Time.deltaTime;
			}
			break;

		}

		//

		this.is_contact_floor = false;
	}


	void OnCollisionStay(Collision other)
	{
		// おににぶつかったら、減速する.
		//

		if(other.gameObject.tag == "OniGroup") {

			if(this.step != STEP.MISS) {

				this.next_step = STEP.MISS;

				// プレイヤーがおににぶつかったときの処理.

				this.scene_control.OnPlayerMissed();

				// おにグループに、プレイヤーがぶつかったことを覚えておく.

				OniGroupControl	oni_group = other.gameObject.GetComponent<OniGroupControl>();
				
				oni_group.onPlayerHitted();
			}
		}

		// 着地してる？.
		if(other.gameObject.tag == "Floor") {

			this.is_contact_floor = true;
		}
	}

	// CollisionStay がよばれないときもあるから、こちらも設定しておく.
	void OnCollisionEnter(Collision other)
	{
		this.OnCollisionStay(other);
	}


	// -------------------------------------------------------------------------------- //

	// 攻撃ヒットエフェクトを再生する.
	public void		playHitEffect(Vector3 position)
	{
		this.fx_hit.transform.position = position;

		this.fx_hit.Play();
	}

	// 攻撃ヒット音を鳴らす.
	public void		playHitSound()
	{
		this.sword_audio.PlayOneShot(this.SwordHitSound);
	}

	// 『攻撃できない中』タイマーをリセットする（すぐに攻撃可にする）.
	public void 	resetAttackDisableTimer()
	{
		this.attack_disable_timer = 0.0f;
	}

	// 攻撃を開始してから（マウスのボタンをクリックしてから）の経過時間を求める.
	public float	GetAttackTimer()
	{
		return(PlayerControl.ATTACK_TIME - this.attack_timer);
	}

	// プレイヤーのスピード率（0.0f ～ 1.0f）を取得する.
	public float	GetSpeedRate()
	{
		float	player_speed_rate = Mathf.InverseLerp(0.0f, PlayerControl.RUN_SPEED_MAX, this.GetComponent<Rigidbody>().velocity.magnitude);

		return(player_speed_rate);
	}

	// 止まって
	public void 	StopRequest()
	{
		this.is_running = false;
	}
	
	// プレイヤー操作可能
	public void		Playable()
	{
		this.is_playable = true;
	}
	
	// プレイヤー操作禁止
	public void		UnPlayable()
	{
		this.is_playable = false;
	}
	
	// プレイヤーが停止した？.
	public bool 	IsStopped()
	{
		bool	is_stopped = false;

		do {

			if(this.is_running) {

				break;
			}

			if(this.run_speed > 0.0f) {

				break;
			}

			//

			is_stopped = true;

		} while(false);

		return(is_stopped);
	}

	// 減速し続けた場合の、停止予想位置を求める.
	public float CalcDistanceToStop()
	{
		float distance = this.GetComponent<Rigidbody>().velocity.sqrMagnitude/(2.0f*PlayerControl.run_speed_sub);

		return(distance);
	}

	// -------------------------------------------------------------------------------- //

	// 攻撃の入力があった？.
	private bool	is_attack_input()
	{
		bool	is_attacking = false;

		// マウスの左ボタンがクリックされたら、攻撃.
		//
		// OnMouseDown() は自分の上でクリックされたときしか呼ばれない
		// 今回は画面のどこでクリックしても反応するようにしたいので、
		// Input.GetMouseButtonDown() を使う.
		//
		if(Input.GetMouseButtonDown(0)) {

			is_attacking = true;
		}

		// デバッグ用の自動攻撃.
		if(SceneControl.IS_AUTO_ATTACK) {

			GameObject[] oni_groups = GameObject.FindGameObjectsWithTag("OniGroup");

			foreach(GameObject oni_group in oni_groups) {

				float	distance = oni_group.transform.position.x - this.transform.position.x;
				
				distance -= 1.0f/2.0f;
				distance -= OniGroupControl.collision_size/2.0f;

				// 後ろにいるものは無視.
				// （今回はゲームの仕様的にありえないが、念のため）.
				//
				if(distance < 0.0f) {

					continue;
				}

				// 衝突までの予想時間.

				float	time_left = distance/(this.GetComponent<Rigidbody>().velocity.x - oni_group.GetComponent<OniGroupControl>().run_speed);

				// 離れていくものは無視.
				//
				if(time_left < 0.0f) {

					continue;
				}

				if(time_left < 0.1f) {
				//if(time_left < 0.05f) {

					is_attacking = true;
				}
			}
		}

		return(is_attacking);
	}

	// 攻撃のコントロール.
	private void	attack_control()
	{
		if(!this.is_playable) {
			return;	
		}
		
		if(this.attack_timer > 0.0f) {

			// 攻撃判定発生中.

			this.attack_timer -= Time.deltaTime;

			// 攻撃判定終了チェック.
			if(this.attack_timer <= 0.0f) {

				// コライダー（攻撃の当たり判定）の当たり判定を無効にする.
				//
				attack_collider.SetPowered(false);
			}

		} else {

			this.attack_disable_timer -= Time.deltaTime;

			if(this.attack_disable_timer > 0.0f) {

				// まだ攻撃できない中.

			} else {

				this.attack_disable_timer = 0.0f;

				if(this.is_attack_input()) {

					// コライダー（攻撃の当たり判定）の当たり判定を有効にする.
					//
					attack_collider.SetPowered(true);
		
					this.attack_timer = PlayerControl.ATTACK_TIME;
	
					this.attack_disable_timer = PlayerControl.ATTACK_DISABLE_TIME;

					// 攻撃モーションを再生する.

					Animation	animation = this.transform.GetComponentInChildren<Animation>();

					// 同じモーションを初めから再生し直したいときは、一度 stop() しないとだめっぽい.
					//animation.Stop();

					// 次に再生するモーションを選ぶ.
					//
					// 『おに』の吹き飛ぶ方向を決める時に『直前の攻撃モーション』を知りたいので、
					// 再生後ではなく再生前にモーションの選択をする.
					//
					switch(this.attack_motion) {

						default:
						case ATTACK_MOTION.RIGHT:	this.attack_motion = ATTACK_MOTION.LEFT;	break;
						case ATTACK_MOTION.LEFT:	this.attack_motion = ATTACK_MOTION.RIGHT;	break;
					}

					switch(this.attack_motion) {

						default:
						case ATTACK_MOTION.RIGHT:	animation.CrossFade("P_attack_R", 0.2f);	break;
						case ATTACK_MOTION.LEFT:	animation.CrossFade("P_attack_L", 0.2f);	break;
					}

					// 攻撃モーションが終わったら、走りモーションに戻る.
					animation.CrossFadeQueued("P_run");

					this.attack_voice_audio.PlayOneShot(this.AttackSound[this.attack_sound_index]);

					this.attack_sound_index = (this.attack_sound_index + 1)%this.AttackSound.Length;

					this.sword_audio.PlayOneShot(this.SwordSound);

				}
			}
		}
	}

	// 剣の軌跡エフェクト.
	private	void	sword_fx_control()
	{

		do {
		
			if(this.attack_timer <= 0.0f) {
		
				break;
			}
		
			if(this.kiseki_left.isPlaying()) {
		
				break;
			}
		
			Animation					animation = this.transform.GetComponentInChildren<Animation>();
			AnimationState				state;
			AnimatedTextureExtendedUV	anim_player;
		
			switch(this.attack_motion) {
		
				default:
				case ATTACK_MOTION.RIGHT:
				{
					state = animation["P_attack_R"];
					anim_player = this.kiseki_right;
				}
				break;
		
				case ATTACK_MOTION.LEFT:
				{
					state = animation["P_attack_L"];
					anim_player = this.kiseki_left;
				}
				break;
			}
		
			float	start_time    = 2.5f;
			float	current_frame = state.time*state.clip.frameRate;
			
			if(current_frame < start_time) {
			
				break;
			}
		
			anim_player.startPlay(state.time - start_time/state.clip.frameRate);
		
		} while(false);
	}
}
