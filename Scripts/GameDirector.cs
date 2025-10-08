using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// ゲーム全体を管理
/// </summary>
public class GameDirector : MonoBehaviour
{
    //各ステージ情報
    [SerializeField] List<StageData> stageDatas = new List<StageData>();

    //現在のステージ数
    int stageNum = 0;
    //現在のステージデータ
    StageData stageData;

    //キャンセルトクーン
     CancellationTokenSource cancelToken;

    //スコア管理
    int totalScore;

    [SerializeField] TextMeshProUGUI ScoreText;
    //BGM名
    string bgmName;

    //獲得スコアを分割して一時待機させるキュー
    Queue<int> scoreQueue = new Queue<int>();

    //プレイヤーの残機
    int playerCount = 3;

    //プレイヤープレハブ
    [SerializeField] GameObject playerPrefab;

    //プレイヤー出現場所
    [SerializeField] Transform playerSpawn;

    //残機UI表示
    [SerializeField] Transform playerCountUI;

    [SerializeField] GameObject iconPrefab;

    //ゲームオバーUI
    [SerializeField] GameObject gameOverUI;

    //ゲームオバーフラグ
    bool gameOverFlg;

    //背景オブジェクト
    //GameObject background;


    async void Start()
    {
        //ゲームオバー非表示
        gameOverUI.SetActive(false);


        //プレイヤー生成
        CreatePlayer();

        //キャンセルトークンの生成 
         cancelToken = new CancellationTokenSource();

        //キャンセルトークンの取得
        CancellationToken token = cancelToken.Token;

        //スコアの初期化
        ScoreText.text = totalScore.ToString("D0");

        //保持されていたステージ数をリセット
        stageNum = MyPlayer.Instance.STAGE_NUM;


        //すべてのステージを読み込むまで繰り返す
        while (stageDatas.Count > stageNum)
        {
            Debug.Log("While中");
            try
            {
                //ステージ読み込み開始
                await StageStart(token);
            }

            //Tokenによってawaitしているタスクがキャンセルされた場合の例外処理
            catch (System.OperationCanceledException e)
            {
                Debug.Log($"ステージスタートがキャンセルされました >> " + e);



                //whileループを抜ける
                break;
            }
        }

        //ゲームオーバー時はここで処理を終わらせる
        if (gameOverFlg) return;

        // await StageStart(token);
        Debug.Log("すべてのステージをクリア!");


        SceneManager.LoadScene("03_Ending");
    }

    //ステージのwave読み込み開始
    async UniTask StageStart(CancellationToken token)
    {



        //現在のステージデータ取得
        stageData = stageDatas[stageNum];


        ////前の背景を消去
        //if(background != null)
        //{
        //    Destroy(background);
        //}

        ////背景生成
        //background = Instantiate(stageData.background, transform);

        //現在のステージデータから敵をスポーンさせる
        foreach (StageData.WaveInfo waveInfo in stageData.stage)
        {

            //Delay時間待機
            await UniTask.Delay((int)(waveInfo.delay * 1000f), false, PlayerLoopTiming.Update, token);
            //try
            //{
            //    //Delay時間待機
            //    //Updateのタイミングで再開しキャンセルトークンからキャンセルが飛んで来たら即中断可能
            //    await UniTask.Delay((int)(waveInfo.delay * 1000f), false, PlayerLoopTiming.Update, token);

            //}

            //catch(System.OperationCanceledException e) 
            //{
            //    Debug.Log("Delay時間の待機処理がキャンセルされました　>> " + e);

            //    //UniTaskの停止
            //    cancelToken.Cancel();

            //    //foreachのループ処理を終わらせる
            //    break;
            //}
            //Wave生成
            GameObject wave = Instantiate(waveInfo.wavePrefab);

            //ステージ開始Waveの場合
            if (waveInfo.waveType == StageData.WaveType.start)
            {
                //ステージ数をセット
                WaveStart waveStart = wave.GetComponent<WaveStart>();
                waveStart.SetStageNum(stageNum + 1);

                //ステージBGM
                bgmName = SoundData.BgmType.start.ToString();

                GSound.Instance.PlayBgm(bgmName, true);
            }

            //ステージクリアのWaveの場合
            else if (waveInfo.waveType == StageData.WaveType.clear)
            {
                //ステージ数を一つ進める
                stageNum++;

                MyPlayer.Instance.STAGE_NUM = stageNum;

                //クリアBGM
                bgmName = SoundData.BgmType.clear.ToString();

                GSound.Instance.PlayBgm(bgmName, false);
            }

            //ウェーブエンディングの場合
            else if (waveInfo.waveType == StageData.WaveType.ending)
            {
                //スコアをセット
                WaveEnding waveEnding = wave.GetComponent<WaveEnding>();

                waveEnding.SetScore(totalScore);

                //ステージ数を進める
                stageNum++;

                MyPlayer.Instance.STAGE_NUM = stageNum;

                //エンディングBGM
                bgmName = SoundData.BgmType.clear.ToString();

                GSound.Instance.PlayBgm(bgmName, false);
            }

            //ボスWaveの場合
            else if(waveInfo .waveType == StageData.WaveType.boss)
            {
                //ボスBGM
                bgmName = SoundData.BgmType.boss.ToString();

                GSound.Instance.PlayBgm(bgmName, true);
            }

            //すべての敵を倒さないと次へいけない場合
            if (waveInfo.completFlg)
            {
                try
                {
                    //現在のWaveが破棄されるまで待機
                    await UniTask.WaitUntil(() => wave == null, PlayerLoopTiming.Update, token);
                }
                catch (System.OperationCanceledException e)
                {
                    Debug.Log("Wave破棄までの待機処理がキャンセルされました" + e);

                    //UniTaskの停止
                    cancelToken.Cancel();

                    //foreachのループ処理を終わらせる
                    break;
                }
            }
        }
    
    }

