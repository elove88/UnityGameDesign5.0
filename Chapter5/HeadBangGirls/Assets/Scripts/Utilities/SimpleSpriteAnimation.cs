using UnityEngine;
using System.Collections;
//シンプルなコマ送りアニメーションを実現するコンポーネント
public class SimpleSpriteAnimation: MonoBehaviour {
//Public variables
	public float animationFrameRateSecond=0.2f;
	public int divisionCountX=1;
	public int divisionCountY=1;
//Public methods
	public void BeginAnimation( int fromIndex, int toIndex, bool loop=false ){
		m_currentIndex = m_fromIndex = fromIndex;
		m_toIndex = toIndex;
		m_loop = loop;
		m_frameElapsedTime = 0;
		SetMaterilTextureUV();
	}
	//現在のメインテクスチャを取得
	public Texture GetTexture(){
		return GetComponent<Renderer>().material.GetTexture("_MainTex");
	}
	//テクスチャ表示部分のRectを取得
	public Rect GetUVRect(int frameIndex){
		int frameIndexNormalized=frameIndex;
		if(frameIndex>=divisionCountX*divisionCountY) 
			frameIndexNormalized=frameIndex%(divisionCountX*divisionCountY);
		float posX=((frameIndexNormalized%divisionCountX)/(float)divisionCountX);
		float posY=(1- ((1+(frameIndexNormalized/divisionCountX))/(float)divisionCountY));
		return new Rect( 
			posX, 
			posY, 
			GetComponent<Renderer>().material.mainTextureScale.x, 
			GetComponent<Renderer>().material.mainTextureScale.y
		);
	}
	public Rect GetCurrentFrameUVRect(){
		return GetUVRect(m_currentIndex);
	}
	//明確な指定が無い場合のアニメーションを設定
	public void SetDefaultAnimation( int defaultFromIndex, int defaultToIndex ){
		m_currentIndex = m_fromIndex = m_defaultFromIndex = defaultFromIndex;
		m_toIndex = m_defaultToIndex = defaultToIndex;
	}
	//ピクセル幅を取得
	public float GetWidth(){
		return GetComponent<Renderer>().material.mainTextureScale.x * GetComponent<Renderer>().material.GetTexture("_MainTex").width;
	}
	//ピクセル高さを取得
	public float GetHeight(){
		return GetComponent<Renderer>().material.mainTextureScale.y * GetComponent<Renderer>().material.GetTexture("_MainTex").height;
	}
	//アニメーションのコマを進める
	public void AdvanceFrame(){
		if(m_fromIndex<m_toIndex){
			if( m_currentIndex <= m_toIndex ){
				m_currentIndex++;
				if( m_toIndex < m_currentIndex ){
					if( m_loop ){
						m_currentIndex=m_fromIndex;
					}
					else{
						m_currentIndex = m_fromIndex = m_defaultFromIndex;
						m_toIndex = m_defaultToIndex;
					}
				}
				SetMaterilTextureUV();
			}
		}
		else{
			if( m_currentIndex >= m_toIndex ){
				m_currentIndex--;
				if( m_toIndex > m_currentIndex ){
					if( m_loop ){
						m_currentIndex=m_fromIndex;
					}
					else{
						m_currentIndex = m_fromIndex = m_defaultFromIndex;
						m_toIndex = m_defaultToIndex;
					}
				}
				SetMaterilTextureUV();
			}
		}
	}
	void Start () {
		GetComponent<Renderer>().material.mainTextureScale = new Vector2(1.0f/divisionCountX,1.0f/divisionCountY);
	}
	// Update is called once per frame
	void Update () {
		m_frameElapsedTime+=Time.deltaTime;
		if( animationFrameRateSecond < m_frameElapsedTime ){
			m_frameElapsedTime=0;
			AdvanceFrame();
		}
	}
	//コマ番号から適切なテクスチャ座標UVを設定
	void SetMaterilTextureUV(){
		Rect uvRect = GetCurrentFrameUVRect();
		GetComponent<Renderer>().material.mainTextureOffset=new Vector2(uvRect.x,uvRect.y);
	}
//Private variables
	float m_frameElapsedTime=0;
	int m_fromIndex = 0, m_toIndex = 0;
	int m_defaultFromIndex = 0, m_defaultToIndex = 0;
	bool m_loop = false;
	int m_currentIndex=0;
	
}
