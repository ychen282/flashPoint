﻿using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Collections;
using SocketIO;

public class OperationManager
{
    public GameManager gm;

    public List<Button> prefabs;

    List<Operation> possibleOp = new List<Operation>();

    public GameObject opPanel;

    public List<GameObject> options;

    private int x, z;

    public SocketIOComponent socket;

    public OperationManager(GameManager gm)
    {
        this.gm = gm;

        prefabs = gm.prefabs;
        opPanel = gm.opPanel;
        options = gm.options;

        opPanel.SetActive(false);


    }

    public void selectTile(int x, int z)
    {
        this.x = x;
        this.z = z;

        possibleOp = new List<Operation>();

        getPossibleOps();

        Debug.Log(possibleOp.Count);

        if(possibleOp.Count>0) opPanel.SetActive(true);

        for(int i = 0; i < possibleOp.Count; i++)
        {
            Button newObject = gm.instantiateOp(possibleOp[i].prefab, options[i].transform,true);
            switch (possibleOp[i].type) {
                case OperationType.Move:
                    newObject.onClick.AddListener(move);
                    break;
                case OperationType.Treat:
                    newObject.onClick.AddListener(treat);
                    break;
                case OperationType.CarryV:
                    newObject.onClick.AddListener(carryV);
                    break;
                case OperationType.LeadV:
                    newObject.onClick.AddListener(leadV);
                    break;
                case OperationType.CarryHazmat:
                    newObject.onClick.AddListener(carryHazmat);
                    break;
                case OperationType.RemoveHazmat:
                    newObject.onClick.AddListener(removeHazmat);
                    break;
                case OperationType.Command:
                    newObject.onClick.AddListener(command);
                    break;
                case OperationType.Imaging:
                    newObject.onClick.AddListener(imaging);
                    break;
                case OperationType.ExtingSmoke:
                    newObject.onClick.AddListener(extingSmoke);
                    break;
                case OperationType.ExtingFire:
                    newObject.onClick.AddListener(extingFire);
                    break;
                case OperationType.Drive:
                    newObject.onClick.AddListener(drive);
                    break;
                case OperationType.Remote:
                    newObject.onClick.AddListener(remote);
                    break;
                case OperationType.Ride:
                    newObject.onClick.AddListener(ride);
                    break;
                case OperationType.StopDrive:
                    newObject.onClick.AddListener(stopDrive);
                    break;
                case OperationType.GetOff:
                    newObject.onClick.AddListener(getOff);
                    break;
                case OperationType.DeckGun:
                    newObject.onClick.AddListener(move);
                    break;
            }
        }
    }

