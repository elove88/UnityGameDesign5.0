using UnityEngine;
using System.Collections;

public class NekoControl : MonoBehaviour {

	private RoomControl		room_control = null;
	private SceneControl	scene_control = null;
	public EffectControl	effect_control = null;

	// ---------------------------------------------------------------- //

	public enum STEP {

		NONE = -1,

		STAND = 0,			// たち.
		RUN,				// 走り.
		JUMP,				// ジャンプ.
		MISS,				// ミス.
		GAMEOVER,			// ゲームオーバー.

		FREE_MOVE,			// 自由移動（デバッグ用）.

		NUM,
	};

	public STEP			step      = STEP.NONE;
	public STEP			next_step = STEP.NONE;
	public float		step_timer = 0.0f;
	public bool			is_grounded;				// 着地してる？.

	// ---------------------------------------------------------------- //

	// ジャンプ中のいろいろ.
	public struct ActionStand {

		public bool		is_fade_anim;				// アニメーションをフェードする？（毎フレーム true に戻る）.
	};

	// ジャンプ中のいろいろ.
	public struct ActionJump {

		public STEP		prevoius_step;				// ジャンプする前のステップ（立ちジャンプ or 走りながらジャンプ）.

		public bool		is_key_released;			// ジャンプ後、スペースキーを離した？.

		public Vector3	launch_velocity_xz;
	};

	// ミスしたときのいろいろ.
	public struct ActionMiss {

		public bool		is_steel;					// 鉄板に当たった？.
	};

	public ActionJump	action_jump;
	public ActionMiss	action_miss;
	public ActionStand	action_stand;

	public Vector3		previous_velocity;

	private	bool		is_fallover = true;
		
	private	bool		is_auto_drive = false;		// 自動運転（クリアーした後）.

	// ---------------------------------------------------------------- //

	public static float	JUMP_HEIGHT_MAX = 5.0f;								// ジャンプの高さ.
	public static float	JUMP_KEY_RELEASE_REDUCE = 0.5f;						// ジャンプ中にキーを離したときの、上昇速度のスケール.

	public static float	RUN_SPEED_MAX   = 5.0f;								// 走りのスピードの最大値.
	public static float	RUN_ACCELE      = RUN_SPEED_MAX/2.0f;				// 走りのスピードの加速.

	public static float	SLIDE_SPEED_MAX = 2.0f;								// 左右移動のスピード.
	public static float	SLIDE_ACCEL     = SLIDE_SPEED_MAX/0.1f;				// 左右移動の加速度.

	public static float SLIDE_ACCEL_SCALE_JUMP = 0.1f;						// 左右移動の加速度のスケール（ジャンプ中）.

	public static float	RUN_SPEED_DECELE_MISS      = RUN_SPEED_MAX/2.0f;	// ミスした時の減速度.
	public static float	RUN_SPEED_DECELE_MISS_JUMP = RUN_SPEED_MAX/5.0f;	// ミスした時の減速度（ジャンプ中）.

	public static Vector3 COLLISION_OFFSET = Vector3.up*0.2f;

	// ---------------------------------------------------------------- //

	public static float SLIDE_ROTATION_MAX = 0.2f;							// 左右移動のローテーションスピード.
	public static float SLIDE_ROTATION_SPEED = SLIDE_ROTATION_MAX/0.1f;		// 左右移動のローテーション加速度.
	public static float SLIDE_ROTATION_COEFFICIENT = 2.0f;					// 左右移動のローテーション加速度の係数.

	public static float JUMP_ROTATION_MAX = 0.25f;							// 上下のローテーションスピード（ジャンプ中）.
	public static float JUMP_ROTATION_SPEED = JUMP_ROTATION_MAX/0.1f;		// 上下のローテーション加速度（ジャンプ中）.
	public static float JUMP_ROTATION_COEFFICIENT = 0.25f;					// 上下のローテーション加速度の係数（ジャンプ中）.

	public static float SLIDE_VELOCITY = 1.0f;								// 左右移動のローテーション速度.
	public static float JUMP_VELOCITY = 4.0f;								// 上下のローテーション速度（ジャンプ中）.
	
