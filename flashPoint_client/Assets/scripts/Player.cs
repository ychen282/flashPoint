﻿using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class Player : MonoBehaviour {

	public string playerName;
	public Vector3 position;
	public string	id;

	void Start () {

		this.name = playerName;
	}

}
