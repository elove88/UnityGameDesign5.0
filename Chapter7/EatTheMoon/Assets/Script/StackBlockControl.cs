using UnityEngine;
using System.Collections;

// 積まれているブロック全体の制御.
public class StackBlockControl {

	public GameObject	StackBlockPrefab = null;

	// 落下するブロックの列の数.
	public static int	FALL_LINE_NUM = 3;
	public static int	BLOCK_NUM_X = 9;
	public static int	BLOCK_NUM_Y = 7 + FALL_LINE_NUM;

	// 地面のブロックが何列目？.
	//
	// ０～GROUND_LINE + 1 列目までは空中
	// GROUND_LINE ～ BLOCK_NUM_Y - 1 列目は地面の下.
	public static int	GROUND_LINE = FALL_LINE_NUM;

	public StackBlock[,]	blocks;

	public	ConnectChecker	connect_checker = null;		// 連鎖チェッカー.
	public	BlockFeeder		block_feeder = null;		// 次に出てくるブロックの色を決める.
	public 	SceneControl	scene_control = null;

	private bool	is_has_swap_block = false;			// スワップ動作中のブロックがある？.
	private bool	is_swap_end_frame = false;			// スワップ動作が終わった瞬間のみ true.

	public int		fall_request = 0;					// 落下してほしいライン数（リクエストがたまっている数）.
	private int		fall_count = 0;						// 落下中の列の数.

	public bool[]	is_color_enable = null;				// 各色のブロックが出現する？.

	public bool		is_scroll_enable = true;
	public bool		is_connect_check_enable = true;

	// ---------------------------------------------------------------- //

	public struct Combo {

		public bool	is_now_combo;

		// 連鎖の回数.
		public int	combo_count_last;		// 直前.
		public int	combo_count_current;	// 今（連鎖中のとき）.

	};

	public Combo combo;

	public int		eliminate_count;		// 消したブロックの数.
	public int		eliminate_to_fall;
	public int		eliminate_to_cake;		// ケーキが出るまでに消すブロックの数の残り.

	public static int	ELIMINATE_TO_FALL_INIT = 5;
	public static int	ELIMINATE_TO_CAKE_INIT = 5;
	public static int	ELIMINATE_TO_CAKE_INIT_2ND = 25;
	
	// ---------------------------------------------------------------- //

	public int		score = 0;
	public int		continuous_connect_num = 0;

	// カラーチェンジのときの、変化後のカラー.
	private int	change_color_index0 = -1;
	private int	change_color_index1 = -1;

	// ---------------------------------------------------------------- //

	public void	create()
	{
		//

		this.blocks = new StackBlock[BLOCK_NUM_X, BLOCK_NUM_Y];

		for(int y = 0;y < BLOCK_NUM_Y;y++) {

			for(int x = 0;x < BLOCK_NUM_X;x++) {

				GameObject	game_object = GameObject.Instantiate(this.StackBlockPrefab) as GameObject;

				StackBlock	block = game_object.GetComponent<StackBlock>();

				block.place.x = x;
				block.place.y = y;

				this.blocks[x, y] = block;

				block.setUnused();

				block.stack_control = this;
			}
		}

		//

		this.is_color_enable = new bool[Block.NORMAL_COLOR_NUM];

		for(int i = 0;i < this.is_color_enable.Length;i++) {

			this.is_color_enable[i] = true;
		}

		// ピンクは封印
		this.is_color_enable[(int)Block.COLOR_TYPE.PINK] = false;

		//

		this.connect_checker = new ConnectChecker();

		this.connect_checker.stack_control = this;
		this.connect_checker.blocks = this.blocks;
		this.connect_checker.create();

		this.block_feeder = new BlockFeeder();
		this.block_feeder.control = this;
		this.block_feeder.create();

		//

		this.setColorToAllBlock();

		//

		this.combo.is_now_combo        = false;
		this.combo.combo_count_last    = 0;
		this.combo.combo_count_current = 0;

		this.eliminate_count = 0;
		this.eliminate_to_fall = ELIMINATE_TO_FALL_INIT;
		this.eliminate_to_cake = ELIMINATE_TO_CAKE_INIT;


		this.is_scroll_enable = true;
		this.is_connect_check_enable = true;
	}

