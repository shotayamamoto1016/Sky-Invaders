using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// �Q�[���S�̂��Ǘ�
/// </summary>
public class GameDirector : MonoBehaviour
{
    //�e�X�e�[�W���
    [SerializeField] List<StageData> stageDatas = new List<StageData>();

    //���݂̃X�e�[�W��
    int stageNum = 0;
    //���݂̃X�e�[�W�f�[�^
    StageData stageData;

    //�L�����Z���g�N�[��
     CancellationTokenSource cancelToken;

    //�X�R�A�Ǘ�
    int totalScore;

    [SerializeField] TextMeshProUGUI ScoreText;
    //BGM��
    string bgmName;

    //�l���X�R�A�𕪊����Ĉꎞ�ҋ@������L���[
    Queue<int> scoreQueue = new Queue<int>();

    //�v���C���[�̎c�@
    int playerCount = 3;

    //�v���C���[�v���n�u
    [SerializeField] GameObject playerPrefab;

    //�v���C���[�o���ꏊ
    [SerializeField] Transform playerSpawn;

    //�c�@UI�\��
    [SerializeField] Transform playerCountUI;

    [SerializeField] GameObject iconPrefab;

    //�Q�[���I�o�[UI
    [SerializeField] GameObject gameOverUI;

    //�Q�[���I�o�[�t���O
    bool gameOverFlg;

    //�w�i�I�u�W�F�N�g
    //GameObject background;


    async void Start()
    {
        //�Q�[���I�o�[��\��
        gameOverUI.SetActive(false);


        //�v���C���[����
        CreatePlayer();

        //�L�����Z���g�[�N���̐��� 
         cancelToken = new CancellationTokenSource();

        //�L�����Z���g�[�N���̎擾
        CancellationToken token = cancelToken.Token;

        //�X�R�A�̏�����
        ScoreText.text = totalScore.ToString("D0");

        //�ێ�����Ă����X�e�[�W�������Z�b�g
        stageNum = MyPlayer.Instance.STAGE_NUM;


        //���ׂẴX�e�[�W��ǂݍ��ނ܂ŌJ��Ԃ�
        while (stageDatas.Count > stageNum)
        {
            Debug.Log("While��");
            try
            {
                //�X�e�[�W�ǂݍ��݊J�n
                await StageStart(token);
            }

            //Token�ɂ����await���Ă���^�X�N���L�����Z�����ꂽ�ꍇ�̗�O����
            catch (System.OperationCanceledException e)
            {
                Debug.Log($"�X�e�[�W�X�^�[�g���L�����Z������܂��� >> " + e);



                //while���[�v�𔲂���
                break;
            }
        }

        //�Q�[���I�[�o�[���͂����ŏ������I��点��
        if (gameOverFlg) return;

        // await StageStart(token);
        Debug.Log("���ׂẴX�e�[�W���N���A!");


        SceneManager.LoadScene("03_Ending");
    }

