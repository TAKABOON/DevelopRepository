﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;
using static UnityEngine.SceneManagement.SceneManager;

public class SceneManager : SingletonMonoBehaviour<SceneManager>
{
    [SerializeField] Image imgCover;

    public static readonly string StageNamePrefix = "Stage";

    string OldAdditiveSceneName { get; set; }
    bool IsSceneChanging { get; set; }
    bool IsFading { get; set; }
    Sequence FadeSequence { get; set; }


    protected override void Awake()
    {
        base.Awake();
        if (isCreated) return;

        DontDestroyOnLoad(gameObject);

        FadeSequence = DOTween.Sequence();
        FadeSequence.Append(imgCover.DOFade(1, 0.35f).SetEase(Ease.Linear));
        FadeSequence.SetAutoKill(false);
        FadeSequence.Pause();
        FadeSequence.OnStepComplete(() => IsFading = !IsFading);
    }

    public void ChangeScene(string sceneName = "", float delay = 0.0f)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            return;
        }
        StartCoroutine(ChangeSceneCoroutine(sceneName, delay));
    }

    IEnumerator ChangeSceneCoroutine(string sceneName, float delay = 0.0f)
    {
        if (IsSceneChanging) yield break;

        // フェードイン開始
        IsSceneChanging = true;

        if (delay > 0.0f)
        {
            yield return new WaitForSeconds(delay);
        }

        imgCover.raycastTarget = true;
        FadeSequence.PlayForward();

        // フェードイン待ち
        yield return new WaitWhile(() => !IsFading);

        // 古い加算シーンを破棄
        if (!string.IsNullOrEmpty(OldAdditiveSceneName))
        {
            var async = UnloadSceneAsync(OldAdditiveSceneName);
            yield return new WaitWhile(() => !async.isDone);
            OldAdditiveSceneName = "";
        }

        Resources.UnloadUnusedAssets();

        // 基礎シーンロード
        if (!string.IsNullOrEmpty(sceneName))
        {
            var async = LoadSceneAsync(sceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
            yield return new WaitWhile(() => !async.isDone);
        }

        // フェードアウト開始
        FadeSequence.PlayBackwards();

        // フェードアウト待ち
        yield return new WaitWhile(() => IsFading);
        imgCover.raycastTarget = false;

        IsSceneChanging = false;
    }
}
