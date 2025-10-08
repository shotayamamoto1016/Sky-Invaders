using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Threading;

public class Wave : MonoBehaviour
{
    //�G�X�|�[���̏��N���X
    //�J�X�^���N���X��\�����邽�߂Ɏg�p
    [System.Serializable]
    public class EnemySpawn
    {
        //�o������ꏊ�I�u�W�F�N�g
        public Transform spawnPos;

        //�o������G�̎��
        public GameObject enemyPrefab;

        //�o������܂ł̑ҋ@����
        public float delay;

        //�A�C�e���h���b�v�Ώ�
        public bool itemDrop;           
    }

    //Wave�̏ڍא���
    [SerializeField] string detail;
    //�G�X�|�[����񃊃X�g
    [SerializeField] EnemySpawn[] enemySpawns;

    //�L�����Z���g�N�[��
     CancellationTokenSource cancelToken;

   // private CancellationToken token;

    
   async void Start()
    {
        //�L�����Z���g�[�N���̐��� 
         cancelToken = new CancellationTokenSource();

        //�L�����Z���g�[�N���̎擾
         CancellationToken token = cancelToken.Token;

        // GameObject��Destroy���ꂽ�玩���L�����Z�������g�[�N�����擾
       // token = this.GetCancellationTokenOnDestroy();

        //�G�X�|�[�����X�g����G���X�|�[��������
        foreach (EnemySpawn spawn in enemySpawns)
        {
            Spawn(spawn);
        }

        //Wave�̎q�v�f��Enemy���S�č폜�����܂őҋ@
        try
        {
            //Wave�̎q�v�f��Enemy���S�č폜�����܂őҋ@
            //�q�I�u�W�F�N�g��0���ǂ������肷��
            await UniTask.WaitUntil(() => transform.childCount == 0, PlayerLoopTiming.Update, token);
        }
        catch (System.OperationCanceledException e)
        {
            Debug.Log($"Wave�q�v�f��0�ɂȂ�܂ł̑ҋ@�������L�����Z������܂��� >> " + e);

            cancelToken.Cancel();

            //������Start���I��点��
            return;
        }

        //Wave�̍폜
        Destroy(gameObject);
    }

    //�G���X�|�[��������
    async void Spawn(EnemySpawn spawn)
    {
        //Delay���ԑҋ@
        await UniTask.Delay((int)(spawn.delay * 1000f));
        //�G����
        GameObject enemy = Instantiate(spawn.enemyPrefab, spawn.spawnPos.position, Quaternion.identity);
        //���������G���q�v�f�ɒǉ�
        //���̓G�𐶐����Ǘ��p�e�I�u�W�F�N�g�ɓ����
        if(enemy != null)
        {
            enemy.transform.parent = transform;
        }

        //�A�C�e���h���b�v�t���O���Z�b�g
        enemy.GetComponent<EnemyController>().itemDrop = spawn.itemDrop;

        if (spawn.spawnPos != null)
        {
            //�X�|�[���ꏊ�I�u�W�F�N�g���폜
            Destroy(spawn.spawnPos.gameObject);
        }
        
       
    }

    //�j�����ꂽ���Ɏ����I�ɌĂ΂��
    void OnDestroy()
    {
        if (cancelToken != null)
        {
            cancelToken.Cancel();
        }
    }

    //�I�u�W�F�N�g����A�N�e�B�u�ɂȂ����Ƃ�
    private void OnDisable()
    {
        if (cancelToken != null)
        {
            cancelToken.Cancel();
        }
    }

    //�A�v�����I�������Ƃ�
    private void OnApplicationQuit()
    {
        if (cancelToken != null)
        {
            cancelToken.Cancel();
        }
    }


}
