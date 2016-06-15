using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// SubScreen右上へのロックスロット表示制御.
// ----------------------------------------------------------------------------
public class LockSlot : MonoBehaviour {

	public GameObject lockSlot1;	// ロックスロット1使用中 表示の為のオブジェクト.
	public GameObject lockSlot2;	// ロックスロット2使用中 表示の為のオブジェクト.
	public GameObject lockSlot3;	// ロックスロット3使用中 表示の為のオブジェクト.
	public GameObject lockSlot4;	// ロックスロット4使用中 表示の為のオブジェクト.
	public GameObject lockSlot5;	// ロックスロット5使用中 表示の為のオブジェクト.
	public GameObject lockSlot6;	// ロックスロット6使用中 表示の為のオブジェクト.

	private GUITexture[] lockSlotGUITexture;	// LockSlot(ロックスロット表示の為のオブジェクト).
	
	private bool isEnable = false;	// 表示有効.
	
	void Start ()
	{
		// ロックスロットのメモリ領域確保.
		lockSlotGUITexture = new GUITexture[6];
		
		// ロックスロットの表示の為のオブジェクトが全てそろっているか?
		if (
			lockSlot1 && lockSlot2 && lockSlot3 &&
			lockSlot4 && lockSlot5 && lockSlot6 )
		{
			isEnable = true;
		
			// LockSlotのインスタンスを取得.
			lockSlotGUITexture[0] = lockSlot1.GetComponent<GUITexture>();
			lockSlotGUITexture[1] = lockSlot2.GetComponent<GUITexture>();
			lockSlotGUITexture[2] = lockSlot3.GetComponent<GUITexture>();
			lockSlotGUITexture[3] = lockSlot4.GetComponent<GUITexture>();
			lockSlotGUITexture[4] = lockSlot5.GetComponent<GUITexture>();
			lockSlotGUITexture[5] = lockSlot6.GetComponent<GUITexture>();
			
			// 初期値表示.
			for( int i = 0; i < lockSlotGUITexture.Length; i++ )
			{
				lockSlotGUITexture[i].enabled = false;
			}
		}
	}
	
	// ------------------------------------------------------------------------
	// ロックオンの数だけロックスロットを使用中として表示.
	// ------------------------------------------------------------------------
	public void SetLockCount( int lockCount )
	{
		if ( isEnable )
		{
			for( int i = 0; i < lockSlotGUITexture.Length; i++ )
			{
				if ( i < lockCount )
				{
					lockSlotGUITexture[i].enabled = true;
				}
				else
				{
					lockSlotGUITexture[i].enabled = false;
				}
			}			
		}
	}
}