	// ---------------------------------------------------------------- //

	public AudioClip START_SOUND = null;
	public AudioClip FAILED_STEEL_SOUND = null;
	public AudioClip FAILED_FUSUMA_SOUND = null;
	public AudioClip FAILED_NEKO_SOUND = null;
	public AudioClip JUMP_SOUND = null;
	public AudioClip LANDING_SOUND = null;
	public AudioClip FALL_OVER_SOUND = null;

	// ---------------------------------------------------------------- //

	NekoColiResult	coli_result;

	// ---------------------------------------------------------------- //

	public void	onRoomProceed()
	{
		this.coli_result.shoji_hit_info_first.is_enable = false;
	}

	// Use this for initialization
	void Start ()
	{
		this.room_control   = GameObject.FindGameObjectWithTag("RoomControl").GetComponent<RoomControl>();
		this.scene_control  = GameObject.FindWithTag("MainCamera").GetComponent<SceneControl>();
		this.effect_control = GameObject.FindGameObjectWithTag("EffectControl").GetComponent<EffectControl>();


		//

		this.is_grounded = false;

		GetComponent<AudioSource>().clip = START_SOUND;
		GetComponent<AudioSource>().Play();

		this.previous_velocity = Vector3.zero;

		this.next_step = STEP.STAND;

		this.coli_result = new NekoColiResult();
		this.coli_result.neko = this;
		this.coli_result.create();
	
		this.action_stand.is_fade_anim = true;	

	}