    void getPossibleOps()
    {

        int currentX = gm.tileMap.selectedUnit.currentX/6;
        int currentZ = gm.tileMap.selectedUnit.currentZ/6;

        Debug.Log(currentX);
        Debug.Log(currentZ);

        int diffX = Math.Abs(x - currentX);
        int diffZ = Math.Abs(z - currentZ);

        Debug.Log(diffX);
        Debug.Log(diffZ);

        if (diffX + diffZ == 0) //same tile (hazmat, poi)
        {
            int[] key = new int[] { x, z };
            Fireman fireman = gm.fireman;
            //------ POI---------------
            if (gm.pOIManager.placedPOI.ContainsKey(key))
            {
                POI p = gm.pOIManager.placedPOI[key];
                if (p.status == POIStatus.Treated)
                {
                    Operation op = new Operation(this, OperationType.LeadV);
                    possibleOp.Add(op);
                }
                if (p.type == POIType.Victim)
                {
                    if (!fireman.carryingVictim)
                    {
                        Operation op = new Operation(this, OperationType.CarryV);
                        possibleOp.Add(op);
                    }
                    if (fireman.role == Role.Paramedic&&fireman.FreeAP>=1)
                    {
                        Operation op = new Operation(this, OperationType.Treat);
                        possibleOp.Add(op);
                    }
                }
            }

            //------Hazmat------------
            if (gm.hazmatManager.placedHazmat.ContainsKey(key))
            {
                if (fireman.role == Role.HazmatTech&&fireman.FreeAP>=2)
                {
                    Operation op = new Operation(this, OperationType.RemoveHazmat);
                    possibleOp.Add(op);
                }
                if (!fireman.carryingVictim)
                {
                    Operation op = new Operation(this, OperationType.CarryHazmat);
                    possibleOp.Add(op);
                }
            }

            if (gm.tileMap.tiles[x, z] == 3 ) // fire deck gun && ride
            {
                int vx = gm.engine.GetComponent<Engine>().x / 6;
                int vz = gm.engine.GetComponent<Engine>().z / 6;

                if (vx == x && vz == z)
                {
                    if (fireman.driving)
                    {
                        Operation op = new Operation(this, OperationType.StopDrive);
                        possibleOp.Add(op);
                    }

                    int minX=0, maxX = 0;
                    int minZ=0, maxZ = 0;
                    if (x < 5 && z < 4)
                    {
                        minX = 1;
                        maxX = 4;
                        minZ = 1;
                        maxZ = 3;
                    }
                    else if (x >= 5 && z < 4)
                    {
                        minX = 5;
                        maxX = 8;
                        minZ = 1;
                        maxZ = 3;
                    }
                    else if (x >= 5 && z >= 4)
                    {
                        minX = 5;
                        maxX = 8;
                        minZ = 4;
                        maxZ = 6;
                    }
                    else if (x < 5 && z >= 4)
                    {
                        minX = 1;
                        maxX = 4;
                        minZ = 4;
                        maxZ = 6;
                    }
                    bool existsF = false;
                    foreach (JSONObject o in gm.players.Values)
                    {
                        for(int i = minX; i <= maxX; i++)
                        {
                            for(int j = minZ; j <= maxZ; j++)
                            {
                                if (o["Location"].Equals(i / 6 + "," + j / 6))
                                {
                                    existsF = true;
                                    break;
                                }
                            }
                            if (existsF) break;
                        }
                        if (existsF) break;
                    }
                    if (!existsF)
                    {
                        Operation op = new Operation(this, OperationType.DeckGun);
                        possibleOp.Add(op);
                    }

                    if (fireman.riding)
                    {
                        Operation op = new Operation(this, OperationType.GetOff);
                        possibleOp.Add(op);
                    }

                    if (!fireman.riding)
                    {
                        Operation op = new Operation(this, OperationType.Ride);
                        possibleOp.Add(op);
                    }
                }
            }

            if(gm.tileMap.tiles[x, z] == 4)
            {
                int vx = gm.ambulance.GetComponent<Ambulance>().x / 6;
                int vz = gm.ambulance.GetComponent<Ambulance>().z / 6;

                if (fireman.FreeAP >= 2 && (Math.Abs(currentX - vx) != 9 && Math.Abs(currentZ - vz) != 7)){
                    Operation op = new Operation(this, OperationType.Remote);
                    possibleOp.Add(op);
                }
                if (fireman.FreeAP >= 4 && (Math.Abs(currentX - vx) == 9 || Math.Abs(currentZ - vz) == 7))
                {
                    Operation op = new Operation(this, OperationType.Remote);
                    possibleOp.Add(op);
                }
            }
        }
        else if (diffX + diffZ == 1) // neighbor tile (fire, move)
        {
            Debug.Log("neighbor");
            Boolean extingFire = false;
            Boolean extingsmoke = false;
            // check obstable (wall/closed door)
            if (currentX - x == 0)// same column, check above and below
            {
                if (currentZ < z) // below the target
                {
                    int[] key = new int[] { x, z };
                    if (!gm.wallManager.hwallStores.ContainsKey(key)||gm.wallManager.hwallStores.ContainsKey(key) && gm.wallManager.hwallStores[key].GetComponent<Wall>().type == 2) // no wall or destroyed wall
                    {
                        if (gm.tileMap.tiles[x, z] >= 1 && gm.tileMap.tiles[x, z] <= 2) 
                        {
                            extingsmoke = true;
                        }

                        if (gm.tileMap.tiles[x, z] == 2)
                        {
                            extingFire = true;
                        }
                    }
                }
                else // above the target
                {
                    int[] key = new int[] { currentX, currentZ };
                    if (!gm.wallManager.hwallStores.ContainsKey(key) || gm.wallManager.hwallStores.ContainsKey(key) && gm.wallManager.hwallStores[key].GetComponent<Wall>().type == 2) // no wall or destroyed wall
                    {
                        if (gm.tileMap.tiles[x, z] >= 1 && gm.tileMap.tiles[x, z] <= 2)
                        {
                            extingsmoke = true;
                        }

                        if (gm.tileMap.tiles[x, z] == 2)
                        {
                            extingFire = true;
                        }
                    }
                }
            }
            else if (currentZ == z) // same row, check right and left
            {
                if (currentX < x) // left of the target
                {
                    int[] key = new int[] { x, z };
                    if (!gm.wallManager.vwallStores.ContainsKey(key) || gm.wallManager.vwallStores.ContainsKey(key) && gm.wallManager.vwallStores[key].GetComponent<Wall>().type == 2) // no wall or destroyed wall
                    {
                        if (gm.tileMap.tiles[x, z] >= 1 && gm.tileMap.tiles[x, z] <= 2)
                        {
                            extingsmoke = true;
                        }

                        if (gm.tileMap.tiles[x, z] == 2)
                        {
                            extingFire = true;
                        }
                    }
                }
                else // right to the target
                {
                    int[] key = new int[] { currentX, currentZ };
                    if (!gm.wallManager.vwallStores.ContainsKey(key) || gm.wallManager.vwallStores.ContainsKey(key) && gm.wallManager.vwallStores[key].GetComponent<Wall>().type == 2) // no wall or destroyed wall
                    {
                        if (gm.tileMap.tiles[x, z] >= 1 && gm.tileMap.tiles[x, z] <= 2)
                        {
                            extingsmoke = true;
                        }

                        if (gm.tileMap.tiles[x, z] == 2)
                        {
                            extingFire = true;
                        }
                    }
                }
            }

            // check for AP

            if (gm.tileMap.selectedUnit.role == Role.Paramedic || gm.tileMap.selectedUnit.role == Role.RescueSpec) // paramedic and rescue spec: double AP
            {
                if (extingsmoke&&gm.tileMap.selectedUnit.FreeAP>=2)
                {
                    Operation op = new Operation(this, OperationType.ExtingSmoke);
                    possibleOp.Add(op);
                }
                if (extingFire && gm.tileMap.selectedUnit.FreeAP >= 4)
                {
                    Operation op = new Operation(this, OperationType.ExtingFire);
                    possibleOp.Add(op);
                }
            }
            else if (gm.tileMap.selectedUnit.role == Role.CAFS) // cafs
            {
                if(extingsmoke && (gm.tileMap.selectedUnit.FreeAP+ gm.tileMap.selectedUnit.remainingSpecAp) >= 1)
                {
                    Operation op = new Operation(this, OperationType.ExtingSmoke);
                    possibleOp.Add(op);
                }
                if (extingFire && (gm.tileMap.selectedUnit.FreeAP + gm.tileMap.selectedUnit.remainingSpecAp) >= 2)
                {
                    Operation op = new Operation(this, OperationType.ExtingFire);
                    possibleOp.Add(op);
                }

            }
            else
            {
                if (extingsmoke && gm.tileMap.selectedUnit.FreeAP >= 1)
                {
                    Operation op = new Operation(this, OperationType.ExtingSmoke);
                    possibleOp.Add(op);
                }
                if (extingFire && gm.tileMap.selectedUnit.FreeAP >= 2)
                {
                    Operation op = new Operation(this, OperationType.ExtingFire);
                    possibleOp.Add(op);
                }
            }

            //----for move
            Boolean moveTo = false;
            int[] keyM = new int[2];
            if (currentX == x) // same column
            {
                if (currentZ < z) // below the target
                {
                    keyM[0] = x;
                    keyM[1] = z;
                }
                else // above
                {
                    keyM[0] = currentX;
                    keyM[1] = currentZ;
                }
                if (!gm.wallManager.hwallStores.ContainsKey(keyM) || gm.wallManager.hwallStores.ContainsKey(keyM) && gm.wallManager.hwallStores[keyM].GetComponent<Wall>().type == 2) {
                    moveTo = true;
                }
            }
            else // same row
            {
                if (currentX < x) // left
                {
                    keyM[0] = x;
                    keyM[1] = z;
                }
                else
                {
                    keyM[0] = currentX;
                    keyM[1] = currentZ;
                }
                if (!gm.wallManager.vwallStores.ContainsKey(keyM) || gm.wallManager.vwallStores.ContainsKey(keyM) && gm.wallManager.vwallStores[keyM].GetComponent<Wall>().type == 2)
                {
                    moveTo = true;
                }
            }
            Debug.Log(extingFire);
            Debug.Log(gm.tileMap.selectedUnit.FreeAP);
            Debug.Log(moveTo);
            if (moveTo) {
                Fireman fireman = gm.tileMap.selectedUnit;
                if (fireman.role == Role.RescueSpec)
                {
                    if (extingFire && fireman.FreeAP + fireman.remainingSpecAp >= 2)
                    {
                        Operation op = new Operation(this, OperationType.Move);
                        possibleOp.Add(op);
                    }else if (!extingFire && fireman.FreeAP + fireman.remainingSpecAp >= 1)
                    {
                        Operation op = new Operation(this, OperationType.Move);
                        possibleOp.Add(op);
                    }
                }
                else
                {
                    Debug.Log("else");
                    if(extingFire && fireman.FreeAP >= 2)
                    {
                        Operation op = new Operation(this, OperationType.Move);
                        possibleOp.Add(op);
                    }else if (!extingFire && fireman.FreeAP >= 1)
                    {

                        Operation op = new Operation(this, OperationType.Move);
                        possibleOp.Add(op);
                    }
                }
            }

        }
        else // not neighboring 
        {
            int[] key = new int[] { x, z };
            Fireman fireman = gm.tileMap.selectedUnit;
            if (fireman.role == Role.ImagingTech&&fireman.FreeAP>=1&&gm.pOIManager.placedPOI.ContainsKey(key)&&gm.pOIManager.placedPOI[key].status==POIStatus.Hidden)
            {
                Operation op = new Operation(this, OperationType.Imaging);
                possibleOp.Add(op);
            }

            if (fireman.role == Role.Captain)
            {
                foreach(JSONObject o in gm.players.Values)
                {
                    if (o["Location"] .Equals( x*6 + "," + z*6))
                    {
                        Operation op = new Operation(this, OperationType.Command);
                        possibleOp.Add(op);
                    }
                }
            }

            if (gm.tileMap.tiles[x, z] == 3) // engine
            {
                int vx = gm.engine.GetComponent<Engine>().x / 6;
                int vz = gm.engine.GetComponent<Engine>().z / 6;

                if (gm.tileMap.tiles[fireman.currentX/6, fireman.currentZ/6] == 3&&currentX==vx&&currentZ==vz)
                {
                    if (fireman.FreeAP >= 2 && (Math.Abs(x - currentX) !=9 && Math.Abs(currentZ - z) !=7 ))
                    {
                        Operation op = new Operation(this, OperationType.Drive);
                        possibleOp.Add(op);
                    }

                    if (fireman.FreeAP >= 4 && (Math.Abs(x - currentX) ==9 || Math.Abs(z - currentZ) ==7))
                    {
                        Operation op = new Operation(this, OperationType.Drive);
                        possibleOp.Add(op);
                    }

                }
            }

            if (gm.tileMap.tiles[x, z] == 4) // ambulance
            {
                int vx = gm.ambulance.GetComponent<Ambulance>().x / 6;
                int vz = gm.ambulance.GetComponent<Ambulance>().z / 6;

                if (fireman.FreeAP >= 2 && (Math.Abs(x - currentX) != 9 && Math.Abs(currentZ - z) != 7))
                {
                    if (currentX == vx && currentZ == vz)
                    {
                        Operation op = new Operation(this, OperationType.Drive);
                        possibleOp.Add(op);
                    }

                    Operation op1 = new Operation(this, OperationType.Remote);
                    possibleOp.Add(op1);
                }

                if (fireman.FreeAP >= 4 && (Math.Abs(x - currentX) == 9 || Math.Abs(z - currentZ) == 7))
                {
                    if (currentX == vx && currentZ == vz)
                    {
                        Operation op = new Operation(this, OperationType.Drive);
                        possibleOp.Add(op);
                    }
                    Operation op1 = new Operation(this, OperationType.Remote);
                    possibleOp.Add(op1);
                }
            }
        }


    }

