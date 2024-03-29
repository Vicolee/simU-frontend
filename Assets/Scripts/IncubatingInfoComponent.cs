using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class IncubatingInfoComponent : MonoBehaviour
{
    public TextMeshProUGUI usernameTMP;
    public Image displayUserImage;
    public GameObject trainAgentPanel;
    public Slider slider;
    public Guid incubatingId;
    public TrainAgentManager trainAgentManager;

    // A method to set the player details
    public void SetPlayerDetails(Sprite profileImage, string username)
    {
        displayUserImage.sprite = profileImage;
        usernameTMP.text = username;
    }

    // Set the maximum value of the slider
    public void SetMaxValue(float maxValue)
    {
        slider.maxValue = maxValue;
    }

    // Set the progress value of the slider
    public void SetProgress(float progress)
    {
        slider.value = progress;
    }

    public void ClickPrefab()
    {
        trainAgentManager.SetAgentID(incubatingId);
        trainAgentManager.SetPanelDetails(displayUserImage.sprite, usernameTMP);
    }
}
