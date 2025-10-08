using Cysharp.Threading.Tasks;  
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;


public class EnemyController : MonoBehaviour


{
    //�ړ����x
    [SerializeField] float speed = 1.0f;

    //HP
    [SerializeField] protected  int maxHp = 5;

    protected int currentHp;

    //���e�G�t�F�N�g�v���n�u
    [SerializeField] GameObject explosionPrefab;

    [SerializeField] float explosionSize = 0.5f;

    //�_���[�W���ɃX�v���C�g�̐F��ύX���邽�߂Ɏ擾
    SpriteRenderer spriteRenderer;

    //�_���[�W���t���O
    bool isDamage;

    [SerializeField] int score;

    //�t���C�X�R�A�e�L�X�g
    [SerializeField] GameObject flyScorePrefab;

    [SerializeField] GameObject itemPrefab;

    //�A�C�e���o���t���O
    public bool itemDrop;

    //�e�𔭎�(�e�̃X�s�[�h�𒲐�)
    //protected�̓N���X���g�܂��͌p�������N���X����A�N�Z�X�\
    //virtual�T�u�N���X��override���邱�Ƃŏ������㏑���ł���
    protected virtual void Shot(List<Transform > shotPositions, GameObject bullet , float speedRate = 1)
    {
        foreach (Transform shotPos in shotPositions)
        {
            GameObject bulletObj = Instantiate(bullet, shotPos.position, shotPos.rotation);

            bulletObj.GetComponent<EnemyBullet>().SpeedRate(speedRate);
        }
        
    }

    //���C���J�����ɕt���Ă���^�O��
     const string MAIN_CAMERA_TAG = "MainCamera";

    //�J�����ɉf���Ă��邩���f����
     protected bool isRendered = false;

    //�J�����^�̕ϐ�
     Camera mainCamera;

    //private CancellationToken token;

    //�L�����Z���g�[�N��
 // CancellationTokenSource cancelToken;

    protected virtual void Start()
    {
        mainCamera = Camera.main;

        //HP��������
        currentHp = maxHp;

        //�_���[�W���ɃX�v���C�g�̐F��ω������邽�߂Ɏ擾
        spriteRenderer = GetComponent<SpriteRenderer>();

        //token = this.GetCancellationTokenOnDestroy();

        //�L�����Z���g�[�N���̐��� 
        //cancelToken = new CancellationTokenSource();
               //CancellationToken�̎擾  
       // CancellationToken token = cancelToken.Token;
    }

    //�ړ�
    protected virtual void Move(Vector3 moveDir)
    {
        transform.position += moveDir * speed * Time.deltaTime;
    }

    //�e�𔭎�
    //�I�[�o�[���[�h(�������O�̃��\�b�h�ł������̎�ނ␔���Ⴆ�Εʂ̃��\�b�h�Ƃ��Ē�`�ł���)�𗘗p���Ă���
    protected virtual void Shot(List<Transform> shotPositions, GameObject bullet)
    {
        foreach (Transform shotPos in shotPositions)
        {
            Instantiate(bullet, shotPos.position, shotPos.rotation);
        }
       
    }

    //�Ȃɂ��ƐG�ꂽ��
    private void OnTriggerEnter2D(Collider2D other)
    {
        //�J�����ɉf���Ă��Ȃ��ꍇ�͓����蔻�薳��
        if (isRendered == false) return;

        if (other.gameObject.tag == "PlayerBullet")
        {
            //�_���[�W���͖��G����
            if (isDamage) return;

            //�_���[�W����
            Damage(1);
        }
    }

    //�_���[�W���󂯂�HP�����炷
    protected virtual async void Damage(int d)
    {
        //HP�����炷
        currentHp -= d;

        //HP0�ŏ���
        if(currentHp <= 0)
        {
            Destroy(gameObject);

            //���e�G�t�F�N�g
            GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);

            //���e�G�t�F�N�g�̃T�C�Y��ύX
            explosion.GetComponent<Explosion>().SetSize(explosionSize);

            GameObject.Find("GameDirector").GetComponent<GameDirector>().AddScore(score);

            //�X�R�A�̃t���C�e�L�X�g����
            GameObject flyScore = Instantiate(flyScorePrefab, transform.position, Quaternion.identity);

            //�X�R�A�l���Z�b�g
            flyScore.GetComponent<FlyText>().SetText(score.ToString());

            //�h���b�v�A�C�e��
            if(itemPrefab != null && itemDrop == true)
            {
                Instantiate(itemPrefab, transform.position, Quaternion.identity);
            }
        }

        //�_���[�W������
        else
        {
            //���݂̃X�v���C�g�̐F��ێ�
            Color currentColor = spriteRenderer.color;

            //�X�v���C�g�̐F��Ԃɂ���
            spriteRenderer.color = Color.red;

            //�_���[�W���̃t���O���Z�b�g
            isDamage = true;

            //0.05�b�҂�
            await UniTask.Delay((int)(0.05f * 1000f));

            //�X�v���C�g�̐F��߂�
            spriteRenderer.color = currentColor;

            //�t���O������
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
