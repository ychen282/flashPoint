﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using SocketIO;
using System;
using System.Linq;
using UnityEngine.UI;

//using Newtonsoft.Json;
//using System.Web.Script.Serialization;


public class GameManager: MonoBehaviour
{
    public SocketIOComponent socket;
    public GameObject firemanObject;
    public GameObject firemanplusObject;
    public GameObject ambulance;
    public GameObject engine;
  //  public GameObject clickableVehicle;
    public TileType[] tileTypes;
    public DoorType[] doorTypes;
    public WallType[] wallTypes;
 //   public VehicleType[] vehicleTypes;
    public GameObject[] poiPrefabs;
	public int mapSizeX = 10;
	public int mapSizeZ = 8;
	public int damaged_wall_num = 0;
	public int rescued_vict_num = 0;

	public JSONObject game_info = StaticInfo.game_info;

    public WallManager wallManager;
 //   public VehicleManager vehicleManager;
    private TileMap tileMap;
    public DoorManager doorManager;
	public FireManager fireManager;
    public POIManager pOIManager;

    private JSONObject room;
    private JSONObject participants;
    private String level;
    private String numberOfPlayer;
    private Dictionary<String, JSONObject> players = new Dictionary<string, JSONObject>();
    public Ambulance amB;
    public Engine enG;
    public Fireman fireman;

    public Boolean isMyTurn = false;

    public List<Notification> chatLog = new List<Notification>();
    public GameObject notificationPanel, notificationText;

    public Text chat;

    public Text nameAP;


    void Start()
    {
        StartCoroutine(ConnectToServer());
        socket.On("LocationUpdate_SUCCESS", LocationUpdate_SUCCESS);
        socket.On("TileUpdate_Success", TileUpdate_Success);
        socket.On("WallUpdate_Success", WallUpdate_Success);
        socket.On("checkingTurn_Success", checkingTurn_Success);
        socket.On("changingTurn_Success", changingTurn_Success);
        socket.On("isMyTurnUpdate", isMyTurnUpdate);
        socket.On("sendChat_Success", sendChat_Success);
        socket.On("DoorUpdate_Success", DoorUpdate_Success);
        socket.On("sendNotification_Success",sendNotification_SUCCESS);

        if (game_info != null)
        {
            room = game_info[StaticInfo.roomNumber];
            participants = room["participants"];
            level = room["level"].ToString();
            numberOfPlayer = room["numberOfPlayer"].ToString();

            List<string> p = participants.keys;
            foreach (var v in p)
            {
                //Debug.Log(participants[v]);
                var o = participants[v];
                players[v] = o;
                //Debug.Log(players[v]);
            }
        }

        fireman = initializeFireman();
        amB = initializeAmbulance();
        enG = initializeEngine();
        wallManager = new WallManager(wallTypes,this);
        doorManager = new DoorManager(doorTypes,this);
    //    vehicleManager = new VehicleManager(vehicleTypes,this);
        tileMap = new TileMap(tileTypes,this, fireman, enG, amB);
		fireManager = new FireManager(this, tileMap, mapSizeX, mapSizeZ);
        pOIManager = new POIManager(this);

        displayAP(Convert.ToInt32(players[StaticInfo.name]["AP"].ToString()));
     //   vehicleManager.StartvehicleManager();

        tileMap.GenerateFiremanVisual(players);
        registerNewFireman(fireman);
        checkTurn();	//initialize isMyTurn variable at start

    }

    public void displayAP(int ap){
        nameAP.text= StaticInfo.name + " has " + fireman.FreeAP + " AP" ;
    }

    void WallUpdate_Success(SocketIOEvent obj)
    {
        Debug.Log("wall update successful");
        var x = Convert.ToInt32(obj.data.ToDictionary()["x"]);
        var z = Convert.ToInt32(obj.data.ToDictionary()["z"]);
        var type = Convert.ToInt32(obj.data.ToDictionary()["type"]);
        var horizontal = Convert.ToInt32(obj.data.ToDictionary()["horizontal"]);

        //Debug.Log(x);
        //Debug.Log(z);
        //Debug.Log(type);
        Debug.Log(obj.data);
        Debug.Log(obj.data.ToDictionary()["x"]);
        Debug.Log(obj.data.ToDictionary()["z"]);
        Debug.Log(obj.data.ToDictionary()["type"]);
        Debug.Log(obj.data.ToDictionary()["horizontal"]);

		// Bottom is temporarily commented out:
		//wallManager.BreakWall(x, z, type, horizontal, false);
	}

	void DoorUpdate_Success(SocketIOEvent obj)
    {
        Debug.Log("door update successful");
        var x = Convert.ToInt32(obj.data.ToDictionary()["x"]);
        var z = Convert.ToInt32(obj.data.ToDictionary()["z"]);
        var type = Convert.ToInt32(obj.data.ToDictionary()["type"]);
        var toType = Convert.ToInt32(obj.data.ToDictionary()["toType"]);

        //Debug.Log(x);
        //Debug.Log(z);
        //Debug.Log(type);
        Debug.Log(obj.data);
        Debug.Log(obj.data.ToDictionary()["x"]);
        Debug.Log(obj.data.ToDictionary()["z"]);
        Debug.Log(obj.data.ToDictionary()["type"]);
        Debug.Log(obj.data.ToDictionary()["toType"]);

        doorManager.ChangeDoor(x, z, toType, type);
    }