	// すべてのブロックの色を選ぶ.
	public void		setColorToAllBlock()
	{
		// places ... 色を選ぶブロックの順番を格納するための配列.
		//
		// place[0] 最初に色を選ぶブロック（の場所）.
		// place[1] ２番目に色を選ぶブロック.
		// place[2] ３番目に色を選ぶブロック.
		//            :
		//

		var	places = new StaticArray<StackBlock.PlaceIndex>(BLOCK_NUM_X*(BLOCK_NUM_Y - GROUND_LINE));

		// とりあえず左上から順番に並べる.

		for(int y = GROUND_LINE;y < BLOCK_NUM_Y;y++) {

			for(int x = 0;x < BLOCK_NUM_X;x++) {

				StackBlock.PlaceIndex	place;

				place.x = x;
				place.y = y;

				places.push_back(place);
			}
		}
	#if true
		// 順番をシャッフルする.
		// ここをコメントアウトすると、スタート時のブロックの並びが
		// 左上から順番っぽくなってしまいます.
		for(int i = 0;i < places.size() - 1;i++) {

			int j = Random.Range(i + 1, places.size());

			places.swap(i, j);
		}
	#endif
		this.block_feeder.connect_arrow_num = 1;

		foreach(StackBlock.PlaceIndex place in places) {

			StackBlock	block = this.blocks[place.x, place.y];

			block.setColorType(this.block_feeder.getNextColorStart(place.x, place.y));
			block.setVisible(true);
		}
	}

