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
    public Funnel[] funnels;    //깔때기들 (4*3개)
    public GameObject[] grids;      //그리드들 (4*3개)
    public SpriteRenderer[] gridHintRenderers;  //힌트들 (4*3개)

    public GameObject homePanel;      //홈 화면
    public GameObject stageSelectionPanel;    //스테이지 선택 화면
    public GameObject ingamePanel;    //인게임 UI
    public GameObject pausePanel;     //일시정지 화면
    public GameObject hintAskPanel;   //힌트<->광고 동의 화면
    public GameObject clearPanel;     //클리어 창
    public GameObject exitAskPanel; //게임 나가기 창

    public GameObject stageButtonPrefab; //스테이지 선택 버튼 프리팹
    private List<StageButton> stageButtons;    //스테이지 선택 버튼들
    public GameObject stageButtonParent;  //스테이지 선택 버튼의 부모가 될 오브젝트(슬라이드용)

    public TextMeshProUGUI goalCountText;   //목표 움직임 횟수 텍스트
    public TextMeshProUGUI moveCountText;   //실제 움직임 횟수 텍스트
    public TextMeshProUGUI rewindCountText; //남은 되감기 횟수 텍스트
    public TextMeshProUGUI stageNumberText; //스테이지 번호 텍스트
    public TextMeshProUGUI hintExplainText; //힌트 사용법 텍스트

    public Image clearStarTwo;              //클리어 별 2번째
    public Image clearStarThree;            //클리어 별 3번째
    public TextMeshProUGUI clearMoveCountText;  //최종 움직임 횟수 텍스트

    public TextMeshProUGUI exitAskText;

    public TextMeshProUGUI adAskOrFailText;
    public GameObject adYesButton;

    private StageGenerator generator;   //스테이지 생성기
    [HideInInspector]
    public List<int> genStageData;        //생성된 스테이지 정보

    [HideInInspector]
    public int playingStage;   //플레이 중인 스테이지 번호

    [HideInInspector]
    public int stageRow;       //깔때기 행
    [HideInInspector]
    public int stageCol;       //깔때기 열
    [HideInInspector]
    public int stageMix;       //깔때기 섞은 횟수

    private int moveCount;      //움직인 횟수
    private int rewindCount;    //되감기 남은 횟수

    private Stack<int> gaveFunnels; //준 깔때기(되감기 용)
    private Stack<int> gotFunnels;  //받은 깔때기(되감기 용)

    private Vector2 touchStartCoord;    //터치한 좌표(월드 아니고 화면)
    private Funnel touchedFunnel;       //터치된 깔때기
    private Funnel landedFunnel;    //이전에 가리키고 있던 깔때기

    private bool hintMode;
    private int hintOpened;

    public Color[] intToColor;      //사용할 색깔들


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
        //할당
        stageButtons = new List<StageButton>();
        genStageData = new List<int>();
        gaveFunnels = new Stack<int>();
        gotFunnels = new Stack<int>();
        userClearInfo = new List<int>();

        audio = gameObject.GetComponent<AudioSource>();

        //최초 데이터 생성
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

        //스테이지 생성기 초기화
        generator = new StageGenerator();
        generator.InitializeGenerator();

        //언어 설정
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
            textSelectStage.text = "스테이지\n선택";
            textLastStage.text = "마지막\n스테이지";
            textResume.text = "계속하기";
            textRestart.text = "다시하기";
            textHome.text = "홈으로";
            hintExplainText.text = "정답 색깔을 볼 깔때기를 선택하세요.";
            exitAskText.text = "게임을 종료할까요?";
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

    //버튼 추가 함수
    void AddButton()
    {
        //버튼 생성 및 부모 아래에 위치
        GameObject buttonObj = Instantiate(stageButtonPrefab);
        buttonObj.SetActive(true);
        buttonObj.transform.SetParent(stageButtonParent.transform);
        buttonObj.transform.localScale = Vector3.one;

        //버튼 정보 바꾸기
        StageButton button = buttonObj.GetComponent<StageButton>();
        button.manager = this;
        button.stageNum = stageButtons.Count;
        button.numText.text = button.stageNum.ToString();
        stageButtons.Add(button);
    }

    //스테이지 선택 창의 버튼에 달린 별&자물쇠 등을 유저 클리어 정보에 따라 킴
    void UpdateButtonClearStatus()
    {
        for (int i = 1; i < stageButtons.Count; i++)
            stageButtons[i].SetButtonClearStatus(userClearInfo[i]);
    }

    //홈 화면에서 스테이지 선택창으로 넘어가는 버튼
    public void ButtonLevelSelection()
    {
        audio.PlayOneShot(buttonSound);
        UpdateButtonClearStatus();
        homePanel.SetActive(false);
        stageSelectionPanel.SetActive(true);
    }

    //홈 화면에서 가장 마지막 스테이지로 넘어가는 버튼
    public void ButtonLastStage()
    {
        audio.PlayOneShot(buttonSound);
        StartStage(userClearInfo.Count - 2);
        homePanel.SetActive(false);
        stageSelectionPanel.SetActive(false);
    }

    //클리어 패널에서 다음 스테이지로 넘어가는 버튼
    public void ButtonNextStage()
    {
        audio.PlayOneShot(buttonSound);
        StartStage(playingStage + 1);
        clearPanel.SetActive(false);
    }

    //클리어 패널에서 스테이지를 재시작하는 버튼
    //이때 힌트를 킨 상태였다면 힌트를 유지함
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

    //일시정지 화면에서 계속하는 버튼
    public void ButtonContinue()
    {
        audio.PlayOneShot(buttonSound);
        pausePanel.SetActive(false);
    }

    //홈 화면으로 돌아가는 버튼
    public void ButtonReturnHome()
    {
        audio.PlayOneShot(buttonSound);
        homePanel.SetActive(true);
        stageSelectionPanel.SetActive(false);
        clearPanel.SetActive(false);
        pausePanel.SetActive(false);
    }

    //인게임에서 되감기 버튼
    public void ButtonRewind()
    {
        if (playingStage == 0) return;
        RewindState();
        rewindCountText.text = rewindCount.ToString() + "/5";
        if (isEnglish) moveCountText.text = "Move: " + moveCount.ToString();
        else moveCountText.text = "이동: " + moveCount.ToString();
    }

    //인게임에서 일시정지 버튼
    public void ButtonPause()
    {
        if (playingStage != 0)
        {
            audio.PlayOneShot(buttonSound);
            pausePanel.SetActive(true);
        }
    }

    //인게임에서 힌트 보기 버튼
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
                adAskOrFailText.text = "광고를 보고\n힌트를 얻으세요!";
                adYesButton.SetActive(true);
            }
            else
            {
                adAskOrFailText.text = "더 이상 힌트를 받을 수 없어요.";
                adYesButton.SetActive(false);
            }
        }
        hintAskPanel.SetActive(true);
    }

    //광고 시청 동의 버튼
    public void ButtonAgreeAd()
    {
        if (adManager.ShowAd()) hintAskPanel.SetActive(false);
        else
        {
            if (isEnglish) adAskOrFailText.text = "There is no ad left.\nPlease try it later.";
            else adAskOrFailText.text = "광고가 남아있지 않아요ㅠ\n잠시 후에 시도해보세요.";
            adYesButton.SetActive(false);
        }
    }

    //광고 시청 거절 버튼
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

    //스테이지 시작
    public bool StartStage(int stage)
    {
        //열 수 있는 스테이지인지 확인
        if (stage >= userClearInfo.Count || userClearInfo[stage] == -1) return false;
        playingStage = stage;

        if (stage == 0 || (stage == 1 && userClearInfo[0] == 0))
        {
            tutorial.gameObject.SetActive(true);
            stageSelectionPanel.SetActive(false);
            return true;
        }

        //스테이지의 행, 열, 섞기 횟수를 정함
        SetStageForm();

        //스테이지 번호로 시드를 줌
        Random.InitState(stage);

        //스테이지 생성 시도
        int tryCnt = 100;
        bool success = false;
        do
        {
            tryCnt--;
            success = generator.generateStage(ref genStageData, stageRow, stageCol, stageMix);
        }
        while (!success && tryCnt > 0);

        //생성 실패면 종료
        if (!success)
        {
            Debug.Log("wrong generation");
            return false;
        }

        //생성한 데이터대로 오브젝트(깔때기, 그리드)를 셋팅함
        SetIngameObject();

        //인게임 스탯(움직임, 되감기 횟수, 되감기 스택, 힌트 오픈 수)을 초기화하고 UI를 스탯대로 바꿈
        SetIngameStat();

        //스테이지 번호 UI
        if (isEnglish) stageNumberText.text = "Stage " + playingStage.ToString();
        else stageNumberText.text = "스테이지 " + playingStage.ToString();
        
        return true;
    }

    //스테이지 번호에 따라 행, 열, 섞기 횟수를 정함
    public void SetStageForm()
    {
        //스테이지 번호에 따라 난이도 조절
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

    //깔때기와 그리드를 스테이지 데이터에 맞게 조정함
    public void SetIngameObject()
    {
        int idx = 0;

        //생성된 스테이지 정보대로 깔때기와 그리드를 조정함
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

        //사용되지 않는 깔때기와 그리드를 끔
        for (int i = stageRow * stageCol; i < funnels.Length; i++)
        {
            funnels[i].gameObject.SetActive(false);
            grids[i].SetActive(false);
        }
    }

    //인게임 스탯(움직임, 되감기 횟수, 되감기 스택)을 초기화하고 UI를 수정함
    public void SetIngameStat()
    {
        //움직임 횟수랑 되감기 횟수를 초기화
        moveCount = 0;
        rewindCount = 5;

        //힌트 오픈 수를 초기화
        hintOpened = 0;

        //되감기에 쓰일 스택 초기화
        gaveFunnels.Clear();
        gotFunnels.Clear();

        //UI 초기화
        if (isEnglish) moveCountText.text = "Move: 0";
        else moveCountText.text = "이동: 0";
        goalCountText.text = "★★★: " + (stageMix + 5).ToString() + "\n★★: " + (stageMix + 20).ToString();
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

    //사용자 입력을 받음
    void GetInput()
    {
        //터치가 하나인가?
        if (Input.touchCount != 1) return;

        if (Input.touches[0].phase == TouchPhase.Began) //터치 시작
        {
            //터치된 것이 깔때기이면 해당 깔때기 정보 저장
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
        else if (Input.touches[0].phase == TouchPhase.Moved && touchedFunnel != null)   //터치 드래그 중
        {
            //선택된 깔때기가 터치한 지점을 따라가게 함
            touchedFunnel.transform.position = Camera.main.ScreenToWorldPoint(Input.touches[0].position);
            touchedFunnel.transform.Translate(Vector3.forward);

            //만일 다른 깔때기까지 올라갔으면 교환 가능 여부에 따라 하이라이트를 킴
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



        else if (Input.touches[0].phase == TouchPhase.Ended && touchedFunnel != null)   //터치 끝
        {
            //깔때기를 원래 자리에 돌려놓음
            touchedFunnel.ResetPosition();
            touchedFunnel.collider.enabled = true;

            //마지막 터치 지점이 어떤 깔때기든 가리키고 있었음
            if (landedFunnel != null)
            {
                landedFunnel.highlight.gameObject.SetActive(false); //하이라이트를 끔
                //마지막으로 플레이어가 가리킨 깔때기와 교환이 가능하다면
                if (landedFunnel.highlight.color == Color.green)
                {
                    //해당 인덱스의 깔때기와 색깔을 주고받음
                    if (touchedFunnel.GiveColor(landedFunnel))
                    {
                        moveCount++;
                        if (isEnglish) moveCountText.text = "Move: " + moveCount.ToString();
                        else moveCountText.text = "이동: " + moveCount.ToString();
                        gaveFunnels.Push(touchedFunnel.row * stageCol + touchedFunnel.col);
                        gotFunnels.Push(landedFunnel.row * stageCol + landedFunnel.col);

                        //게임이 클리어되었는지 확인
                        if (CheckGameClear()) audio.PlayOneShot(clearSound);
                        else audio.PlayOneShot(moveSound);
                    }
                }
            }
        }
    }

    //사용자 입력을 받음
    void GetTutorialInput()
    {
        //터치가 하나인가?
        if (Input.touchCount != 1) return;

        if (Input.touches[0].phase == TouchPhase.Began) //터치 시작
        {
            //터치된 것이 깔때기이면 해당 깔때기 정보 저장
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.touches[0].position), Vector2.zero, 0f);
            if (hit.collider != null && hit.collider.tag == "Funnel")
            {
                touchStartCoord = Input.touches[0].position;
                touchedFunnel = hit.collider.gameObject.GetComponent<Funnel>();
                //알맞은 것을 터치했는지 확인
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
        else if (Input.touches[0].phase == TouchPhase.Moved && touchedFunnel != null)   //터치 드래그 중
        {
            //선택된 깔때기가 터치한 지점을 따라가게 함
            touchedFunnel.transform.position = Camera.main.ScreenToWorldPoint(Input.touches[0].position);
            touchedFunnel.transform.Translate(Vector3.forward);

            //만일 다른 깔때기까지 올라갔으면 교환 가능 여부에 따라 하이라이트를 킴
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
        else if (Input.touches[0].phase == TouchPhase.Ended && touchedFunnel != null)   //터치 끝
        {
            //깔때기를 원래 자리에 돌려놓음
            touchedFunnel.ResetPosition();
            touchedFunnel.collider.enabled = true;

            //마지막 터치 지점이 어떤 깔때기든 가리키고 있었음
            if (landedFunnel != null)
            {
                landedFunnel.highlight.gameObject.SetActive(false); //하이라이트를 끔

                //알맞은 것에서 손가락을 뗐는지 확인
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

                //마지막으로 플레이어가 가리킨 깔때기와 교환이 가능하다면
                if (landedFunnel.highlight.color == Color.green)
                {
                    //해당 인덱스의 깔때기와 색깔을 주고받음
                    if (touchedFunnel.GiveColor(landedFunnel))
                    {
                        tutorial.state++;
                        tutorial.SetHandPosition();
                        moveCount++;
                        if (isEnglish) moveCountText.text = "Move: " + moveCount.ToString();
                        else moveCountText.text = "이동: " + moveCount.ToString();
                        gaveFunnels.Push(touchedFunnel.row * stageCol + touchedFunnel.col);
                        gotFunnels.Push(landedFunnel.row * stageCol + landedFunnel.col);

                        //게임이 클리어되었는지 확인
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

    //이전 단계로 되돌아가기
    public bool RewindState()
    {
        //되돌아갈 수 있는지 확인
        if (rewindCount == 0) return false;
        if (gaveFunnels.Count == 0 || gotFunnels.Count == 0) return false;

        //스택에서 타겟 깔때기들을 가져옴
        int selectedR = gaveFunnels.Peek() / stageCol;
        int selectedC = gaveFunnels.Pop() % stageCol;
        int targetR = gotFunnels.Peek() / stageCol;
        int targetC = gotFunnels.Pop() % stageCol;

        //되돌려줌
        funnels[selectedR * stageCol + selectedC].RegainColor(funnels[targetR * stageCol + targetC]);

        moveCount--;
        rewindCount--;
        audio.PlayOneShot(rewindSound);
        return true;
    }

    //게임이 클리어 되었는지 확인
    bool CheckGameClear()
    {
        //모든 깔때기가 색깔이 한 종류로 통일되었는지 확인
        for (int i = 0; i < stageRow * stageCol; i++)
            if (!funnels[i].IsUnited()) return false;

        //움직인 횟수에 따라 클리어 창의 정보를 바꿈
        if (isEnglish) clearMoveCountText.text = "Move: " + moveCount.ToString();
        else clearMoveCountText.text = "이동: " + moveCount.ToString();
        if (moveCount <= stageMix + 5) clearStarThree.color = Color.yellow;
        else clearStarThree.color = Color.black;
        if (moveCount <= stageMix + 20) clearStarTwo.color = Color.yellow;
        else clearStarTwo.color = Color.black;
        clearPanel.SetActive(true);

        //마지막 스테이지 였다면 다음 스테이지를 오픈함
        if (userClearInfo[playingStage + 1] == -1)
        {
            userClearInfo[playingStage + 1] = 0;
            userClearInfo.Add(-1);
            AddButton();
        }        

        //움직인 횟수와 과거 클리어 정보를 비교하여 갱신
        if (userClearInfo[playingStage] < 3 && moveCount <= stageMix + 5)
            userClearInfo[playingStage] = 3;
        else if (userClearInfo[playingStage] < 2 && moveCount <= stageMix + 20)
            userClearInfo[playingStage] = 2;
        else if (userClearInfo[playingStage] == 0)
            userClearInfo[playingStage] = 1;

        //게임 저장
        GameSave();
        return true;
    }
         

    //게임을 저장
    public void GameSave()
    {
        //바이너리 파일 clearInfo.bin 생성
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/clearInfo.bin");
        //userClearInfo를 파일에 저장
        bf.Serialize(file, userClearInfo);
        file.Close();
    }

    //게임을 불러옴
    public void GameLoad()
    {
        //파일 존재 여부 확인
        if (File.Exists(Application.persistentDataPath + "/clearInfo.bin"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/clearInfo.bin", FileMode.Open);
            //userClearInfo로 파일 데이터 불러옴
            userClearInfo = (List<int>)bf.Deserialize(file);
            file.Close();
            //버튼 갯수를 데이터에 맞게 조정
            stageButtons.Add(null);
            while (stageButtons.Count < userClearInfo.Count) AddButton();
        }
    }
}
