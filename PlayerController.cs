using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem.LowLevel;
using DG.Tweening;

public class PlayerController : MonoBehaviour
{
    //�ړ����x
    //[SerializeField]��private�ł��C���X�y�N�^�[�ɕ\���E�ۑ��\�ɂ���
    //public�����S�łق��̃X�N���v�g����A�N�Z�X�ł��Ȃ�
    [SerializeField] private float speed = 5f;

   // [SerializeField] private float speedUpInterval = 2f;

    //�e�v���n�u
   [SerializeField] GameObject bulletPrefab;

    [SerializeField] GameObject laserPrefab;

   //�A�ˑ��x
   [SerializeField] float shotInterval = 3.0f;
   float delta;

   //�e�̔��ˈʒu
   [SerializeField] Transform singleShot;

    [SerializeField] Transform laserShotR;

    [SerializeField] Transform laserShotL;

    [SerializeField] Transform tripleShotR;

    [SerializeField] Transform tripleShotL;

    [SerializeField] Transform quintupleShotR;

    [SerializeField] Transform quintupleShotL;

    //�V���b�g���x��
    int shotLevel;

    //�A�C�e���J�E���g�̐錾
    [SerializeField] int itemCount = 0;

    //�p���[�A�b�v�̊Ԋu��錾
    [SerializeField] int powarUpInterval = 5;

    //�X�s�[�h�A�b�v���邽�߂̃J�E���g�錾
    [SerializeField] int speedUpCount = 0;

    //���e�G�t�F�N�g�v���n�u��錾
    [SerializeField] GameObject explosionPrefab;

    //�Q��
    Animator _animator;

    //��ʐ�����
    Vector3 screenSize;
    Vector3 worldSize;

    //�v���C���̈ʒu����
    public Queue<Vector3> playerPosHistory = new Queue<Vector3>();

    //�ő嗚��ۑ���
    int maxHistoryCount = 300;

    //�ЂƂO�̃t���[���̈ʒu�ۑ�
    Vector3 beforePlayerPos;

    //�I�v�V�����v���n�u
    [SerializeField] GameObject optionPrefab;

    //�ǉ������I�v�V�������i�[���郊�X�g
    List<GameObject> options = new List<GameObject>();

    //�����ړ����[�h
    bool isAutoMode = true;

    //���G���[�h
    bool isInvincible = true;

    //�I�v�V�����v���n�u�A�C�e��
    [SerializeField] GameObject optionItemPrefab;