	public void		update()
	{

		this.is_swap_end_frame = false;

		// ---------------------------------------------------------------- //
	#if false
		// "0" を押すと、上からブロックが降ってくる.
		if(Input.GetKeyDown(KeyCode.Keypad0)) {

			this.blockFallRequest();
		}


		// "1" を押すと、カラーチェンジ.
		if(Input.GetKeyDown(KeyCode.Keypad1)) {

			this.startColorChange();
		}

		// "2" を押すと、リスタート.
		if(Input.GetKeyDown(KeyCode.Keypad2)) {

			for(int y = 0;y < BLOCK_NUM_Y;y++) {
	
				for(int x = 0;x < BLOCK_NUM_X;x++) {

					StackBlock	block = this.blocks[x, y];

					block.setUnused();					
				}
			}

			this.setColorToAllBlock();
		}

		// "8" を押すと、メガクラッシュ.
		if(Input.GetKeyDown(KeyCode.Keypad8)) {

			for(int x = 0;x < BLOCK_NUM_X;x++) {
	
				for(int y = GROUND_LINE - this.fall_count;y < BLOCK_NUM_Y;y++) {

					StackBlock	block = this.blocks[x, y];

					block.beginVanishAction();
				}
			}
		}

		if(Input.GetKeyDown(KeyCode.Keypad9)) {

			//this.CheckConnect();
			//this.block_feeder.beginFeeding();
			
			for(int	x = 0;x < BLOCK_NUM_X;x++) {

				StackBlock	block = this.blocks[x, BLOCK_NUM_Y - 1];

				block.beginIdle(this.block_feeder.getNextColorAppearFromBottom(x));
			}
		}
	#endif

		// ---------------------------------------------------------------- //
		// 空のブロック（連鎖した後の灰色）があったら、スワップ動作を開始する.

		for(int x = 0;x < BLOCK_NUM_X;x++) {

			for(int y = GROUND_LINE;y < BLOCK_NUM_Y - 1;y++) {

				StackBlock	upper_block = this.blocks[x, y];
				StackBlock	under_block = this.blocks[x, y + 1];

				do {

					if(!(upper_block.isVacant() && !under_block.isVacant())) {

						break;
					}

					if(upper_block.isNowSwapAction() || under_block.isNowSwapAction()) {

						break;
					}

					//

					StackBlock.beginSwapAction(upper_block, under_block);

					this.scene_control.playSe(SceneControl.SE.SWAP);

				} while(false);

			}
		}

		// ---------------------------------------------------------------- //
		// 空のブロックが一番下の列に到達したら、新たな色をセットする.

		for(int x = 0;x < BLOCK_NUM_X;x++) {

			StackBlock	block = this.blocks[x, BLOCK_NUM_Y - 1];

			if(!block.isVacant()) {

				continue;
			}

			if(block.isNowSwapAction()) {

				continue;
			}

			block.beginIdle(this.block_feeder.getNextColorAppearFromBottom(x));
		}


		// ---------------------------------------------------------------- //
		// スワップ動作が終了した瞬間がどうかを調べる.

		// 『終了した瞬間』を拾いたいので、前のフレームの結果を保存しておく.

		bool	is_has_swap_block_prev = this.is_has_swap_block;

		this.is_has_swap_block = false;

		foreach(StackBlock block in this.blocks) {

			if(block.isVanishAfter()) {

				this.is_has_swap_block = true;
				break;
			}
		}

		if(is_has_swap_block_prev && !this.is_has_swap_block) {

			this.is_swap_end_frame = true;
		}

		// ---------------------------------------------------------------- //
		// 上からブロックが降ってくる.

		do {

			if(this.fall_request <= 0) {

				break;
			}

			if(this.fall_count >= FALL_LINE_NUM) {

				break;
			}

			int		y = GROUND_LINE - 1 - this.fall_count;

			Block.COLOR_TYPE[] colors = this.block_feeder.getNextColorsAppearFromTop(y);

			for(int x = 0;x < BLOCK_NUM_X;x++) {

				StackBlock	block = this.blocks[x, y];

				block.beginFallAction(colors[x]);
			}

			this.fall_count++;
			this.fall_request--;

		} while(false);

		// ---------------------------------------------------------------- //
		// 落下が終了したら、全体をスクロール.

		this.scroll_control();

		// ---------------------------------------------------------------- //
		// 連鎖チェック.

		if(this.is_swap_end_frame) {
	
			this.CheckConnect();
		}

		// ---------------------------------------------------------------- //
		// ケーキの上にブロックが落下したときは、ケーキが上がってくる.
		// （親切設計）.

		if(this.block_feeder.cake.is_enable) {

			for(int y = StackBlockControl.GROUND_LINE + 1;y < StackBlockControl.BLOCK_NUM_Y;y++) {

				int	x = this.block_feeder.cake.x;

				if(!this.blocks[x, y].isCakeBlock()) {

					continue;
				}

				// ドロップしたブロックが着地するまで、一番上のブロックは非表示になっている.
				// その非表示の間はスキップ.
				if(!this.blocks[x, y - 1].isVisible()) {

					continue;
				}

				// 連鎖後のブロックはほっといてもスワップされる.
				// （というより VanishAction の途中でカラーがかわってしまうからスキップしない
				// 　とまずい）.
				if(this.blocks[x, y - 1].isVanishAfter()) {

					continue;
				}

				//

				StackBlock.beginSwapAction(this.blocks[x, y - 1], this.blocks[x, y]);
			}
		}
	}

