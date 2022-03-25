using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Tutorial : MonoBehaviour
{
    public GameManager manager;
    public GameObject tutorialPanel;
    public GameObject hand;
    private float moveLength;

    public int state;

    public GameObject step1;
    public GameObject step2;
    public GameObject step3;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI step1Text;
    public TextMeshProUGUI step2Text;
    public TextMeshProUGUI step3Text;

    void OnEnable()
    {
        StartTutorial();
    }

    void Update()
    {
        MoveHand();
    }

    public void StartTutorial()
    {
        tutorialPanel.SetActive(true);
        step1.SetActive(true);
        step2.SetActive(false);
        step3.SetActive(false);
        if (manager.isEnglish)
        {
            titleText.text = "How To Play";
            step1Text.text = "The goal of the game is to sort the mixed colors and unify each funnel into different colors.";
            step2Text.text = "You can sort the colors by dragging the funnel to another funnel. The color at the bottom of the funnel is moved to the top of the other funnel.";
            step3Text.text = "Dragging is only allowed up, down, left and right. You can't do it diagonally.";
        }
        else
        {
            titleText.text = "���� ���";
            step1Text.text = "�� ������ ��ǥ�� �����ִ� ������� �� �����Ͽ� �� �򶧱⸦ ���� �ٸ� ������ �����ϴ� ���Դϴ�.";
            step2Text.text = "�򶧱⸦ �ٸ� �򶧱�� �巡�� �ϸ� ������ ������ �� �ֽ��ϴ�. �̶� �򶧱� �� �Ʒ��� ������ �ٸ� �򶧱��� �� ���� �Ű����ϴ�.";
            step3Text.text = "�巡�״� �����¿�θ� �����մϴ�.�밢�� �������δ� �� �� �����ϴ�.";
        }

        int[] data = { 1, 0, 1, 1, 1, 2, 0, 2, 2, 2, 3, 3, 3, 2, 1, 0, 0, 0, 3, 3 };
        manager.genStageData.Clear();
        for (int i = 0; i < data.Length; i++) manager.genStageData.Add(data[i]);
        manager.stageRow = 2;
        manager.stageCol = 2;
        manager.stageMix = 5;
        manager.playingStage = 0;

        //�������� ��ȣ UI
        manager.stageNumberText.text = "Tutorial";

        manager.SetIngameObject();
        manager.SetIngameStat();

        state = 0;
        moveLength = 0.0f;
    }

    public void ButtonNextTutorial()
    {
        if (step1.activeSelf)
        {
            step1.SetActive(false);
            step2.SetActive(true);
        }
        else if (step2.activeSelf)
        {
            step2.SetActive(false);
            step3.SetActive(true);
        }
        else if (step3.activeSelf)
            tutorialPanel.SetActive(false);
    }

    public void SetHandPosition()
    {
        moveLength = 0.0f;
        switch (state)
        {
            case 0:
            case 1:
                hand.transform.position = manager.funnels[2].originalPosition;
                hand.transform.position += new Vector3(0.3f, -0.3f, 0.0f);
                break;
            case 2:
            case 3:
            case 4:
                hand.transform.position = manager.funnels[3].originalPosition;
                hand.transform.position += new Vector3(0.3f, -0.3f, 0.0f);
                break;
        }
    }

    public void MoveHand()
    {
        switch (state)
        {
            case 0:
            case 4:
                hand.transform.Translate(Vector3.up * Time.deltaTime);
                break;
            case 1:
                hand.transform.Translate(Vector3.right * Time.deltaTime);
                break;
            case 2:
            case 3:
                hand.transform.Translate(Vector3.left * Time.deltaTime);
                break;
        }
        moveLength += Time.deltaTime;
        if (moveLength >= 1.4f) SetHandPosition();
    }
}
