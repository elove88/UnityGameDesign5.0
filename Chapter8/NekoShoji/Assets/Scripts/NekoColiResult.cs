using UnityEngine;
using System.Collections;

public class NekoColiResult  {

	public NekoControl	neko = null;

	public static float	THROUGH_GAP_LIMIT = 0.4f;			//!< 格子の中心からこれ以上ずれていたら、組み木に当たる.
	public static float UNLOCK_DISTANCE = 3.0f;				//!< 誘導を解除する距離.

	// ---------------------------------------------------------------- //

	// 障子の組み木に当たったときの情報いろいろ.
	//
	public struct ShojiHitInfo {

		public bool			is_enable;

		public ShojiControl	shoji_control;

		public ShojiControl.HoleIndex	hole_index;
	};

	public ShojiHitInfo		shoji_hit_info;
	public ShojiHitInfo		shoji_hit_info_first;

	// 障子の穴に当たったときの情報いろいろ.
	//
	public struct HoleHitInfo {

		public SyoujiPaperControl	paper_control;
	};

	public StaticArray<HoleHitInfo>	hole_hit_infos;

	// ふすま、障子の組み木にあたったときの情報いろいろ.
	//
	public struct ObstacleHitInfo {

		public bool			is_enable;

		public GameObject	go;
		public bool			is_steel;
	};

	public ObstacleHitInfo	obstacle_hit_info;

	// 誘導をかける、障子の格子
	//
	public struct LockTarget {

		public bool						enable;

		public ShojiControl.HoleIndex	hole_index;

		public Vector3					position;
	};

	public LockTarget	lock_target;

	public bool			is_steel = false;

	// ---------------------------------------------------------------- //

	public void create()
	{
		this.hole_hit_infos = new StaticArray<HoleHitInfo>(4);

		this.shoji_hit_info.is_enable       = false;
		this.shoji_hit_info_first.is_enable = false;

		this.obstacle_hit_info.is_enable = false;

		this.lock_target.enable = false;
	}

	// 前のフレームのコリジョン結果をチェックする.

	public void	resolveCollision()
	{
		// 「格子の穴」をすり抜けて一定距離進んだら、誘導を解除する.
		if(this.lock_target.enable) {

			if(this.neko.transform.position.z > this.lock_target.position.z + UNLOCK_DISTANCE) {

				this.lock_target.enable = false;
			}
		}

		if(!this.lock_target.enable) {

			this.resolve_collision_sub();
		}
	}

	private void	resolve_collision_sub()
	{
		bool	is_collied_obstacle = false;

		this.is_steel = false;

		// ふすま/鉄板に当たったかを最初に調べる.
		//
		// ふすま/鉄板に当たった場合でも穴を通過したときは、ミスにしたくないので.

		if(this.obstacle_hit_info.is_enable) {

			is_collied_obstacle = true;

			this.is_steel = this.obstacle_hit_info.is_steel;
		}

		//

		if(this.shoji_hit_info.is_enable) {

			// 組み木と当たった？.

			ShojiControl			shoji_control = this.shoji_hit_info.shoji_control;
			ShojiControl.HoleIndex	hole_index    = this.shoji_hit_info.hole_index;

			if(shoji_control.isValidHoleIndex(hole_index)) {

				SyoujiPaperControl		paper_control = shoji_control.papers[hole_index.x, hole_index.y];

				if(paper_control.isSteel()) {

					// 格子の穴が、鉄板だったとき.

					is_collied_obstacle = true;

					this.is_steel = true;

				} else  {

					// 格子の穴が、「紙」「やぶれ紙」だったとき.

					// 「格子の穴」にホーミングするときの、目標位置.
					Vector3	position = NekoColiResult.get_hole_homing_position(shoji_control, hole_index);

					//
	
					Vector3	diff = this.neko.transform.position - position;
	
					//Debug.Log(diff.x.ToString() + " " + diff.y);
	
					if(Mathf.Abs(diff.x) < THROUGH_GAP_LIMIT && Mathf.Abs(diff.y) < THROUGH_GAP_LIMIT) {
	
						// 穴の中心からある程度近い位置を通過したら、格子の穴を通りぬける.
						// （ホーミング）.

						is_collied_obstacle = false;

						this.lock_target.enable     = true;
						this.lock_target.hole_index = hole_index;
						this.lock_target.position   = position;


						// 「格子の穴」モデルに、プレイヤーがヒットしたことを通知する.
						paper_control.onPlayerCollided();

					} else {

						// 穴の中心から大きくずれていた場合は、格子にぶつかったことにする.

						is_collied_obstacle = true;
					}
				}

			} else {

				// 障子の、格子の穴以外の場所にぶつかったとき.

				is_collied_obstacle = true;
			}

		} else {

			// 組み木と当たらなかったときは、二つ以上の「格子の穴」とヒットすることはないはず.
			// （二つの「格子の穴」とヒットするときは、その間の組み木にもヒットするから）
			// なので、組み木と当たらなかったときは this.hole_hit_infos[0] だけ調べれば十分.
			if(this.hole_hit_infos.size() > 0) {

				// 「格子の穴」のみにヒットした.

				HoleHitInfo			hole_hit_info = this.hole_hit_infos[0];
				SyoujiPaperControl	paper_control = hole_hit_info.paper_control;
				ShojiControl		shoji_control = paper_control.shoji_control;

				paper_control.onPlayerCollided();

				// 「格子の穴」を通りぬける.
				// （ホーミング）.

				// ロックする（誘導の目標位置にする）.

				// 「格子の穴」の中心を求める.

				ShojiControl.HoleIndex	hole_index = paper_control.hole_index;

				Vector3	position = NekoColiResult.get_hole_homing_position(shoji_control, hole_index);

				this.lock_target.enable = true;
				this.lock_target.hole_index = hole_index;
				this.lock_target.position   = position;
			}
		}


		if(is_collied_obstacle) {

			// 障害物（障子の組み木、ふすま）に当たった.

			if(this.neko.step != NekoControl.STEP.MISS) {

				this.neko.beginMissAction(this.is_steel);
			}
		}

	}


	// 「格子の穴」にホーミングするときの、目標位置.
	private static Vector3	get_hole_homing_position(ShojiControl shoji_control, ShojiControl.HoleIndex hole_index)
	{
		Vector3		position;
	
		position = shoji_control.getHoleWorldPosition(hole_index.x, hole_index.y);
	
		// コリジョンの中心からオブジェクトの原点へのオフセット.
		// コリジョンの中心が、「格子の穴」の中心を通るようにする.
		position += -NekoControl.COLLISION_OFFSET;

		return(position);
	}

}
