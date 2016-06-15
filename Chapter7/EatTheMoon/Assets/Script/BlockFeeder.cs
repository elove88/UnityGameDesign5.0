using UnityEngine;
using System.Collections;

// 次に出てくるブロックの色を決める.
public class BlockFeeder {

	public StackBlockControl	control = null;

	private StaticArray<int>				connect_num = null;
	private StaticArray<Block.COLOR_TYPE>	candidates  = null;		// 出現する色の候補.

	// 出待ちなケーキ.
	public struct Cake {

		public bool				is_enable;
		public int				x;
		public Block.COLOR_TYPE	color_type;		// カラータイプ（ケーキは今のところ２種類ある）.
	};

	public Cake			cake;				// 出待ち中のケーキ.
	private int			cake_count = 0;		// 出現したケーキの数.
	private int			cake_request = 0;

	// ---------------------------------------------------------------- //

	public void	create()
	{
		this.connect_num = new StaticArray<int>(Block.NORMAL_COLOR_NUM);
		this.connect_num.resize(this.connect_num.capacity());

		this.candidates = new StaticArray<Block.COLOR_TYPE>(Block.NORMAL_COLOR_NUM);

		this.cake.is_enable  = false;
	}

	// あがり目（同じ色が４つ並んでいる）をつくっていい数.
	public int	connect_arrow_num = 1;

	// 次のブロックの色を取得する（ゲームスタート時に、全部を埋めるとき用）.
	public Block.COLOR_TYPE	getNextColorStart(int x, int y)
	{
#if false
		Block.COLOR_TYPE	color_type;

		color_type = (Block.COLOR_TYPE)Random.Range((int)Block.NORMAL_COLOR_FIRST, (int)Block.NORMAL_COLOR_LAST + 1);

		return(color_type);
#else
		StackBlock[,]		blocks          = this.control.blocks;
		ConnectChecker		connect_checker = this.control.connect_checker;
		Block.COLOR_TYPE	org_color;
		int					sel;

		//

		org_color = blocks[x, y].color_type;

		// 『出現する色の候補のリスト』を初期化する.
		// （リストにすべての色が含まれるようにする）.
		this.init_candidates();

		// 各色を置いたとき、同じ色が何個並ぶかを調べておく.

		for(int i = 0;i < (int)Block.NORMAL_COLOR_NUM;i++) {

			blocks[x, y].setColorType((Block.COLOR_TYPE)i);

			connect_checker.clearAll();

			this.connect_num[i] = connect_checker.checkConnect(x, y);
		}

		if(this.connect_arrow_num > 0) {

			// まだあがり目ができてもいい場合.

			// connect_num[] の中の最大値（最大で max_num 個同じ色のブロックが並ぶ）.
			int		max_num = this.get_max_connect_num();

			// max_num じゃないものを削除する（最大値をとるものだけを候補に残す）.
			this.erase_candidate_if_not(max_num);

			sel = Random.Range(0, candidates.size());

			// 同じ色が４つ並んだら、あがり目の残り数を減らしておく
			if(this.connect_num[(int)candidates[sel]] >= 4) {

				this.connect_arrow_num--;
			}

		} else {

			// もうあがり目を作れない場合.

			// 同じ色が４つならんでしまう色を候補から外す.
			for(int i = candidates.size() - 1;i >= 0;i--) {
	
				if(this.connect_num[(int)candidates[i]] >= 4) {

					candidates.erase_by_index(i);
				}
			}

			if(candidates.size() == 0) {

				this.init_candidates();
				Debug.Log("give up");
			}

			// connect_num[] の中の最大値（最大で max_num 個同じ色のブロックが並ぶ）.
			int		max_num = this.get_max_connect_num();

			// max_num じゃないものを削除する（最大値をとるものだけを候補に残す）.
			this.erase_candidate_if_not(max_num);

			sel = Random.Range(0, candidates.size());
		}


		//

		blocks[x, y].setColorType(org_color);

		return((Block.COLOR_TYPE)candidates[sel]);
#endif
	}

	// 候補のリストを初期化する（すべての色が候補に含まれるようにする）.
	private void	init_candidates()
	{
		this.candidates.resize(0);

		for(int i = 0;i < this.candidates.capacity();i++) {

			if(!this.control.is_color_enable[i]) {

				continue;
			}

			this.candidates.push_back((Block.COLOR_TYPE)i);
		}
	}

	// 同じ色のブロックが並ぶ数が一番多い色の、その並ぶ数を取得する.
	private int		get_max_connect_num()
	{
		int		sel = 0;

		for(int i = 1;i < candidates.size();i++) {

			if(this.connect_num[(int)this.candidates[i]] > this.connect_num[(int)this.candidates[sel]]) {

				sel = i;
			}
		}

		return(this.connect_num[(int)this.candidates[sel]]);
	}

	// 同じ色が並んでいる数が connect_num じゃない色を削除する.
	private void	erase_candidate_if_not(int connect_num)
	{
		for(int i = candidates.size() - 1;i >= 0;i--) {
	
			if(this.connect_num[(int)candidates[i]] != connect_num) {
	
				candidates.erase_by_index(i);
			}
		}
	}

	// 指定の色を候補からはずす.
	private void	erase_color_from_candidates(Block.COLOR_TYPE color)
	{
		for(int i = candidates.size() - 1;i >= 0;i--) {

			if(candidates[i] == color) {
				
				candidates.erase_by_index(i);
			}
		}
	}