	// ブロックの落下が終了したときの、スクロール制御.
	private void	scroll_control()
	{
		do {

			if(this.fall_count <= 0) {

				break;
			}

			//

			bool	is_has_fall_block = false;

			for(int x = 0;x < StackBlockControl.BLOCK_NUM_X;x++) {

				StackBlock	block = this.blocks[x, StackBlockControl.GROUND_LINE - 1];

				if(block.isNowFallAction()) {

					is_has_fall_block = true;
					break;
				}
			}

			if(!is_has_fall_block) {

				if(this.is_scroll_enable) {

					// ケーキがスクロールアウトしたときの処理.
					if(this.block_feeder.cake.is_enable) {

						StackBlock	block = this.blocks[this.block_feeder.cake.x, StackBlockControl.BLOCK_NUM_Y - 1];
	
						if(block.isCakeBlock()) {
	
							this.block_feeder.onDropBlock(block);
						}
					}

					// 地中のブロックを一列づつ下にずらす.
					for(int y = StackBlockControl.BLOCK_NUM_Y - 1;y >= StackBlockControl.GROUND_LINE;y--) {
		
						for(int x = 0;x < StackBlockControl.BLOCK_NUM_X;x++) {
		
							this.blocks[x, y].relayFrom(this.blocks[x, y - 1]);
						}
					}
	
					// 落下中のラインが複数あったとき、落下中のラインを一つづつずらす.
					if(this.fall_count >= 2) {
		
						for(int y = StackBlockControl.GROUND_LINE - 1;y > StackBlockControl.GROUND_LINE - 1 - (this.fall_count - 1);y--) {
			
							for(int x = 0;x < StackBlockControl.BLOCK_NUM_X;x++) {
			
								this.blocks[x, y].relayFrom(this.blocks[x, y - 1]);
							}
						}
					}
					
					// 落下中のラインの、一番上のラインを非表示にする.
					for(int x = 0;x < StackBlockControl.BLOCK_NUM_X;x++) {
		
						this.blocks[x, StackBlockControl.GROUND_LINE - 1 - (this.fall_count - 1)].setUnused();
					}
				}

				this.fall_count--;

				this.scene_control.heightGain();

				this.scene_control.playSe(SceneControl.SE.JUMP);
				this.scene_control.playSe(SceneControl.SE.LANDING);
			}

		} while(false);
	}

	// 連鎖のチェック.
	public bool	CheckConnect()
	{
		bool	ret = false;

		if(this.is_connect_check_enable) {

			ret = this.check_connect_sub();
		}

		return(ret);
	}

	private bool	check_connect_sub()
	{

		bool	is_connect = false;

		int		connect_num = 0;

		this.connect_checker.clearAll();

		for(int y = GROUND_LINE;y < StackBlockControl.BLOCK_NUM_Y;y++) {

			for(int x = 0;x < StackBlockControl.BLOCK_NUM_X;x++) {

				if(!this.blocks[x, y].isConnectable()) {

					continue;
				}

				// 同じ色が並んでいるブロックの数をチェックする.

				int connect_block_num = this.connect_checker.checkConnect(x, y);

				// 同じ色の並びが４つ以下だったら消えない.

				if(connect_block_num < 4) {

					continue;
				}

				connect_num++;

				// つながっているブロックを消す.

				for(int i = 0;i < connect_block_num;i++) {

					StackBlock.PlaceIndex index = this.connect_checker.connect_block[i];

					this.blocks[index.x, index.y].beginVanishAction();
				}

				//

				this.eliminate_count += connect_block_num;
				is_connect = true;

				//

				this.continuous_connect_num++;
				this.score += this.continuous_connect_num*connect_block_num;
			}
		}

		//

		if(is_connect) {

			if(this.combo.is_now_combo) {

				this.combo.combo_count_current++;

				// 連鎖したら、上からブロックを降らす.
				this.fall_request++;

				this.scene_control.playSe(SceneControl.SE.COMBO);

			} else {

				this.combo.is_now_combo = true;
				this.combo.combo_count_current  = 1;
			}

			this.scene_control.playSe(SceneControl.SE.DROP_CONNECT);

			// ブロックを一定個数消すごとに、ケーキを出現させる.
			//
			do {

				// ケーキが出現中、あるいは出現まち中はカウントダウンしない.
				if(this.block_feeder.isCakeAppeared()) {

					break;
				}
				if(this.block_feeder.isCakeRequested()) {

					break;
				}

				this.eliminate_to_cake -= connect_num;

				if(this.eliminate_to_cake <= 0) {

					this.block_feeder.requestCake();
					this.eliminate_to_cake = ELIMINATE_TO_CAKE_INIT_2ND;

					// すぐにブロックが降ってくるようにする.
					if(this.fall_request == 0) {

						this.fall_request++;
					}
				}

			} while(false);

			this.eliminate_to_fall -= connect_num;

		} else {

			// 連鎖が終わった.

			// 連鎖しなくても、ブロックを一定数消したら上からブロックが降ってくる.
			if(this.combo.combo_count_current > 1) {

				// 連鎖した（＝ブロックが降った）はずなので、残り個数をリセット.

				this.eliminate_to_fall = ELIMINATE_TO_FALL_INIT;

			} else {

				// 連鎖しなかった.


				// ブロックを一定数消したら上からブロックを降らせる.

				if(this.eliminate_to_fall <= 0) {

					if(this.fall_request == 0) {

						this.fall_request++;
					}

					this.eliminate_to_fall = ELIMINATE_TO_FALL_INIT;
				}
			}

			this.combo.is_now_combo = false;
			this.combo.combo_count_last = this.combo.combo_count_current;
			this.combo.combo_count_current = 0;

			this.continuous_connect_num = 0;
		}

		// ゴールまでの残り以上には、降らせられない.
		if(SceneControl.MAX_HEIGHT_LEVEL - this.scene_control.height_level < this.fall_request) {

			this.fall_request = SceneControl.MAX_HEIGHT_LEVEL - this.scene_control.height_level;
		}

		return(is_connect);
	}

