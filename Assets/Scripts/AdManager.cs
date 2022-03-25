using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using GoogleMobileAds.Api;

public class AdManager : MonoBehaviour
{
    public GameManager manager;
    RewardBasedVideoAd videoAd;
    void Start()
    {
        MobileAds.Initialize(status => { });
        videoAd = RewardBasedVideoAd.Instance;
        RequestRewardBasedVideo();

        //요청한 광고 로드 성공
        videoAd.OnAdLoaded += AdLoaded;
        //요청한 광고 로드 실패
        videoAd.OnAdFailedToLoad += FailedToLoad;
        //광고 열어서 화면 전면이 광고로 덮힘
        videoAd.OnAdOpening += AdOpening;
        //광고 탭해서 시작함
        videoAd.OnAdStarted += AdStarted;
        //광고 봤으므로 보상줌
        videoAd.OnAdRewarded += AdRewarded;
        //광고 닫힘
        videoAd.OnAdClosed += AdClosed;
        //광고 클릭해서 어플 나가짐
        videoAd.OnAdLeavingApplication += AdLeavingApplication;
        //videoAd.OnAdCompleted +=
        
        RequestRewardBasedVideo();
    }

    private void RequestRewardBasedVideo()
    {
        string adUnitId = "00000000000000000000";
        AdRequest request = new AdRequest.Builder().Build();
        videoAd.LoadAd(request, adUnitId);
    }

    public void AdLoaded(object sender, EventArgs args)
    {

    }

    public void FailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
        if (manager.isEnglish) manager.adAskOrFailText.text = "There is no ad left.\nPlease wait for a while.";
        else manager.adAskOrFailText.text = "광고가 남아있지 않아요ㅠ\n잠시 후에 시도해보세요.";

    }

    public void AdOpening(object sender, EventArgs args)
    {

    }

    public void AdStarted(object sender, EventArgs args)
    {

    }

    public void AdClosed(object sender, EventArgs args)
    {
        RequestRewardBasedVideo();  //다음 광고 요청
    }

    public void AdRewarded(object sender, Reward args)
    {
        string type = args.Type;
        double amount = args.Amount;

        StartCoroutine(manager.ShowHint());
    }

    public void AdLeavingApplication(object sender, EventArgs args)
    {

    }

    public bool ShowAd()
    {
        if (videoAd.IsLoaded())
        {
            videoAd.Show();
            return true;
        }
        else
        {
            Debug.Log("Ad Load Failed.");
            return false;
        }
    }
}