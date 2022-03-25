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

        //��û�� ���� �ε� ����
        videoAd.OnAdLoaded += AdLoaded;
        //��û�� ���� �ε� ����
        videoAd.OnAdFailedToLoad += FailedToLoad;
        //���� ��� ȭ�� ������ ����� ����
        videoAd.OnAdOpening += AdOpening;
        //���� ���ؼ� ������
        videoAd.OnAdStarted += AdStarted;
        //���� �����Ƿ� ������
        videoAd.OnAdRewarded += AdRewarded;
        //���� ����
        videoAd.OnAdClosed += AdClosed;
        //���� Ŭ���ؼ� ���� ������
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
        else manager.adAskOrFailText.text = "���� �������� �ʾƿ��\n��� �Ŀ� �õ��غ�����.";

    }

    public void AdOpening(object sender, EventArgs args)
    {

    }

    public void AdStarted(object sender, EventArgs args)
    {

    }

    public void AdClosed(object sender, EventArgs args)
    {
        RequestRewardBasedVideo();  //���� ���� ��û
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