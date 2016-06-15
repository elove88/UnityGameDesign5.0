using UnityEngine;
using System.Collections;

public class Hud : MonoBehaviour {
	private const int PLAYER_IMAGE_WIDTH = 32;
	private GameCtrl m_gameCtrl;

	public Texture m_gameOverImage;
	public Texture m_stageClearImage;
	public Texture m_gameStartImage;
	public Texture m_playerImage;
	public GUIText m_demoText;
	
	private bool m_gameOverImgVisible = false;
	private bool m_stageClearImgVisible = false;
	private bool m_stageStartImgVisible = false;
	
	
	
	// Use this for initialization
	void Start () {
		m_gameCtrl = GameObject.Find("GameCtrl").GetComponent<GameCtrl>();
		OnGameStart();
		
		GlobalParam param = GlobalParam.GetInstance();
		if (param != null) {
			if (param.IsAdvertiseMode())
				m_demoText.enabled = true;
			else
				m_demoText.enabled = false;
		}		
		
	}
	
	void OnGameStart()
	{
		m_gameOverImgVisible = false;
		m_stageClearImgVisible = false;
	}
	
	public void DrawGameOver(bool visible)
	{
		m_gameOverImgVisible = visible;
	}
	
	public void DrawStageClear(bool visible)
	{
		m_stageClearImgVisible = visible;
	}
	
	public void DrawStageStart(bool visible)
	{
		m_stageStartImgVisible = visible;
	}
	

	
	void OnGUI()
	{
		int centerX = Screen.width / 2;
		int centerY = Screen.height / 2;
		int remain = m_gameCtrl.GetRetryRemain();
		// 残機表示.

		//GUI.Label(new Rect(10,10,200,20),"Player");
		for (int i = 0; i < remain; i++)
			GUI.Label(new Rect(10+i*PLAYER_IMAGE_WIDTH,32,PLAYER_IMAGE_WIDTH,PLAYER_IMAGE_WIDTH),m_playerImage);
		
		// GAME OVERの表示.
		if (m_gameOverImgVisible)
			GUI.DrawTexture(new Rect(centerX-256,centerY-32,512,64),m_gameOverImage);
		
		// Stage Clear の表示.
		if (m_stageClearImgVisible)
			GUI.DrawTexture(new Rect(centerX-256,centerY-32,512,64),m_stageClearImage);

		// ステージ開始　の表示.
		if (m_stageStartImgVisible)
			GUI.DrawTexture(new Rect(centerX-64,centerY-32,128,64),m_gameStartImage);
	}
}
