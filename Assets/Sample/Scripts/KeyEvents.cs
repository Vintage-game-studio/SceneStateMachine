﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class KeyEvents : MonoBehaviour
{
	public UnityEvent UnityEventA;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

		if (Input.GetKeyUp(KeyCode.Space))
		{
			UnityEventA.Invoke();
		}
		
	}
}
