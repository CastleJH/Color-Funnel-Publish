using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StageButton  : MonoBehaviour
{
    public GameManager manager;
    public int stageNum;
    public TextMeshProUGUI numText;

    public GameObject starFrames;
    public Image[] starImages;
    public GameObject locker;


    //�������� �����ϴ� ��ư
    public void ButtonStartStage()
    {
        manager.audio.PlayOneShot(manager.buttonSound);
        if (manager.StartStage(stageNum))
            manager.stageSelectionPanel.SetActive(false);
    }

    //��ư�� �޸� ��&�ڹ��踦 �������� Ŭ���� ������ �°� ����
    public void SetButtonClearStatus(int status)
    {
        switch (status)
        {
            case -1: 
                locker.SetActive(true);
                starFrames.SetActive(false);
                break;
            case 0:
            case 1:
            case 2:
            case 3:
                locker.SetActive(false);
                starFrames.SetActive(true);
                for (int i = 0; i < status; i++) starImages[i].color = Color.yellow;
                for (int i = status; i < 3; i++) starImages[i].color = new Color(0.25f, 0.25f, 0.25f);
                break;
        }
    }

    
}