    //void Start()
    //{
    //    opPanel.SetActive(false);


    //    //-------button click------------
    //    prefabs[0].onClick.AddListener(move);
    //    prefabs[1].onClick.AddListener(extingSmoke);
    //    prefabs[2].onClick.AddListener(extingFire);
    //    prefabs[3].onClick.AddListener(treat);
    //    prefabs[4].onClick.AddListener(carryV);
    //    prefabs[5].onClick.AddListener(leadV);
    //    prefabs[6].onClick.AddListener(removeHazmat);
    //    prefabs[7].onClick.AddListener(carryHazmat);
    //    prefabs[8].onClick.AddListener(command);
    //    prefabs[9].onClick.AddListener(imaging);
    //    prefabs[10].onClick.AddListener(drive);
    //    prefabs[11].onClick.AddListener(remote);
    //    prefabs[12].onClick.AddListener(ride);
    //    prefabs[13].onClick.AddListener(deckGun);
    //    prefabs[14].onClick.AddListener(stopDrive);
    //    prefabs[15].onClick.AddListener(getOff);
    //    //prefabs[2].onClick.AddListener(extingFire);
    //    //prefabs[2].onClick.AddListener(extingFire);
    //    //prefabs[2].onClick.AddListener(extingFire);

    //}



    //----Operations---------------

    public void move()
    {
        Debug.Log("move");
    }

    public void extingSmoke()
    {

    }

    public void extingFire()
    {

    }

    public void treat()
    {

    }

    public void carryV()
    {

    }

    public void leadV()
    {

    }

    public void removeHazmat()
    {

    }

    public void carryHazmat()
    {

    }

    public void imaging()
    {

    }

    public void command()
    {

    }

    public void drive()
    {

    }

    public void remote()
    {

    }

    public void ride()
    {

    }

    public void deckGun()
    {

    }

    public void stopDrive()
    {

    }

    public void getOff()
    {

    }

    public void cancel()
    {
        opPanel.SetActive(false);
        Debug.Log("cancel");
    }
}

