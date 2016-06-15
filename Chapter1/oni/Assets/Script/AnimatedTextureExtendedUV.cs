using UnityEngine;
using System.Collections;

public class AnimatedTextureExtendedUV : MonoBehaviour
{

	//vars for the whole sheet
	public int colCount =  4;
	public int rowCount =  4;
	
	//vars for animation
	public int  rowNumber  =  0; //Zero Indexed
	public int colNumber = 0; //Zero Indexed
	public int totalCells = 4;
	public int  fps     = 10;

	//Maybe this should be a private var
	private Vector2 offset;
	
	// 現在時間.
	private	float	timer = 0.0f;
		
	// -------------------------------------------------------------------------------- //

	void Update ()
	{
		do {

			// 最後まで再生したら止める.
			if(this.timer*fps >= (float)totalCells) {

				this.gameObject.SetActive(false);
				break;
			}

			SetSpriteAnimation(colCount,rowCount,rowNumber,colNumber,totalCells,fps);

			this.timer += Time.deltaTime;

		} while(false);
	}

	private void SetSpriteAnimation(int colCount ,int rowCount ,int rowNumber ,int colNumber,int totalCells,int fps ){
	
		// Calculate index
		int index  = (int)(this.timer* fps);
		
		// Repeat when exhausting all cells
		index = index % totalCells;
		
		// Size of every cell
		float sizeX = 1.0f / colCount;
		float sizeY = 1.0f / rowCount;
		Vector2 size =  new Vector2(sizeX,sizeY);
		
		// split into horizontal and vertical index
		var uIndex = index % colCount;
		var vIndex = index / colCount;
		
		// build offset
		// v coordinate is the bottom of the image in opengl so we need to invert.
		float offsetX = (uIndex+colNumber) * size.x;
		float offsetY = (1.0f - size.y) - (vIndex + rowNumber) * size.y;
		Vector2 offset = new Vector2(offsetX,offsetY);
		
		GetComponent<Renderer>().material.SetTextureOffset ("_MainTex", offset);
		GetComponent<Renderer>().material.SetTextureScale  ("_MainTex", size);
	}

	// -------------------------------------------------------------------------------- //

	// 再生を開始する.
	public void		startPlay(float start_time)
	{
		this.timer = start_time;
		this.gameObject.SetActive(true);
	}

	// 再生を停止する.
	public void		stopPlay()
	{
		this.timer = 0.0f;
		this.gameObject.SetActive(false);
	}

	// 再生中？.
	public bool		isPlaying()
	{
		return(this.gameObject.activeSelf);
	}

}
