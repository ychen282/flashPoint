﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VicinityManager : MonoBehaviour {

	public GameManager gm;
	public int[,] tiles;
	public VicinityTile[,] VeteranVicinity; // Used only for Veteran bookkeeping
	public int f_x;
	public int f_z;

	// Constructor
	public VicinityManager( GameManager in_gm, int[,] tile_arr)
	{
		// Connect back to singleton GameManager
		gm = in_gm;

		//	Fetch info about tile status (fire, smoke etc)
		tiles = tile_arr;

		// Initiliaze the 2D array
		VeteranVicinity = new VicinityTile[gm.mapSizeX, gm.mapSizeZ];
		for (int x = 0; x < gm.mapSizeX; x++)
		{
			for (int z = 0; z < gm.mapSizeZ; z++)
			{
				VeteranVicinity[x, z] = new VicinityTile(x, z);
			}
		}
	}


	// Reset status of explored and distFromVet
	public void resetExplored()
	{
		for (int x = 0; x < gm.mapSizeX; x++) {
			for (int z = 0; z < gm.mapSizeZ; z++) {
				VeteranVicinity[x, z].explored = false;
				VeteranVicinity[x, z].distFromVet = -1;
			}
		}
	}
	

	// Used for sanity checks
	public void printVicinityLocations()
	{
		for (int x = 0; x < gm.mapSizeX; x++)
		{
			for (int z = 0; z < gm.mapSizeZ; z++)
			{	// Print to console
				if(VeteranVicinity[x, z].distFromVet != -1 ) Debug.Log("VICINITY LOCATION:  (x, z)  ->  (" + x + ", " + z + ")");
			}
		}
	}

	// Called after the knockdown() called during the Veteran's turn
	public void updateVicinityArr(int in_f_x, int in_f_z)
	{
		// Coordinates of the Veteran
		f_x = in_f_x;
		f_z = in_f_z;
		Debug.Log("VIC-TEST: x, z   " + f_x + ", " + f_z);

		// Mark the vicinity TODO ADD GUI COMPONENT
		VeteranVicinity[f_x, f_z].explored = true;
		VeteranVicinity[f_x, f_z].distFromVet = 0;

		Debug.Log("VIC-TEST: tile[1,3] = " + tiles[1,3]);

		rec_markVicinity(f_x, f_z, 1);

		/*
		if (f_x <= 8) rec_markVicinity(f_x + 1, f_z, 1);
		if (f_x >= 1) rec_markVicinity(f_x - 1, f_z, 1);
		if (f_z <= 6) rec_markVicinity(f_x, f_z + 1, 1);
		if (f_z >= 1) rec_markVicinity(f_x, f_z - 1, 1);
		*/

		// Sanity check to see
		printVicinityLocations();

		// Reset explored status
		resetExplored();
	}

	
	public void rec_markVicinity(int x_loc, int z_loc, int numStepsTaken){
		Debug.Log("Looking at (x, z, num): " + x_loc + ", " + z_loc + ", " + numStepsTaken);

		// Recursive calls to markVicinity
		if(numStepsTaken < 3){
			// Check right
			if (x_loc <= 8 && VeteranVicinity[x_loc + 1, z_loc].explored == false
				&& (tiles[x_loc + 1, z_loc] != 1 && tiles[x_loc + 1, z_loc] != 2) && !gm.wallManager.checkIfVWall(x_loc + 1, z_loc))
			{
				Debug.Log("R: (" + (x_loc + 1) + ", " + z_loc + ", " + numStepsTaken + "): Not explored, normal tile, no vertical wall");
				if (gm.doorManager.checkIfVDoor(x_loc + 1, z_loc))
				{
					Debug.Log("R: (" + (x_loc + 1) + ", " + z_loc + ", " + numStepsTaken + "): Found VDoor (" + (x_loc + 1) + ", " + z_loc + ")");

					if (gm.doorManager.checkIfOpenVDoor(x_loc + 1, z_loc))
					{
						Debug.Log("R: (" + (x_loc + 1) + ", " + z_loc + ", " + numStepsTaken + "): VDoor was open, marking!");

						VeteranVicinity[x_loc + 1, z_loc].explored = true;
						VeteranVicinity[x_loc + 1, z_loc].distFromVet = numStepsTaken;
						rec_markVicinity(x_loc + 1, z_loc, numStepsTaken + 1);
					}
				}
				else
				{
					Debug.Log("R: (" + (x_loc + 1) + ", " + z_loc + ", " + numStepsTaken + "): No door was found, marking!");

					VeteranVicinity[x_loc + 1, z_loc].explored = true;
					VeteranVicinity[x_loc + 1, z_loc].distFromVet = numStepsTaken;
					rec_markVicinity(x_loc + 1, z_loc, numStepsTaken + 1);
				}
			}

			// Check left
			if (x_loc >= 1 && VeteranVicinity[x_loc - 1, z_loc].explored == false
				&& (tiles[x_loc - 1, z_loc] != 1 && tiles[x_loc - 1, z_loc] != 2) && !gm.wallManager.checkIfVWall(x_loc, z_loc))
			{
				Debug.Log("L: (" + (x_loc - 1) + ", " + z_loc + ", " + numStepsTaken + "): Not explored, normal tile, no vertical wall");
				if (gm.doorManager.checkIfVDoor(x_loc, z_loc))
				{
					Debug.Log("L: (" + (x_loc - 1) + ", " + z_loc + ", " + numStepsTaken + "): Found VDoor (" + x_loc + ", " + z_loc + ")");
					
					if (gm.doorManager.checkIfOpenVDoor(x_loc, z_loc))
					{
						Debug.Log("L: (" + (x_loc - 1) + ", " + z_loc + ", " + numStepsTaken + "): VDoor was open, marking!");

						VeteranVicinity[x_loc - 1, z_loc].explored = true;
						VeteranVicinity[x_loc - 1, z_loc].distFromVet = numStepsTaken;
						rec_markVicinity(x_loc - 1, z_loc, numStepsTaken + 1);
					}
				}
				else
				{
					Debug.Log("L: (" + (x_loc - 1) + ", " + z_loc + ", " + numStepsTaken + "): No door was found, marking!");

					VeteranVicinity[x_loc - 1, z_loc].explored = true;
					VeteranVicinity[x_loc - 1, z_loc].distFromVet = numStepsTaken;
					rec_markVicinity(x_loc - 1, z_loc, numStepsTaken + 1);
				}
			}

			// Check tile above
			if (z_loc <= 6 && VeteranVicinity[x_loc, z_loc + 1].explored == false &&
				(tiles[x_loc, z_loc + 1] != 1 && tiles[x_loc, z_loc + 1] != 2) && !gm.wallManager.checkIfHWall(x_loc, z_loc + 1))
			{
				// Check that above us isn't an open door or a wall that is intact
				Debug.Log("U: (" + x_loc + ", " + (z_loc + 1) + ", " + numStepsTaken + "): Not explored, normal tile, no horizontal wall");

				// If there's a door it has to be open to continue
				if (gm.doorManager.checkIfHDoor(x_loc, z_loc + 1))
				{
					Debug.Log("U: (" + x_loc + ", " + (z_loc + 1) + ", " + numStepsTaken + "): Found HDoor (" + x_loc + ", " + (z_loc + 1) + ")");

					if (gm.doorManager.checkIfOpenHDoor(x_loc, z_loc + 1)){
						Debug.Log("U: (" + x_loc + ", " + (z_loc + 1) + ", " + numStepsTaken + "): HDoor was open, marking!");

						VeteranVicinity[x_loc, z_loc + 1].explored = true;
						VeteranVicinity[x_loc, z_loc + 1].distFromVet = numStepsTaken;
						rec_markVicinity(x_loc, z_loc + 1, numStepsTaken + 1);
					}
				}
				else {
					Debug.Log("U: (" + x_loc + ", " + (z_loc + 1) + ", " + numStepsTaken + "): No door was found, marking!");

					VeteranVicinity[x_loc, z_loc + 1].explored = true;
					VeteranVicinity[x_loc, z_loc + 1].distFromVet = numStepsTaken;
					rec_markVicinity(x_loc, z_loc + 1, numStepsTaken + 1);
				}
			}

			// Check tile below
			if (z_loc >= 1 && VeteranVicinity[x_loc, z_loc - 1].explored == false &&
				(tiles[x_loc, z_loc - 1] != 1 && tiles[x_loc, z_loc - 1] != 2) && !gm.wallManager.checkIfHWall(x_loc, z_loc))
			{
				Debug.Log("D: (" + x_loc + ", " + (z_loc - 1) + ", " + numStepsTaken + "): Not explored, normal tile, no horizontal wall");
				if (gm.doorManager.checkIfHDoor(x_loc, z_loc))
				{
					Debug.Log("D: (" + x_loc + ", " + (z_loc - 1) + ", " + numStepsTaken + "): Found HDoor (" + x_loc + ", " + z_loc + ")");

					if (gm.doorManager.checkIfOpenHDoor(x_loc, z_loc))
					{
						Debug.Log("D: (" + x_loc + ", " + (z_loc - 1) + ", " + numStepsTaken + "): HDoor was open, marking!");

						VeteranVicinity[x_loc, z_loc - 1].explored = true;
						VeteranVicinity[x_loc, z_loc - 1].distFromVet = numStepsTaken;
						rec_markVicinity(x_loc, z_loc - 1, numStepsTaken + 1);
					}
				}
				else
				{
					Debug.Log("D: (" + x_loc + ", " + (z_loc - 1) + ", " + numStepsTaken + "): No door was found, marking!");

					VeteranVicinity[x_loc, z_loc - 1].explored = true;
					VeteranVicinity[x_loc, z_loc - 1].distFromVet = numStepsTaken;
					rec_markVicinity(x_loc, z_loc - 1, numStepsTaken + 1);
				}
			}
		}
	}
}