    void TileUpdate_Success(SocketIOEvent obj)
    {
        Debug.Log("tile update successful");
        var x = Convert.ToInt32(obj.data.ToDictionary()["x"]);
        var z = Convert.ToInt32(obj.data.ToDictionary()["z"]);
        var type = Convert.ToInt32(obj.data.ToDictionary()["type"]);

        //Debug.Log(x);
        //Debug.Log(z);
        //Debug.Log(type);
        Debug.Log(obj.data.ToDictionary()["x"]);
        Debug.Log(obj.data.ToDictionary()["z"]);
        Debug.Log(obj.data.ToDictionary()["type"]);

		// Bottom is temporarily commented out:
		// tileMap.buildNewTile(x, z,type);
    }

    void LocationUpdate_SUCCESS(SocketIOEvent obj)
    {
        Debug.Log("Location update successful");

        //update with latest objects
        room = obj.data[StaticInfo.roomNumber];
        participants = room["participants"];
        level = room["level"].ToString();
        numberOfPlayer = room["numberOfPlayer"].ToString();

        List<string> p = participants.keys;
        foreach (var v in p)
        {
            var o = participants[v];
            players[v] = o;
            Debug.Log(v);
            Debug.Log(players[v]);
        }
        tileMap.UpdateFiremanVisual(players);

    }

    void checkingTurn_Success(SocketIOEvent obj)
    {
        //accept value here
        var result = obj.data.ToDictionary()["status"];
        Debug.Log(result);

        if (result.Equals("True"))
        {
            isMyTurn = true;
        }
        else
        {
            isMyTurn = false;
        }
    }

    void changingTurn_Success(SocketIOEvent obj)
    {
        Debug.Log("in changingTurn_Success");
        var name = obj.data.ToDictionary()["Turn"];
        Debug.Log(name);

        if (name.Equals(StaticInfo.name))
        {
            isMyTurn = true;
			Debug.Log("It is now your turn! Refreshing AP");
			fireman.refreshAP();
		}
        else
        {
            isMyTurn = false;
			Debug.Log("It is now someone else's turn!");
		}

    }

    void isMyTurnUpdate(SocketIOEvent obj)
    {
        Debug.Log("in isMyTurnUpdate");
        var name = obj.data.ToDictionary()["Turn"];
        Debug.Log(name);

        if (name.Equals(StaticInfo.name))
        {
            isMyTurn = true;
            sendNotification(". It's your turn.");
        }
        else
        {
            isMyTurn = false;
        }
    }

    void sendChat_Success(SocketIOEvent obj)
    {
        Debug.Log("in sendChat_Success");

        var name = obj.data.ToDictionary()["name"];
        var chat = obj.data.ToDictionary()["chat"];

        var chatString = name + " : " + chat;
        if(chatLog.Count>10){
            Destroy(chatLog[0].textObject.gameObject);
            chatLog.Remove(chatLog[0]);
        }

        Notification notification=new Notification();
        notification.msg=chatString;
        GameObject newText=Instantiate(notificationText,notificationPanel.transform);
        notification.textObject=newText.GetComponent<Text>();
        notification.textObject.text=notification.msg;

        chatLog.Add(notification);
        Debug.Log(chatString);
        // chatLog.Add(chatString);
    }

    IEnumerator ConnectToServer()
    {
        yield return new WaitForSeconds(0.5f);

        socket.Emit("USER_CONNECT");

        yield return new WaitForSeconds(0.5f);

    }

    public Fireman initializeFireman()
    {
        var location = players[StaticInfo.name]["Location"].ToString();
        location = location.Substring(1, location.Length - 2);
        //Debug.Log(location);

        var cord = location.Split(',');
        int x = Convert.ToInt32(cord[0]);
        int z = Convert.ToInt32(cord[1]);

        int ap = Convert.ToInt32(players[StaticInfo.name]["AP"].ToString());
		Debug.Log("Created '" + StaticInfo.name + "' with AP =" + ap);
        Fireman f = new Fireman(StaticInfo.name, Colors.Blue, firemanObject, firemanplusObject, x, z, ap, this);


        return f;
    }

    public Ambulance initializeAmbulance()
    {
        Ambulance amb = new Ambulance(ambulance, 9, 4, this);

        return amb;
    }

    public Engine initializeEngine()
    {
        Engine eng = new Engine(engine, 0, 5, this);

        return eng;
    }

    public void registerNewFireman(Fireman f)
    {
        Debug.Log("let other user know a new fireman has been created");
        UpdateLocation(f.currentX, f.currentZ);//let other user know a new fireman has been created
    }

