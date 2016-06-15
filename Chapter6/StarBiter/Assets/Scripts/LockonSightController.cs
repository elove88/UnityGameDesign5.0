using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// ロックオンサイトの制御.
// ----------------------------------------------------------------------------
public class LockonSightController : MonoBehaviour {
	
	public GameObject lockonEnemy;				// ロックオンした敵機.
	public bool isEnabled = false;				// ロックオンサイト有効.
	
	void Update ()
	{
		// ロックオンした敵機が存在する時はロックオンサイトを敵機に追従する.
		if ( lockonEnemy )
		{
			// 敵機に追従.
			transform.position = new Vector3(
				lockonEnemy.transform.position.x,
				lockonEnemy.transform.position.y + 1f,
				lockonEnemy.transform.position.z );
		}
		
		// ロックオンした敵機が存在しない場合は破壊したと見なしロックオンサイトを消去する.
		if ( !lockonEnemy )
		{
			if ( isEnabled )
			{
				Destroy( this.gameObject );
			}
		}
	}
	
	// ------------------------------------------------------------------------
	// ロックオン対象の敵機を保持する.
	// ------------------------------------------------------------------------
	private void SetLockonEnemy( GameObject lockonEnemy )
	{
		this.lockonEnemy = lockonEnemy;
		isEnabled = true;
	}
	
	// ------------------------------------------------------------------------
	// ロックオンサイトを消去.
	// ------------------------------------------------------------------------
	public void Destroy()
	{
		Destroy( this.gameObject );
	}
}
