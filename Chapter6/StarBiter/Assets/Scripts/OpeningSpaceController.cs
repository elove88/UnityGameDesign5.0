using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// 背景の星空スクロール.
//  - 上から下へスクロールするのみ.
// ----------------------------------------------------------------------------
public class OpeningSpaceController : MonoBehaviour {

	public float scrollSpeedStar1 = 0.2f;			// 星がスクロールするスピード.
	public float scrollSpeedStar2 = 0.5f;			// 星がスクロールするスピード.
	public float scrollSpeedStar3 = 1f;				// 星がスクロールするスピード.
	
	const int MAX_STARS = 3;
	private GameObject[] stars = new GameObject[MAX_STARS];	// 星.
	private float[] scrollSpeed = new float[MAX_STARS];		// 星のスクロールスピード.
	
	private float bgZ1 = -10f;						// 戦闘空間の境界(上端).
	private float bgZ2 = 10f;						// 戦闘空間の境界(下端).
	
	private bool isEaseIn = false;					// イーズイン.
	private float easeInRate = 0.6f;				// 減衰していく割合.
	
	void Start ()
	{
		// 星のインスタンスを取得.
		GameObject star1 = GameObject.FindGameObjectWithTag("Star1");
		GameObject star2 = GameObject.FindGameObjectWithTag("Star2");
		GameObject star3 = GameObject.FindGameObjectWithTag("Star3");
		stars[0] = star1;
		stars[1] = star2;
		stars[2] = star3;
		
		// 星のスクロールスピードを設定.
		scrollSpeed[0] = scrollSpeedStar1;
		scrollSpeed[1] = scrollSpeedStar2;
		scrollSpeed[2] = scrollSpeedStar3;
	}

	void LateUpdate()
	{
		// 星をスクロール(上から下へ).
		Scroll();
	}
	
	// ------------------------------------------------------------------------
	// 星をスクロール(上から下へ動かす).
	// ------------------------------------------------------------------------
	private void Scroll()
	{
		// 星をZ軸のマイナス方向に進める.
		for( int i = 0; i < MAX_STARS; i++ )
		{
			if ( !stars[i] || scrollSpeed[i] == 0 )
			{
				// 星のゲームオブジェクト又はスクロールスピードが未設定の場合は処理しない.
				continue;
			}
			
			// 星を進める.
			Vector3 additionPos = new Vector3( 0, 0, 1f )  * scrollSpeed[i] * Time.deltaTime;
			stars[i].transform.position -= additionPos;
			
			// 星のループ制御.
			IsOutOfWorld( stars[i] );
			
			// イーズイン.
			if ( isEaseIn )
			{
				scrollSpeed[i] -= scrollSpeed[i] * easeInRate * Time.deltaTime;
			}
		}
	}
	
	// ------------------------------------------------------------------------
	// メインカメラの表示領域が星の領域からはみ出した場合のループ制御.
	// ------------------------------------------------------------------------
	private void IsOutOfWorld( GameObject star )
	{
		
		if ( star.transform.position.z < bgZ1 )
		{
			star.transform.position = new Vector3(
				star.transform.position.x,
				star.transform.position.y,
				bgZ2 );
		}

	}
	
	public void SetEaseIn()
	{
		isEaseIn = true;
	}
	
}
