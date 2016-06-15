using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// BOSS Core 破壊された時のメッセージ表示.
// ----------------------------------------------------------------------------
public class CoreController : MonoBehaviour {
	
	private GameObject subScreenMessage;	// SubScreenのメッセージ領域.
	
	void Start () {
		
		// SubScreenMessageのインスタンスを取得.
		subScreenMessage = GameObject.FindGameObjectWithTag("SubScreenMessage");
		
	}
	
	// ------------------------------------------------------------------------
	// BOSS Core が破壊された時の処理.
	// ------------------------------------------------------------------------
	void OnDestroy()
	{
		if ( this.GetComponent<EnemyStatus>() )
		{
			if (
				this.GetComponent<EnemyStatus>().GetIsBreakByPlayer() ||
				this.GetComponent<EnemyStatus>().GetIsBreakByStone() )
			{
				subScreenMessage.SendMessage("SetMessage", " ");
				subScreenMessage.SendMessage("SetMessage", "DEFEATED SPIDER-TYPE." );
				subScreenMessage.SendMessage("SetMessage", "MISSION ACCOMPLISHED." );
				subScreenMessage.SendMessage("SetMessage", " ");
			}
		}
	}
	
}
