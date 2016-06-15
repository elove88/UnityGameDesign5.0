using UnityEngine;
using System.Collections;

public class FloorControl : MonoBehaviour {

	// ---------------------------------------------------------------- //

	public enum STEP {

		NONE = -1,

		CLOSE = 0,			// ふすまが閉まってる.
		OPEN,				// 開いている.

		TO_OPEN,			// ふすまが閉まっているところから開く.

		CLOSE_SHOJI,		// 障子が閉まっている.

		TO_CLOSE_SHOJI,		// 開いているところから、障子が閉まる.

		NUM,
	};

	public STEP			step      = STEP.NONE;
	public STEP			next_step = STEP.NONE;
	public float		step_timer = 0.0f;
	public float		step_timer_prev = 0.0f;


	// ---------------------------------------------------------------- //

	// 床の幅（Z方向）.
	public static float WIDTH = 15.0f;

	// 床モデルの数.
	public static int MODEL_NUM = 3;

	public GameObject	shojiPrefab = null;
	public GameObject	fusumaPrefab = null;

	// ふすま、障子の座標
	public static float		SHUTTER_POSITION_Z       =  15.0f;		// Z座標.
	public static float		SHUTTER_POSITION_OPEN_X  =  4.1f;		// X座標（開いているとき）.
	public static float		SHUTTER_POSITION_CLOSE_X =  1.35f;		// X座標（閉じているとき）.

	public static int		FUSUMA_NUM = 2;
	public static int		SHOJI_NUM = 1;

	private	GameObject[]	fusuma_objects;
	private	ShojiControl	shoji_object = null;

	// ---------------------------------------------------------------- //
	
	// 障子の登場パターンのタイプ.
	public enum CLOSING_PATTERN_TYPE {

		NONE = -1,

		NORMAL = 0,			// ふつう.
		OVERSHOOT,			// 右から現れて、左に止まる.
		SECONDTIME,			// 一回目現れずに二回目で現れる.
		ARCODION,			// 左から障子とふすまが同時に.

		DELAY,				// 左からふすま、少しおくれて右から障子.
		FALLDOWN,			// 上から障子が落ちてくる.
		FLIP,				// ふすまが２枚しまった後、右側がくるんと回って障子になる.

		SLOW,				// ゆっくり.
		SUPER_DELAY,		// 左からふすま、スーパーおくれて右から障子.

		NUM,
	};

	public CLOSING_PATTERN_TYPE		closing_pattern_type = CLOSING_PATTERN_TYPE.NORMAL;
	public bool						is_flip_closing = false;								// 登場パターンを左右反転する？.

	// 障子の登場パターンのデーター.
	public struct ClosingPattern {

		public float	total_time;					// トータルの時間.
		public int		fusuma_num;					// ふすまの数.

		// 毎フレーム更新.

		public 	float[]	fusuma_x;					// 各ふすまのX座標（毎フレーム更新）.
		public 	float	shoji_x;					// 障子のX座標（毎フレーム更新）.
		public	float	shoji_y;
		public	float	shoji_z_offset;				// 障子のZ座標のオフセット（毎フレーム更新）.

		public 	float[]	fusuma_rot_x;				// 各ふすまのX座標（毎フレーム更新）.
		public 	float	shoji_rot_x;				// 障子のX座標（毎フレーム更新）.

		public	bool	is_play_close_sound;		// 障子が閉まっているときの SE を鳴らす.
		public	bool	is_play_close_end_sound;	// 障子が閉まりきったときの SE を鳴らす.

		public	float	se_volume;
		public	float	se_pitch;					// SE のピッチ.

		public	float	previous_distance;			// 前のフレームでの、RoomControl.getDistanceNekoToShutter()
		public	float	local_timer;

		public	ClosingPatternParam	param;			// 汎用パラメーター
	};

	// 障子の登場パターンのデーターの、汎用パラメーター.
	public struct ClosingPatternParam {

		public	float	as_float;
		public	bool	as_bool;
	}

	public ClosingPattern	closing_pattern;

	// Sound
	public AudioClip CLOSE_SOUND = null;
	public AudioClip CLOSE_END_SOUND = null;

	// ---------------------------------------------------------------- //

