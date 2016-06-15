using UnityEngine;
using System.Collections;

public class CharaAnimatorMonster : CharaAnimator {
	protected override void InitializeAnimations()
	{
		
	}
	
	// Update is called once per frame
	public override void Update () 
	{
		if ((m_pauseFlag & PAUSE_GAME) != 0)
			return;
		// ローテーション.
		Quaternion targetRotation = Quaternion.LookRotation(m_move.GetDirection());
		if ((m_pauseFlag & PAUSE_ATTACK) == 0) {
			float t = 1.0f - Mathf.Pow(0.75f,Time.deltaTime*30.0f);
			transform.localRotation = MathUtil.Slerp(transform.localRotation,targetRotation,t);
		}
		if (m_dead)
			GetComponent<Animation>().CrossFade("deadAnim",0.25f);	
		else
			GetComponent<Animation>().CrossFade("idle",0.25f);
	}
}
