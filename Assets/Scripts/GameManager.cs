using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using TMPro;
using System.Runtime.Serialization.Formatters.Binary;

public class GameManager : MonoBehaviour
{
    public bool testFunc;
    public Text debugText;

    public AdManager adManager;
    public Tutorial tutorial;
    public Funnel[] funnels;    //�򶧱�� (4*3��)
    public GameObject[] grids;      //�׸���� (4*3��)
    public SpriteRenderer[] gridHintRenderers;  //��Ʈ�� (4*3��)

    public GameObject homePanel;      //Ȩ ȭ��
    public GameObject stageSelectionPanel;    //�������� ���� ȭ��
    public GameObject ingamePanel;    //�ΰ��� UI
    public GameObject pausePanel;     //�Ͻ����� ȭ��
    public GameObject hintAskPanel;   //��Ʈ<->���� ���� ȭ��
    public GameObject clearPanel;     //Ŭ���� â
    public GameObject exitAskPanel; //���� ������ â

    public GameObject stageButtonPrefab; //�������� ���� ��ư ������
    private List<StageButton> stageButtons;    //�������� ���� ��ư��
    public GameObject stageButtonParent;  //�������� ���� ��ư�� �θ� �� ������Ʈ(�����̵��)

    public TextMeshProUGUI goalCountText;   //��ǥ ������ Ƚ�� �ؽ�Ʈ
    public TextMeshProUGUI moveCountText;   //���� ������ Ƚ�� �ؽ�Ʈ
    public TextMeshProUGUI rewindCountText; //���� �ǰ��� Ƚ�� �ؽ�Ʈ
    public TextMeshProUGUI stageNumberText; //�������� ��ȣ �ؽ�Ʈ
    public TextMeshProUGUI hintExplainText; //��Ʈ ���� �ؽ�Ʈ

    public Image clearStarTwo;              //Ŭ���� �� 2��°
    public Image clearStarThree;            //Ŭ���� �� 3��°
    public TextMeshProUGUI clearMoveCountText;  //���� ������ Ƚ�� �ؽ�Ʈ

    public TextMeshProUGUI exitAskText;

    public TextMeshProUGUI adAskOrFailText;
    public GameObject adYesButton;

    private StageGenerator generator;   //�������� ������
    [HideInInspector]
    public List<int> genStageData;        //������ �������� ����

    [HideInInspector]
    public int playingStage;   //�÷��� ���� �������� ��ȣ

    [HideInInspector]
    public int stageRow;       //�򶧱� ��
    [HideInInspector]
    public int stageCol;       //�򶧱� ��
    [HideInInspector]
    public int stageMix;       //�򶧱� ���� Ƚ��

    private int moveCount;      //������ Ƚ��
    private int rewindCount;    //�ǰ��� ���� Ƚ��

    private Stack<int> gaveFunnels; //�� �򶧱�(�ǰ��� ��)
    private Stack<int> gotFunnels;  //���� �򶧱�(�ǰ��� ��)

    private Vector2 touchStartCoord;    //��ġ�� ��ǥ(���� �ƴϰ� ȭ��)
    private Funnel touchedFunnel;       //��ġ�� �򶧱�
    private Funnel landedFunnel;    //������ ����Ű�� �ִ� �򶧱�

    private bool hintMode;
    private int hintOpened;

    public Color[] intToColor;      //����� �����


    public AudioSource audio;
    public AudioClip moveSound;
    public AudioClip rewindSound;
    public AudioClip clearSound;
    public AudioClip buttonSound;

    [SerializeField]
    public List<int> userClearInfo;
    public bool isEnglish;
    public TextMeshProUGUI textSelectStage;
    public TextMeshProUGUI textLastStage;
    public TextMeshProUGUI textResume;
    public TextMeshProUGUI textRestart;
    public TextMeshProUGUI textHome;