	void Start() 
	{
		//

		this.fusuma_objects = new GameObject[FUSUMA_NUM];

		for(int i = 0;i < FUSUMA_NUM;i++) {

			this.fusuma_objects[i] = Instantiate(this.fusumaPrefab) as GameObject;

			this.fusuma_objects[i].transform.parent = this.gameObject.transform;

			this.fusuma_objects[i].transform.localPosition = new Vector3( SHUTTER_POSITION_OPEN_X, 0.0f, SHUTTER_POSITION_Z);
		}

		//

		this.closing_pattern_type = CLOSING_PATTERN_TYPE.NORMAL;
	}

	void Update()
	{
		this.step_timer_prev = this.step_timer;
		this.step_timer += Time.deltaTime;

		const float		to_open_time = 0.5f;

		// ---------------------------------------------------------------- //
		// 次の状態に移るかどうかを、チェックする.

		if(this.next_step == STEP.NONE) {

			switch(this.step) {
	
				case STEP.TO_OPEN:
				{
					if(this.step_timer > to_open_time) {

						this.next_step = STEP.OPEN;
					}
				}
				break;

				case STEP.TO_CLOSE_SHOJI:
				{
					if(this.step_timer > this.closing_pattern.total_time + Time.deltaTime) {

						this.next_step = STEP.CLOSE_SHOJI;
					}
				}
				break;
			}
		}

		// ---------------------------------------------------------------- //
		// 状態が遷移したときの初期化.

		if(this.next_step != STEP.NONE) {

			switch(this.next_step) {
	
				case STEP.CLOSE:
				{
					this.reset_shutters();

					this.fusuma_objects[0].SetActive(true);
					this.fusuma_objects[1].SetActive(true);

					this.fusuma_objects[0].GetComponent<ShutterControl>().setX(-SHUTTER_POSITION_CLOSE_X);
					this.fusuma_objects[1].GetComponent<ShutterControl>().setX( SHUTTER_POSITION_CLOSE_X);
				}
				break;

				case STEP.OPEN:
				{
					this.reset_shutters();

					this.fusuma_objects[0].SetActive(true);
					this.fusuma_objects[1].SetActive(true);

					this.fusuma_objects[0].GetComponent<ShutterControl>().setX(-SHUTTER_POSITION_OPEN_X);
					this.fusuma_objects[1].GetComponent<ShutterControl>().setX( SHUTTER_POSITION_OPEN_X);
				}
				break;

				case STEP.TO_CLOSE_SHOJI:
				{
					this.closing_pattern_init();
				}
				break;

				case STEP.CLOSE_SHOJI:
				{
				}
				break;
			}

			this.step      = this.next_step;
			this.next_step = STEP.NONE;

			this.step_timer_prev = -Time.deltaTime;
			this.step_timer      = 0.0f;
		}

		// ---------------------------------------------------------------- //
		// 各状態での実行処理.


		switch(this.step) {

			case STEP.TO_OPEN:
			{
				float	rate;
				float	x;

				rate = Mathf.Clamp01(this.step_timer/to_open_time);
				rate = Mathf.Sin(Mathf.Lerp(0.0f, Mathf.PI/2.0f, rate));

				x = Mathf.Lerp(SHUTTER_POSITION_CLOSE_X, SHUTTER_POSITION_OPEN_X, rate);

				this.fusuma_objects[0].GetComponent<ShutterControl>().setX(x);

				//

				x = Mathf.Lerp(-SHUTTER_POSITION_CLOSE_X, -SHUTTER_POSITION_OPEN_X, rate);

				this.fusuma_objects[1].GetComponent<ShutterControl>().setX(x);
			}
			break;

			case STEP.TO_CLOSE_SHOJI:
			{
				this.closing_pattern_execute();
			}
			break;

		}

		// ---------------------------------------------------------------- //
	}

	private void	reset_shutters()
	{
		for(int i = 0;i < this.fusuma_objects.Length;i++) {

			this.fusuma_objects[i].SetActive(false);
		}
	}

