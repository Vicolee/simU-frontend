using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavbarManager : MonoBehaviour
{
    public GameObject myWorldsPanel;
    public GameObject userIcon;
    public GameObject backIcon;

    public GameObject currentPanel;
    public GameObject userProfilePanel;
    public GameObject navbarPanel;

    // Start is called before the first frame update
    void OnEnable()
    {
        navbarPanel.SetActive(true);
        userIcon.SetActive(true);
        backIcon.SetActive(false);
        currentPanel = myWorldsPanel;
    }

    public void OnPressUserButton()
    {
        myWorldsPanel.SetActive(false);
        userProfilePanel.SetActive(true);
        SetCurrentPanel(userProfilePanel);
    }

    public void OnPressBackButton()
    {
        myWorldsPanel.SetActive(true);
        userIcon.SetActive(true);
        backIcon.SetActive(false);
        currentPanel.SetActive(false);
        SetCurrentPanel(myWorldsPanel);
    }

    public void SetCurrentPanel(GameObject panel)
    {
        currentPanel = panel;
        Debug.Log("In SetCurrentPanel. Current panel is " + currentPanel.name);
        if (currentPanel != myWorldsPanel) // for all cases where they are not MyWorldsPanel, always make it available to go back
        {
            userIcon.SetActive(false);
            backIcon.SetActive(true);
        } else
        {
            userIcon.SetActive(true);
            backIcon.SetActive(false);
        }
    }

    public void NavigateBackToMyWorlds()
    {
        navbarPanel.SetActive(true);
        Debug.Log("Navigating back to my worlds");
        SetCurrentPanel(myWorldsPanel);
    }
}