    void Awake()
    {
        //�Ҵ�
        stageButtons = new List<StageButton>();
        genStageData = new List<int>();
        gaveFunnels = new Stack<int>();
        gotFunnels = new Stack<int>();
        userClearInfo = new List<int>();

        audio = gameObject.GetComponent<AudioSource>();

        //���� ������ ����
        if (File.Exists(Application.persistentDataPath + "/clearInfo.bin")) {
            GameLoad();
        }
        else
        {
            userClearInfo.Add(0);
            userClearInfo.Add(0);
            userClearInfo.Add(-1);
            stageButtons.Add(null);
            AddButton();
            AddButton();
        }

        //�������� ������ �ʱ�ȭ
        generator = new StageGenerator();
        generator.InitializeGenerator();

        //��� ����
        isEnglish = Application.systemLanguage == SystemLanguage.Korean ? false : true;
        if (isEnglish)
        {
            textSelectStage.text = "Select\nStage";
            textLastStage.text = "Last\nStage";
            textResume.text = "RESUME";
            textRestart.text = "RESTART";
            textHome.text = "HOME";
            hintExplainText.text = "Select a funnel to see the answer color.";
            exitAskText.text = "Do you want to exit the game?";
        }
        else
        {
            textSelectStage.text = "��������\n����";
            textLastStage.text = "������\n��������";
            textResume.text = "����ϱ�";
            textRestart.text = "�ٽ��ϱ�";
            textHome.text = "Ȩ����";
            hintExplainText.text = "���� ������ �� �򶧱⸦ �����ϼ���.";
            exitAskText.text = "������ �����ұ��?";
        }
        
    }

    void Update()
    {
        GetExitInput();

        if (!pausePanel.activeSelf && !hintAskPanel.activeSelf && !exitAskPanel.activeSelf)
        {
            if (playingStage != 0) GetInput();
            else GetTutorialInput();
        }
    }

    //��ư �߰� �Լ�
    void AddButton()
    {
        //��ư ���� �� �θ� �Ʒ��� ��ġ
        GameObject buttonObj = Instantiate(stageButtonPrefab);
        buttonObj.SetActive(true);
        buttonObj.transform.SetParent(stageButtonParent.transform);
        buttonObj.transform.localScale = Vector3.one;

        //��ư ���� �ٲٱ�
        StageButton button = buttonObj.GetComponent<StageButton>();
        button.manager = this;
        button.stageNum = stageButtons.Count;
        button.numText.text = button.stageNum.ToString();
        stageButtons.Add(button);
    }

    //�������� ���� â�� ��ư�� �޸� ��&�ڹ��� ���� ���� Ŭ���� ������ ���� Ŵ
    void UpdateButtonClearStatus()
    {
        for (int i = 1; i < stageButtons.Count; i++)
            stageButtons[i].SetButtonClearStatus(userClearInfo[i]);
    }

    //Ȩ ȭ�鿡�� �������� ����â���� �Ѿ�� ��ư
    public void ButtonLevelSelection()
    {
        audio.PlayOneShot(buttonSound);
        UpdateButtonClearStatus();
        homePanel.SetActive(false);
        stageSelectionPanel.SetActive(true);
    }

    //Ȩ ȭ�鿡�� ���� ������ ���������� �Ѿ�� ��ư
    public void ButtonLastStage()
    {
        audio.PlayOneShot(buttonSound);
        StartStage(userClearInfo.Count - 2);
        homePanel.SetActive(false);
        stageSelectionPanel.SetActive(false);
    }

    //Ŭ���� �гο��� ���� ���������� �Ѿ�� ��ư
    public void ButtonNextStage()
    {
        audio.PlayOneShot(buttonSound);
        StartStage(playingStage + 1);
        clearPanel.SetActive(false);
    }

    //Ŭ���� �гο��� ���������� ������ϴ� ��ư
    //�̶� ��Ʈ�� Ų ���¿��ٸ� ��Ʈ�� ������
    public void ButtonRestartStage()
    {
        audio.PlayOneShot(buttonSound);
        bool[] hintOn = new bool[gridHintRenderers.Length];
        for (int i = 0; i < 12; i++)
            hintOn[i] = gridHintRenderers[i].gameObject.activeSelf;
        StartStage(playingStage);
        for (int i = 0; i < gridHintRenderers.Length; i++) 
            gridHintRenderers[i].gameObject.SetActive(hintOn[i]);
        clearPanel.SetActive(false);
        pausePanel.SetActive(false);
    }

