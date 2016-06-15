using UnityEngine;
using System.Collections;

/// <summary>
/// 生成パターン
/// </summary>
public class GeneratePattern : MonoBehaviour {

    // 発生時間
    [SerializeField]
    private float[] timing = new float[] { };
    // 生成状況変更
    [SerializeField]
    private GenerateParameter[] variation = new GenerateParameter[]{ };

    private int current = 0;
    private int validSize;

    private RandomGenerator generator = null;

	void Start () 
    {
        validSize = (timing.Length > variation.Length) ? variation.Length : timing.Length;

        generator = GetComponent<RandomGenerator>();

        // カウンター開始
        StartCoroutine("Counter");
    }

    private IEnumerator Counter()
    {
        yield return new WaitForSeconds(timing[current]);

        // パラメータ変更
        Debug.Log("Change Generate Parameter:time=" + timing[current]);
        if (generator) generator.SetParam(variation[current]);

        current++;
        if (current < validSize)
        {
            // カウント再開
            StartCoroutine("Counter");
        }
    }
}
