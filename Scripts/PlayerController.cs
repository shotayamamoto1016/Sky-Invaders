using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem.LowLevel;
using DG.Tweening;

public class PlayerController : MonoBehaviour
{
    //移動速度
    //[SerializeField]はprivateでもインスペクターに表示・保存可能にする
    //publicより安全でほかのスクリプトからアクセスできない
    [SerializeField] private float speed = 5f;

   // [SerializeField] private float speedUpInterval = 2f;

    //弾プレハブ
   [SerializeField] GameObject bulletPrefab;

    [SerializeField] GameObject laserPrefab;

   //連射速度
   [SerializeField] float shotInterval = 3.0f;
   float delta;

   //弾の発射位置
   [SerializeField] Transform singleShot;

    [SerializeField] Transform laserShotR;

    [SerializeField] Transform laserShotL;

    [SerializeField] Transform tripleShotR;

    [SerializeField] Transform tripleShotL;

    [SerializeField] Transform quintupleShotR;

    [SerializeField] Transform quintupleShotL;

    //ショットレベル
    int shotLevel;

    //アイテムカウントの宣言
    [SerializeField] int itemCount = 0;

    //パワーアップの間隔を宣言
    [SerializeField] int powarUpInterval = 5;

    //スピードアップするためのカウント宣言
    [SerializeField] int speedUpCount = 0;

    //爆弾エフェクトプレハブを宣言
    [SerializeField] GameObject explosionPrefab;

    //参照
    Animator _animator;

    //画面制限域
    Vector3 screenSize;
    Vector3 worldSize;

    //プレイヤの位置履歴
    public Queue<Vector3> playerPosHistory = new Queue<Vector3>();

    //最大履歴保存数
    int maxHistoryCount = 300;

    //ひとつ前のフレームの位置保存
    Vector3 beforePlayerPos;

    //オプションプレハブ
    [SerializeField] GameObject optionPrefab;

    //追加したオプションを格納するリスト
    List<GameObject> options = new List<GameObject>();

    //自動移動モード
    bool isAutoMode = true;

    //無敵モード
    bool isInvincible = true;

    //オプションプレハブアイテム
    [SerializeField] GameObject optionItemPrefab;

    void Start()
    {
        //コンポーネント取得
        _animator = GetComponent<Animator>();

        //スクリーンサイズをピクセル座標に変換する
         screenSize = new Vector3(Screen.width, Screen.height, 0);
        //ピクセル座標をワールド座標へ変換
        //ScreenToWorldPointとはカメラが見ているスクリーン(ピクセル)上の座標を、ゲーム内のワールド空間に変換する関数。
         worldSize = Camera.main.ScreenToWorldPoint(screenSize);

        //出撃演出
        Vector2 goalPos = new Vector2(0, -worldSize.y * 0.8f);

        AutoMoveMode(goalPos, 2f, Ease.OutBack);

        //半透明にする
        Color color = GetComponent<SpriteRenderer>().color;

        color.a = 0.25f;

        GetComponent<SpriteRenderer>().color = color;
    }

   
    void Update()
    {
        //自動移動モードの時は操作不要
        if (isAutoMode) return;

        delta += Time.deltaTime;

        //左右入力値
        //Input.GetAxisRaw("Horizontal")はA / D,左右キーの入力値を取得する
        float moveX = Input.GetAxisRaw("Horizontal");

        //上下入力値
        //Input.GetAxisRaw("Vertical")はW / S.上下キーの入力値を取得する
        float moveY = Input.GetAxisRaw("Vertical");

        //現在の位置から毎フレームごと同じ速度で移動する
        transform.Translate(moveX * speed * Time.deltaTime, moveY * speed * Time.deltaTime, 0);

        //左右移動アニメーション
        //アニメーターにMoveHという値を渡すことで、moveXの値が変わるとアニメーションも切り替えられる
        _animator.SetFloat("MoveH", moveX);

        //弾を発射　
        //左Ctrl(Macはcontrolキー及びマウス左クリック及びZキー)
         if (Input.GetButton("Fire1") || Input.GetKey(KeyCode.Z))
        {
            if (delta > shotInterval)
          {
             //ショットレベルごとに弾と発射位置を変更
             switch(shotLevel)
                {
                    //ショットレベル0
                    case 0:
                        //弾をsingleshotの位置と向きで生成
                        Instantiate(bulletPrefab, singleShot.position, singleShot.rotation);
                        break;

                    //ショットレベル1
                    case 1:
                        //弾をlasershotの位置と向きで生成
                        Instantiate(laserPrefab, laserShotR.position, laserShotR.rotation);
                        Instantiate(laserPrefab, laserShotL.position, laserShotL.rotation);
                        break;

                    //ショットレベル2
                    case 2:
                        //弾をlasershotの位置と向きで生成
                        Instantiate(laserPrefab, laserShotR.position, laserShotR.rotation);
                        Instantiate(laserPrefab, laserShotL.position, laserShotL.rotation);

                        //弾をtripleshotの位置と向きで生成
                        Instantiate(bulletPrefab, tripleShotR.position, tripleShotR.rotation);
                        Instantiate(bulletPrefab, tripleShotL.position, tripleShotL.rotation);
                        break;

                    //ショットレベル３
                    case 3:
                        //弾をlasershotの位置と向きで生成
                        Instantiate(laserPrefab, laserShotR.position, laserShotR.rotation);
                        Instantiate(laserPrefab, laserShotL.position, laserShotL.rotation);

                        //弾をtripleshotの位置と向きで生成
                        Instantiate(bulletPrefab, tripleShotR.position, tripleShotR.rotation);
                        Instantiate(bulletPrefab, tripleShotL.position, tripleShotL.rotation);

                        //弾をQuintupleShotの位置と向きで生成
                        Instantiate(bulletPrefab, quintupleShotR.position, quintupleShotR.rotation);
                        Instantiate(bulletPrefab, quintupleShotL.position, quintupleShotL.rotation);

                        break;
                }


                delta = 0;

                //オプションにも攻撃させる
                if(options.Count > 0)
                {
                    foreach (GameObject option in options)
                    {
                        option.GetComponent<Option>().Shot();
                    }
                }

                //弾を発射した時のSE
                string seName = SoundData.SeType.shot.ToString();

                GSound.Instance.PlaySe(seName);
          }
        }

         //自動モードの時は画面制限をかけない
        if (isAutoMode == false)
        {

            //画面外にでないように制限
            Vector3 playerPos = transform.position;
            //X（左右）の位置を -worldSize.xからworldSize.x の範囲に制限
            //Mathf.Clamp(value,min,max)は、valueがminより小さいとminを返し、valueがmaxより大きいとmaxを返す
            playerPos.x = Mathf.Clamp(playerPos.x, -worldSize.x, worldSize.x);
            playerPos.y = Mathf.Clamp(playerPos.y, -worldSize.y, worldSize.y);
            transform.position = playerPos;

        }
    }