	// Update is called once per frame
	void Update ()
	{

		Animation	animation = this.GetComponentInChildren<Animation>();

		// ---------------------------------------------------------------- //

		// 着地するときに地面にめり込んでしまうので.
		// （かっこ悪いけど）.

		if(this.transform.position.y < 0.0f) {

			this.is_grounded = true;

			Vector3	pos = this.transform.position;

			pos.y = 0.0f;

			this.transform.position = pos;
		}
		
		// ---------------------------------------------------------------- //
		// ステップ内の経過時間を進める.

		this.step_timer += Time.deltaTime;

		// ---------------------------------------------------------------- //
		// 次の状態に移るかどうかを、チェックする.

		// 前のフレームのコリジョン結果を調べる.

		if(this.step != STEP.MISS) {

			this.coli_result.resolveCollision();
		}

		//

		if(this.next_step == STEP.NONE) {

			switch(this.step) {
	
				case STEP.NONE:
				{
					this.next_step = STEP.STAND;
				}
				break;
	
				case STEP.STAND:
				{
					// シフトキーで走り始める.
					if(Input.GetKeyDown(KeyCode.LeftShift)) {
	
						this.next_step = STEP.RUN;
					}
					// スペースキーでジャンプ.
					if(Input.GetKeyDown(KeyCode.Space)) {
	
						this.next_step = STEP.JUMP;
					}
				}
				break;
	
				case STEP.RUN:
				{
					if(!this.is_auto_drive) {

						if(Input.GetKeyDown(KeyCode.Space)) {
		
							this.next_step = STEP.JUMP;
						}
					}
				}
				break;

				case STEP.JUMP:
				{
					// 着地したら立ち、または走りへ.
					if(this.is_grounded) {
					
						GetComponent<AudioSource>().clip = LANDING_SOUND;
						GetComponent<AudioSource>().Play();
						this.next_step = this.action_jump.prevoius_step;
					}
				}
				break;

				case STEP.MISS:
				{
					if(this.step_timer > 3.0f) {
					
						GameObject.FindWithTag("MainCamera").transform.SendMessage("applyDamage", 1);

						if(this.scene_control.getLifeCount() > 0) {

							this.transform.position = this.room_control.getRestartPosition();

							this.room_control.onRestart();

							// アニメーションは補間しない.
							this.action_stand.is_fade_anim = false;
						
							this.next_step = STEP.STAND;

						} else {

							this.next_step = STEP.GAMEOVER;
						}
					}
				}
				break;
			}
		}

		// ---------------------------------------------------------------- //
		// 状態が遷移したときの初期化.

		if(this.next_step != STEP.NONE) {

			switch(this.next_step) {
	
				case STEP.STAND:
				{
					Vector3 v = this.GetComponent<Rigidbody>().velocity;

					v.x = 0.0f;
					v.z = 0.0f;

					this.GetComponent<Rigidbody>().velocity = v;

					// 立ちアニメーションの再生.

					if(this.action_stand.is_fade_anim) {

						animation.CrossFade("M01_nekostanding", 0.2f);

					} else {

						animation.CrossFade("M01_nekostanding", 0.0f);
					}

					this.action_stand.is_fade_anim = true;	
				}
				break;

				case STEP.RUN:
				{
					animation.CrossFade("M02_nekodash", 0.2f);
				}
				break;

				case STEP.JUMP:
				{
					Vector3	v = this.GetComponent<Rigidbody>().velocity;

					v.y = Mathf.Sqrt(2.0f*9.8f*JUMP_HEIGHT_MAX);

					this.GetComponent<Rigidbody>().velocity = v;

					//

					this.action_jump.is_key_released = false;
					this.action_jump.prevoius_step   = this.step;

					this.action_jump.launch_velocity_xz = this.GetComponent<Rigidbody>().velocity;
					this.action_jump.launch_velocity_xz.y = 0.0f;

					//

					animation.CrossFade("M03_nekojump", 0.2f);
					GetComponent<AudioSource>().clip = JUMP_SOUND;
					GetComponent<AudioSource>().Play();
				}
				break;

				case STEP.MISS:
				{				
					// 後ろに跳ね返る.

					Vector3 v = this.GetComponent<Rigidbody>().velocity;

					v.z *= -0.5f;

					this.GetComponent<Rigidbody>().velocity = v;
						
					// エフェクト
					this.effect_control.createMissEffect(this);

					// 鉄板に当たった音 or ふすまに当たった音.
					//
					if(this.action_miss.is_steel) {

						GetComponent<AudioSource>().PlayOneShot(FAILED_STEEL_SOUND);

					} else {

						GetComponent<AudioSource>().PlayOneShot(FAILED_FUSUMA_SOUND);
					}

					// やられ声.
					//
					GetComponent<AudioSource>().PlayOneShot(FAILED_NEKO_SOUND);
					
					animation.CrossFade("M03_nekofailed01", 0.2f);

					this.coli_result.lock_target.enable = false;

					this.is_fallover = false;
				}
				break;

				case STEP.FREE_MOVE:
				{
					this.GetComponent<Rigidbody>().useGravity = false;

					this.GetComponent<Rigidbody>().velocity = Vector3.zero;
				}
				break;

			}

			this.step      = this.next_step;
			this.next_step = STEP.NONE;

			this.step_timer = 0.0f;
		}

		// ---------------------------------------------------------------- //
		// 各状態での実行処理.

		// 左右移動、ジャンプによるローテーション.
		this.rotation_control();

		switch(this.step) {

			case STEP.STAND:
			{
			}
			break;

			case STEP.RUN:
			{
				// 前への加速.

				Vector3	v = this.GetComponent<Rigidbody>().velocity;

				v.z += (RUN_ACCELE)*Time.deltaTime;

				v.z = Mathf.Clamp(v.z, 0.0f, RUN_SPEED_MAX);

				// 左右への平行移動.

				if(this.is_auto_drive) {

					v = this.side_move_auto_drive(v, 1.0f);

				} else {

					v = this.side_move(v, 1.0f);
				}

				//

				this.GetComponent<Rigidbody>().velocity = v;
			}
			break;

			case STEP.JUMP:
			{
				Vector3 v = this.GetComponent<Rigidbody>().velocity;

				// ジャンプ中にキーを離したら、上昇速度を減らす.
				// （キーを押す長さでジャンプの高さを制御できるように）.

				do {

					if(!Input.GetKeyUp(KeyCode.Space)) {
					
						break;
					}

					// 一度離した後はやらない（連打対策）.
					if(this.action_jump.is_key_released) {

						break;
					}

					// 下降中はやらない.
					if(this.GetComponent<Rigidbody>().velocity.y <= 0.0f) {

						break;
					}

					//

					v.y *= JUMP_KEY_RELEASE_REDUCE;

					this.GetComponent<Rigidbody>().velocity = v;

					this.action_jump.is_key_released = true;

				} while(false);

				// 左右への平行移動.
				// （ジャンプ中も多少は制御できるようにしたい）.
				//
				if(this.is_auto_drive) {

					this.GetComponent<Rigidbody>().velocity = this.side_move_auto_drive(this.GetComponent<Rigidbody>().velocity, SLIDE_ACCEL_SCALE_JUMP);

				} else {

					this.GetComponent<Rigidbody>().velocity = this.side_move(this.GetComponent<Rigidbody>().velocity, SLIDE_ACCEL_SCALE_JUMP);
				}

				//

				// 組子に当たったときは、穴の中心の方へ誘導する.
				if(this.coli_result.shoji_hit_info.is_enable) {
	
					//
	
					v = this.GetComponent<Rigidbody>().velocity;
			
					if(this.coli_result.lock_target.enable) {

						v = this.coli_result.lock_target.position  - this.transform.position;
					}

					v.z = this.action_jump.launch_velocity_xz.z;
						
					this.GetComponent<Rigidbody>().velocity = v;
				}
			}
			break;


			case STEP.MISS:
			{
				GameObject.FindWithTag("MainCamera").transform.SendMessage("nekoFailed");

				// 徐々に減速する.

				Vector3 v = this.GetComponent<Rigidbody>().velocity;

				v.y = 0.0f;

				float	speed_xz = v.magnitude;

				if(this.is_grounded) {	

					speed_xz -= RUN_SPEED_DECELE_MISS*Time.deltaTime;

				} else {

					speed_xz -= RUN_SPEED_DECELE_MISS_JUMP*Time.deltaTime;
				}

				speed_xz = Mathf.Max(0.0f, speed_xz);

				v.Normalize();

				v *= speed_xz;

				v.y = this.GetComponent<Rigidbody>().velocity.y;

				this.GetComponent<Rigidbody>().velocity = v;

				do {

					if(this.is_fallover) {

						break;
					}

					if(!this.is_grounded) {

						break;
					}

					if(animation["M03_nekofailed01"].normalizedTime < 1.0f) {

						break;
					}

					animation.CrossFade("M03_nekofailed02", 0.2f);
					GetComponent<AudioSource>().clip = FALL_OVER_SOUND;
					GetComponent<AudioSource>().Play();

					this.is_fallover = true;

				} while(false);
			}
			break;

			case STEP.FREE_MOVE:
			{
				float	speed = 400.0f;

				Vector3	v = Vector3.zero;
				
				if(Input.GetKey(KeyCode.RightArrow)) {

					v.x = +speed*Time.deltaTime;
				}
				if(Input.GetKey(KeyCode.LeftArrow)) {

					v.x = -speed*Time.deltaTime;
				}
				if(Input.GetKey(KeyCode.UpArrow)) {

					v.y = +speed*Time.deltaTime;
				}
				if(Input.GetKey(KeyCode.DownArrow)) {

					v.y = -speed*Time.deltaTime;
				}
				if(Input.GetKey(KeyCode.LeftShift)) {

					v.z = +speed*Time.deltaTime;
				}
				if(Input.GetKey(KeyCode.RightShift)) {

					v.z = -speed*Time.deltaTime;
				}

				this.GetComponent<Rigidbody>().velocity = v;
			}
			break;

		}

		// ---------------------------------------------------------------- //

		this.is_grounded = false;

		this.coli_result.shoji_hit_info.is_enable = false;

		this.coli_result.hole_hit_infos.clear();

		this.coli_result.obstacle_hit_info.is_enable = false;

		this.previous_velocity = this.GetComponent<Rigidbody>().velocity;

		animation["M02_nekodash"].speed = 4.0f;
	}

