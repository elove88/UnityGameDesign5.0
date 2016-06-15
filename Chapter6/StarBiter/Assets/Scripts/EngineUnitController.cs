using UnityEngine;
using System.Collections;

public class EngineUnitController : MonoBehaviour {
	
	private GameObject subScreenMessage;				// SubScreenのメッセージ領域.
	
	void Start () 
	{
		// SubScreenMessageのインスタンスを取得.
		subScreenMessage = GameObject.FindGameObjectWithTag("SubScreenMessage");
	}
	
	void OnDestroy()
	{
		if ( this.GetComponent<EnemyStatus>() )
		{
			if (
				this.GetComponent<EnemyStatus>().GetIsBreakByPlayer() ||
				this.GetComponent<EnemyStatus>().GetIsBreakByStone() )
			{
				subScreenMessage.SendMessage("SetMessage", " ");
				subScreenMessage.SendMessage("SetMessage", "DESTROYED DEFENSIVE BULKHEAD." );
				subScreenMessage.SendMessage("SetMessage", " ");
			}
		}
	}
}
