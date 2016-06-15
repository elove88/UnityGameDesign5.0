using UnityEngine;
using System.Collections;

public class GameCtrl : MonoBehaviour {
	public Hud m_hud;
	public Map m_map;
	public int RETRY = 2;
	public GameObject m_enemyPrefab;
	public GameObject m_treasureGen;	
	
	// BGM関係.
	public AudioChannels m_audio;
	public AudioClip m_bgmClip;
	
	public AudioClip m_seStageClear;
	public AudioClip m_seGameOver;
	
	public FollowCamera m_camera;
	private int m_retry_remain ;
	private ArrayList m_objList = new ArrayList();
	private int m_stageNo = 0;
	
	// Use this for initialization
	void Start () {
		GameStart();
	}
	
	void Init()
	{
		m_retry_remain = RETRY;
		GameObject.Find("Map").SendMessage("OnGameStart");
		GameObject.Find("Player").SendMessage("OnGameStart");
	}
	
	
	// Update is called once per frame
	void Update () {
		if (GlobalParam.GetInstance().IsAdvertiseMode() && Input.GetMouseButtonDown(0))
			Application.LoadLevel("TitleScene");
	
	}
	
	public void Retry()
	{
		if (m_retry_remain > 0) {
			m_retry_remain--;
			Restart();

		} else {
			StartCoroutine("GameOverSeq");
		}
	}
	
	IEnumerator GameOverSeq()
	{
		m_audio.StopAll();
		m_audio.PlayOneShot(m_seGameOver,1.0f,0);
		m_hud.DrawGameOver(true);
		yield return new WaitForSeconds(5.0f);
		Application.LoadLevel("TitleScene");
	}
	
	public int GetRetryRemain()
	{
		return m_retry_remain;	
	}
	
	public void PlayerIsDead()
	{
		if (GlobalParam.GetInstance().IsAdvertiseMode())
			Application.LoadLevel("TitleScene");
		else
			Retry();
	}
	
	public void AddObjectToList(GameObject o)
	{
		m_objList.Add(o);
	}
	
	public void RemoveObjectFromList(GameObject o)
	{
		m_objList.Remove(o);
	}
	
	
	public void Restart()
	{
		GameObject[] enemys = GameObject.FindGameObjectsWithTag("Enemy");
		for (int i = 0; i < enemys.Length; i++)
			enemys[i].SendMessage("OnRestart");
		GameObject.Find("Player").SendMessage("OnRestart");
		TreasureGenerator treasureGen = FindObjectOfType(typeof(TreasureGenerator)) as TreasureGenerator;
		if (treasureGen != null)
			treasureGen.SendMessage("OnRestart");
		HitStop(true);
		m_hud.DrawStageStart(true);
		StartCoroutine("RestartWait");
	
	}
	
	IEnumerator RestartWait()
	{
		yield return new WaitForSeconds(1.0f);
		HitStop(false);
		m_hud.DrawStageStart(false);
	}
	
	public void GameStart()
	{
		m_retry_remain = RETRY;
		m_hud.DrawStageClear(false);
		m_stageNo = 1;
//		GameObject.Find("Map").SendMessage("OnGameStart");
		GameObject.Find("Player").SendMessage("OnGameStart");
		GameObject.Find("Score").SendMessage("OnGameStart");		
		OnStageStart();
	}
	
	public void OnEatAll()
	{
		GameObject.Find("Player").SendMessage("OnStageClear");
		GameObject[] enemys = GameObject.FindGameObjectsWithTag("Enemy");
		for (int i = 0; i < enemys.Length; i++)
			enemys[i].SendMessage("OnStageClear");
		GameObject.Find("Player").SendMessage("OnStageClear");
		m_hud.DrawStageClear(true);
		StartCoroutine("StageClear");

	}
	
	public void OnStageStart()
	{
		// 敵全消し.
		GameObject[] enemys = GameObject.FindGameObjectsWithTag("Enemy");
		for (int i = 0; i < enemys.Length; i++)
			Destroy(enemys[i]);
		
		// その他オブジェクト消す.
		TreasureGenerator treasureGenInst = FindObjectOfType(typeof(TreasureGenerator)) as TreasureGenerator;
		if (treasureGenInst != null)
			Destroy(treasureGenInst.gameObject);
		
		GameObject.Find("Map").SendMessage("OnStageStart");
		
		// 敵スポーン.
		for (int i = 1; i < 5; i++) {
			Vector3 pos = m_map.GetSpawnPoint(i);
			if (pos == Vector3.zero)
				continue;
			GameObject e = (GameObject)Instantiate(m_enemyPrefab,pos,Quaternion.identity);
			MonsterCtrl mc = e.GetComponent<MonsterCtrl>();
			mc.m_aiType = (MonsterCtrl.AI_TYPE)((int)MonsterCtrl.AI_TYPE.TRACER + i - 1);
			mc.SetSpawnPosition(m_map.GetSpawnPoint(Map.SPAWN_POINT_TYPE.BLOCK_SPAWN_POINT_PLAYER + i));
			mc.SetDifficulty(m_stageNo);
			
			mc.SendMessage("OnStageStart");
		}
		
		
		// ステージ番号更新.
		GameObject.Find("Stage").GetComponent<GUIText>().text = "Stage "+m_stageNo;
		
		// 宝物スポーン.
		Vector3 trasurePos = m_map.GetSpawnPoint(Map.SPAWN_POINT_TYPE.BLOCK_SPAWN_TREASURE);
		
		if (trasurePos != Vector3.zero)
			 Instantiate(m_treasureGen,trasurePos,Quaternion.identity);
		
		m_hud.DrawStageClear(false);
		GameObject.Find("Player").SendMessage("OnStageStart");
		m_hud.DrawStageStart(true);
		HitStop(true);
		
		// BGM再生.
		m_audio.StopAll();
		m_audio.PlayLoop(m_bgmClip,1.0f,0.0f);
		
		StartCoroutine("StageStartWait");
	}
	
	IEnumerator StageStartWait()
	{
		yield return new WaitForSeconds(1.0f);
		m_hud.DrawStageStart(false);
		HitStop(false);
	}
	
	
	IEnumerator StageClear()
	{
		m_audio.StopAll();
		m_audio.PlayOneShot(m_seStageClear,1.0f,0);
		yield return new WaitForSeconds(3.0f);
		m_stageNo++;
		OnStageStart();
	}
	
	public int GetStageNo()
	{
		return m_stageNo;
	}
	
	public void HitStop(bool enable)
	{
		GameObject[] enemys = GameObject.FindGameObjectsWithTag("Enemy");
		for (int i = 0; i < enemys.Length; i++)
			enemys[i].SendMessage("HitStop",enable);
		GameObject.FindGameObjectWithTag("Player").SendMessage("HitStop",enable);
	}
	
	public void OnAttack()
	{
		m_camera.OnAttack();
		HitStop(true);
	}
	
	public void OnEndAttack()
	{
		m_camera.OnEndAttack();
		HitStop(false);
	}
}