	void	OnGUI()
	{
		/*if(this.coli_result.lock_target.enable) {

			GUI.Label(new Rect(10, 10, 100, 20), this.coli_result.lock_target.hole_index.x.ToString() + " " + this.coli_result.lock_target.hole_index.y.ToString());

		} else {

			GUI.Label(new Rect(10, 10, 100, 20), "disable");
		}*/
	}

	// ---------------------------------------------------------------- //
	// コリジョン関連.

	void 	OnCollisionStay(Collision other)
	{
		this.on_collision_common(other);
	}
	void 	OnCollisionEnter(Collision other)
	{
		this.on_collision_common(other);
	}
	private void	on_collision_common(Collision other)
	{
		// 障子のコリジョンに当たったかを調べる.
		//
		do {

			if(other.gameObject.tag != "Syouji") {

				break;
			}

			ShojiControl	shoji_control = other.gameObject.GetComponent<ShojiControl>();

			if(shoji_control == null) {

				break;
			}

			// 障子のコリジョンにヒットしたことを記録しておく.


			Vector3		position = this.transform.TransformPoint(NekoControl.COLLISION_OFFSET);

			ShojiControl.HoleIndex	hole_index = shoji_control.getClosetHole(position);

			this.coli_result.shoji_hit_info.is_enable = true;
			this.coli_result.shoji_hit_info.hole_index = hole_index;
			this.coli_result.shoji_hit_info.shoji_control = shoji_control;

		} while(false);

		// ふすまにあたった？.
		
		do {
		
			if(other.gameObject.tag != "Obstacle") {
		
				break;
			}
		
			this.coli_result.obstacle_hit_info.is_enable = true;
			this.coli_result.obstacle_hit_info.go        = other.gameObject;
			this.coli_result.obstacle_hit_info.is_steel  = false;

		} while(false);
	}
	