    //�X�e�[�W��wave�ǂݍ��݊J�n
    async UniTask StageStart(CancellationToken token)
    {



        //���݂̃X�e�[�W�f�[�^�擾
        stageData = stageDatas[stageNum];


        ////�O�̔w�i������
        //if(background != null)
        //{
        //    Destroy(background);
        //}

        ////�w�i����
        //background = Instantiate(stageData.background, transform);

        //���݂̃X�e�[�W�f�[�^����G���X�|�[��������
        foreach (StageData.WaveInfo waveInfo in stageData.stage)
        {

            //Delay���ԑҋ@
            await UniTask.Delay((int)(waveInfo.delay * 1000f), false, PlayerLoopTiming.Update, token);
            //try
            //{
            //    //Delay���ԑҋ@
            //    //Update�̃^�C�~���O�ōĊJ���L�����Z���g�[�N������L�����Z�������ŗ����瑦���f�\
            //    await UniTask.Delay((int)(waveInfo.delay * 1000f), false, PlayerLoopTiming.Update, token);

            //}

            //catch(System.OperationCanceledException e) 
            //{
            //    Debug.Log("Delay���Ԃ̑ҋ@�������L�����Z������܂����@>> " + e);

            //    //UniTask�̒�~
            //    cancelToken.Cancel();

            //    //foreach�̃��[�v�������I��点��
            //    break;
            //}
            //Wave����
            GameObject wave = Instantiate(waveInfo.wavePrefab);

            //�X�e�[�W�J�nWave�̏ꍇ
            if (waveInfo.waveType == StageData.WaveType.start)
            {
                //�X�e�[�W�����Z�b�g
                WaveStart waveStart = wave.GetComponent<WaveStart>();
                waveStart.SetStageNum(stageNum + 1);

                //�X�e�[�WBGM
                bgmName = SoundData.BgmType.start.ToString();

                GSound.Instance.PlayBgm(bgmName, true);
            }

            //�X�e�[�W�N���A��Wave�̏ꍇ
            else if (waveInfo.waveType == StageData.WaveType.clear)
            {
                //�X�e�[�W������i�߂�
                stageNum++;

                MyPlayer.Instance.STAGE_NUM = stageNum;

                //�N���ABGM
                bgmName = SoundData.BgmType.clear.ToString();

                GSound.Instance.PlayBgm(bgmName, false);
            }

            //�E�F�[�u�G���f�B���O�̏ꍇ
            else if (waveInfo.waveType == StageData.WaveType.ending)
            {
                //�X�R�A���Z�b�g
                WaveEnding waveEnding = wave.GetComponent<WaveEnding>();

                waveEnding.SetScore(totalScore);

                //�X�e�[�W����i�߂�
                stageNum++;

                MyPlayer.Instance.STAGE_NUM = stageNum;

                //�G���f�B���OBGM
                bgmName = SoundData.BgmType.clear.ToString();

                GSound.Instance.PlayBgm(bgmName, false);
            }

            //�{�XWave�̏ꍇ
            else if(waveInfo .waveType == StageData.WaveType.boss)
            {
                //�{�XBGM
                bgmName = SoundData.BgmType.boss.ToString();

                GSound.Instance.PlayBgm(bgmName, true);
            }

            //���ׂĂ̓G��|���Ȃ��Ǝ��ւ����Ȃ��ꍇ
            if (waveInfo.completFlg)
            {
                try
                {
                    //���݂�Wave���j�������܂őҋ@
                    await UniTask.WaitUntil(() => wave == null, PlayerLoopTiming.Update, token);
                }
                catch (System.OperationCanceledException e)
                {
                    Debug.Log("Wave�j���܂ł̑ҋ@�������L�����Z������܂���" + e);

                    //UniTask�̒�~
                    cancelToken.Cancel();

                    //foreach�̃��[�v�������I��点��
                    break;
                }
            }
        }
    
    }

    //�X�R�A���Z�b�g
    public void AddScore(int score)
    {
        totalScore += score;

        ScoreText.text = totalScore.ToString("D0");

        int splitScore = score / 50;

        //���������X�R�A������L���[�ɒǉ�
        while(score > 0)
        {
            scoreQueue.Enqueue(splitScore);

            score -= splitScore;

            //�����ōŌ�̗]�������
            if(score < splitScore)
            {
                scoreQueue.Enqueue(score);

                score = 0;
            }
        }
    }

    //Update��ɏ��������Update
    private void LateUpdate()
    {
        //�L���[�ɒǉ�����Ă��関�ǉ��X�R�A���`�F�b�N
        CheckScoreQueue();

        //SE�f�[�^�������1�t���[�����ƂɂP��SE��炷
        GSound.Instance.CheckSeQueue();
    }

    //�L���[�Ɋi�[���ꂽ�����X�R�A������X�R�A�\�L
    private void CheckScoreQueue()
    {
        //�L���[�Ƀf�[�^������Έ�����Ăяo���ď�������
        if(scoreQueue .Count > 0)
        {
            //�L���[���番���X�R�A�����o��
            int s = scoreQueue.Dequeue();

            //���o���������X�R�A���g�[�^���X�R�A�֒ǉ�
            totalScore += s;

            //3�����ƂɃJ���}����ꂽ�\�L
            ScoreText.text = totalScore.ToString("D0");
        }
    }

    
    //�v���C���[�̎c�@�Ǘ�
    public void CreatePlayer()
    {
        //�c�@���c���Ă���ꍇ
        if(playerCount > 0)
        {
            playerCount--;

            //�v���C������
            //Instantiate(playerPrefab);

            Instantiate(playerPrefab, playerSpawn.position, Quaternion.identity);

            //�c�@UI�X�V
            //��U���ׂẴA�C�R�����폜
            foreach (Transform icon in playerCountUI)
            {
                Destroy(icon.gameObject);
            }

            //�c�@�̐������A�C�R���𐶐�
            for (int i = 0; i < playerCount; i++)
            {
                Instantiate(iconPrefab, playerCountUI);
            }
        }

        //�c�@0�ŃQ�[���I�o�[
        else
        {
            gameOverUI.SetActive(true);

            gameOverFlg = true;
        }

            Debug.Log($"�v���C���[�c�@�@>> {playerCount}");
    }


    //���g���C�{�^��
    public void OnPressRetryButton()
    {
        //UniTask�̒�~
        cancelToken.Cancel();

        SceneManager.LoadScene("02_Game");
    }

    //�^�C�g���֖߂�{�^��
    public void OnPressTitleButton()
    {
        //UniTask�̒�~
        cancelToken.Cancel();

        //�X�e�[�W�����Z�b�g
        MyPlayer.Instance.STAGE_NUM = 0;

        SceneManager.LoadScene("00_Title");
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
