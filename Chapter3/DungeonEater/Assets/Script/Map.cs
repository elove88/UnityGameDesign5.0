//
// マップ管理クラス.
// 簡略化のためにブロックのサイズは１で固定されていて、配置は正の座標に１でスナップされているとする.
//
using UnityEngine;
using System.Collections;

public class Map : MonoBehaviour {
	public GameCtrl m_gameCtrl;
	
	private const int MAP_ORIGIN_X = 100;
	private const int MAP_ORIGIN_Z = 100;
	// マップのブロックの種類.
	private const char GEM = 'c';
	private const char WALL= '*';
	private const char SWORD = 's';
	private const char PLAYER_SPAWN_POINT = 'p';
	private const char TERASURE_SPAWN_POINT = 't';

	private const float WALL_Y = 0.0f;  // 壁のブロックを置くY座標.
	
	private const float GEM_Y = 0.5f;  // 宝石のY座標.
	public float GEM_SIZE = 1.0f;
	
	// 出現ポイントの種類.
	public enum SPAWN_POINT_TYPE {
		BLOCK_SPAWN_POINT_PLAYER = 0,
		BLOCK_SPAWN_POINT_ENEMY1,
		BLOCK_SPAWN_POINT_ENEMY2,
		BLOCK_SPAWN_POINT_ENEMY3,
		BLOCK_SPAWN_POINT_ENEMY4,
		BLOCK_SPAWN_TREASURE,
		BLOCK_SPAWN_POINT_END
	};
	const int SPAWN_POINT_TYPE_NUM = (int)SPAWN_POINT_TYPE.BLOCK_SPAWN_POINT_END - (int)SPAWN_POINT_TYPE.BLOCK_SPAWN_POINT_PLAYER;
	
	// マップデータ構造体.
	struct MapData {
		public int width;
		public int length;
		public int offset_x;    // data[0][0]はポジションoffset_x,offset_zのブロックを示す.
		public int offset_z; 
		public char[,] data;
		public float[,] height;
		
		public int[,] gemParticleIndex;
	};
	
	
	private MapData m_mapData;
	private Vector3[] m_spawnPositions ;

	// マップ素材.
	private GameObject m_items;
	private GameObject m_mapObjects;
	private GameObject m_mapCollision;
	public AudioChannels m_audio;


	public GameObject[] m_wallObject;
	public GameObject[] m_itemObject;
	public GameObject m_wallForCollision;
	
	// マップのデータ.
	public TextAsset m_defaultMap;
	public TextAsset[] m_map_texasset;
	
	// 宝石.関連
	private ParticleEmitter m_gemEmitter;
	private int m_gemTotalNum;
	private int m_gemCurrentNum;
	public AudioClip m_gemPickupSe;
	private const int GEM_SCORE = 10;
	
	// テスト用のマップ.
	// Use this for initialization
	void Awake () {
		
	}
	
	public void CreateModel()
	{
		LoadFromAsset(m_defaultMap);
		CreateMap(false,"MapModel",true);
	}
	
	private void SetMapData()
	{
		int stageNo = m_gameCtrl.GetStageNo();
		if (stageNo > m_map_texasset.Length)
			stageNo = (stageNo - 3) % 3 + 3;
		LoadFromAsset(m_map_texasset[stageNo-1]);
	}
	