    //�Ͻ����� ȭ�鿡�� ����ϴ� ��ư
    public void ButtonContinue()
    {
        audio.PlayOneShot(buttonSound);
        pausePanel.SetActive(false);
    }

    //Ȩ ȭ������ ���ư��� ��ư
    public void ButtonReturnHome()
    {
        audio.PlayOneShot(buttonSound);
        homePanel.SetActive(true);
        stageSelectionPanel.SetActive(false);
        clearPanel.SetActive(false);
        pausePanel.SetActive(false);
    }

    //�ΰ��ӿ��� �ǰ��� ��ư
    public void ButtonRewind()
    {
        if (playingStage == 0) return;
        RewindState();
        rewindCountText.text = rewindCount.ToString() + "/5";
        if (isEnglish) moveCountText.text = "Move: " + moveCount.ToString();
        else moveCountText.text = "�̵�: " + moveCount.ToString();
    }

    //�ΰ��ӿ��� �Ͻ����� ��ư
    public void ButtonPause()
    {
        if (playingStage != 0)
        {
            audio.PlayOneShot(buttonSound);
            pausePanel.SetActive(true);
        }
    }

    //�ΰ��ӿ��� ��Ʈ ���� ��ư
    public void ButtonHint()
    {
        if (playingStage == 0) return;
        audio.PlayOneShot(buttonSound);
        if (isEnglish)
        {
            if (hintOpened < stageRow * stageCol)
            {
                adAskOrFailText.text = "Watch the Ad\nand get a HINT!";
                adYesButton.SetActive(true);
            }
            else
            {
                adAskOrFailText.text = "There is no more hint you can get.";
                adYesButton.SetActive(false);
            }
        }
        else
        {
            if (hintOpened < stageRow * stageCol)
            {
                adAskOrFailText.text = "���� ����\n��Ʈ�� ��������!";
                adYesButton.SetActive(true);
            }
            else
            {
                adAskOrFailText.text = "�� �̻� ��Ʈ�� ���� �� �����.";
                adYesButton.SetActive(false);
            }
        }
        hintAskPanel.SetActive(true);
    }

    //���� ��û ���� ��ư
    public void ButtonAgreeAd()
    {
        if (adManager.ShowAd()) hintAskPanel.SetActive(false);
        else
        {
            if (isEnglish) adAskOrFailText.text = "There is no ad left.\nPlease try it later.";
            else adAskOrFailText.text = "���� �������� �ʾƿ��\n��� �Ŀ� �õ��غ�����.";
            adYesButton.SetActive(false);
        }
    }

    //���� ��û ���� ��ư
    public void ButtonCloseAd()
    {
        audio.PlayOneShot(buttonSound);
        hintAskPanel.SetActive(false);
    }

    public void ButtonExitGame()
    {
        Application.Quit();
    }

    public void ButtonCancleExit()
    {
        exitAskPanel.SetActive(false);
    }


    void AskExitGame()
    {
        exitAskPanel.SetActive(true);
    }

    public IEnumerator ShowHint()
    {
        yield return new WaitForEndOfFrame();
        hintExplainText.gameObject.SetActive(true);
        hintMode = true;
    }

