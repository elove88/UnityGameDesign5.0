using UnityEngine;
using System.Collections;

public class PlayerControl : MonoBehaviour {

	public int	lx = 0;

	// 持ち上げ中のブロックのプレハブ.
	public 	GameObject	CarryBlockPrefab = null;

	public	GameObject	effect;

	// テクスチャー.
	public Texture[]	textures_normal = null;		// 通常時.
	public Texture[]	textures_carry  = null;		// ブロック持ち上げ中.
	public Texture[]	textures_eating = null;		// ケーキ食べ中.
	public Texture		texture_hungry  = null;

	public AudioClip	audio_walk;
	public AudioClip	audio_pick;

	// ---------------------------------------------------------------- //

	// 持ち上げ中のブロック.
	public CarryBlock	carry_block = null;

	public SceneControl	scene_control = null;


	// 表示用簡易スプライト.
	public SimpleSprite	sprite = null;


	// ---------------------------------------------------------------- //
	// ライフ.

	public static int	LIFE_MIN = 0;				// 最大値.
	public static int	LIFE_MAX = 100;				// 最大値.
	public static int	LIFE_ADD_CAKE = LIFE_MAX;	// ケーキを食べたときに増える値.
	public static int	LIFE_SUB = -2;

	public int	life;								// 現在値.

	// ---------------------------------------------------------------- //

	public enum STEP {

		NONE = -1,

		NORMAL = 0,			// ふつう.
		CARRY,				// ブロック持ち上げ中.
		EATING,				// もぐもぐ中.
		HUNGRY,				// 腹減った（ミス）.

		GOAL_ACT,			// ゴール演出.

		NUM,
	};

	public STEP			step;
	public STEP			next_step = STEP.NONE;
	public float		step_timer = 0.0f;

	public bool			is_controlable = true;

	// ---------------------------------------------------------------- //

	void 	Start()
	{
		this.SetLinedPosition(StackBlockControl.BLOCK_NUM_X/2);

		GameObject game_object = Instantiate(this.CarryBlockPrefab) as GameObject;

		this.carry_block = game_object.GetComponent<CarryBlock>();

		this.carry_block.player             = this;
		this.carry_block.transform.position = this.transform.position + new Vector3(0.0f, 1.0f, 0.0f);
		this.carry_block.GetComponent<Renderer>().enabled   = false;

		//

		this.sprite = this.gameObject.AddComponent<SimpleSprite>();

		this.sprite.SetTexture(this.textures_normal[0]);

		//

		this.life = LIFE_MAX;

		this.is_controlable = true;
	}