    //一定間隔で呼ばれるUpdate
    private void FixedUpdate()
    {
        //プレイヤの位置履歴を毎フレーム取得
        //1フレーム前から移動している場合のみ履歴に保存
        if(transform .position != beforePlayerPos)
        {
            //プレイヤの位置を履歴に保存
            //Enqueueでキューにデータを追加
            playerPosHistory.Enqueue(transform.position);

            //プレイヤの位置を格納して1フレーム前のプレイヤの位置として使用
            beforePlayerPos = transform.position;
        }

        //最大履歴数を超えた場合は古い履歴から削除
        if(playerPosHistory .Count > maxHistoryCount)
        {
            //Dequeueで古いほうからデータを取り出す
            playerPosHistory.Dequeue();
        }
    }




    //敵と触れたら自分を破壊する
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Enemy" || other.gameObject.tag== "EnemyBullet")
        {
            //無敵モードの場合
            if (isInvincible) return;

            Destroy(gameObject);

            //爆弾エフェクトを作成
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);

            //プレイヤーが消滅したときのSE
            string seName = SoundData.SeType.die.ToString();

            GSound.Instance.PlaySe(seName);

            //残機確認
            GameObject.Find("GameDirector").GetComponent<GameDirector>().CreatePlayer();

            //置き去りオプションをアイテムに変換
            ConvertOptionToItem();
        }

        //アイテム取得
        else if(other.gameObject .tag == "Item")
        {
            //Itemスクリプト取得
            Item item = other.GetComponent<Item>();

            if (item != null)
            {
                //アイテム種類ごとの処理
                switch (item.itemtype)
                {
                    //ショットパワーアップ
                    case Item.ItemType.powerUp:
                        //アイテムを取得した時のSE
                        string seName = SoundData.SeType.item1.ToString();

                        GSound.Instance.PlaySe(seName);
                        itemCount++;

                        if(itemCount >= powarUpInterval)
                        {
                            shotLevel++;

                            shotLevel = Mathf.Clamp(shotLevel, 0, 3);

                            itemCount = 0;
                        }

                        speedUpCount++;

                        if(speedUpCount  == 10)
                        {
                            speed += 1;

                            speedUpCount = 0;
                        }
                        //shotLevel++;
                      //  shotLevel = Mathf.Clamp(shotLevel, 0, 2);
                        break;

                    //オプション追加
                    case Item.ItemType.option:
                        //アイテムを取得した時のSE
                       string se2Name = SoundData.SeType.item2.ToString();

                        GSound.Instance.PlaySe(se2Name);

                        //オプション生成
                        if (options.Count < 5)
                        {
                            GameObject option = Instantiate(optionPrefab, transform.position, Quaternion.identity);

                            //オプションがプレイヤから離れて追従するフレーム数
                            int delayFrame = option.GetComponent<Option>().delayFrame;

                            option.GetComponent<Option>().delayFrame = delayFrame + options.Count * delayFrame;

                            //オプションの数を増やす
                            options.Add(option);
                        }

                        break;
                }

                //アイテムを取得した時のSE
                //string seName = SoundData.SeType.item1.ToString();

              // GSound.Instance.PlaySe(seName);
            }
        }

    }

    //自動移動モード
    void AutoMoveMode(Vector2 goalPos, float duration, Ease ease)
    {

        //自動モード開始
        isAutoMode = true;

        //DOTween処理をまとめるシークエンス
        Sequence sequence = DOTween.Sequence();

        //goalPos座標までduration時間かけて移動
        sequence.Join(transform.DOMove(goalPos, duration)
            .SetDelay(1f)
            .SetEase(ease)
            .OnComplete(() => {

                //自動モード終了
                isAutoMode = false;

            }));

        sequence.Append(GetComponent<SpriteRenderer>().DOColor(new Color(0, 0, 0, 0), 1f)
            .SetEase(Ease.Flash, 16)
            .OnComplete(() => {

                //無敵モード終了
                isInvincible = false;

                //半透明を元に戻す
                Color color = GetComponent<SpriteRenderer>().color;

                color.a = 1f;

                GetComponent<SpriteRenderer>().color = color;

            }));

        sequence.SetLink(gameObject).Play();
    }

    //置き去りにされたオプションをアイテムに変換
    void ConvertOptionToItem()
    {
        //取得済みオプションの数だけオプションアイテムを生成
        foreach (GameObject option in options)
        {
            Instantiate(optionItemPrefab, option.transform.position, Quaternion.identity);

            //アイテム生成後はオプション本体は削除
            Destroy(option);
        }

        //リスト内をクリア
        options.Clear();
    }
}
