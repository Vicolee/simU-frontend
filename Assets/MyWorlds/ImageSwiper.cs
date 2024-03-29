using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using Clients;
using UnityEngine.Networking;
using Newtonsoft.Json.Converters;


public struct WorldSprite
{
    public Guid id;
    public string name;
    public Sprite sprite;
    public Guid creatorId;
}

public class ImageSwiper : MonoBehaviour
{
    public GameObject myWorldsPanel;
    float swipeThreshold = 500f;
    public int currentIndex = 0;
    private Vector2 startTouchPosition;
    private Vector2 currentSwipe;
    private RectTransform rectTransform;
    private Image displayImage;
    private List<HTTPClient.UserWorld> userWorlds;
    private List<WorldSprite> worldSprites;
    public Text displayWorldName;
    public Camera uiCamera;
    public GameObject leaveWorldPanel;
    private bool isLongPress = false;
    private float timeTouchStarted;
    private float longPressDuration = 1.0f; // Duration to trigger a long press

    void OnEnable()
    {
        if (myWorldsPanel != null) {
            MyWorldsManager myWorldsManager = myWorldsPanel.GetComponent<MyWorldsManager>();
            userWorlds = myWorldsManager.userWorlds;
            InitializeDisplayImage();
            rectTransform = GetComponent<RectTransform>(); // Get the RectTransform
            UpdateWorldsDisplay();        
        }
    }

    private void InitializeDisplayImage()
    {
        Transform displayImageTransform = FindDeepChildHelper(myWorldsPanel.transform, "ThumbnailImage");
        displayImage = displayImageTransform.GetComponent<Image>();
    }

    public void SetupUserWorlds(List<HTTPClient.UserWorld> userWorlds)
    {
        this.userWorlds = userWorlds;
        Debug.Log("In SetupUserWorlds userWorlds count: " + userWorlds.Count);
        StartLoadingUserWorlds();
    }

    private Transform FindDeepChildHelper(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName)
                return child;
            
