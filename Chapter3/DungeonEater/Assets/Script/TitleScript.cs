using UnityEngine;
using System.Collections;

public class TitleScript : MonoBehaviour {
	public Texture m_titleTex;
	public float m_go_adv = 10.0f;
	private bool m_PlayButtonPushed = false;
	
	// Update is called once per frame
	void Update () {
		m_go_adv -= Time.deltaTime;
		if (m_go_adv < 0.0f && !m_PlayButtonPushed) {
			GlobalParam.GetInstance().SetMode(true);
			Application.LoadLevel("GameScene");
		}
	}
	
	void OnGUI()
	{
	
		float sw = Screen.width;
		float sh = Screen.height;
		float r =  (float)m_titleTex.height / (float)m_titleTex.width;
		float h = sw*r;
		GUI.Label (new Rect (0, (sh-h)/2.0f, sw, sw*r), m_titleTex);

		float bt_w = 200;
		if (GUI.Button(new Rect((sw-bt_w)/2.0f,sh-64,bt_w,50),"Play")) {
			if (!m_PlayButtonPushed) {
				m_PlayButtonPushed = true;
				GetComponent<AudioSource>().Play();
				GlobalParam.GetInstance().SetMode(false);
				//Application.LoadLevel("GameScene");
				StartCoroutine(StartGame());
			}
		}
		
		GUILayout.BeginArea(new Rect(0,0,Screen.width,Screen.height));
		GUILayout.FlexibleSpace();
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.Label("Copyright (C) 2012 METAL BRAGE. All Rights Reserved.");
		GUILayout.EndHorizontal();
		GUILayout.EndArea();
	}
	
	IEnumerator StartGame()
	{
		yield return new WaitForSeconds(1.0f);
		Application.LoadLevel("GameScene");
	}
}