	public void OnStageStart()
	{
		DeleteMap();
		SetMapData();
		CreateMap(true,"MapCollision",false);
		CreateMap(false,"MapBlocks",false);
		m_gemCurrentNum = m_gemTotalNum;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	private void LoadFromAsset(TextAsset asset)
	{
		m_mapData.offset_x = MAP_ORIGIN_X;
		m_mapData.offset_z = MAP_ORIGIN_Z;
		
		if (asset != null) {

			string txtMapData = asset.text;

			// 空の要素を削除するためのオプション.
			System.StringSplitOptions	option = System.StringSplitOptions.RemoveEmptyEntries;

			// 改行コードで１行ごとに切り出す..
			string[] lines = txtMapData.Split(new char[] {'\r','\n'},option);

			// "," で１文字ごとに切り出す.
			char[] spliter = new char[1] {','};

			// 一行目はマップの大きさ.
			string[] sizewh = lines[0].Split(spliter,option);
			m_mapData.width = int.Parse(sizewh[0]);
			m_mapData.length = int.Parse(sizewh[1]);

			char[,] mapdata = new char[m_mapData.length,m_mapData.width];

			for (int lineCnt = 0; lineCnt < m_mapData.length; lineCnt++) {

				// テキストファイル中で大きすぎる値が指定されていても大丈夫なよう、
				// 一応チェックする.
				if (lines.Length <= lineCnt+1)
					break;

				// "," で１文字ごとに切り出す.
				string[] data = lines[m_mapData.length-lineCnt].Split(spliter,option);

				for (int col = 0; col < m_mapData.width; col++) {

					if (data.Length <= col)
						break;

					mapdata[lineCnt,col] = data[col][0];
				}
			}
			m_mapData.data = mapdata;
		} else {
			Debug.LogWarning("Map data asset is null");
		}
	}
	
	//
	// 宝石とアイテムを配置する.
	//
	void SetupGemsAndItems()
	{
		m_mapData.gemParticleIndex = new int[m_mapData.length,m_mapData.width];
		m_gemTotalNum = 0;
		
		// 宝石の数を数え、同数のパーティクルを発生させる.
		for (int x = 0; x < m_mapData.width; x++) {
			for (int z = 0; z < m_mapData.length; z++) {
				if (IsGem(x,z))
					m_gemTotalNum++;
			}
		}
		m_gemEmitter = GetComponent<ParticleEmitter>() as ParticleEmitter;
		m_gemEmitter.Emit(m_gemTotalNum);
		
		// パーティクルの配置.
		Particle[] gemParticle = m_gemEmitter.particles;
		int gemCnt = 0;
		for (int x = 0; x < m_mapData.width; x++) {
			for (int z = 0; z < m_mapData.length; z++) {
				m_mapData.gemParticleIndex[z,x] = -1;
				if (IsGem(x,z)) {
					gemParticle[gemCnt].position = new Vector3((float)x+m_mapData.offset_x,
					                                           	GEM_Y,
					                                           (float)z+m_mapData.offset_z);
					gemParticle[gemCnt].size = GEM_SIZE;
					m_mapData.gemParticleIndex[z,x] = gemCnt;
					gemCnt++;
				}
				
				// アイテム生成.
				if (m_mapData.data[z,x] == SWORD) {
					// int itemType = (m_mapData.data[z,x] >> 8) & 0xf;
					Vector3 pos = new Vector3((float)x + m_mapData.offset_x,
					                      GEM_Y,
					                      (float)z+m_mapData.offset_z);
					// GameObject o = (GameObject)Instantiate(m_itemObject[itemType],pos,Quaternion.identity);
					GameObject o = (GameObject)Instantiate(m_itemObject[0],pos,Quaternion.identity);
					o.transform.parent = m_items.transform;
				}
					
					
			}
		}
		m_gemEmitter.particles = gemParticle;
	}
	
	// 宝石を置く？.
	bool	IsGem(int x, int z)
	{
		bool	ret = false;

		// "GEM" とモンスターのスポーン位置に宝石を置く.

		switch(m_mapData.data[z,x]) {

			case GEM:
			case '1':
			case '2':
			case '3':
			case '4':
			{
				ret = true;
			}
			break;

		}

		return(ret);
	}

	void CreateMap(bool collisionMode,string mapName,bool modelOnly = false)  // todo: コリジョンモード廃止.
	{
		
		m_mapObjects = new GameObject(mapName);
		m_spawnPositions = new Vector3[SPAWN_POINT_TYPE_NUM];
		if (m_items != null)
			Destroy(m_items);
		m_items = new GameObject("Item Folder");
		
		for (int x = 0; x < m_mapData.width; x++) {
			for (int z = 0; z < m_mapData.length; z++) {
				// ブロックのモデルを配置.
				switch (m_mapData.data[z,x]) {

				// 壁.
				case WALL:
					if (collisionMode) {
						GameObject o = Instantiate(m_wallForCollision,
					                           new Vector3(x+m_mapData.offset_x,0.5f,z+m_mapData.offset_z),
					                           Quaternion.identity) as GameObject;
						o.transform.parent = m_mapObjects.transform;
					} else {
						// int objectIndex = (m_mapData.data[z,x] >> 8) & 0xf;
						//GameObject o = Instantiate(m_wallObject[objectIndex],
						//                           new Vector3(x+m_mapData.offset_x,WALL_Y,z+m_mapData.offset_z),
						//                           Quaternion.identity) as GameObject;
						GameObject o = Instantiate(m_wallObject[0],
						                           new Vector3(x+m_mapData.offset_x,WALL_Y,z+m_mapData.offset_z),
						                           Quaternion.identity) as GameObject;
						o.transform.parent = m_mapObjects.transform;
					}
					break;

				// プレイヤー.
				case PLAYER_SPAWN_POINT:
					m_spawnPositions[(int)SPAWN_POINT_TYPE.BLOCK_SPAWN_POINT_PLAYER] = new Vector3(x+m_mapData.offset_x,0.0f,z+m_mapData.offset_z);
					break;

				// 宝箱.
				case TERASURE_SPAWN_POINT:
					m_spawnPositions[(int)SPAWN_POINT_TYPE.BLOCK_SPAWN_TREASURE] = new Vector3(x+m_mapData.offset_x,0.0f,z+m_mapData.offset_z);
					break;

				// モンスター.
				case '1':
				case '2':
				case '3':
				case '4':
					int enemyType = int.Parse(m_mapData.data[z,x].ToString());
					m_spawnPositions[enemyType] = new Vector3(x+m_mapData.offset_x,0.0f,z+m_mapData.offset_z);
					break;

				// モンスターAIのチェック用.
				// 同じ位置に「追いかけ」と「まちぶせ」をつくる.
				case '5':
					m_spawnPositions[1] = new Vector3(x+m_mapData.offset_x,0.0f,z+m_mapData.offset_z);
					m_spawnPositions[2] = new Vector3(x+m_mapData.offset_x,0.0f,z+m_mapData.offset_z);
					break;

				default:
					
					break;
				}				
			}
		}
		
		// マップデータ作成のみ.
		if (modelOnly)
			return;
		
		Transform[] children = m_mapObjects.GetComponentsInChildren<Transform>();
		m_mapObjects.AddComponent<CombineChildren_Custom>();
		m_mapObjects.GetComponent<CombineChildren_Custom>().Combine();
		Destroy(m_mapObjects.GetComponent<CombineChildren_Custom>());
		
		for (int i = 1; i < children.Length; i++)
			MyDestroy(children[i].gameObject,collisionMode);
		
		if (collisionMode) {
			m_mapObjects.AddComponent<MeshCollider>();
			m_mapObjects.GetComponent<MeshCollider>().sharedMesh = m_mapObjects.GetComponent<MeshFilter>().mesh;
			MyDestroy(m_mapObjects.GetComponent<MeshRenderer>(),collisionMode);
			m_mapCollision = m_mapObjects;
		}
		
		if (!collisionMode)
			SetupGemsAndItems();
	}
	
	private void MyDestroy(Object o,bool makeCollisionMode)
	{
			Destroy(o);
	}
	
			
	
	void DeleteMap()
	{
		if (m_mapObjects != null)
			Destroy(m_mapObjects);
		m_mapObjects = null;

		if (m_mapCollision != null)
			Destroy(m_mapCollision);
		m_mapCollision = null;
		if (m_gemEmitter != null)
			m_gemEmitter.ClearParticles();
		if (m_items != null)
			Destroy(m_items);
		m_items = null;
		
	}
	
	
	//
	// 位置からブロックを得る.
	//
	int GetBlockFromPos(Vector3 pos)
	{
		int grid_x = Mathf.RoundToInt(pos.x);
		int grid_z = Mathf.RoundToInt(pos.z);
		
		// マップの位置になおす.
		grid_x -= m_mapData.offset_x;
		grid_z -= m_mapData.offset_z;
		// 範囲内か？.
		if (grid_x < 0 || grid_z < 0 || grid_x >= m_mapData.width || grid_z >= m_mapData.length)
			return 0;
		return m_mapData.data[grid_z,grid_x];
		
	}	
	
	//---------------------------------------
	// 出現ポイント.
	//---------------------------------------
	public Vector3 GetSpawnPoint(SPAWN_POINT_TYPE type)
	{
		int t = (int)type;
		if (t >= m_spawnPositions.Length ) {
			Debug.LogWarning("Spawn Point is not found");
			return new Vector3((float)MAP_ORIGIN_X,0,(float)MAP_ORIGIN_Z);
		}
		
		return m_spawnPositions[t];
	}
	
	public Vector3 GetSpawnPoint(int type)  // TODO: 違いが上の関数とわかりづらい.
	{
		return m_spawnPositions[type];
	}
	

	public bool PositionToIndex(Vector3 pos,out int x,out int z)
	{
		x = Mathf.RoundToInt(pos.x);
		z = Mathf.RoundToInt(pos.z);
		
		// マップの位置になおす.
		x -= m_mapData.offset_x;
		z -= m_mapData.offset_z;
		// 範囲チェック.
		if (x < 0 || z < 0 || x >= m_mapData.width || z >= m_mapData.length)
			return false;
		return true;
		
		
	}
	
	public void PickUpItem(Vector3 p)
	{
		int gx,gz;
		bool ret = PositionToIndex(p,out gx,out gz);
		
		if (ret) {
			int idx = m_mapData.gemParticleIndex[gz,gx];
			if (idx >= 0) {
				Particle[] gemParticle = m_gemEmitter.particles;
				gemParticle[idx].size = 0;
				m_gemEmitter.particles = gemParticle;
				m_mapData.gemParticleIndex[gz,gx] = -1;
				m_audio.PlayOneShot(m_gemPickupSe,1.0f,0);
				Score.AddScore(GEM_SCORE);
				m_gemCurrentNum--;
				if (m_gemCurrentNum <= 0)
					m_gameCtrl.OnEatAll();
			}
		}
	}
	
	public int GetGemRemainNum()
	{
		return m_gemCurrentNum;
	}
}

