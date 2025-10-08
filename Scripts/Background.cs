using NUnit.Framework;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Background : MonoBehaviour
{
    [SerializeField] float speed = 1;

    //List<Transform>は複数のTransformを格納するための入れ物
    //背景オブジェクトのTransformを格納するからリストを作成
    List<Transform> backgrounds = new List<Transform>();

    //子オブジェクトのスプライトサイズ
    Vector3 spriteSize;

    //リスト内を入れ替えたときに子オブジェクトを一時格納
　　//cacheObjは、リスト要素を入れ替えるときの一時保管場所
    Transform cacheObj;

    
    void Start()
    {
        //foreachは、コレクションの要素を順番にとりだす
        //foreachを使うと自動的に子のTransformを0番目から順番に取り出してくれる。
        foreach (Transform child in transform)
        {
            //backgroundsにchild(子オブジェクトのTransform)を追加
            backgrounds.Add(child);
        }

        //スプライトサイズの取得
        //backgrounds[0]は、一番目の背景オブジェクトのTransform(ここではBottom)
        //sprite.bounds.sizeは、スプライト(画像)の実際の大きさを表す
        spriteSize = backgrounds[0].GetComponent<SpriteRenderer>().sprite.bounds.size;

        //$" "はC#の文字列補間機能で、{spriteSize}の部分が変数に入れ替わる
        Debug.Log($"spriteSize >> {spriteSize}");

        //子オブジェクトを隙間なく並べる
        //backgrounds.Countは、背景リストの要素数
        for (int i = 0; i < backgrounds.Count-1; i++)
        {
            //次の子オブジェクトを自分の後にピッタリ配置する
            //new Vector3(0, spriteSize.y, 0);は、Y方向の背景の高さの分だけ上に移動する
            //i番目の背景を、i+1番目の背景の位置にもっていく
            backgrounds[i + 1].position = backgrounds[i].position + new Vector3(0, spriteSize.y, 0);
        }
    }

   
    void Update()
    {
        //child(個々の背景オブジェクト)をリスト化したものを順番に取り出す
        //childを移動させる
        foreach (Transform child in backgrounds)
        {
            child.Translate(0, -speed * Time.deltaTime, 0);

            //画面の外まで移動した
            //child.position.yは、子オブジェクトの現在のY座標
            //-spriteSize.yは、背景の高さの分だけ下に移動すること
            if (child.position.y < -spriteSize.y)
            {
                //入れ替えを対象としてキャッシュへ格納
                cacheObj = child;
            }
        }

        //画面の並び入れ替え
        //再配置する背景がある場合に実行
        if(cacheObj != null)
        {
            //現在最後に配置されている背景画像の座標を取得
            //backgrounds[backgrounds.Count - 1]は、一番上にある背景(リストの一番下のオブジェクト)
            Vector3 lastPos = backgrounds[backgrounds.Count - 1].position;

            //画面外まで移動したので一番後へ入れ替え
            //lastPosのY方向に背景の高さの分だけ上に移動した位置を計算する
            cacheObj.position = lastPos + new Vector3(0, spriteSize.y, 0);

            //配列内を入れ替える
            //cacheobjをbackgroundsのリストの最後にもっていく
            backgrounds.Add(cacheObj);

            //リストの先頭要素(古い位置の背景)を削除する
            backgrounds.RemoveAt(0);

            //cacheObjを空にすることで、次のフレームが来たときまたは別の背景が画面外に出たときに再利用する
            cacheObj = null;
        }
    }
}
