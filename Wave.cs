using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Threading;

public class Wave : MonoBehaviour
{
    //敵スポーンの情報クラス
    //カスタムクラスを表示するために使用
    [System.Serializable]
    public class EnemySpawn
    {
        //出現する場所オブジェクト
        public Transform spawnPos;

        //出現する敵の種類
        public GameObject enemyPrefab;

        //出現するまでの待機時間
        public float delay;

        //アイテムドロップ対象
        public bool itemDrop;           
    }

    //Waveの詳細説明
    [SerializeField] string detail;
    //敵スポーン情報リスト
    [SerializeField] EnemySpawn[] enemySpawns;

    //キャンセルトクーン
     CancellationTokenSource cancelToken;

   // private CancellationToken token;

    
   async void Start()
    {
        //キャンセルトークンの生成 
         cancelToken = new CancellationTokenSource();

        //キャンセルトークンの取得
         CancellationToken token = cancelToken.Token;

        // GameObjectがDestroyされたら自動キャンセルされるトークンを取得
       // token = this.GetCancellationTokenOnDestroy();

        //敵スポーンリストから敵をスポーンさせる
        foreach (EnemySpawn spawn in enemySpawns)
        {
            Spawn(spawn);
        }

        //Waveの子要素のEnemyが全て削除されるまで待機
        try
        {
            //Waveの子要素のEnemyが全て削除されるまで待機
            //子オブジェクトが0かどうか判定する
            await UniTask.WaitUntil(() => transform.childCount == 0, PlayerLoopTiming.Update, token);
        }
        catch (System.OperationCanceledException e)
        {
            Debug.Log($"Wave子要素が0になるまでの待機処理がキャンセルされました >> " + e);

            cancelToken.Cancel();

            //ここでStartを終わらせる
            return;
        }

        //Waveの削除
        Destroy(gameObject);
    }

    //敵をスポーンさせる
    async void Spawn(EnemySpawn spawn)
    {
        //Delay時間待機
        await UniTask.Delay((int)(spawn.delay * 1000f));
        //敵生成
        GameObject enemy = Instantiate(spawn.enemyPrefab, spawn.spawnPos.position, Quaternion.identity);
        //生成した敵を子要素に追加
        //次の敵を生成し管理用親オブジェクトに入れる
        if(enemy != null)
        {
            enemy.transform.parent = transform;
        }

        //アイテムドロップフラグをセット
        enemy.GetComponent<EnemyController>().itemDrop = spawn.itemDrop;

        if (spawn.spawnPos != null)
        {
            //スポーン場所オブジェクトを削除
            Destroy(spawn.spawnPos.gameObject);
        }
        
       
    }

    //破棄された時に自動的に呼ばれる
    void OnDestroy()
    {
        if (cancelToken != null)
        {
            cancelToken.Cancel();
        }
    }

    //オブジェクトが非アクティブになったとき
    private void OnDisable()
    {
        if (cancelToken != null)
        {
            cancelToken.Cancel();
        }
    }

    //アプリが終了したとき
    private void OnApplicationQuit()
    {
        if (cancelToken != null)
        {
            cancelToken.Cancel();
        }
    }


}
