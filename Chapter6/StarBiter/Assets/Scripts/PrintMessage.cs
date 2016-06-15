using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// PrintMessage
//  - sub screenに表示するメッセージを制御する.
//  - 使い方.
//    - 表示したいメッセージを引数としてSetMessageを呼び出す.
//  - 動き仕様.
//    - SetMessageにて渡されたメッセージについて最初のメッセージから順次処理する.
//    - メッセージを少しずつsub screenへ表示.
//    - 指定した行数を超えた場合は最初の行から消していく.
// ----------------------------------------------------------------------------
public class PrintMessage : MonoBehaviour {
	
	private ArrayList messages = new ArrayList();
	private bool isPrinting = false;
	private GUIText subScreenGUIText;
	const int MAX_ROW_COUNT = 6;
	
	private static int		ADDITION_NUM = 1;		// 一度に表示する文字数.
	private static string 	CURSOR_STR = "_";		// カーソルの文字.
	
	void Start () {
		
		// sub screenのインスタンスを取得.
		subScreenGUIText = GameObject.FindGameObjectWithTag("SubScreenMessage").GetComponent<GUIText>();
		
		// sub screenを改行で埋める(最初のメッセージは最下行から表示を始めるようにする為).
		subScreenGUIText.text = "\n\n\n\n\n\n";
		
		// ゲーム開始のメッセージを表示.
		SetMessage("STAND BY ALERT.");
		SetMessage("ENEMY FLEETS ARE APPROACHING.");
		SetMessage(" ");
		
	}
	
	void Update () {
	
		// 表示すべきメッセージの有無を確認.
		if ( messages.Count > 0 )
		{
			// sub screenへの表示をしている最中は新たなメッセージは処理しない.
			if ( !isPrinting )
			{
				// メッセージの表示処理を呼び出す.
				isPrinting = true;
				string tmp = messages[0] as string;
				messages.RemoveAt(0); 
				StartCoroutine( "PlayMessage", tmp );
			}
		}
	}
	
	// ------------------------------------------------------------------------
	// メッセージをメモリにセット(先入れ先出し).
	// ------------------------------------------------------------------------
	public void SetMessage( string message )
	{
		messages.Add( message );
	}
	
	// ------------------------------------------------------------------------
	// メッセージをsub screenへ少しずつ表示.
	// ------------------------------------------------------------------------
	IEnumerator PlayMessage( string message )
	{
		char[] charactors = new char[256];
			
		// 領域以上の文字は破棄する.
		if ( message.Length > 255 )
		{
			message = message.Substring(0, 254);
		}
		
		// 一文字ずつに分割.
		charactors = message.ToCharArray();
		
		// 表示文字を取得.
		string subScreenText = subScreenGUIText.text;

		subScreenText += "\n";

		// 一度に表示する文字.
		// 固定値＋キューにたまっている行数.
		// 表示待ちメッセージがキューにたくさんたまってきたときに、
		// 表示スピードが速くなるように.	
		int	additionNum = ADDITION_NUM + messages.Count;

		for(int i = 0;i < charactors.Length;i += additionNum)
		{
			// 末尾のカーソルを一度削除.
			if(subScreenText.EndsWith(CURSOR_STR)) {

				subScreenText = subScreenText.Remove(subScreenText.Length - 1);
			}


			// バッファー中の文字を additionNum 文字づつ追加.
			for(int j = 0;j < additionNum;j++) {

				if(i + j >= charactors.Length) {

					break;
				}

				subScreenText += charactors[i + j];
			}


			// カーソルを追加.
			subScreenText += CURSOR_STR;

			// 画面からはみ出す行は削除する.

			string[] lines = subScreenText.Split("\n"[0]);

			if(lines.Length > MAX_ROW_COUNT) {

				subScreenText = "";

				// 後ろから MAX_ROW_COUNT 行を追加する.
				for(int j = lines.Length - MAX_ROW_COUNT;j < lines.Length;j++) {

					subScreenText += lines[j];

					if(j < lines.Length - 1) {

						subScreenText += "\n";
					}
				}
			}

			subScreenGUIText.text = subScreenText;
			
			// ウエイト.
			yield return new WaitForSeconds( 0.001f );
		}
		
		// 全文表示終わったら、カーソルは表示しない.
		if(subScreenText.EndsWith(CURSOR_STR)) {

			subScreenText = subScreenText.Remove(subScreenText.Length - 1);

			subScreenGUIText.text = subScreenText;
		}
			
		// 表示処理終了.
		isPrinting = false;
	}

}
