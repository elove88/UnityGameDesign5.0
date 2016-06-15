using UnityEngine;
using System.Collections;

/// <summary>
/// アクティブソナーとパッシブソナーの切り替え
/// </summary>
public class SonarSwitcher : MonoBehaviour
{

    [SerializeField]
    private GameObject activeObj = null;
    [SerializeField]
    private GameObject passiveObj = null;
    [SerializeField]
    private int offsetPixel = 10;   // 左端からの位置オフセット
    [SerializeField]
    private float aspect = 0.4f;    // 画面に対するサイズ比
    [SerializeField]
    private int cameraRayoutPixel = 8;  // 見栄え上、テクスチャサイズより少し内側のサイズでカメラ位置を決める

    private GameObject currentObj = null;

    private float radius = 0;

    public enum SonarMode {
        None,
        PassiveSonar,
        ActiveSonar
    }
    private SonarMode mode = SonarMode.None;

	void Start () 
    {
        InitTexturePos();
    }

    void Update()
    {
        // 押してる間だけアクティブソナー
        if (Input.GetKeyDown(KeyCode.Space)) SetMode(SonarMode.ActiveSonar);
        if (Input.GetKeyUp(KeyCode.Space)) SetMode(SonarMode.PassiveSonar);
    }

    void SetMode( SonarMode mode_ )
    {
        if (mode == mode_) return;

        // スクリーンサイズに合わせてサイズ・位置調整
        if (currentObj != null)
        {
            Destroy(currentObj);
        }

        switch (mode_)
        {
            case SonarMode.ActiveSonar:
                CreateSonar(activeObj);
                break;

            case SonarMode.PassiveSonar:
                CreateSonar(passiveObj);
                break;

            default:
                GetComponent<GUITexture>().enabled = false;
                break;
        }

        mode = mode_;
    }

    void OnGameStart()
    {
    }

    void OnAwakeStage(int index)
    { 
        InitCameraPos();
    }

    void OnStageReset()
    {
//        InitCameraPos();
        Debug.Log("OnStageReset");
        // パッシブソナーがデフォルト
        SetMode(SonarMode.PassiveSonar);
    }

    private void InitTexturePos()
    {
        float size = Screen.height * aspect;

        GetComponent<GUITexture>().enabled = true;
        GetComponent<GUITexture>().pixelInset = new Rect(offsetPixel, Screen.height - offsetPixel - size, size, size);
    }
    private void InitCameraPos()
    {
        // カメラにテクスチャサイズを伝える
        Rect cameraRect = new Rect(GetComponent<GUITexture>().pixelInset);
        cameraRect.x += cameraRayoutPixel;
        cameraRect.y += cameraRayoutPixel;
        cameraRect.width -= cameraRayoutPixel * 2;
        cameraRect.height -= cameraRayoutPixel * 2;

        GameObject cameraObj = GameObject.Find("/Field/Player/SonarCamera");
        if (cameraObj)
        {
            SonarCamera sonarCamera = cameraObj.GetComponent<SonarCamera>();
            sonarCamera.SetRect(cameraRect);
            radius = sonarCamera.Radius();
        }
//        SetSize(activeObj);
//        SetSize(passiveObj);
    }

    private void SetSize(GameObject obj)
    {
        ActiveSonar activeSonar = currentObj.GetComponent<ActiveSonar>();
        if (activeSonar) activeSonar.SetMaxRadius(radius);
        SonarEffect effecter = currentObj.GetComponent<SonarEffect>();
        if (effecter) effecter.Init(GetComponent<GUITexture>().pixelInset);
    }

    private void CreateSonar( GameObject obj ) 
    {
        currentObj = Object.Instantiate(obj, Vector3.zero, Quaternion.identity) as GameObject;
        currentObj.transform.parent = transform;

        SetSize(currentObj);
   }
}