	// 登場パターンを初期化する.
	private void	closing_pattern_init()
	{
		switch(this.closing_pattern_type) {

			case CLOSING_PATTERN_TYPE.NORMAL:
			{
				this.closing_pattern.fusuma_num = 1;
				this.closing_pattern.total_time = 0.5f;
			}
			break;

			case CLOSING_PATTERN_TYPE.OVERSHOOT:
			{
				this.closing_pattern.fusuma_num = 2;
				this.closing_pattern.total_time = 1.0f;
			}
			break;

			case CLOSING_PATTERN_TYPE.SECONDTIME:
			{
				this.closing_pattern.fusuma_num = 2;
				this.closing_pattern.total_time = 1.0f;
			}
			break;

			case CLOSING_PATTERN_TYPE.ARCODION:
			{
				this.closing_pattern.fusuma_num = 1;
				this.closing_pattern.total_time = 0.5f;
			}
			break;

			case CLOSING_PATTERN_TYPE.DELAY:
			{
				this.closing_pattern.fusuma_num = 1;
				this.closing_pattern.total_time = 0.8f;
			}
			break;

			case CLOSING_PATTERN_TYPE.FALLDOWN:
			{
				this.closing_pattern.fusuma_num = 2;
				this.closing_pattern.total_time = 1.0f;
			}
			break;

			case CLOSING_PATTERN_TYPE.FLIP:
			{
				this.closing_pattern.fusuma_num = 2;
				this.closing_pattern.total_time = 1.0f;
			}
			break;

			case CLOSING_PATTERN_TYPE.SLOW:
			{
				this.closing_pattern.fusuma_num = 2;
				//this.closing_pattern.total_time = 2.0f;
				this.closing_pattern.total_time = this.closing_pattern.param.as_float;
			}
			break;

			case CLOSING_PATTERN_TYPE.SUPER_DELAY:
			{
				this.closing_pattern.fusuma_num = 1;
				this.closing_pattern.total_time = 2.5f;

			}
			break;
		}

		this.closing_pattern.fusuma_x     = new float[this.closing_pattern.fusuma_num];
		this.closing_pattern.fusuma_rot_x = new float[this.closing_pattern.fusuma_num];

		//

		this.reset_shutters();

		for(int i = 0;i < this.closing_pattern.fusuma_num;i++) {

			this.fusuma_objects[i].SetActive(true);

			this.closing_pattern.fusuma_x[i] = -SHUTTER_POSITION_OPEN_X;

			this.closing_pattern.fusuma_rot_x[i] = 0.0f;
		}

		this.closing_pattern.shoji_x = SHUTTER_POSITION_OPEN_X;

		this.closing_pattern.shoji_rot_x = 0.0f;

		// 左側にあるふすまを左右反転する.
		//

		Vector3	scale = new Vector3(-1.0f, 1.0f, 1.0f);

		if(this.is_flip_closing) {

			scale.x *= -1.0f;
		}

		this.fusuma_objects[0].transform.localScale = scale;

		scale.x *= -1.0f;

		for(int i = 1;i < this.closing_pattern.fusuma_num;i++) {

			this.fusuma_objects[i].transform.localScale = scale;
		}

	}

	// ステップタイマーが time をまたいだ？.
	private bool	is_step_timer_reach(float time)
	{
		bool	ret = false;

		if(this.step_timer_prev < time && time <= this.step_timer) {

			ret = true;
		}

		return(ret);
	}
	