    //�������� ����
    public bool StartStage(int stage)
    {
        //�� �� �ִ� ������������ Ȯ��
        if (stage >= userClearInfo.Count || userClearInfo[stage] == -1) return false;
        playingStage = stage;

        if (stage == 0 || (stage == 1 && userClearInfo[0] == 0))
        {
            tutorial.gameObject.SetActive(true);
            stageSelectionPanel.SetActive(false);
            return true;
        }

        //���������� ��, ��, ���� Ƚ���� ����
        SetStageForm();

        //�������� ��ȣ�� �õ带 ��
        Random.InitState(stage);

        //�������� ���� �õ�
        int tryCnt = 100;
        bool success = false;
        do
        {
            tryCnt--;
            success = generator.generateStage(ref genStageData, stageRow, stageCol, stageMix);
        }
        while (!success && tryCnt > 0);

        //���� ���и� ����
        if (!success)
        {
            Debug.Log("wrong generation");
            return false;
        }

        //������ �����ʹ�� ������Ʈ(�򶧱�, �׸���)�� ������
        SetIngameObject();

        //�ΰ��� ����(������, �ǰ��� Ƚ��, �ǰ��� ����, ��Ʈ ���� ��)�� �ʱ�ȭ�ϰ� UI�� ���ȴ�� �ٲ�
        SetIngameStat();

        //�������� ��ȣ UI
        if (isEnglish) stageNumberText.text = "Stage " + playingStage.ToString();
        else stageNumberText.text = "�������� " + playingStage.ToString();
        
        return true;
    }

    //�������� ��ȣ�� ���� ��, ��, ���� Ƚ���� ����
    public void SetStageForm()
    {
        //�������� ��ȣ�� ���� ���̵� ����
        if (playingStage <= 10)
        {
            stageRow = 2;
            stageCol = 2;
            if (playingStage <= 5) stageMix = 5;
            else stageMix = 8;
        }
        else if (playingStage <= 30)
        {
            stageRow = 2;
            stageCol = 3;
            if (playingStage <= 20) stageMix = 10;
            else stageMix = 15;
        }
        else if (playingStage <= 50)
        {
            stageRow = 3;
            stageCol = 3;
            if (playingStage <= 40) stageMix = 20;
            else stageMix = 25;
        }
        else
        {
            stageRow = 4;
            stageCol = 3;
            if (playingStage <= 60) stageMix = 35;
            else stageMix = 45;
        }
    }

    //�򶧱�� �׸��带 �������� �����Ϳ� �°� ������
    public void SetIngameObject()
    {
        int idx = 0;

        //������ �������� ������� �򶧱�� �׸��带 ������
        for (int i = 0; i < stageRow * stageCol; i++)
        {
            gridHintRenderers[i].gameObject.SetActive(false);
            gridHintRenderers[i].color = intToColor[genStageData[idx++]];

            funnels[i].SetOriginalPosition(stageRow, stageCol, i / stageCol, i % stageCol);
            funnels[i].InitializeColor(genStageData[idx + 3], genStageData[idx + 2], genStageData[idx + 1], genStageData[idx]);
            funnels[i].ResetPosition();
            funnels[i].gameObject.SetActive(true);

            idx += 4;

            grids[i].transform.position = funnels[i].originalPosition;
            grids[i].transform.Translate(Vector3.forward * 3);
            grids[i].SetActive(true);
        }

        //������ �ʴ� �򶧱�� �׸��带 ��
        for (int i = stageRow * stageCol; i < funnels.Length; i++)
        {
            funnels[i].gameObject.SetActive(false);
            grids[i].SetActive(false);
        }
    }

    //�ΰ��� ����(������, �ǰ��� Ƚ��, �ǰ��� ����)�� �ʱ�ȭ�ϰ� UI�� ������
    public void SetIngameStat()
    {
        //������ Ƚ���� �ǰ��� Ƚ���� �ʱ�ȭ
        moveCount = 0;
        rewindCount = 5;

        //��Ʈ ���� ���� �ʱ�ȭ
        hintOpened = 0;

        //�ǰ��⿡ ���� ���� �ʱ�ȭ
        gaveFunnels.Clear();
        gotFunnels.Clear();

        //UI �ʱ�ȭ
        if (isEnglish) moveCountText.text = "Move: 0";
        else moveCountText.text = "�̵�: 0";
        goalCountText.text = "�ڡڡ�: " + (stageMix + 5).ToString() + "\n�ڡ�: " + (stageMix + 20).ToString();
        rewindCountText.text = "5/5";
    }