    void Start()
    {
        //�R���|�[�l���g�擾
        _animator = GetComponent<Animator>();

        //�X�N���[���T�C�Y���s�N�Z�����W�ɕϊ�����
         screenSize = new Vector3(Screen.width, Screen.height, 0);
        //�s�N�Z�����W�����[���h���W�֕ϊ�
        //ScreenToWorldPoint�Ƃ̓J���������Ă���X�N���[��(�s�N�Z��)��̍��W���A�Q�[�����̃��[���h��Ԃɕϊ�����֐��B
         worldSize = Camera.main.ScreenToWorldPoint(screenSize);

        //�o�����o
        Vector2 goalPos = new Vector2(0, -worldSize.y * 0.8f);

        AutoMoveMode(goalPos, 2f, Ease.OutBack);

        //�������ɂ���
        Color color = GetComponent<SpriteRenderer>().color;

        color.a = 0.25f;

        GetComponent<SpriteRenderer>().color = color;
    }

   
    void Update()
    {
        //�����ړ����[�h�̎��͑���s�v
        if (isAutoMode) return;

        delta += Time.deltaTime;

        //���E���͒l
        //Input.GetAxisRaw("Horizontal")��A / D,���E�L�[�̓��͒l���擾����
        float moveX = Input.GetAxisRaw("Horizontal");

        //�㉺���͒l
        //Input.GetAxisRaw("Vertical")��W / S.�㉺�L�[�̓��͒l���擾����
        float moveY = Input.GetAxisRaw("Vertical");

        //���݂̈ʒu���疈�t���[�����Ɠ������x�ňړ�����
        transform.Translate(moveX * speed * Time.deltaTime, moveY * speed * Time.deltaTime, 0);

        //���E�ړ��A�j���[�V����
        //�A�j���[�^�[��MoveH�Ƃ����l��n�����ƂŁAmoveX�̒l���ς��ƃA�j���[�V�������؂�ւ�����
        _animator.SetFloat("MoveH", moveX);

        //�e�𔭎ˁ@
        //��Ctrl(Mac��control�L�[�y�у}�E�X���N���b�N�y��Z�L�[)
         if (Input.GetButton("Fire1") || Input.GetKey(KeyCode.Z))
        {
            if (delta > shotInterval)
          {
             //�V���b�g���x�����Ƃɒe�Ɣ��ˈʒu��ύX
             switch(shotLevel)
                {
                    //�V���b�g���x��0
                    case 0:
                        //�e��singleshot�̈ʒu�ƌ����Ő���
                        Instantiate(bulletPrefab, singleShot.position, singleShot.rotation);
                        break;

                    //�V���b�g���x��1
                    case 1:
                        //�e��lasershot�̈ʒu�ƌ����Ő���
                        Instantiate(laserPrefab, laserShotR.position, laserShotR.rotation);
                        Instantiate(laserPrefab, laserShotL.position, laserShotL.rotation);
                        break;

                    //�V���b�g���x��2
                    case 2:
                        //�e��lasershot�̈ʒu�ƌ����Ő���
                        Instantiate(laserPrefab, laserShotR.position, laserShotR.rotation);
                        Instantiate(laserPrefab, laserShotL.position, laserShotL.rotation);

                        //�e��tripleshot�̈ʒu�ƌ����Ő���
                        Instantiate(bulletPrefab, tripleShotR.position, tripleShotR.rotation);
                        Instantiate(bulletPrefab, tripleShotL.position, tripleShotL.rotation);
                        break;

                    //�V���b�g���x���R
                    case 3:
                        //�e��lasershot�̈ʒu�ƌ����Ő���
                        Instantiate(laserPrefab, laserShotR.position, laserShotR.rotation);
                        Instantiate(laserPrefab, laserShotL.position, laserShotL.rotation);

                        //�e��tripleshot�̈ʒu�ƌ����Ő���
                        Instantiate(bulletPrefab, tripleShotR.position, tripleShotR.rotation);
                        Instantiate(bulletPrefab, tripleShotL.position, tripleShotL.rotation);

                        //�e��QuintupleShot�̈ʒu�ƌ����Ő���
                        Instantiate(bulletPrefab, quintupleShotR.position, quintupleShotR.rotation);
                        Instantiate(bulletPrefab, quintupleShotL.position, quintupleShotL.rotation);

                        break;
                }


                delta = 0;

                //�I�v�V�����ɂ��U��������
                if(options.Count > 0)
                {
                    foreach (GameObject option in options)
                    {
                        option.GetComponent<Option>().Shot();
                    }
                }

                //�e�𔭎˂�������SE
                string seName = SoundData.SeType.shot.ToString();

                GSound.Instance.PlaySe(seName);
          }
        }

         //�������[�h�̎��͉�ʐ����������Ȃ�
        if (isAutoMode == false)
        {

            //��ʊO�ɂłȂ��悤�ɐ���
            Vector3 playerPos = transform.position;
            //X�i���E�j�̈ʒu�� -worldSize.x����worldSize.x �͈̔͂ɐ���
            //Mathf.Clamp(value,min,max)�́Avalue��min��菬������min��Ԃ��Avalue��max���傫����max��Ԃ�
            playerPos.x = Mathf.Clamp(playerPos.x, -worldSize.x, worldSize.x);
            playerPos.y = Mathf.Clamp(playerPos.y, -worldSize.y, worldSize.y);
            transform.position = playerPos;

        }
    }