	void 	OnTriggerEnter(Collider other)
	{
		this.on_trigger_common(other);
	}
	
	private void	on_trigger_common(Collider other)
	{
		// 穴を通過した？.

		do {

			if(other.gameObject.tag != "Hole") {

				break;
			}


			SyoujiPaperControl	paper_control = other.GetComponent<SyoujiPaperControl>();

			if(paper_control == null) {

				break;
			}

			// 格子のトリガーを通過したことを記録しておく.

			if(paper_control.step == SyoujiPaperControl.STEP.STEEL) {

				// 鉄板の場合は、障害物にあたったことにする.

				this.coli_result.obstacle_hit_info.is_enable = true;
				this.coli_result.obstacle_hit_info.go        = other.gameObject;
				this.coli_result.obstacle_hit_info.is_steel  = true;

			} else {

				// 紙だった場合.
				if(!this.coli_result.hole_hit_infos.full()) {
	
					NekoColiResult.HoleHitInfo		hole_hit_info;
			
					hole_hit_info.paper_control = paper_control;
			
					this.coli_result.hole_hit_infos.push_back(hole_hit_info);
				}
			}

		} while(false);
	}

	// ---------------------------------------------------------------- //

	public void	beginMissAction(bool is_steel)
	{
		this.GetComponent<Rigidbody>().velocity = this.previous_velocity;
		this.action_miss.is_steel = is_steel;

		this.next_step = STEP.MISS;
	}

	// ---------------------------------------------------------------- //