    void GetExitInput()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            AskExitGame();
            return;
        }
    }

    //����� �Է��� ����
    void GetInput()
    {
        //��ġ�� �ϳ��ΰ�?
        if (Input.touchCount != 1) return;

        if (Input.touches[0].phase == TouchPhase.Began) //��ġ ����
        {
            //��ġ�� ���� �򶧱��̸� �ش� �򶧱� ���� ����
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.touches[0].position), Vector2.zero, 0f);
            if (hit.collider != null && hit.collider.tag == "Funnel")
            {
                if (!hintMode)
                {
                    touchStartCoord = Input.touches[0].position;
                    touchedFunnel = hit.collider.gameObject.GetComponent<Funnel>();
                    touchedFunnel.collider.enabled = false;
                }
                else
                {
                    touchedFunnel = null;
                    Funnel hintFunnel = hit.collider.gameObject.GetComponent<Funnel>();
                    int idx = hintFunnel.row * stageCol + hintFunnel.col;
                    if (!gridHintRenderers[idx].gameObject.activeSelf)
                    {
                        gridHintRenderers[idx].gameObject.SetActive(true);
                        hintMode = false;
                        hintOpened++;
                        hintExplainText.gameObject.SetActive(false);
                    }

                }
            }
            else touchedFunnel = null;
        }
        else if (Input.touches[0].phase == TouchPhase.Moved && touchedFunnel != null)   //��ġ �巡�� ��
        {
            //���õ� �򶧱Ⱑ ��ġ�� ������ ���󰡰� ��
            touchedFunnel.transform.position = Camera.main.ScreenToWorldPoint(Input.touches[0].position);
            touchedFunnel.transform.Translate(Vector3.forward);

            //���� �ٸ� �򶧱���� �ö����� ��ȯ ���� ���ο� ���� ���̶���Ʈ�� Ŵ
            if (landedFunnel != null) landedFunnel.highlight.gameObject.SetActive(false);
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.touches[0].position), Vector2.zero, 0f);
            if (hit.collider != null && hit.collider.tag == "Funnel")
            {
                landedFunnel = hit.collider.gameObject.GetComponent<Funnel>();
                landedFunnel.highlight.gameObject.SetActive(true);
                if (landedFunnel.colorElems.Count != 4 &&
                    (Mathf.Abs(landedFunnel.row - touchedFunnel.row) +
                    Mathf.Abs(landedFunnel.col - touchedFunnel.col) == 1))
                    landedFunnel.highlight.color = Color.green;
                else landedFunnel.highlight.color = Color.red;
            }
            else landedFunnel = null;
        }



        else if (Input.touches[0].phase == TouchPhase.Ended && touchedFunnel != null)   //��ġ ��
        {
            //�򶧱⸦ ���� �ڸ��� ��������
            touchedFunnel.ResetPosition();
            touchedFunnel.collider.enabled = true;

            //������ ��ġ ������ � �򶧱�� ����Ű�� �־���
            if (landedFunnel != null)
            {
                landedFunnel.highlight.gameObject.SetActive(false); //���̶���Ʈ�� ��
                //���������� �÷��̾ ����Ų �򶧱�� ��ȯ�� �����ϴٸ�
                if (landedFunnel.highlight.color == Color.green)
                {
                    //�ش� �ε����� �򶧱�� ������ �ְ����
                    if (touchedFunnel.GiveColor(landedFunnel))
                    {
                        moveCount++;
                        if (isEnglish) moveCountText.text = "Move: " + moveCount.ToString();
                        else moveCountText.text = "�̵�: " + moveCount.ToString();
                        gaveFunnels.Push(touchedFunnel.row * stageCol + touchedFunnel.col);
                        gotFunnels.Push(landedFunnel.row * stageCol + landedFunnel.col);

                        //������ Ŭ����Ǿ����� Ȯ��
                        if (CheckGameClear()) audio.PlayOneShot(clearSound);
                        else audio.PlayOneShot(moveSound);
                    }
                }
            }
        }
    }

    //����� �Է��� ����
    void GetTutorialInput()
    {
        //��ġ�� �ϳ��ΰ�?
        if (Input.touchCount != 1) return;

        if (Input.touches[0].phase == TouchPhase.Began) //��ġ ����
        {
            //��ġ�� ���� �򶧱��̸� �ش� �򶧱� ���� ����
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.touches[0].position), Vector2.zero, 0f);
            if (hit.collider != null && hit.collider.tag == "Funnel")
            {
                touchStartCoord = Input.touches[0].position;
                touchedFunnel = hit.collider.gameObject.GetComponent<Funnel>();
                //�˸��� ���� ��ġ�ߴ��� Ȯ��
                switch (tutorial.state)
                {
                    case 0:
                        if (touchedFunnel.row == 1 && touchedFunnel.col == 0) touchedFunnel.collider.enabled = false;
                        else touchedFunnel = null;
                        break;
                    case 1:
                        if (touchedFunnel.row == 1 && touchedFunnel.col == 0) touchedFunnel.collider.enabled = false;
                        else touchedFunnel = null;
                        break;
                    case 2:
                        if (touchedFunnel.row == 1 && touchedFunnel.col == 1) touchedFunnel.collider.enabled = false;
                        else touchedFunnel = null;
                        break;
                    case 3:
                        if (touchedFunnel.row == 1 && touchedFunnel.col == 1) touchedFunnel.collider.enabled = false;
                        else touchedFunnel = null;
                        break;
                    case 4:
                        if (touchedFunnel.row == 1 && touchedFunnel.col == 1) touchedFunnel.collider.enabled = false;
                        else touchedFunnel = null;
                        break;
                }
            }
            else touchedFunnel = null;
        }
        else if (Input.touches[0].phase == TouchPhase.Moved && touchedFunnel != null)   //��ġ �巡�� ��
        {
            //���õ� �򶧱Ⱑ ��ġ�� ������ ���󰡰� ��
            touchedFunnel.transform.position = Camera.main.ScreenToWorldPoint(Input.touches[0].position);
            touchedFunnel.transform.Translate(Vector3.forward);

            //���� �ٸ� �򶧱���� �ö����� ��ȯ ���� ���ο� ���� ���̶���Ʈ�� Ŵ
            if (landedFunnel != null) landedFunnel.highlight.gameObject.SetActive(false);
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.touches[0].position), Vector2.zero, 0f);
            if (hit.collider != null && hit.collider.tag == "Funnel")
            {
                landedFunnel = hit.collider.gameObject.GetComponent<Funnel>();
                landedFunnel.highlight.gameObject.SetActive(true);
                if (landedFunnel.colorElems.Count != 4 &&
                    (Mathf.Abs(landedFunnel.row - touchedFunnel.row) +
                    Mathf.Abs(landedFunnel.col - touchedFunnel.col) == 1))
                    landedFunnel.highlight.color = Color.green;
                else landedFunnel.highlight.color = Color.red;
            }
            else landedFunnel = null;
        }
        else if (Input.touches[0].phase == TouchPhase.Ended && touchedFunnel != null)   //��ġ ��
        {
            //�򶧱⸦ ���� �ڸ��� ��������
            touchedFunnel.ResetPosition();
            touchedFunnel.collider.enabled = true;

            //������ ��ġ ������ � �򶧱�� ����Ű�� �־���
            if (landedFunnel != null)
            {
                landedFunnel.highlight.gameObject.SetActive(false); //���̶���Ʈ�� ��

                //�˸��� �Ϳ��� �հ����� �ô��� Ȯ��
                switch (tutorial.state)
                {
                    case 0:
                        if (landedFunnel.row != 0 && landedFunnel.col != 0) return;
                        break;
                    case 1:
                        if (landedFunnel.row != 1 && landedFunnel.col != 1) return;
                        break;
                    case 2:
                        if (landedFunnel.row != 1 && landedFunnel.col != 0) return;
                        break;
                    case 3:
                        if (landedFunnel.row != 1 && landedFunnel.col != 0) return;
                        break;
                    case 4:
                        if (landedFunnel.row != 0 && landedFunnel.col != 1) return;
                        break;
                }

                //���������� �÷��̾ ����Ų �򶧱�� ��ȯ�� �����ϴٸ�
                if (landedFunnel.highlight.color == Color.green)
                {
                    //�ش� �ε����� �򶧱�� ������ �ְ����
                    if (touchedFunnel.GiveColor(landedFunnel))
                    {
                        tutorial.state++;
                        tutorial.SetHandPosition();
                        moveCount++;
                        if (isEnglish) moveCountText.text = "Move: " + moveCount.ToString();
                        else moveCountText.text = "�̵�: " + moveCount.ToString();
                        gaveFunnels.Push(touchedFunnel.row * stageCol + touchedFunnel.col);
                        gotFunnels.Push(landedFunnel.row * stageCol + landedFunnel.col);

                        //������ Ŭ����Ǿ����� Ȯ��
                        if (CheckGameClear())
                        {
                            tutorial.gameObject.SetActive(false);
                            audio.PlayOneShot(clearSound);
                        }
                        else audio.PlayOneShot(moveSound);
                    }
                }
            }
        }
    }

    //���� �ܰ�� �ǵ��ư���
    public bool RewindState()
    {
        //�ǵ��ư� �� �ִ��� Ȯ��
        if (rewindCount == 0) return false;
        if (gaveFunnels.Count == 0 || gotFunnels.Count == 0) return false;

        //���ÿ��� Ÿ�� �򶧱���� ������
        int selectedR = gaveFunnels.Peek() / stageCol;
        int selectedC = gaveFunnels.Pop() % stageCol;
        int targetR = gotFunnels.Peek() / stageCol;
        int targetC = gotFunnels.Pop() % stageCol;

        //�ǵ�����
        funnels[selectedR * stageCol + selectedC].RegainColor(funnels[targetR * stageCol + targetC]);

        moveCount--;
        rewindCount--;
        audio.PlayOneShot(rewindSound);
        return true;
    }

    //������ Ŭ���� �Ǿ����� Ȯ��
    bool CheckGameClear()
    {
        //��� �򶧱Ⱑ ������ �� ������ ���ϵǾ����� Ȯ��
        for (int i = 0; i < stageRow * stageCol; i++)
            if (!funnels[i].IsUnited()) return false;

        //������ Ƚ���� ���� Ŭ���� â�� ������ �ٲ�
        if (isEnglish) clearMoveCountText.text = "Move: " + moveCount.ToString();
        else clearMoveCountText.text = "�̵�: " + moveCount.ToString();
        if (moveCount <= stageMix + 5) clearStarThree.color = Color.yellow;
        else clearStarThree.color = Color.black;
        if (moveCount <= stageMix + 20) clearStarTwo.color = Color.yellow;
        else clearStarTwo.color = Color.black;
        clearPanel.SetActive(true);

        //������ �������� ���ٸ� ���� ���������� ������
        if (userClearInfo[playingStage + 1] == -1)
        {
            userClearInfo[playingStage + 1] = 0;
            userClearInfo.Add(-1);
            AddButton();
        }        

        //������ Ƚ���� ���� Ŭ���� ������ ���Ͽ� ����
        if (userClearInfo[playingStage] < 3 && moveCount <= stageMix + 5)
            userClearInfo[playingStage] = 3;
        else if (userClearInfo[playingStage] < 2 && moveCount <= stageMix + 20)
            userClearInfo[playingStage] = 2;
        else if (userClearInfo[playingStage] == 0)
            userClearInfo[playingStage] = 1;

        //���� ����
        GameSave();
        return true;
    }
         

    //������ ����
    public void GameSave()
    {
        //���̳ʸ� ���� clearInfo.bin ����
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/clearInfo.bin");
        //userClearInfo�� ���Ͽ� ����
        bf.Serialize(file, userClearInfo);
        file.Close();
    }

    //������ �ҷ���
    public void GameLoad()
    {
        //���� ���� ���� Ȯ��
        if (File.Exists(Application.persistentDataPath + "/clearInfo.bin"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/clearInfo.bin", FileMode.Open);
            //userClearInfo�� ���� ������ �ҷ���
            userClearInfo = (List<int>)bf.Deserialize(file);
            file.Close();
            //��ư ������ �����Ϳ� �°� ����
            stageButtons.Add(null);
            while (stageButtons.Count < userClearInfo.Count) AddButton();
        }
    }
}
