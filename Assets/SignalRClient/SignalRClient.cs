using Microsoft.AspNetCore.SignalR.Client;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using UnityEngine;


namespace Clients {

public class SignalRClient
{
    private readonly string username;
    private readonly HubConnection _connection;
    private Dictionary<Guid, Location> userLocations = new Dictionary<Guid, Location>(); // userId: location info about user
    private static SignalRClient instance;

    private SignalRClient(string url, string authToken)
    {
        _connection = new HubConnectionBuilder()
            .WithUrl(url, options =>
            { 
                options.AccessTokenProvider = () => Task.FromResult(authToken);
            })
            .Build();
    }

    public static SignalRClient Instance
    {
        get
        {
            if (instance == null)
            {
                throw new Exception("SignalRClient has not been initialized. Call Initialize method first.");
            }
            return instance;
        }
    }

    public static async Task Initialize(string authToken)
    {
        if (instance == null)
        {
            // string baseURL = "http://localhost:5000/unity";
            string baseURL = "https://simyou.azurewebsites.net/unity";
            Debug.Log("Initializing SignalRClient with URL:" + baseURL);
            instance = new SignalRClient(baseURL, authToken);
            Debug.Log("Post instance assignment" + instance);
            await instance.ConnectAsync();
            if (instance._connection.State == HubConnectionState.Connected)
            {
                Debug.Log("IM ACTUALLY CONNECTED");
            }
            else{
                Debug.Log("I am not actually connected");
            }
        }         
    }

    public static bool IsConnected()
    {
        return instance != null && instance._connection.State == HubConnectionState.Connected;
    }

    /// <summary>
    /// Connects the client to the server.
    /// </summary>
    /// <returns></returns>
    public async Task ConnectAsync()
    {
        await _connection.StartAsync();
    }

    /// <summary>
    /// Disconnects the client from the server.
    /// </summary>
    /// <returns></returns>
    public async Task DisconnectAsync()
    {
        await _connection.StopAsync();
    }

    /// <summary>
    /// Broadcasts a message to all connected clients.
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public async Task BroadcastMessage(string funcName, string message)
    {
        await _connection.SendAsync(funcName, username, message);
    }

    /// <summary>
    /// Registers a handler for incoming messages.
    /// </summary>
    public void RegisterReceiveMessageHandler()
    {
        _connection.On<string, string>("ReceiveMessage", (user, message) =>
        {
            Console.WriteLine($"({user}): {message}");
        });
    }

    // Calls the backend method UpdateLocation with the current location of the user
    public async Task UpdateLocation(int xCoordinate, int yCoordinate)
    {
            var location = new Location
            {
                X_coordinate = xCoordinate,
                Y_coordinate = yCoordinate
            };

            try
            {
                await _connection.SendAsync("UpdateLocation", location.X_coordinate, location.Y_coordinate);
            } catch (Exception ex) 
            {
                Debug.Log("In updateLocation not Connected, failed to update");
            }
    }

     public void RegisterUpdateLocationHandler(OtherPlayerMovement otherPlayerMovementScript)
    {
        // TODO: Refactor handleError out into a combined handler
        _connection.On<string>("HandleError", (error) =>
        {
            Debug.Log($"Error: {error}");
        });
        _connection.On<Guid, int, int>("UpdateLocation", (userId, xCoord, yCoord) =>
        {
            Debug.Log($"User {userId} moved to X: {xCoord}, Y: {yCoord}");
            
            userLocations[userId] = new Location { X_coordinate = xCoord, Y_coordinate = yCoord }; // TODO: Check if this is needed
            otherPlayerMovementScript.UpdateLocation(userId, xCoord, yCoord);
        });
    }

    /// <summary>
    /// Sends a <paramref name="message"/> to the user with the given <paramref name="receiverId"/>
    /// </summary>
    /// <param name="receiverId">The ID of the message target</param>
    /// <param name="message">The actual contents of the message</param>
    /// <returns></returns>
    public async Task SendChat(Guid receiverId, string message)
    {
        try
        {
            await _connection.SendAsync("SendChat", receiverId, message);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending message: {ex.Message}");
        }
    }