	// 左右移動、ジャンプによるローテーション.
	private void rotation_control()
	{

		// ---------------------------------------------------------------- //
		// 上下のローテーション.
		Quaternion	current = this.transform.GetChild(0).transform.localRotation;
		Quaternion	rot     = current;

		if(this.transform.position.y > 0.0f || this.step == STEP.JUMP) {		
			// ↑処理の順番の都合上、ジャンプの１フレーム目は y == 0.0f なので、.
			//   step も見てジャンプの１フレーム目もここにくるようにする.
	
			rot.x = -this.GetComponent<Rigidbody>().velocity.y/20.0f;
		
			float	rot_x_diff = rot.x - current.x;
			float	rot_x_diff_limit = 2.0f;

			rot_x_diff = Mathf.Clamp(rot_x_diff, -rot_x_diff_limit*Time.deltaTime, rot_x_diff_limit*Time.deltaTime);

			rot.x = current.x + rot_x_diff;

		} else {
		
			rot.x = current.x;
			rot.x *= 0.9f;
		}

		if(this.step == STEP.MISS) {

			rot.x = current.x;

			if(this.is_grounded) {

				rot.x *= 0.9f;
			}
		}

		// ---------------------------------------------------------------- //
		// 左右のローテーション.

		rot.y = 0.0f;	
		
		rot.y = this.GetComponent<Rigidbody>().velocity.x/10.0f;
		
		float	rot_y_diff = rot.y - current.y;
		
		rot_y_diff = Mathf.Clamp(rot_y_diff, -0.015f, 0.015f);
		
		rot.y = current.y + rot_y_diff;

	
		rot.z = 0.0f;

		// ---------------------------------------------------------------- //

		// 子供（モデル）のみを回転する.

		this.transform.GetChild(0).transform.localRotation = Quaternion.identity;
		this.transform.GetChild(0).transform.localPosition = Vector3.zero;

		this.transform.GetChild(0).transform.Translate(COLLISION_OFFSET);
		this.transform.GetChild(0).transform.localRotation *= rot;
		this.transform.GetChild(0).transform.Translate(-COLLISION_OFFSET);
	}

	// 左右への平行移動.
	private	Vector3	side_move(Vector3 velocity, float slide_speed_scale)
	{

		if(Input.GetKey(KeyCode.LeftArrow)) {

			velocity.x -= SLIDE_ACCEL*slide_speed_scale*Time.deltaTime;

		} else if(Input.GetKey(KeyCode.RightArrow)) {

			velocity.x += SLIDE_ACCEL*slide_speed_scale*Time.deltaTime;

		} else {

			// 左右キーがどちらも押されていないときは、速度０に戻る.

			if(velocity.x > 0.0f) {

				velocity.x -= SLIDE_ACCEL*slide_speed_scale*Time.deltaTime;

				velocity.x = Mathf.Max(velocity.x, 0.0f);

			} else {

				velocity.x += SLIDE_ACCEL*slide_speed_scale*Time.deltaTime;

				velocity.x = Mathf.Min(velocity.x, 0.0f);
			}
		}

		velocity.x = Mathf.Clamp(velocity.x, -SLIDE_SPEED_MAX, SLIDE_SPEED_MAX);

		return(velocity);
	}

	// 左右への平行移動（自動運転）.
	private	Vector3	side_move_auto_drive(Vector3 velocity, float slide_speed_scale)
	{
		const float		center_x = 0.0001f;

		if(this.transform.position.x > center_x) {

			velocity.x -= SLIDE_ACCEL*slide_speed_scale*Time.deltaTime;

		} else if(this.transform.position.x < -center_x) {

			velocity.x += SLIDE_ACCEL*slide_speed_scale*Time.deltaTime;

		} else {

			// 左右キーがどちらも押されていないときは、速度０に戻る.

			if(velocity.x > 0.0f) {

				velocity.x -= SLIDE_ACCEL*slide_speed_scale*Time.deltaTime;

				velocity.x = Mathf.Max(velocity.x, 0.0f);

			} else {

				velocity.x += SLIDE_ACCEL*slide_speed_scale*Time.deltaTime;

				velocity.x = Mathf.Min(velocity.x, 0.0f);
			}
		}

		// 真中に近づいてきたら、徐々に横移動が少なくなる（直進に近くなる）ように.
		velocity.x = Mathf.Clamp(velocity.x, -Mathf.Abs(this.transform.position.x), Mathf.Abs(this.transform.position.x));


		velocity.x = Mathf.Clamp(velocity.x, -SLIDE_SPEED_MAX, SLIDE_SPEED_MAX);

		return(velocity);
	}	
	
	// 自動運転開始（クリアー後）.
	public void	beginAutoDrive()
	{
		this.is_auto_drive = true;
	}

}