    public GameObject instantiateObject(GameObject w, Vector3 v, Quaternion q)
    {
        GameObject objectW = (GameObject)Instantiate(w, v, q);
        return objectW;
    }

    public void DestroyObject(GameObject w)
    {
        Destroy(w);
    }

    public void UpdateLocation(int x, int z)
    {
        Debug.Log("Update Location");
        StaticInfo.Location = new int[] { x, z };
        Dictionary<String, String> update = new Dictionary<string, string>();
        update["room"] = StaticInfo.roomNumber;
        update["name"] = StaticInfo.name;
        update["Location"] = StaticInfo.Location[0] + "," + StaticInfo.Location[1];

        socket.Emit("Location", new JSONObject(update));
    }

    public void UpdateTile(int x, int z, int type)
    {
        Debug.Log("Update tile");
        Dictionary<String, string> updateTile = new Dictionary<string, string>();
        updateTile["x"] = x.ToString();
        updateTile["z"] = z.ToString();
        updateTile["type"] = type.ToString();

        socket.Emit("UpdateTile", new JSONObject(updateTile));
    }

    public void UpdateWall(int x, int z, int type, int horizontal)
    {
        Debug.Log("Update wall");
        Dictionary<String, string> updateWall = new Dictionary<string, string>();
        updateWall["x"] = x.ToString();
        updateWall["z"] = z.ToString();
        updateWall["type"] = type.ToString();
        updateWall["horizontal"] = horizontal.ToString();

        socket.Emit("UpdateWall", new JSONObject(updateWall));
    }

    public void UpdateDoor(int x, int z, int toType, int type)
    {
        Debug.Log("Update door");
        Dictionary<String, string> updateDoor = new Dictionary<string, string>();
        updateDoor["x"] = x.ToString();
        updateDoor["z"] = z.ToString();
        updateDoor["toType"] = toType.ToString();
        updateDoor["type"] = type.ToString();

        socket.Emit("UpdateDoor", new JSONObject(updateDoor));
    }

    public void EndTurn()
    {
        Debug.Log("Ending Turn");

		// advanceFire, n.b parameters only matter for testing
		fireManager.advanceFire(1, 4, false);
		Debug.Log("Finished advFire, redistributing AP");
		



		checkTurn();
        //do stuff here...

        //if (isMyTurn)
        //{
			changeTurn();
        //}
        //else
        //{
        //    Debug.Log("This not your turn! Don't click end turn!");
        //}
    }

    public void checkTurn()
    {
        Debug.Log("checking turn");
        Dictionary<String, String> checkingTurn = new Dictionary<string, string>();
        checkingTurn["room"] = StaticInfo.roomNumber;
        checkingTurn["name"] = StaticInfo.name;

        socket.Emit("checkingTurn", new JSONObject(checkingTurn));
        //System.Threading.Thread.Sleep(2000);
    }

    public void changeTurn()
    {
        Debug.Log("changing turn");
        Dictionary<String, String> changingTurn = new Dictionary<string, string>();
        changingTurn["room"] = StaticInfo.roomNumber;
        changingTurn["name"] = StaticInfo.name;

        socket.Emit("changingTurn", new JSONObject(changingTurn));
    }

    public void SendChat()
    {
        Debug.Log(chat.text);
        Dictionary<String, String> sendChat = new Dictionary<string, string>();
        sendChat["name"] = StaticInfo.name;
        sendChat["chat"] = chat.text;

        var chatString = StaticInfo.name + " : " + chat.text;
        if(chatLog.Count>20){
            Destroy(chatLog[0].textObject.gameObject);
            chatLog.Remove(chatLog[0]);
        }

        Notification notification=new Notification();
        notification.msg=chatString;
        GameObject newText=Instantiate(notificationText,notificationPanel.transform);
        notification.textObject=newText.GetComponent<Text>();
        notification.textObject.text=notification.msg;

        chatLog.Add(notification);
        // chatLog.Add(chatString);

        socket.Emit("sendChat", new JSONObject(sendChat));
    }

    public void sendNotification(string msg){
        Dictionary<string, string> message = new Dictionary<string, string>();
        message["name"]=StaticInfo.name;
        message["text"]=msg;
        socket.Emit("sendNotification",new JSONObject(message));
    }

    void sendNotification_SUCCESS(SocketIOEvent obj){
        var name = obj.data.ToDictionary()["name"];
        var text = obj.data.ToDictionary()["text"];

        var chatString = name + " " + text;
        if(chatLog.Count>20){
            Destroy(chatLog[0].textObject.gameObject);
            chatLog.Remove(chatLog[0]);
        }

        Notification notification=new Notification();
        notification.msg=chatString;
        GameObject newText=Instantiate(notificationText,notificationPanel.transform);
        notification.textObject=newText.GetComponent<Text>();
        notification.textObject.text=notification.msg;

        chatLog.Add(notification);
        Debug.Log(chatString);
    }

}

public class Notification{
    public string msg;
    public Text textObject;
}