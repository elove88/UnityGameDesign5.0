using UnityEngine;
using System.Collections;

public class AttackColliderControl : MonoBehaviour {

	public PlayerControl	player = null;

	// 攻撃判定発生中？.
	private bool		is_powered = false;

	// -------------------------------------------------------------------------------- //

	void Start ()
	{
		this.SetPowered(false);
	}
	
	// Update is called once per frame
	void Update ()
	{
	}

	// OnTriggerEnter はコリジョン同士が接した瞬間しか呼ばれないので、
	// 攻撃判定の球が発生したときにオニが球の内側に完全に入っていると、
	// うまくひろえない.
	//void OnTriggerEnter(Collider other)
	void OnTriggerStay(Collider other)
	{
		do {

			if(!this.is_powered) {

				break;
			}

			if(other.tag != "OniGroup") {
	
				break;
			}

			OniGroupControl	oni = other.GetComponent<OniGroupControl>();

			if(oni == null) {

				break;
			}

			//

			oni.OnAttackedFromPlayer();

			// 『攻撃できない中』タイマーをリセットする（すぐに攻撃可にする）.
			this.player.resetAttackDisableTimer();

			// 攻撃ヒットエフェクトを再生する.
			this.player.playHitEffect(oni.transform.position);

			// 攻撃ヒット音を鳴らす.
			this.player.playHitSound();

		} while(false);
	}

	public void	SetPowered(bool sw)
	{
		this.is_powered = sw;

		if(SceneControl.IS_DRAW_PLAYER_ATTACK_COLLISION) {

			this.GetComponent<Renderer>().enabled = sw;
		}
	}
}