    //���Ԋu�ŌĂ΂��Update
    private void FixedUpdate()
    {
        //�v���C���̈ʒu�����𖈃t���[���擾
        //1�t���[���O����ړ����Ă���ꍇ�̂ݗ����ɕۑ�
        if(transform .position != beforePlayerPos)
        {
            //�v���C���̈ʒu�𗚗��ɕۑ�
            //Enqueue�ŃL���[�Ƀf�[�^��ǉ�
            playerPosHistory.Enqueue(transform.position);

            //�v���C���̈ʒu���i�[����1�t���[���O�̃v���C���̈ʒu�Ƃ��Ďg�p
            beforePlayerPos = transform.position;
        }

        //�ő嗚�𐔂𒴂����ꍇ�͌Â���������폜
        if(playerPosHistory .Count > maxHistoryCount)
        {
            //Dequeue�ŌÂ��ق�����f�[�^�����o��
            playerPosHistory.Dequeue();
        }
    }




    //�G�ƐG�ꂽ�玩����j�󂷂�
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Enemy" || other.gameObject.tag== "EnemyBullet")
        {
            //���G���[�h�̏ꍇ
            if (isInvincible) return;

            Destroy(gameObject);

            //���e�G�t�F�N�g���쐬
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);

            //�v���C���[�����ł����Ƃ���SE
            string seName = SoundData.SeType.die.ToString();

            GSound.Instance.PlaySe(seName);

            //�c�@�m�F
            GameObject.Find("GameDirector").GetComponent<GameDirector>().CreatePlayer();

            //�u������I�v�V�������A�C�e���ɕϊ�
            ConvertOptionToItem();
        }

        //�A�C�e���擾
        else if(other.gameObject .tag == "Item")
        {
            //Item�X�N���v�g�擾
            Item item = other.GetComponent<Item>();

            if (item != null)
            {
                //�A�C�e����ނ��Ƃ̏���
                switch (item.itemtype)
                {
                    //�V���b�g�p���[�A�b�v
                    case Item.ItemType.powerUp:
                        //�A�C�e�����擾��������SE
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

                    //�I�v�V�����ǉ�
                    case Item.ItemType.option:
                        //�A�C�e�����擾��������SE
                       string se2Name = SoundData.SeType.item2.ToString();

                        GSound.Instance.PlaySe(se2Name);

                        //�I�v�V��������
                        if (options.Count < 5)
                        {
                            GameObject option = Instantiate(optionPrefab, transform.position, Quaternion.identity);

                            //�I�v�V�������v���C�����痣��ĒǏ]����t���[����
                            int delayFrame = option.GetComponent<Option>().delayFrame;

                            option.GetComponent<Option>().delayFrame = delayFrame + options.Count * delayFrame;

                            //�I�v�V�����̐��𑝂₷
                            options.Add(option);
                        }

                        break;
                }

                //�A�C�e�����擾��������SE
                //string seName = SoundData.SeType.item1.ToString();

              // GSound.Instance.PlaySe(seName);
            }
        }

    }

    //�����ړ����[�h
    void AutoMoveMode(Vector2 goalPos, float duration, Ease ease)
    {

        //�������[�h�J�n
        isAutoMode = true;

        //DOTween�������܂Ƃ߂�V�[�N�G���X
        Sequence sequence = DOTween.Sequence();

        //goalPos���W�܂�duration���Ԃ����Ĉړ�
        sequence.Join(transform.DOMove(goalPos, duration)
            .SetDelay(1f)
            .SetEase(ease)
            .OnComplete(() => {

                //�������[�h�I��
                isAutoMode = false;

            }));

        sequence.Append(GetComponent<SpriteRenderer>().DOColor(new Color(0, 0, 0, 0), 1f)
            .SetEase(Ease.Flash, 16)
            .OnComplete(() => {

                //���G���[�h�I��
                isInvincible = false;

                //�����������ɖ߂�
                Color color = GetComponent<SpriteRenderer>().color;

                color.a = 1f;

                GetComponent<SpriteRenderer>().color = color;

            }));

        sequence.SetLink(gameObject).Play();
    }

    //�u������ɂ��ꂽ�I�v�V�������A�C�e���ɕϊ�
    void ConvertOptionToItem()
    {
        //�擾�ς݃I�v�V�����̐������I�v�V�����A�C�e���𐶐�
        foreach (GameObject option in options)
        {
            Instantiate(optionItemPrefab, option.transform.position, Quaternion.identity);

            //�A�C�e��������̓I�v�V�����{�͍̂폜
            Destroy(option);
        }

        //���X�g�����N���A
        options.Clear();
    }
}