	// ブロックを持ち上げたときにやること
	public void	pickBlock(int x)
	{
		for(int y = GROUND_LINE;y < BLOCK_NUM_Y - 1;y++) {

			this.blocks[x, y].relayFrom(this.blocks[x, y + 1]);
		} 

		// 念のため一番上のブロックを表示ONにしておく.
		// （ぽい捨て直後で非表示になっている場合があるので）.
		this.blocks[x, GROUND_LINE].setVisible(true);

		// 一番下に新たにブロックを発生させる.
		this.blocks[x, BLOCK_NUM_Y - 1].setColorType(this.block_feeder.getNextColorAppearFromBottom(x));

		this.blocks[x, BLOCK_NUM_Y - 1].swap_action.init();
	}

	// ブロックをぽい捨てしたときにやること.
	public void	dropBlock(int x, Block.COLOR_TYPE color, int org_x)
	{
		// ブロックが（プレイヤーの操作で）画面下に押し出された時の処理.
		this.block_feeder.onDropBlock(this.blocks[x, BLOCK_NUM_Y - 1]);

		// いっこづつ下にずらす.
		for(int y = BLOCK_NUM_Y - 1;y > GROUND_LINE;y--) {

			this.blocks[x, y].relayFrom(this.blocks[x, y - 1]);
		} 

		// 一番上のブロックを、ぽい捨てされたブロックと同じ色にする.
		this.blocks[x, GROUND_LINE].beginIdle(color);

		// キャリーブロックが着地するまでは、非表示にしておく.
		this.blocks[x, GROUND_LINE].setVisible(false);
		this.blocks[x, GROUND_LINE].swap_action.is_active = false;
		this.blocks[x, GROUND_LINE].color_change_action.is_active = false;
		this.blocks[x, GROUND_LINE].position_offset.y = this.blocks[x, GROUND_LINE + 1].position_offset.y;

		// 上から落ちてくるブロックがぶつかったときのために、速度を適当に初期化しておく.
		this.blocks[x, GROUND_LINE].velocity.y = -StackBlock.OFFSET_REVERT_SPEED;

		if(color == Block.COLOR_TYPE.RED) {

			this.blocks[x, GROUND_LINE].step = StackBlock.STEP.VACANT;
		}
	}

