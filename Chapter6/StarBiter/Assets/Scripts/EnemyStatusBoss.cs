using UnityEngine;
using System.Collections;

public class EnemyStatusBoss : EnemyStatus {
	
	public bool isBoss = false;							// 自身はBOSS(=true).
	public int bossGuardLimit = 0;						// この数値以上のLifeがある場合は攻撃をガードする.
	private bool canGuard = false;						// 攻撃をガードできる.	
	
	private GameObject boss;							// Bossのインスタンス.
	
	public override void StartSub()
	{
		// BOSS用.
		if ( isBoss )
		{
			// BOSSに追従する.
			SetIsFollowingLeader( true );
			
			// BOSSのインスタンスを取得.
			boss = GameObject.FindGameObjectWithTag("Boss");
			
			// カード可能か確認.
			if ( CanGuard() )
			{
				canGuard = true;
			}
		}
	}
	
	public override void UpdateSub()
	{
		// BOSS用.
		if ( isBoss && canGuard )
		{
			if ( !CanGuard() )
			{
				canGuard = false;
				
				// コリジョンを有効にする.
				EnableCollider();
			}
		}
	}
	
	public override void DestroyEnemySub()
	{
		// ボスの場合は.
		if ( isBoss )
		{
			if ( transform.parent )
			{
				transform.parent.SendMessage( "SetLockonBonus", lockonBonus );
				transform.parent.SendMessage( "SetIsBreak", true );
			}
		}
	}
	
	private bool CanGuard()
	{
		if ( !isBoss )
		{
			return false;
		}
		
		int bossLife = boss.GetComponent<EnemyStatus>().GetLife();

		if ( bossLife >= bossGuardLimit )
		{
			return true;
		}
		return false;
	}
	
	protected override void DestroyEnemyEx()
	{
		// 何も処理しない.
	}
	
	private void EnableCollider()
	{
		// コリジョンを有効にする(子オブジェクトのColliderをすべて有効にする).
		Transform[] children = this.GetComponentsInChildren<Transform>();
  		foreach ( Transform child in children )
		{
			if ( child.tag == "Enemy" )
			{
				if ( child.GetComponent<SphereCollider>() )
				{
					// colliderを有効にする.
					child.GetComponent<SphereCollider>().enabled = true;
				}
			}
		}
	}
}
