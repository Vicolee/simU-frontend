using System;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using Clients;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using System.Threading.Tasks;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector2 targetPosition;
    private Animator animator;
    private float moveSpeed = 5.0f;
     public Vector2 direction = Vector2.zero;
    private Guid collidedCharacterId;

    public GameObject InteractButton;
    public GameObject NurtureButton;
    public Tilemap tilemap; // Reference to the Tilemap
    public bool collided = false;
    public Collider2D collidedPlayer;


    // Start is called before the first frame update
    async void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    public void SetTilemap(Tilemap tilemap)
    {
        Debug.Log("Setting tilemap");
        this.tilemap = tilemap;
    }
    
    private async Task MovePlayerByClick()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            // Clicked on a UI element, do not move the player
            if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
            {
                return;
            }

            if (touch.phase == TouchPhase.Began)
            {
                targetPosition = Camera.main.ScreenToWorldPoint(touch.position);
                // targetPosition.x = (int) targetPosition.x;
                // targetPosition.y = (int) targetPosition.y;

                if (IsWithinTilemapBounds(targetPosition)){
                    await SignalRClient.Instance.UpdateLocation((int) targetPosition.x, (int) targetPosition.y);
                }
                else {
                    Debug.Log("Not going to send this location to the backend, it is out of the tilemap boundary.");
                }
            }
        }

        // This fixes the player automatically going to 0,0 on start
        if (targetPosition.x == 0f && targetPosition.y == 0f)
        {
            animator.SetBool("moving", false);
            rb.velocity = Vector2.zero;
            return;
        }

        // Calculate the direction from the current position to the target position
        direction = (targetPosition - (Vector2)transform.position).normalized;

        // Update the animator parameters
        animator.SetFloat("moveX", direction.x);
        animator.SetFloat("moveY", direction.y);

        // Check if the distance between the player and the target is greater than the stopping distance
        if (Vector2.Distance(targetPosition, transform.position) > 0.1f)
        {

            // Check if the next position is within the bounds of the tilemap
            if (IsWithinTilemapBounds(targetPosition))
            {
                if (!collided){
                // Move the player towards the target position
                animator.SetBool("moving", true);
                rb.velocity = new Vector2(Mathf.Round(direction.x * moveSpeed), Mathf.Round(direction.y * moveSpeed));
                }
                else{

                    if (Vector2.Distance(transform.position, collidedPlayer.transform.position) <= 0.75f) {
                        // Adjust the position to move away slightly
                        transform.position += new Vector3(1f, 1f, 0f);
                    }


                    // Check the local position after the InverseTransformPoint
                    Vector2 positionRelative = transform.InverseTransformPoint(collidedPlayer.transform.position);

                    float moveRelative = Vector2.Distance(positionRelative, direction);

                    if (moveRelative > 1.75f)
                    {
                        rb.velocity = new Vector2(Mathf.Round(direction.x * moveSpeed), Mathf.Round(direction.y * moveSpeed));
                        animator.SetBool("moving", true);
                    }
                    else
                        rb.velocity = Vector2.zero;
                        animator.SetBool("moving", false);
                    }
            }
            else
            {
                // Stop the player if the next position is outside the tilemap bounds
                targetPosition = transform.position;
                animator.SetBool("moving", false);
                rb.velocity = Vector2.zero;
            }
        }
        else
        {
            // If the player is within the stopping distance, stop its movement and animation
            animator.SetBool("moving", false);
            rb.velocity = Vector2.zero;
        }
    }

    private bool IsWithinTilemapBounds(Vector2 position)
    {
        // Convert world position to tile position
        Vector3Int cellPosition = tilemap.WorldToCell(position);

        // Check if the cell position is within the bounds of the tilemap
        return tilemap.cellBounds.Contains(cellPosition);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"Collided with GameObject: {collision.gameObject.name}, Tag: {collision.gameObject.tag}");
        
        if (collision.gameObject.CompareTag("user") || collision.gameObject.CompareTag("agent"))
        {
            InteractButton.SetActive(true);
            collided = true;
            collidedPlayer = collision;

            CharacterComponent characterComponent = collision.gameObject.GetComponent<CharacterComponent>();

            if (characterComponent != null)
            {
                // Access NPC information
                collidedCharacterId = characterComponent.GetCharacterId();
                string collidedCharacterType = characterComponent.GetCharacterType();

                Debug.Log($"Collided with Character Type: {collidedCharacterType} (ID: {collidedCharacterId})");

                // Store the CollidedCharacterId in PlayerPrefs to access in the Chat scene

                PlayerPrefs.SetString("CollidedCharacterId", collidedCharacterId.ToString());
                PlayerPrefs.SetString("CollidedCharacterType", collidedCharacterType);
            }
        }
        else if (collision.gameObject.CompareTag("egg")){
            collided = true;
            collidedPlayer = collision;

            CharacterComponent eggComponent = collision.gameObject.GetComponent<CharacterComponent>();

            if (eggComponent != null)
            {
                // Access NPC information
                collidedCharacterId = eggComponent.GetCharacterId();
                string collidedCharacterType = eggComponent.GetCharacterType();

                Debug.Log($"Collided with Character Type: {collidedCharacterType} (ID: {collidedCharacterId})");

                GameObject gameClient = GameObject.Find("GameClient");
                gameClient.GetComponent<GameClient>().eggId = collidedCharacterId;
            }
            NurtureButton.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        // Hide click to chat button
        InteractButton.SetActive(false);
        NurtureButton.SetActive(false);
        if (collided)
        {
            collided = false;
            collidedPlayer = null;
        }
    }

    public void StopMovement()
    {
        rb.velocity = Vector2.zero;
        animator.SetBool("moving", false);
    }

    // Update is called once per frame
    async void Update()
    {
        await MovePlayerByClick();
    }
}