            Transform found = FindDeepChildHelper(child, childName);
            if (found != null)
                return found;
        }
        return null; // Not found
    }


    private void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Vector2 localPoint;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, touch.position, uiCamera, out localPoint))
            {
                if (rectTransform.rect.Contains(localPoint)) // Check if the localPoint is within the rect
                {
                    switch (touch.phase)
                    {
                        case TouchPhase.Began:
                            startTouchPosition = touch.position;
                            timeTouchStarted = Time.time; // Record the time when the touch starts
                            isLongPress = false; // Reset long press flag
                            Debug.Log("Touch began");
                            break;

                        case TouchPhase.Moved:
                            if (!isLongPress) // Only check for swipe if it's not a long press
                            {
                                currentSwipe = touch.position - startTouchPosition;

                                if (currentSwipe.magnitude > swipeThreshold)
                                {
                                    if (currentSwipe.x < 0)
                                    {
                                        MoveToNextImage();
                                    }
                                    else if (currentSwipe.x > 0)
                                    {
                                        MoveToPreviousImage();
                                    }
                                    // Consider resetting startTouchPosition here if you want continuous swiping
                                }
                            }
                            break;

                        case TouchPhase.Stationary:
                            // Check if it's been a long press
                            if (Time.time - timeTouchStarted > longPressDuration && !isLongPress)
                            {
                                isLongPress = true; // Prevents further swipe detection for this touch
                                ShowLeaveWorldPanel(); // Show your delete confirmation
                            }
                            break;

                        case TouchPhase.Ended:
                        case TouchPhase.Canceled:
                            // Reset everything for the next touch
                            isLongPress = false;
                            break;
                    }
                }
            }
        }
    }

    public void StartLoadingUserWorlds()
    {
        Debug.Log("In StartLoadingUserWorlds");
        StartCoroutine(LoadWorldSprites(userWorlds));
    }

    IEnumerator LoadWorldSprites(List<HTTPClient.UserWorld> userWorldList)
    {
        worldSprites = new List<WorldSprite>(); // Initialize or clear the existing list
        Debug.Log("In LoadWorldSprites userWorld count: " + userWorldList.Count);
        HashSet<Guid> worldIds = new HashSet<Guid>();
        foreach (HTTPClient.UserWorld world in userWorldList)
        {
            if (worldIds.Contains(world.id))
            {
                continue;
            } else {
                worldIds.Add(world.id);
            }
            Debug.Log("Loading world: " + world.name);
            // Debug.Log("Loading world thumbnail: " + world.thumbnail_URL);

            string thumbnail_URL = world.thumbnail_URL;
            // Basic validation of the URL
            if (string.IsNullOrEmpty(thumbnail_URL) || !Uri.IsWellFormedUriString(thumbnail_URL, UriKind.Absolute))
            {
                Debug.Log($"Invalid or malformed URL: {thumbnail_URL}");
                thumbnail_URL = "https://picsum.photos/200"; // TODO: Use better default photo
            }

            UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(thumbnail_URL);
            yield return uwr.SendWebRequest(); // Wait for the download to complete

            if (uwr.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(uwr);
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                
                // Create a new WorldSprite and add it to the list
                worldSprites.Add(new WorldSprite { id = world.id, name = world.name, sprite = sprite, creatorId = world.creatorId });
            }
            else
            {
                Debug.LogError("Failed to download image: " + uwr.error);
            }

            uwr.Dispose(); // Manually dispose of the UnityWebRequest
        }

        // After all images are loaded, update user's world display
        UpdateWorldsDisplay();
    }

    public void MoveToNextImage()
    {
        // if (worldSprites.Count == 0)
        // {
        //     StartLoadingUserWorlds(); // try loading worlds again
        //     return;
        // }
        Debug.Log("Moving to next image");
        if (worldSprites.Count == 0)
        {
            Debug.Log("No worlds to move to");
            return;
        }
        currentIndex = (currentIndex + 1) % worldSprites.Count; // Wrap to the beginning if at the end
        UpdateWorldsDisplay();
    }

    public void MoveToPreviousImage()
    {
        if (worldSprites.Count == 0)
        {
            Debug.Log("No worlds to move to");
            return;
        }

        if (currentIndex == 0)
        {
            currentIndex = worldSprites.Count - 1; // Wrap to the end if at the beginning
        }
        else
        {
            currentIndex--;
        }
        UpdateWorldsDisplay();
    }

    private void UpdateWorldsDisplay()
    {
        if(worldSprites != null && worldSprites.Count > 0)
        {
            Debug.Log("Updating world display, worldSprites count: " + worldSprites.Count);
            displayImage.sprite = worldSprites[currentIndex].sprite;
            displayWorldName.text = worldSprites[currentIndex].name;
        }
    }

    private void ShowLeaveWorldPanel()
    {
        if (worldSprites.Count > 0)
        {
            leaveWorldPanel.SetActive(true);
        }
    }

    public string GetCurrentWorldId()
    {
        if (worldSprites.Count > 0)
        {
            return worldSprites[currentIndex].id.ToString();
        }
        return null; // or string.Empty if you prefer
    }

    public string GetCurrentWorldCreatorId()
    {
        if (worldSprites.Count > 0)
        {
            Debug.Log("Current world creator id: " + worldSprites[currentIndex].creatorId.ToString());
            return worldSprites[currentIndex].creatorId.ToString();
        }
        return null; // or string.Empty if you prefer
    }

    public string GetCurrentWorldName()
    {
        if (worldSprites.Count > 0)
        {
            return worldSprites[currentIndex].name;
        }
        return ""; // or string.Empty if you prefer
    }

    public void AddWorld()
    {
        myWorldsPanel.SetActive(true);
        StartLoadingUserWorlds(); 
        UpdateWorldsDisplay();
    }

    public void RemoveWorld()
    {
        Debug.Log("In removeworld userWorldsCount: " + userWorlds.Count);
        currentIndex = currentIndex % userWorlds.Count; // Wrap to the beginning if at the end
    }
}
