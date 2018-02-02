using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPauseGame : UIBase
{
    public Button buttonContinue;
    public Button buttonRestart;
    public Button buttonGiveUp;

    public override void Show()
    {
        base.Show();
        buttonContinue.onClick.RemoveListener(OnClickContinue);
        buttonContinue.onClick.AddListener(OnClickContinue);
        buttonRestart.onClick.RemoveListener(OnClickRestart);
        buttonRestart.onClick.AddListener(OnClickRestart);
        buttonGiveUp.onClick.RemoveListener(OnClickGiveUp);
        buttonGiveUp.onClick.AddListener(OnClickGiveUp);
    }

    public void OnClickContinue()
    {
        Hide();
    }

    public void OnClickRestart()
    {
        Hide();
        GamePlayManager.Singleton.Restart();
    }

    public void OnClickGiveUp()
    {
        Hide();
        GamePlayManager.Singleton.Giveup(Show);
    }
}