	// 登場パターンの実行.
	private void closing_pattern_execute()
	{
		float	rate;

		// 「毎フレーム更新する値」を初期化しておく.

		for(int i = 0;i < this.closing_pattern.fusuma_num;i++) {

			this.closing_pattern.fusuma_x[i]     = SHUTTER_POSITION_OPEN_X;
			this.closing_pattern.fusuma_rot_x[i] = 0.0f;
		}

		this.closing_pattern.shoji_x        = SHUTTER_POSITION_OPEN_X;
		this.closing_pattern.shoji_y        = 0.0f;
		this.closing_pattern.shoji_z_offset = 0.0f;
		this.closing_pattern.shoji_rot_x    = 0.0f;

		this.closing_pattern.is_play_close_sound     = false;
		this.closing_pattern.is_play_close_end_sound = false;

		this.closing_pattern.se_volume = 1.0f;
		this.closing_pattern.se_pitch  = 1.0f;

		// 現在の位置、回転等を更新する.

		switch(this.closing_pattern_type) {

			case CLOSING_PATTERN_TYPE.NORMAL:
			{
				rate = Mathf.Clamp01(this.step_timer/this.closing_pattern.total_time);
				rate = Mathf.Sin(Mathf.Lerp(0.0f, Mathf.PI/2.0f, rate));

				this.closing_pattern.shoji_x = Mathf.Lerp(SHUTTER_POSITION_OPEN_X, SHUTTER_POSITION_CLOSE_X, rate);

				this.closing_pattern.fusuma_x[0] = Mathf.Lerp(-SHUTTER_POSITION_OPEN_X, -SHUTTER_POSITION_CLOSE_X, rate);

				if(this.is_step_timer_reach(0.0f)) {

					this.closing_pattern.is_play_close_sound = true;
				}
				if(this.is_step_timer_reach(this.closing_pattern.total_time)) {

					this.closing_pattern.is_play_close_end_sound = true;
				}
			}
			break;

			case CLOSING_PATTERN_TYPE.OVERSHOOT:
			{
				rate = Mathf.Clamp01(this.step_timer/this.closing_pattern.total_time);
				rate = Mathf.Sin(Mathf.Lerp(0.0f, Mathf.PI/2.0f, rate));

				this.closing_pattern.shoji_x = Mathf.Lerp(SHUTTER_POSITION_OPEN_X, -SHUTTER_POSITION_CLOSE_X, rate);

				if(rate < 0.5f) {

					rate = Mathf.InverseLerp(0.0f, 0.5f, rate);

					this.closing_pattern.fusuma_x[0] = Mathf.Lerp(-SHUTTER_POSITION_OPEN_X, -SHUTTER_POSITION_CLOSE_X, rate);

				} else {

					rate = Mathf.InverseLerp(0.5f, 1.0f, rate);

					this.closing_pattern.fusuma_x[0] = Mathf.Lerp(-SHUTTER_POSITION_CLOSE_X, -SHUTTER_POSITION_OPEN_X, rate);

					this.closing_pattern.fusuma_x[1] = Mathf.Lerp( SHUTTER_POSITION_OPEN_X,  SHUTTER_POSITION_CLOSE_X, rate);
				}

				if(this.is_step_timer_reach(0.0f)) {

					this.closing_pattern.is_play_close_sound = true;
				}
				if(this.is_step_timer_reach(this.closing_pattern.total_time*Mathf.Asin(0.5f)/(Mathf.PI/2.0f))) {

					this.closing_pattern.is_play_close_end_sound = true;
				}
			}
			break;
			
			case CLOSING_PATTERN_TYPE.SECONDTIME:
			{
				rate = Mathf.Clamp01(this.step_timer/this.closing_pattern.total_time);
				rate = Mathf.Sin(Mathf.Lerp(0.0f, Mathf.PI/2.0f, rate));

				this.closing_pattern.fusuma_x[1] = Mathf.Lerp( SHUTTER_POSITION_OPEN_X, -SHUTTER_POSITION_CLOSE_X, rate);

				if(rate < 0.5f) {

					rate = Mathf.InverseLerp(0.0f, 0.5f, rate);

					this.closing_pattern.fusuma_x[0] = Mathf.Lerp(-SHUTTER_POSITION_OPEN_X, -SHUTTER_POSITION_CLOSE_X, rate);

				} else {

					rate = Mathf.InverseLerp(0.5f, 1.0f, rate);

					this.closing_pattern.fusuma_x[0] = Mathf.Lerp(-SHUTTER_POSITION_CLOSE_X, -SHUTTER_POSITION_OPEN_X, rate);
					
					this.closing_pattern.shoji_x = Mathf.Lerp(SHUTTER_POSITION_OPEN_X, SHUTTER_POSITION_CLOSE_X, rate);
				}

				if(this.is_step_timer_reach(0.0f)) {

					this.closing_pattern.is_play_close_sound = true;
				}
				if(this.is_step_timer_reach(this.closing_pattern.total_time*Mathf.Asin(0.5f)/(Mathf.PI/2.0f))) {

					this.closing_pattern.is_play_close_end_sound = true;
				}
			}
			break;

			case CLOSING_PATTERN_TYPE.ARCODION:
			{
				rate = Mathf.Clamp01(this.step_timer/this.closing_pattern.total_time);
				rate = Mathf.Sin(Mathf.Lerp(0.0f, Mathf.PI/2.0f, rate));

				this.closing_pattern.shoji_x = Mathf.Lerp(-SHUTTER_POSITION_OPEN_X, SHUTTER_POSITION_CLOSE_X, rate);
				this.closing_pattern.shoji_z_offset = 0.01f;

				this.closing_pattern.fusuma_x[0] = Mathf.Lerp(-SHUTTER_POSITION_OPEN_X, -SHUTTER_POSITION_CLOSE_X, rate);

				if(this.is_step_timer_reach(0.0f)) {

					this.closing_pattern.is_play_close_sound = true;
				}
				if(this.is_step_timer_reach(this.closing_pattern.total_time)) {

					this.closing_pattern.is_play_close_end_sound = true;
				}
			}
			break;

			case CLOSING_PATTERN_TYPE.DELAY:
			{
				rate = Mathf.Clamp01(this.step_timer/this.closing_pattern.total_time);

				const float	time0 = 0.3f;
				const float	time1 = 0.7f;

				if(rate < time0) {

					// 右側にふすまがさっとしまる.

					rate = Mathf.InverseLerp(0.0f, time0, rate);
					rate = Mathf.Sin(Mathf.Lerp(0.0f, Mathf.PI/2.0f, rate));

					this.closing_pattern.fusuma_x[0] = Mathf.Lerp(-SHUTTER_POSITION_OPEN_X, -SHUTTER_POSITION_CLOSE_X, rate);
					this.closing_pattern.shoji_x     = SHUTTER_POSITION_OPEN_X;

				} else if(rate < time1) {

					// しばらくためる.

					this.closing_pattern.fusuma_x[0] = -SHUTTER_POSITION_CLOSE_X;
					this.closing_pattern.shoji_x     =  SHUTTER_POSITION_OPEN_X;

				} else {

					// 左側に障子がさっとしまる.

					rate = Mathf.InverseLerp(time1, 1.0f, rate);
					rate = Mathf.Sin(Mathf.Lerp(0.0f, Mathf.PI/2.0f, rate));

					this.closing_pattern.fusuma_x[0] = -SHUTTER_POSITION_CLOSE_X;

					if(this.closing_pattern.param.as_bool) {

						// 障子が左から（ふすまの裏から）出てくる.

						this.closing_pattern.shoji_x =  Mathf.Lerp(-SHUTTER_POSITION_CLOSE_X, SHUTTER_POSITION_CLOSE_X, rate);

						// ふすまのモデルからはみ出ないよう、少し後ろにずらす.
						this.closing_pattern.shoji_z_offset = 0.01f;

					} else {

						this.closing_pattern.shoji_x =  Mathf.Lerp(SHUTTER_POSITION_OPEN_X, SHUTTER_POSITION_CLOSE_X, rate);
					}
				}

				if(this.is_step_timer_reach(0.0f)) {

					this.closing_pattern.is_play_close_sound = true;
					this.closing_pattern.se_pitch = 2.0f;
				}
				if(this.is_step_timer_reach(this.closing_pattern.total_time*time1)) {

					this.closing_pattern.is_play_close_sound = true;
					this.closing_pattern.se_pitch = 2.0f;
				}
				if(this.is_step_timer_reach(this.closing_pattern.total_time)) {

					this.closing_pattern.is_play_close_end_sound = true;
					this.closing_pattern.se_pitch = 1.5f;
				}
			}
			break;

			case CLOSING_PATTERN_TYPE.FALLDOWN:
			{
				const float		height0 = 6.0f;
				const float		height1 = height0/16.0f;

				rate = Mathf.Clamp01(this.step_timer/this.closing_pattern.total_time);

				if(rate < 0.1f) {

					// 両側からふすまが閉まる（だいぶすき間をあけて）.

					rate = Mathf.InverseLerp(0.0f, 0.1f, rate);

					this.closing_pattern.fusuma_x[0] = Mathf.Lerp(-SHUTTER_POSITION_OPEN_X, -SHUTTER_POSITION_CLOSE_X*2.0f, rate);
					this.closing_pattern.fusuma_x[1] = Mathf.Lerp( SHUTTER_POSITION_OPEN_X,  SHUTTER_POSITION_CLOSE_X*2.0f, rate);

					this.closing_pattern.shoji_y = height0;

				} else {

					// 上から障子が落ちてくる.

					rate = Mathf.InverseLerp(0.1f, 1.0f, rate);

					this.closing_pattern.fusuma_x[0] = -SHUTTER_POSITION_CLOSE_X*2.0f;
					this.closing_pattern.fusuma_x[1] =  SHUTTER_POSITION_CLOSE_X*2.0f;

					this.closing_pattern.shoji_x = 0.0f;

					//

					const float	fall_time0 = 0.5f;
					const float	fall_time1 = 0.75f;
	
					if(rate < fall_time0) {
	
						rate = Mathf.InverseLerp(0.0f, fall_time0, rate);
	
						rate = rate*rate;

						this.closing_pattern.shoji_y = Mathf.Lerp(height0, 0.0f, rate);

					} else if(rate < fall_time1) {
	
						// バウンドする.

						this.closing_pattern.shoji_x = 0.0f;
	
						rate = Mathf.InverseLerp(fall_time0, fall_time1, rate);
	
						rate = Mathf.Lerp(-1.0f, 1.0f, rate);
	
						rate = 1.0f - rate*rate;

						this.closing_pattern.shoji_y = Mathf.Lerp(0.0f, height1, rate);

					} else {
	
						Vector3	position = this.shoji_object.transform.position;
		
						position.y = 0.0f;
		
						this.shoji_object.transform.position = position;
					}
				}

				if(this.is_step_timer_reach(0.0f)) {

					this.closing_pattern.is_play_close_sound = true;
					this.closing_pattern.se_pitch = 3.0f;
				}
				if(this.is_step_timer_reach(this.closing_pattern.total_time*0.1f)) {

					this.closing_pattern.is_play_close_sound = true;
				}
				if(this.is_step_timer_reach(this.closing_pattern.total_time*(0.1f + 0.9f*0.5f))) {

					this.closing_pattern.is_play_close_end_sound = true;
				}
				if(this.is_step_timer_reach(this.closing_pattern.total_time*(0.1f + 0.9f*0.75f))) {

					this.closing_pattern.is_play_close_end_sound = true;
					this.closing_pattern.se_volume = 0.1f;
				}
			}
			break;

			case CLOSING_PATTERN_TYPE.FLIP:
			{
				rate = Mathf.Clamp01(this.step_timer/this.closing_pattern.total_time);
				rate = Mathf.Sin(Mathf.Lerp(0.0f, Mathf.PI/2.0f, rate));

				const float	time0 = 0.3f;
				const float	time1 = 0.7f;

				if(rate < time0) {

					// さっとしまる（両方ともふすま）.

					rate = Mathf.InverseLerp(0.0f, time0, rate);

					this.closing_pattern.fusuma_x[0] = Mathf.Lerp(-SHUTTER_POSITION_OPEN_X, -SHUTTER_POSITION_CLOSE_X, rate);
					this.closing_pattern.fusuma_x[1] = Mathf.Lerp( SHUTTER_POSITION_OPEN_X,  SHUTTER_POSITION_CLOSE_X, rate);
					this.closing_pattern.shoji_x     = SHUTTER_POSITION_OPEN_X;

				} else if(rate < time1) {

					// しばらくためる.

					this.closing_pattern.fusuma_x[0] = -SHUTTER_POSITION_CLOSE_X;
					this.closing_pattern.fusuma_x[1] =  SHUTTER_POSITION_CLOSE_X;
					this.closing_pattern.shoji_x     =  SHUTTER_POSITION_OPEN_X;

				} else {

					// 右側がくるんとまわって障子になる.

					this.closing_pattern.fusuma_x[0] = -SHUTTER_POSITION_CLOSE_X;
					this.closing_pattern.fusuma_x[1] =  SHUTTER_POSITION_CLOSE_X;
					this.closing_pattern.shoji_x     =  SHUTTER_POSITION_OPEN_X;

					//

					rate = Mathf.InverseLerp(time1, 1.0f, rate);

					if(rate < 0.5f) {

						// ０～９０度　ふすまを表示.

						rate = Mathf.InverseLerp(0.0f, 0.5f, rate);

						this.closing_pattern.fusuma_x[1] =  SHUTTER_POSITION_CLOSE_X;
						this.closing_pattern.shoji_x     =  SHUTTER_POSITION_OPEN_X;

						//

						this.closing_pattern.fusuma_rot_x[1] = Mathf.Lerp(0.0f, 90.0f, rate);
						this.closing_pattern.shoji_rot_x     = 0.0f;


					} else {

						// ９０～１８０度　障子を表示.

						rate = Mathf.InverseLerp(0.5f, 1.0f, rate);

						this.closing_pattern.fusuma_x[1] =  SHUTTER_POSITION_OPEN_X;
						this.closing_pattern.shoji_x     =  SHUTTER_POSITION_CLOSE_X;

						//

						this.closing_pattern.fusuma_rot_x[1] = 0.0f;
						this.closing_pattern.shoji_rot_x     = Mathf.Lerp(-90.0f, 0.0f, rate);

					}
				}

				if(this.is_step_timer_reach(0.0f)) {

					this.closing_pattern.is_play_close_sound = true;
					this.closing_pattern.se_pitch = 2.0f;
				}
				if(this.is_step_timer_reach(this.closing_pattern.total_time*time0)) {

					this.closing_pattern.is_play_close_end_sound = true;
					this.closing_pattern.se_pitch = 1.5f;
				}
				if(this.is_step_timer_reach(this.closing_pattern.total_time)) {

					this.closing_pattern.is_play_close_end_sound = true;
					this.closing_pattern.se_pitch = 1.5f;
				}
			}
			break;

			case CLOSING_PATTERN_TYPE.SLOW:
			{
				rate = Mathf.Clamp01(this.step_timer/this.closing_pattern.total_time);
				rate = Mathf.Sin(Mathf.Lerp(0.0f, Mathf.PI/2.0f, rate));

				this.closing_pattern.shoji_x = Mathf.Lerp(SHUTTER_POSITION_OPEN_X, SHUTTER_POSITION_CLOSE_X, rate);

				this.closing_pattern.fusuma_x[0] = Mathf.Lerp(-SHUTTER_POSITION_OPEN_X, -SHUTTER_POSITION_CLOSE_X, rate);

				if(this.is_step_timer_reach(0.0f)) {

					this.closing_pattern.is_play_close_sound = true;
					this.closing_pattern.se_pitch = 0.5f;
				}
				if(this.is_step_timer_reach(this.closing_pattern.total_time)) {

					this.closing_pattern.is_play_close_end_sound = true;
					this.closing_pattern.se_pitch = 0.5f;
					this.closing_pattern.se_volume = 0.5f;
				}
			}
			break;

			case CLOSING_PATTERN_TYPE.SUPER_DELAY:
			{
				rate = Mathf.Clamp01(this.step_timer/this.closing_pattern.total_time);

				const float	time0 = 0.1f;
				float time1 = this.closing_pattern.param.as_float;
				float time2 = time1 + 0.1f;

				if(rate < time0) {

					// ふすまがさっとしまる.

					rate = Mathf.InverseLerp(0.0f, time0, rate);
					rate = Mathf.Sin(Mathf.Lerp(0.0f, Mathf.PI/2.0f, rate));

					this.closing_pattern.fusuma_x[0] = Mathf.Lerp(-SHUTTER_POSITION_OPEN_X, -SHUTTER_POSITION_CLOSE_X, rate);
					this.closing_pattern.shoji_x     = SHUTTER_POSITION_OPEN_X;

				} else if(rate < time1) {

					// しばらくためる.

					this.closing_pattern.fusuma_x[0] = -SHUTTER_POSITION_CLOSE_X;
					this.closing_pattern.shoji_x     =  SHUTTER_POSITION_OPEN_X;

				} else if(rate < time2) {

					// 左側に障子がさっとしまる.

					rate = Mathf.InverseLerp(time1, time2, rate);
					rate = Mathf.Sin(Mathf.Lerp(0.0f, Mathf.PI/2.0f, rate));

					this.closing_pattern.fusuma_x[0] = -SHUTTER_POSITION_CLOSE_X;
					this.closing_pattern.shoji_x     =  Mathf.Lerp(SHUTTER_POSITION_OPEN_X, SHUTTER_POSITION_CLOSE_X, rate);

				} else {

					this.closing_pattern.fusuma_x[0] = -SHUTTER_POSITION_CLOSE_X;
					this.closing_pattern.shoji_x     =  SHUTTER_POSITION_CLOSE_X;
				}
				//

				if(this.is_step_timer_reach(0.0f)) {

					this.closing_pattern.is_play_close_sound = true;
					this.closing_pattern.se_pitch = 2.0f;
				}
				if(this.is_step_timer_reach(this.closing_pattern.total_time*time1)) {

					this.closing_pattern.is_play_close_sound = true;
					this.closing_pattern.se_pitch = 2.0f;
				}
				if(this.is_step_timer_reach(this.closing_pattern.total_time*time2)) {

					this.closing_pattern.is_play_close_end_sound = true;
					this.closing_pattern.se_pitch = 1.5f;
				}
			}
			break;
		}

		// 位置、回転等を GameObject に反映させる.

		for(int i = 0;i < this.closing_pattern.fusuma_num;i++) {

			if(!this.is_flip_closing) {

				this.fusuma_objects[i].GetComponent<ShutterControl>().setX(this.closing_pattern.fusuma_x[i]);
				this.fusuma_objects[i].transform.rotation = Quaternion.AngleAxis(this.closing_pattern.fusuma_rot_x[i], Vector3.up);

			} else {

				this.fusuma_objects[i].GetComponent<ShutterControl>().setX(-this.closing_pattern.fusuma_x[i]);
				this.fusuma_objects[i].transform.rotation = Quaternion.AngleAxis(-this.closing_pattern.fusuma_rot_x[i], Vector3.up);
			}
		}

		if(this.shoji_object != null) {

			Vector3	position = this.shoji_object.transform.localPosition;

			if(!this.is_flip_closing) {

				position.x = this.closing_pattern.shoji_x;
				position.y = this.closing_pattern.shoji_y;

				this.shoji_object.transform.rotation = Quaternion.AngleAxis(this.closing_pattern.shoji_rot_x, Vector3.up);

			} else {

				position.x = -this.closing_pattern.shoji_x;
				position.y =  this.closing_pattern.shoji_y;

				this.shoji_object.transform.rotation = Quaternion.AngleAxis(-this.closing_pattern.shoji_rot_x, Vector3.up);
			}

			position.z = SHUTTER_POSITION_Z + this.closing_pattern.shoji_z_offset;

			this.shoji_object.transform.localPosition = position;
		}

		// サウンド.

		if(this.closing_pattern.is_play_close_sound) {

			this.GetComponent<AudioSource>().PlayOneShot(this.CLOSE_SOUND, this.closing_pattern.se_volume);
			this.GetComponent<AudioSource>().pitch = this.closing_pattern.se_pitch;
		}
		if(this.closing_pattern.is_play_close_end_sound) {

			this.GetComponent<AudioSource>().PlayOneShot(this.CLOSE_END_SOUND, this.closing_pattern.se_volume);
			this.GetComponent<AudioSource>().pitch = this.closing_pattern.se_pitch;
		}
	}

