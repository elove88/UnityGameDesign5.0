using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// SubScreen右上へのロックボーナス表示制御.
// ----------------------------------------------------------------------------
public class LockBonus : MonoBehaviour {
	
	public Texture2D lockBonus0;			// ロックボーナス x 0  画像.
	public Texture2D lockBonus2;			// ロックボーナス x 2  画像.
	public Texture2D lockBonus4;			// ロックボーナス x 4  画像.
	public Texture2D lockBonus8;			// ロックボーナス x 8  画像.
	public Texture2D lockBonus16;			// ロックボーナス x 16 画像.
	public Texture2D lockBonus32;			// ロックボーナス x 32 画像.
	public Texture2D lockBonus64;			// ロックボーナス x 64 画像.
	
	private GUITexture lockBonusGUITexture;	// LockBonus(ロックボーナス表示の為のオブジェクト).
	
	private bool isEnable = false;			// 表示有効.
	
	void Start ()
	{
		// LockBonusのインスタンスを取得.
		lockBonusGUITexture =
			GameObject.FindGameObjectWithTag("LockBonus")
				.GetComponent<GUITexture>();
		
		// ロックボーナスの画像が全てそろっているか?
		if (
			lockBonus0 && lockBonus2 && lockBonus4 &&
			lockBonus8 && lockBonus16 && lockBonus32 &&
			lockBonus64 )
		{
			isEnable = true;
			
			// 初期値表示.
			lockBonusGUITexture.texture = lockBonus0;
		}
		
	}
	
	// ------------------------------------------------------------------------
	// 指定したロックボーナスの画像表示.
	// ------------------------------------------------------------------------
	public void SetLockCount( int lockCount )
	{
		if ( isEnable )
		{
			switch( lockCount )
			{
			case 0:
				lockBonusGUITexture.texture = lockBonus0;
				break;
			case 1:
				lockBonusGUITexture.texture = lockBonus2;
				break;
			case 2:
				lockBonusGUITexture.texture = lockBonus4;
				break;
			case 3:
				lockBonusGUITexture.texture = lockBonus8;
				break;
			case 4:
				lockBonusGUITexture.texture = lockBonus16;
				break;
			case 5:
				lockBonusGUITexture.texture = lockBonus32;
				break;
			case 6:
				lockBonusGUITexture.texture = lockBonus64;
				break;
			default:
				break;
			}
		}
	}
}
