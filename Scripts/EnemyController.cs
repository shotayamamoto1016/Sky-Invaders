using Cysharp.Threading.Tasks;  
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;


public class EnemyController : MonoBehaviour


{
    //移動速度
    [SerializeField] float speed = 1.0f;

    //HP
    [SerializeField] protected  int maxHp = 5;

    protected int currentHp;

    //爆弾エフェクトプレハブ
    [SerializeField] GameObject explosionPrefab;

    [SerializeField] float explosionSize = 0.5f;

    //ダメージ時にスプライトの色を変更するために取得
    SpriteRenderer spriteRenderer;

    //ダメージ中フラグ
    bool isDamage;

    [SerializeField] int score;

    //フライスコアテキスト
    [SerializeField] GameObject flyScorePrefab;

    [SerializeField] GameObject itemPrefab;

    //アイテム出現フラグ
    public bool itemDrop;

    //弾を発射(弾のスピードを調整)
    //protectedはクラス自身または継承したクラスからアクセス可能
    //virtualサブクラスでoverrideすることで処理を上書きできる
    protected virtual void Shot(List<Transform > shotPositions, GameObject bullet , float speedRate = 1)
    {
        foreach (Transform shotPos in shotPositions)
        {
            GameObject bulletObj = Instantiate(bullet, shotPos.position, shotPos.rotation);

            bulletObj.GetComponent<EnemyBullet>().SpeedRate(speedRate);
        }
        
    }

    //メインカメラに付いているタグ名
     const string MAIN_CAMERA_TAG = "MainCamera";

    //カメラに映っているか判断する
     protected bool isRendered = false;

    //カメラ型の変数
     Camera mainCamera;

    //private CancellationToken token;

    //キャンセルトークン
 // CancellationTokenSource cancelToken;

    protected virtual void Start()
    {
        mainCamera = Camera.main;

        //HPを初期化
        currentHp = maxHp;

        //ダメージ中にスプライトの色を変化させるために取得
        spriteRenderer = GetComponent<SpriteRenderer>();

        //token = this.GetCancellationTokenOnDestroy();

        //キャンセルトークンの生成 
        //cancelToken = new CancellationTokenSource();
               //CancellationTokenの取得  
       // CancellationToken token = cancelToken.Token;
    }

    //移動
    protected virtual void Move(Vector3 moveDir)
    {
        transform.position += moveDir * speed * Time.deltaTime;
    }

    //弾を発射
    //オーバーロード(同じ名前のメソッドでも引数の種類や数が違えば別のメソッドとして定義できる)を利用している
    protected virtual void Shot(List<Transform> shotPositions, GameObject bullet)
    {
        foreach (Transform shotPos in shotPositions)
        {
            Instantiate(bullet, shotPos.position, shotPos.rotation);
        }
       
    }

    //なにかと触れたら
    private void OnTriggerEnter2D(Collider2D other)
    {
        //カメラに映っていない場合は当たり判定無効
        if (isRendered == false) return;

        if (other.gameObject.tag == "PlayerBullet")
        {
            //ダメージ中は無敵時間
            if (isDamage) return;

            //ダメージ処理
            Damage(1);
        }
    }

    //ダメージを受けてHPを減らす
    protected virtual async void Damage(int d)
    {
        //HPを減らす
        currentHp -= d;

        //HP0で消滅
        if(currentHp <= 0)
        {
            Destroy(gameObject);

            //爆弾エフェクト
            GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);

            //爆弾エフェクトのサイズを変更
            explosion.GetComponent<Explosion>().SetSize(explosionSize);

            GameObject.Find("GameDirector").GetComponent<GameDirector>().AddScore(score);

            //スコアのフライテキスト生成
            GameObject flyScore = Instantiate(flyScorePrefab, transform.position, Quaternion.identity);

            //スコア値をセット
            flyScore.GetComponent<FlyText>().SetText(score.ToString());

            //ドロップアイテム
            if(itemPrefab != null && itemDrop == true)
            {
                Instantiate(itemPrefab, transform.position, Quaternion.identity);
            }
        }

        //ダメージ時処理
        else
        {
            //現在のスプライトの色を保持
            Color currentColor = spriteRenderer.color;

            //スプライトの色を赤にする
            spriteRenderer.color = Color.red;

            //ダメージ中のフラグをセット
            isDamage = true;

            //0.05秒待つ
            await UniTask.Delay((int)(0.05f * 1000f));

            //スプライトの色を戻す
            spriteRenderer.color = currentColor;

            //フラグを解除
            isDamage = false;
        }





    }

    private void OnBecameVisible()
    {
        if (mainCamera.tag == MAIN_CAMERA_TAG)
        {
            isRendered = true;
        }
    }

    private void OnBecameInvisible()
    {
        isRendered = false;
    }

}