	//! 障子をアタッチする（この部屋モデルの子供にする）.
	public void	attachShouji(ShojiControl shoji)
	{
		this.shoji_object = shoji;

		if(this.shoji_object != null) {

			this.shoji_object.transform.parent = this.gameObject.transform;

			this.shoji_object.transform.localPosition = new Vector3( SHUTTER_POSITION_OPEN_X, 0.0f, SHUTTER_POSITION_Z);
		}
	}

	// 障子の登場パターンをセットする.
	public void	setClosingPatternType(CLOSING_PATTERN_TYPE type, bool is_flip)
	{
		ClosingPatternParam		param;

		param.as_float = 0.0f;
		param.as_bool = true;

		this.setClosingPatternType(type, is_flip, param);
	}
	// 障子の登場パターンをセットする.
	public void	setClosingPatternType(CLOSING_PATTERN_TYPE type, bool is_flip, ClosingPatternParam param)
	{
		this.closing_pattern_type = type;

		this.is_flip_closing = is_flip;

		this.closing_pattern.param = param;
	}

	public void	setClose()
	{
		this.next_step = STEP.CLOSE;
	}
	public void	setOpen()
	{
		this.next_step = STEP.OPEN;
	}

	public void	beginOpen()
	{
		this.next_step = STEP.TO_OPEN;
	}
	public void	beginCloseShoji()
	{
		this.next_step = STEP.TO_CLOSE_SHOJI;
	}
}
