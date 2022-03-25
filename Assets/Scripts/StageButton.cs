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


    //스테이지 시작하는 버튼
    public void ButtonStartStage()
    {
        manager.audio.PlayOneShot(manager.buttonSound);
        if (manager.StartStage(stageNum))
            manager.stageSelectionPanel.SetActive(false);
    }

    //버튼에 달린 별&자물쇠를 스테이지 클리어 정보에 맞게 변경
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