	// 次のブロックの色を取得する（画面上から降ってくるブロック）.
	public Block.COLOR_TYPE[] getNextColorsAppearFromTop(int y)
	{
		Block.COLOR_TYPE[]	colors = new Block.COLOR_TYPE[StackBlockControl.BLOCK_NUM_X];

		for(int i = 0;i < StackBlockControl.BLOCK_NUM_X;i++) {

			colors[i] = this.get_next_color_appear_from_top_sub(i, y, colors);
		}

		// 出待ちなケーキがあるときは、ケーキを出す.
		if(this.cake_request > 0) {

			this.cake.is_enable  = true;
			this.cake.x          = Random.Range(0, StackBlockControl.BLOCK_NUM_X);
			this.cake.color_type = (Block.COLOR_TYPE)((int)Block.CAKE_COLOR_FIRST + this.cake_count%2);

			colors[this.cake.x] = this.cake.color_type;

			this.cake_count++;

			this.cake_request = Mathf.Max(this.cake_request - 1, 0);
		}

		return(colors);
	}
	private Block.COLOR_TYPE	get_next_color_appear_from_top_sub(int x, int y, Block.COLOR_TYPE[] colors)
	{
		StackBlock[,]		blocks     = this.control.blocks;
		Block.COLOR_TYPE	color_type = Block.NORMAL_COLOR_FIRST;
		int					sel;

		this.init_candidates();

		// とりあえず、左と下のブロックと同じ色にならないように.

		this.erase_color_from_candidates(blocks[x, y + 1].color_type);

		if(x > 0) {

			this.erase_color_from_candidates(colors[x - 1]);
		}

		//

		sel = Random.Range(0, candidates.size());

		color_type = this.candidates[sel];

		return(color_type);
	}

	// 次のブロックの色を取得する（画面下から新たに出現するブロック）.
	public Block.COLOR_TYPE	getNextColorAppearFromBottom(int x)
	{
		StackBlock[,]		blocks     = this.control.blocks;
		Block.COLOR_TYPE	color_type = Block.NORMAL_COLOR_FIRST;

		this.init_candidates();

		//

		int		y;

		for(y = (int)StackBlockControl.BLOCK_NUM_Y - 1 - 1;y >= 0;y--) {

			StackBlock	block = blocks[x, y];

			if(block.isConnectable()) {

				break;
			}
		}

		if(y >= 0) {

			Block.COLOR_TYPE	erase_color = blocks[x, y].color_type;

			for(int i = 0;i < candidates.size();i++) {

				if(candidates[i] == erase_color) {

					candidates.erase_by_index(i);
					break;
				}
			}
		}

		color_type = candidates[Random.Range(0, candidates.size())];

#if false
		int[]	block_num = new int[(int)Block.COLOR_TYPE.NORMAL_COLOR_NUM];

		for(int i = 0;i < (int)Block.COLOR_TYPE.NORMAL_COLOR_NUM;i++) {

			block_num[i] = 0;
		}

		//

		for(int i = -1;i <= 1;i++) {

			if(x + i < 0 || StackBlockControl.BLOCK_NUM_X <= x + i) {

				continue;
			}

			for(int y = 0;y < (int)StackBlockControl.BLOCK_NUM_Y;y++) {
	
				StackBlock	block = blocks[x, y];
	
				if(!block.isConnectable()) {
	
					continue;
				}
	
				block_num[(int)block.color_type]++;
			}
		}

		//

		int		sel = 0;

		for(int i = 1;i < (int)Block.COLOR_TYPE.NORMAL_COLOR_NUM;i++) {

			if(block_num[i] < block_num[sel]) {

				sel = i;
			}
		}

		//

		StaticArray<Block.COLOR_TYPE> candidates = new StaticArray<Block.COLOR_TYPE>();

		candidates.create((int)Block.COLOR_TYPE.NORMAL_COLOR_NUM);

		for(int i = 0;i < (int)Block.COLOR_TYPE.NORMAL_COLOR_NUM;i++) {

			if(block_num[i] <= block_num[sel]) {

				candidates.push_back((Block.COLOR_TYPE)i);
			}
		}

		color_type = candidates[Random.Range(0, candidates.size())];
#endif
		return(color_type);
	}

	// ---------------------------------------------------------------- //

	//ブロックが（プレイヤーの操作で）画面下に押し出された時の処理.
	public void	onDropBlock(StackBlock block)
	{
#if true
		do {

			if(!block.isCakeBlock()) {

				break;
			}

			if(!this.cake.is_enable) {

				break;
			}

			//

			this.clearCake();

		} while(false);

#else
		// ケーキが（プレイヤーの操作によって）画面下に押し出されたときは
		// ケーキの出現数を増やしておく.
		// （そうしないといつまでたってもケーキが画面に出てこなくなる時があるので）.
		if(block.isCakeBlock()) {

			if(this.cake.is_enable) {

				this.cake.is_enable  = true;
				this.cake.is_active  = false;
				this.cake.x          = block.place.x;
				this.cake.color_type = block.color_type;
			}
		}

		if(this.cake.is_enable && !this.cake.is_active) {

			this.cake.out_count++;
		}
#endif
	}

	// ケーキ出して.
	public void	requestCake()
	{
		if(!this.cake.is_enable) {

			this.cake_request++;
		}
	}

	// ケーキを食べた.
	public void	onEatCake()
	{
		this.clearCake();
	}

	// ケーキ出現中？.
	public bool	isCakeAppeared()
	{
		return(this.cake.is_enable);
	}

	// ケーキ出待ち？.
	public bool	isCakeRequested()
	{
		return(this.cake_request > 0);
	}

	// ケーキの出現情報をクリアーする.
	public void	clearCake()
	{
		this.cake.is_enable  = false;
		this.cake.x          = -1;
		this.cake.color_type = Block.COLOR_TYPE.NONE;
	}
}