	// キャリーブロックが、落とし終わった（着地した）ときの処理.
	// （キャリーブロック＝プレイヤーが持ち上げているブロック）.
	//
	public void	endDropBlockAction(int x)
	{
		if(this.is_has_swap_block) {

			// スワップ中のブロックがあるときは連鎖が起こらない.

		} else {

			// 連鎖のチェック.
			this.CheckConnect();
		}

		// 落とされた位置にあるスタックブロックの表示をONにする.
		// （キャリーブロックが非表示になるから）.
		//
		this.blocks[x, GROUND_LINE].setVisible(true);

		this.scene_control.playSe(SceneControl.SE.DROP);
	}


	// 場所のインデックスから、座標を求める.
	public static Vector3	calcIndexedPosition(StackBlock.PlaceIndex place)
	{
		Vector3		position;

		position.x = (-(BLOCK_NUM_X/2.0f - 0.5f) + place.x)*Block.SIZE_X;

		position.y = (-0.5f - (place.y - GROUND_LINE))*Block.SIZE_Y;

		position.z = 0.0f;

		return(position);
	}

	// ケーキを食べたときの処理.
	public void	onEatCake()
	{
		this.block_feeder.onEatCake();

		this.startColorChange();
	}

	// ブロックの色が変わる（特定の色だけになる）.
	// ケーキを食べたときの効果.
	public void	startColorChange()
	{
		int		color_index = 0;

		var		after_color = new Block.COLOR_TYPE[2];

		// ------------------------------------------------ //
		// なるべく旋回と違う、かつランダムに色をふたつ選ぶ.

		var		candidates = new StaticArray<int>(Block.NORMAL_COLOR_NUM);

		for(int i = 0;i < Block.NORMAL_COLOR_NUM;i++) {

			// 前回と同じ色は候補からはずす.
			if(i == this.change_color_index0 || i == this.change_color_index1) {

				continue;
			}
			if(!this.is_color_enable[i]) {

				continue;
			}

			//

			candidates.push_back(i);
		}

		// 0 ～ N - 1 の乱数を重複なくふたつ選ぶため、
		//
		// color0 = 0 ～ N - 2 の範囲の乱数.
		// color1 = color0 ～ N - 1 の範囲の乱数.
		//
		// を選ぶ.
		this.change_color_index0 = Random.Range(0, candidates.size() - 1);
		this.change_color_index1 = Random.Range(this.change_color_index0 + 1, candidates.size());

		this.change_color_index0 = candidates[this.change_color_index0];
		this.change_color_index1 = candidates[this.change_color_index1];

		// ------------------------------------------------ //
		// ブロックの色を変える.

		after_color[0] = (Block.COLOR_TYPE)change_color_index0;
		after_color[1] = (Block.COLOR_TYPE)change_color_index1;

		for(int x = 0;x < BLOCK_NUM_X;x++) {

			for(int y = GROUND_LINE - this.fall_count;y < BLOCK_NUM_Y;y++) {

				StackBlock	block = this.blocks[x, y];

				if(block.isVacant()) {

					continue;
				}

				// 最初からカラーチェンジ後のカラーだった場合はスキップ.
				if(System.Array.Exists(after_color, c => c == block.color_type)) {

					continue;
				}

				if(!block.isNormalColorBlock()) {

					continue;
				}

				// カラーチェンジを開始する.

				Block.COLOR_TYPE	target_color;

				target_color = after_color[color_index%after_color.Length];

				block.beginColorChangeAction(target_color);

				color_index++;
			}
		}
	}

	// 上からブロック降らせて（ゴール演出用）.
	public void		blockFallRequest()
	{
		this.fall_request++;

		//this.is_fall_request = true;
	}
	
	// N番目の有効なカラーを取得する.
	public	Block.COLOR_TYPE	getNthEnableColor(int n)
	{
		Block.COLOR_TYPE	color = Block.COLOR_TYPE.NONE;

		int		sum = 0;

		for(int i = 0;i < (int)Block.COLOR_TYPE.NUM;i++) {

			if(!this.is_color_enable[i]) {

				continue;
			}

			if(sum == n) {

				color = (Block.COLOR_TYPE)i;
				break;
			}

			sum++;
		}

		return(color);
	}

}
