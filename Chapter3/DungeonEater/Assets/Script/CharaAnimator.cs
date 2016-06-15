using UnityEngine;
using System.Collections;

public class CharaAnimator : MonoBehaviour {
	protected GridMove m_move;
	protected bool m_dead = false;
	protected  const uint PAUSE_NONE = 0;
	protected  const uint PAUSE_GAME = 1;
	protected  const uint PAUSE_ATTACK= 2;
	protected  const uint PAUSE_STAGECLESR = 4;
	
	protected uint  m_pauseFlag = PAUSE_NONE; // 停止中フラグ.

	
	// Use this for initialization
	void Start () {
		m_move = GetComponent<GridMove>();
		m_dead = false;
		m_pauseFlag = PAUSE_NONE;
	
		InitializeAnimations();
	}
	
	protected virtual void InitializeAnimations()
	{
		GetComponent<Animation>()["run"].speed = 2.0f;
		
		// 足音イベント.
		AnimationEvent ev = new AnimationEvent();
		ev.time = 0.0f;
		ev.functionName = "PlayStepSound";
		ev.floatParameter = 1.0f;
		GetComponent<Animation>()["run"].clip.AddEvent(ev);
		
		AnimationEvent ev2 = new AnimationEvent();
		ev2.time = GetComponent<Animation>()["run"].clip.length / 2.0f;
		ev2.functionName = "PlayStepSound";
		ev2.floatParameter = 1.06f;
		GetComponent<Animation>()["run"].clip.AddEvent(ev2);		
	}
	
	public void OnRestart()
	{
		m_dead = false;
		m_pauseFlag = PAUSE_NONE;
		GetComponent<Animation>().Play("idle");
	}
	public void OnGameStart()
	{
		m_dead = false;
		m_pauseFlag = PAUSE_NONE;
		GetComponent<Animation>().Play("idle");
	}
	public void OnStageStart()
	{
		m_dead = false;
		m_pauseFlag = PAUSE_NONE;
		GetComponent<Animation>().Stop();
		GetComponent<Animation>().Play("idle");
	}
	
	
	// Update is called once per frame
	public virtual void Update () {
		if ((m_pauseFlag & PAUSE_GAME) != 0)
			return;
		// ローテーション.
		Quaternion targetRotation = Quaternion.LookRotation(m_move.GetDirection());
		if ((m_pauseFlag & PAUSE_ATTACK) == 0) {
			float t = 1.0f - Mathf.Pow(0.75f,Time.deltaTime*30.0f);
			transform.localRotation = MathUtil.Slerp(transform.localRotation,targetRotation,t);
		}
		if (m_dead) {
			GetComponent<Animation>().CrossFade("deadAnim",0.25f);	
		} else {
			if (m_move.IsRunning())
				GetComponent<Animation>().CrossFade("run",0.25f);	
			else
				GetComponent<Animation>().CrossFade("idle",0.25f);
		}
	}
	
	public void OnDead()
	{
		m_dead = true;
	}
	

	
	public void OnRebone()
	{
		m_dead = false;
	}
	
	public void HitStop(bool enable)
	{
		if (enable)
			m_pauseFlag |= PAUSE_ATTACK;
		else
			m_pauseFlag &= ~PAUSE_ATTACK;
	}
	
	
}