    /// <summary>
    /// Handles a message received from another client through the server.
    /// </summary>
    /// <param name="sender">The client (can also be the server) sending the message</param>
    /// <param name="message">The content of the message</param>
    /// <returns></returns>
    public void MessageHandler(ChatManager.ChatManager chatManager)
    {
        _connection.On<Guid, Guid, string, bool>("MessageHandler", (Guid messageId, Guid senderId, string message, bool isOnline) =>
        {
            Debug.Log($"Message {messageId} received from user {senderId}: {message}. User is online: {isOnline}");
            chatManager.ReceiveMessage(messageId, senderId, message, isOnline);
        });
    }


    /// <summary>
    /// Handles a server request that checks if the user is online.
    /// The server will routinely send this request to check if the client is still logged in.
    /// The client should respond by sending a <see cref="IUnityServer.PingServer"/> request.
    /// </summary>
    /// <returns></returns>
    public async Task UserOnlineCheckHandler()
    {
        _connection.On("UserOnlineCheckHandler", async () =>
        {
            Debug.Log("Server checking if this client is online");
            await PingServer(); // Respond to the server that the user is online
        });
    }


    /// <summary>
    /// Notifies the server that the user is online. We use this to keep track of active users. 
    /// </summary>
    /// <returns></returns>
    public async Task PingServer()
    {
        try
        {
            await _connection.SendAsync("PingServer");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending message: {ex.Message}");
        }    
    }

    /// <summary>
    /// Handles a server request that notifies clients when a world's user logs into the server.
    /// </summary>
    /// <param name="userId">The ID of the newly-logged in user</param>
    /// <returns></returns>
    public void OnUserLoggedInHandler()
    {
        _connection.On<Guid>("OnUserLoggedInHandler", (Guid userId) =>
        {
            Debug.Log($"User {userId} has logged in. Setting them to online in world...");
        });
    }

    /// <summary>
    /// Handles a server request that notifies clients when a world's user logs into the server.
    /// </summary>
    /// <param name="userId">The ID of the newly-logged in user</param>
    /// <returns></returns>
    public void OnUserLoggedOutHandler()
    {
        _connection.On<Guid>("OnUserLoggedOutHandler", (Guid userId) =>
        {
            Debug.Log($"User {userId} has logged out. Setting them to offline in world...");
        });
    }


    /// <summary>
    /// Handles a server request that notifies clients when a new agent is added to the world.
    /// </summary>
    /// <param name="agentId">The ID of the newly-added agent</param>
    /// <returns></returns>
    public void OnAgentAddedHandler()
    {
        _connection.On<Guid>("OnAgentAddedHandler", (Guid agentId) =>
        {
            Debug.Log($"Agent {agentId} has been added to world. Adding agent to world...");
            // TODO: Add method on gameclient to add agent to world
        });
    }

    /// <summary>
    /// Handles a server request that notifies clients when a new user joins the world.
    /// </summary>
    /// <param name="userId">The ID of the newly-added user</param>
    /// <returns></returns>
    public void OnUserAddedToWorldHandler()
    {
        _connection.On<Guid>("OnUserAddedToWorldHandler", (Guid userId) =>
        {
            Debug.Log($"User {userId} has joined the world. Adding user to world...");
        });
    }


    /// <summary>
    /// Handles a server request that notifies clients when a user leaves the world.
    /// </summary>
    /// <param name="userId">The ID of the user who left the world</param>
    /// <returns></returns>
    public void OnUserRemovedFromWorldHandler()
    {
        _connection.On<Guid>("OnUserRemovedFromWorldHandler", (Guid userId) =>
        {
            Debug.Log($"User {userId} has been removed from the world. Removing user to world...");
        });
    }

    public class Location
    {
        public int X_coordinate { get; set; }
        public int Y_coordinate { get; set; }
    }
    public Dictionary<Guid, Location> UserLocations
    {
        get { return userLocations; }
    }

}
}