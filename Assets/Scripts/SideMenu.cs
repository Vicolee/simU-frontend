using UnityEngine;
using TMPro;

public class SideMenu : MonoBehaviour
{
    public GameObject menuPanel;
    public GameObject charactersButton;
    public GameObject charactersSubMenu;
    public GameObject hamburgerIcon;

    public GameObject worldSettingsPanel;
    public GameObject playersListPanel;
    public GameObject hatchedPanel;
    public GameObject incubatingPanel;
    public GameObject swipePrefab;
    public GameObject createAgentPanel;
    public GameObject visualDescPanel; // for asking "what does your bot look like?"
    public GameObject confirmCreatePanel;
    public GameObject agentInfoPanel;
    public GameObject trainAgentPanel;
    public GameObject agentCollabPanel;
    public GameObject viewAnswersPanel;
    public GameObject typeAnswerPanel;
    void Start()
    {
        // Disable the submenu initially
        menuPanel.SetActive(false);
        charactersSubMenu.SetActive(false);
        worldSettingsPanel.SetActive(false);
        playersListPanel.SetActive(false);
        hatchedPanel.SetActive(false);
        incubatingPanel.SetActive(false);
        swipePrefab.SetActive(false);
        createAgentPanel.SetActive(false);
        visualDescPanel.SetActive(false);
        confirmCreatePanel.SetActive(false);
        agentInfoPanel.SetActive(false);
        trainAgentPanel.SetActive(false);
        agentCollabPanel.SetActive(false);
        viewAnswersPanel.SetActive(false);
        typeAnswerPanel.SetActive(false);
    }

    public void ToggleMenu()
    {
        // Toggle the main menu panel's visibility
        menuPanel.SetActive(!menuPanel.activeSelf);
        hamburgerIcon.SetActive(!hamburgerIcon.activeSelf);

        // If the submenu is active, deactivate it when toggling the main menu
        if (charactersSubMenu.activeSelf)
        {
            ToggleCharactersSubMenu();
        }
    }

    public void ToggleCharactersSubMenu()
    {
        // Toggle the submenu's visibility
        charactersSubMenu.SetActive(!charactersSubMenu.activeSelf);

        // Hide the original "Characters" button when the submenu is active
        menuPanel.SetActive(!menuPanel.activeSelf);
    }

    public void ToggleWorldSettingsPanel()
    {
        worldSettingsPanel.SetActive(!worldSettingsPanel.activeSelf);
    }

    public void TogglePlayersListPanel()
    {
        playersListPanel.SetActive(!playersListPanel.activeSelf);
    }

    public void ToggleHatchedPanel()
    {
        hatchedPanel.SetActive(!hatchedPanel.activeSelf);
        incubatingPanel.SetActive(!incubatingPanel.activeSelf);
        swipePrefab.SetActive(!swipePrefab.activeSelf);
    }

    public void ToggleCreateAgentPanel(){
        createAgentPanel.SetActive(!createAgentPanel.activeSelf);
    }

    public void ToggleVisualDescPanel(){
        visualDescPanel.SetActive(!visualDescPanel.activeSelf);
    }

    public void ToggleConfirmCreatePanel(){
        confirmCreatePanel.SetActive(!confirmCreatePanel.activeSelf);
    }
    public void ToggleAgentInfoPanel(){
        agentInfoPanel.SetActive(!agentInfoPanel.activeSelf);
    }
    public void ToggleTrainAgentPanel(){
        trainAgentPanel.SetActive(!trainAgentPanel.activeSelf);
    }
    public void ToggleAgentCollabPanel(){
        agentCollabPanel.SetActive(!agentCollabPanel.activeSelf);
    }
    public void ToggleViewAnswersPanel(){
        viewAnswersPanel.SetActive(!viewAnswersPanel.activeSelf);
    }
    public void ToggleTypeAnswerPanel(){
        typeAnswerPanel.SetActive(!typeAnswerPanel.activeSelf);
    }
}