    //スコアをセット
    public void AddScore(int score)
    {
        totalScore += score;

        ScoreText.text = totalScore.ToString("D0");

        int splitScore = score / 50;

        //分割したスコアを一つずつキューに追加
        while(score > 0)
        {
            scoreQueue.Enqueue(splitScore);

            score -= splitScore;

            //分割で最後の余りを消去
            if(score < splitScore)
            {
                scoreQueue.Enqueue(score);

                score = 0;
            }
        }
    }

    //Update後に処理されるUpdate
    private void LateUpdate()
    {
        //キューに追加せれている未追加スコアをチェック
        CheckScoreQueue();

        //SEデータがあれば1フレームごとに１つのSEを鳴らす
        GSound.Instance.CheckSeQueue();
    }

    //キューに格納された分割スコアを一つずつスコア表記
    private void CheckScoreQueue()
    {
        //キューにデータがあれば一つだけ呼び出して処理する
        if(scoreQueue .Count > 0)
        {
            //キューから分割スコアを取り出す
            int s = scoreQueue.Dequeue();

            //取り出した分割スコアをトータルスコアへ追加
            totalScore += s;

            //3桁ごとにカンマを入れた表記
            ScoreText.text = totalScore.ToString("D0");
        }
    }

    
    //プレイヤーの残機管理
    public void CreatePlayer()
    {
        //残機が残っている場合
        if(playerCount > 0)
        {
            playerCount--;

            //プレイヤ復活
            //Instantiate(playerPrefab);

            Instantiate(playerPrefab, playerSpawn.position, Quaternion.identity);

            //残機UI更新
            //一旦すべてのアイコンを削除
            foreach (Transform icon in playerCountUI)
            {
                Destroy(icon.gameObject);
            }

            //残機の数だけアイコンを生成
            for (int i = 0; i < playerCount; i++)
            {
                Instantiate(iconPrefab, playerCountUI);
            }
        }

        //残機0でゲームオバー
        else
        {
            gameOverUI.SetActive(true);

            gameOverFlg = true;
        }

            Debug.Log($"プレイヤー残機　>> {playerCount}");
    }


    //リトライボタン
    public void OnPressRetryButton()
    {
        //UniTaskの停止
        cancelToken.Cancel();

        SceneManager.LoadScene("02_Game");
    }

    //タイトルへ戻るボタン
    public void OnPressTitleButton()
    {
        //UniTaskの停止
        cancelToken.Cancel();

        //ステージ数リセット
        MyPlayer.Instance.STAGE_NUM = 0;

        SceneManager.LoadScene("00_Title");
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
