using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIGamePlay : UIBase
{
    public Text textWave;
    public Toggle toggleAutoPlay;
    public Toggle toggleSpeedMultiply;
    private bool isAutoPlayDirty;
    private bool isSpeedMultiplyDirty;

    protected override void Awake()
    {
        base.Awake();

        if (toggleAutoPlay != null)
        {
            toggleAutoPlay.onValueChanged.RemoveListener(OnToggleAutoPlay);
            toggleAutoPlay.onValueChanged.AddListener(OnToggleAutoPlay);
        }

        if (toggleSpeedMultiply != null)
        {
            toggleSpeedMultiply.onValueChanged.RemoveListener(OnToggleSpeedMultiply);
            toggleSpeedMultiply.onValueChanged.AddListener(OnToggleSpeedMultiply);
        }
    }

    private void Update()
    {
        var gamePlayManager = GamePlayManager.Singleton;
        if (textWave != null)
            textWave.text = gamePlayManager.CurrentWave <= 0 ? "" : "Wave " + gamePlayManager.CurrentWave + "/" + gamePlayManager.MaxWave;

        if (gamePlayManager.IsAutoPlay != isAutoPlayDirty)
        {
            if (toggleAutoPlay != null)
            toggleAutoPlay.isOn = gamePlayManager.IsAutoPlay;
            isAutoPlayDirty = gamePlayManager.IsAutoPlay;
        }

        if (gamePlayManager.IsSpeedMultiply != isSpeedMultiplyDirty)
        {
            if (toggleSpeedMultiply != null)
                toggleSpeedMultiply.isOn = gamePlayManager.IsSpeedMultiply;
            isSpeedMultiplyDirty = gamePlayManager.IsSpeedMultiply;
        }
    }

    public void OnToggleAutoPlay(bool isOn)
    {
        var gamePlayManager = GamePlayManager.Singleton;
        gamePlayManager.IsAutoPlay = isOn;
    }

    public void OnToggleSpeedMultiply(bool isOn)
    {
        var gamePlayManager = GamePlayManager.Singleton;
        gamePlayManager.IsSpeedMultiply = isOn;
    }
}
