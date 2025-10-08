using NUnit.Framework;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Background : MonoBehaviour
{
    [SerializeField] float speed = 1;

    //List<Transform>�͕�����Transform���i�[���邽�߂̓��ꕨ
    //�w�i�I�u�W�F�N�g��Transform���i�[���邩�烊�X�g���쐬
    List<Transform> backgrounds = new List<Transform>();

    //�q�I�u�W�F�N�g�̃X�v���C�g�T�C�Y
    Vector3 spriteSize;

    //���X�g�������ւ����Ƃ��Ɏq�I�u�W�F�N�g���ꎞ�i�[
�@�@//cacheObj�́A���X�g�v�f�����ւ���Ƃ��̈ꎞ�ۊǏꏊ
    Transform cacheObj;

    
    void Start()
    {
        //foreach�́A�R���N�V�����̗v�f�����ԂɂƂ肾��
        //foreach���g���Ǝ����I�Ɏq��Transform��0�Ԗڂ��珇�ԂɎ��o���Ă����B
        foreach (Transform child in transform)
        {
            //backgrounds��child(�q�I�u�W�F�N�g��Transform)��ǉ�
            backgrounds.Add(child);
        }

        //�X�v���C�g�T�C�Y�̎擾
        //backgrounds[0]�́A��Ԗڂ̔w�i�I�u�W�F�N�g��Transform(�����ł�Bottom)
        //sprite.bounds.size�́A�X�v���C�g(�摜)�̎��ۂ̑傫����\��
        spriteSize = backgrounds[0].GetComponent<SpriteRenderer>().sprite.bounds.size;

        //$" "��C#�̕������ԋ@�\�ŁA{spriteSize}�̕������ϐ��ɓ���ւ��
        Debug.Log($"spriteSize >> {spriteSize}");

        //�q�I�u�W�F�N�g�����ԂȂ����ׂ�
        //backgrounds.Count�́A�w�i���X�g�̗v�f��
        for (int i = 0; i < backgrounds.Count-1; i++)
        {
            //���̎q�I�u�W�F�N�g�������̌�Ƀs�b�^���z�u����
            //new Vector3(0, spriteSize.y, 0);�́AY�����̔w�i�̍����̕�������Ɉړ�����
            //i�Ԗڂ̔w�i���Ai+1�Ԗڂ̔w�i�̈ʒu�ɂ����Ă���
            backgrounds[i + 1].position = backgrounds[i].position + new Vector3(0, spriteSize.y, 0);
        }
    }

   
    void Update()
    {
        //child(�X�̔w�i�I�u�W�F�N�g)�����X�g���������̂����ԂɎ��o��
        //child���ړ�������
        foreach (Transform child in backgrounds)
        {
            child.Translate(0, -speed * Time.deltaTime, 0);

            //��ʂ̊O�܂ňړ�����
            //child.position.y�́A�q�I�u�W�F�N�g�̌��݂�Y���W
            //-spriteSize.y�́A�w�i�̍����̕��������Ɉړ����邱��
            if (child.position.y < -spriteSize.y)
            {
                //����ւ���ΏۂƂ��ăL���b�V���֊i�[
                cacheObj = child;
            }
        }

        //��ʂ̕��ѓ���ւ�
        //�Ĕz�u����w�i������ꍇ�Ɏ��s
        if(cacheObj != null)
        {
            //���ݍŌ�ɔz�u����Ă���w�i�摜�̍��W���擾
            //backgrounds[backgrounds.Count - 1]�́A��ԏ�ɂ���w�i(���X�g�̈�ԉ��̃I�u�W�F�N�g)
            Vector3 lastPos = backgrounds[backgrounds.Count - 1].position;

            //��ʊO�܂ňړ������̂ň�Ԍ�֓���ւ�
            //lastPos��Y�����ɔw�i�̍����̕�������Ɉړ������ʒu���v�Z����
            cacheObj.position = lastPos + new Vector3(0, spriteSize.y, 0);

            //�z��������ւ���
            //cacheobj��backgrounds�̃��X�g�̍Ō�ɂ����Ă���
            backgrounds.Add(cacheObj);

            //���X�g�̐擪�v�f(�Â��ʒu�̔w�i)���폜����
            backgrounds.RemoveAt(0);

            //cacheObj����ɂ��邱�ƂŁA���̃t���[���������Ƃ��܂��͕ʂ̔w�i����ʊO�ɏo���Ƃ��ɍė��p����
            cacheObj = null;
        }
    }
}