	void Update ()
	{
		StackBlockControl	stack_control = this.scene_control.stack_control;

		this.step_timer += Time.deltaTime;

		// ---------------------------------------------------------------- //
#if false
		// "3" を押すと、エネルギー減少.
		if(Input.GetKey(KeyCode.Keypad3)) {

			this.addLife(-100);
		}
		// "4" を押すと、エネルギー減少.
		if(Input.GetKey(KeyCode.Keypad4)) {

			this.addLife(1);
		}
#endif

		// 腹ペコになったら、ゲームオーバー.
		if(this.life <= LIFE_MIN) {

			this.next_step = STEP.HUNGRY;
		}

		//
		// 次の状態に移るかどうかを、チェックする.
		switch(this.step) {

			case STEP.NORMAL:
			case STEP.EATING:
			{
				// 持ち上げ.

				if(this.next_step == STEP.NONE) {

					do {
	
						if(!this.is_carry_input()) {
						
							break;
						}
	
						// 足元のブロック.
						StackBlock	ground_block = stack_control.blocks[this.lx, StackBlockControl.GROUND_LINE];
	
						// 灰色のブロックは持ち上げられない.
						if(!ground_block.isCarriable()) {
	
							break;
						}
	
						// スワップ動作中は持ち上げられない.
						if(ground_block.isNowSwapAction()) {
	
							break;
						}
	
						//
	
						// キャリーブロックを、足元のブロックと同じ色にする.
						this.carry_block.setColorType(ground_block.color_type);
						this.carry_block.startCarry(this.lx);
	
						stack_control.pickBlock(this.lx);
	
						//

						this.GetComponent<AudioSource>().PlayOneShot(this.audio_pick);

						this.next_step = STEP.CARRY;
	
					} while(false);
				}

				if(this.next_step == STEP.NONE) {

					if(this.step == STEP.EATING) {

						if(this.step_timer > 3.0f) {
		
							this.next_step = STEP.NORMAL;
						}
					}
				}
			}
			break;

			case STEP.CARRY:
			{
				if(this.is_carry_input()) {

					// ぽい捨て.

					if(this.carry_block.isCakeBlock()) {

						// 持ち上げていたのがケーキだったら、
						// もぐもぐ＆カラーチェンジ.

						this.carry_block.startHide();

						stack_control.onEatCake();

						this.addLife(LIFE_ADD_CAKE);

						this.GetComponent<AudioSource>().PlayOneShot(scene_control.audio_clips[(int)SceneControl.SE.EATING]);

						//

						this.next_step = STEP.EATING;

					} else {

						// 持ち上げていたのが普通のブロックだったら、ぽい捨て.

						this.drop_block();

						this.addLife(LIFE_SUB);

						this.next_step = STEP.NORMAL;
					}
				}
			}
			break;
		}

		// ---------------------------------------------------------------- //
		// 状態が遷移したときの初期化.

		if(this.next_step != STEP.NONE) {

			switch(this.next_step) {
	
				case STEP.NORMAL:
				{
				}
				break;

				case STEP.HUNGRY:
				{
				}
				break;

				case STEP.GOAL_ACT:
				{
					this.SetHeight(-1);
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

			case STEP.NORMAL:
			case STEP.CARRY:
			case STEP.EATING:
			{
				int		lx = this.lx;
		
				// 左右移動.
		
				do {

					// 持ち上げ、ぽい捨て中は左右に移動できない.
					//
					// 慣れてくると、少しでも動けないときがあるのがストレスになるので
					// 封印
					//
					/*if(this.carry_block.isMoving()) {
		
						break;
					}*/
		
					//

					if(!this.is_controlable) {

						break;
					}
		
					if(Input.GetKeyDown(KeyCode.LeftArrow)) {
			
						lx--;
			
					} else if(Input.GetKeyDown(KeyCode.RightArrow)) {
			
						lx++;

					} else {

						break;
					}
			
					lx = Mathf.Clamp(lx, 0, StackBlockControl.BLOCK_NUM_X - 1);
			
					this.GetComponent<AudioSource>().PlayOneShot(this.audio_walk);

					this.SetLinedPosition(lx);
		
				} while(false);
			}
			break;
		}

		// ---------------------------------------------------------------- //
		// テクスチャーパターンのコントロール.

		switch(this.step) {

			default:
			case STEP.NORMAL:
			{
				// 左→目閉じる→右→目閉じる→ループ.

				int		texture_index;

				texture_index = (int)(this.step_timer*8.0f);
				texture_index %= 4;

				if(texture_index%2 == 0) {

					// 目を閉じる.
					texture_index = 0;

				} else {

					// 右、左
					texture_index = (texture_index/2)%2 + 1;
				}

				this.sprite.SetTexture(this.textures_normal[texture_index]);

			}
			break;

			case STEP.CARRY:
			{
				int		texture_index;

				texture_index = (int)(this.step_timer*8.0f);
				texture_index %= 4;

				if(texture_index%2 == 0) {

					texture_index = 0;

				} else {

					texture_index = (texture_index/2)%2 + 1;
				}

				this.sprite.SetTexture(this.textures_carry[texture_index]);
			}
			break;

			case STEP.EATING:
			{
				int		texture_index = ((int)(this.step_timer/0.1f))%this.textures_eating.Length;

				this.sprite.SetTexture(this.textures_eating[texture_index]);
			}
			break;

			case STEP.HUNGRY:
			{
				this.sprite.SetTexture(this.texture_hungry);
			}
			break;

			case STEP.GOAL_ACT:
			{
				const float		time0 = 0.5f;
				const float		time1 = 0.5f;

				float	time_all = time0 + time1;

				float	t = Mathf.Repeat(this.step_timer, time_all);

				if(t < time0) {

					this.sprite.SetTexture(this.textures_carry[1]);

				} else {

					t -= time0;

					int		texture_index = ((int)(t/0.1f))%this.textures_eating.Length;

					this.sprite.SetTexture(this.textures_eating[texture_index]);
				}
			}
			break;
		}
	}

	// ブロック捨てて.
	public void	dropBlock()
	{
		if(this.step == STEP.CARRY) {

			this.drop_block();

			this.next_step = STEP.NORMAL;
		}
	}
	private void	drop_block()
	{
		this.carry_block.startDrop(this.lx);
	
		this.scene_control.stack_control.dropBlock(this.lx, this.carry_block.color_type, this.carry_block.org_place.x);
	}

	// 位置をセットする.
	public void	SetLinedPosition(int lx)
	{
		this.lx = lx;

		Vector3		position = this.transform.position;

		position.x = -(StackBlockControl.BLOCK_NUM_X/2.0f - 0.5f) + this.lx;

		this.transform.position = position;
	}

	// 高さをセットする.
	public void	SetHeight(int height)
	{
		StackBlock.PlaceIndex place;

		place.x = this.lx;
		place.y = StackBlockControl.GROUND_LINE - 1 + height;

		this.transform.position = StackBlockControl.calcIndexedPosition(place);
	}

	// ライフを増減させる.
	public void		addLife(int val)
	{
		this.life += val;
	
		this.life = Mathf.Min(Mathf.Max(LIFE_MIN, this.life), LIFE_MAX);
	}

	// ライフの現在値（比率）を取得する.
	public float	getLifeRate()
	{
		float	rate = Mathf.InverseLerp((float)LIFE_MIN, (float)LIFE_MAX, (float)this.life);

		return(rate);
	}

	// ライフが０になった？.
	public bool	isHungry()
	{
		bool	ret = (this.life <= LIFE_MIN);

		return(ret);
	}

	// 操作できないようにする.
	public void	setControlable(bool sw)
	{
		this.is_controlable = sw;
	}

	// ゴール時の演出を開始して.
	public void	beginGoalAct()
	{
		this.next_step = STEP.GOAL_ACT;
	}

	// ミス後の復活（残機があるとき）.
	public void	revive()
	{
		this.life = LIFE_MAX;

		this.next_step = STEP.NORMAL;
	}

	// 持ち上げ/ぽい捨てのキー入力があった？.
	private bool	is_carry_input()
	{
		bool	ret;

		if(this.is_controlable) {

			ret = (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow));

		} else {

			ret = false;
		}

		return(ret);
	}

}
