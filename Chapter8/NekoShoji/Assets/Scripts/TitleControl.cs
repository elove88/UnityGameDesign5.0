using UnityEngine;
using System.Collections;

public class TitleControl : MonoBehaviour {

	public Texture2D TitleTexture = null;
	public Texture2D TitleEasy = null;
	public Texture2D TitleNormal = null;
	public Texture2D TitleHard = null;
	
	private bool PlayButtonPushed = false;

	// Use this for initialization
	void Start () {
		
		PlayButtonPushed = false;
		
	}
	
	// Update is called once per frame
	void Update () {

	}
	
	void OnGUI () {

		if(!TitleTexture) {
			Debug.LogError("Assign a Texture in the inspector.");
			return;
		}
		
		GUI.DrawTexture(new Rect(0, 0, TitleTexture.width, TitleTexture.height), TitleTexture);

		if ( GUI.Button(new Rect(0, 3*Screen.height/4 - TitleEasy.height/2 + 40, TitleEasy.width, TitleEasy.height), TitleEasy, GUI.skin.label) ) {
			
			if (!PlayButtonPushed) {
				
				PlayButtonPushed = true;
				GlobalParam.GetInstance().SetMode(0);
				Application.LoadLevel("GameScene");
	
			}
		}
		
		if ( GUI.Button(new Rect(Screen.width/2 - TitleNormal.width/2, 3*Screen.height/4 - TitleNormal.height/2 + 40, TitleNormal.width, TitleNormal.height), TitleNormal, GUI.skin.label) ) {

			if (!PlayButtonPushed) {
				
				PlayButtonPushed = true;
				GlobalParam.GetInstance().SetMode(1);
				Application.LoadLevel("GameScene");
	
			}
		}

		if ( GUI.Button(new Rect(Screen.width - TitleHard.width, 3*Screen.height/4 - TitleHard.height/2 + 40, TitleHard.width, TitleHard.height), TitleHard, GUI.skin.label) ) {

			if (!PlayButtonPushed) {
				
				PlayButtonPushed = true;
				GlobalParam.GetInstance().SetMode(2);
				Application.LoadLevel("GameScene");
	
			}
		}
	}
	
}
