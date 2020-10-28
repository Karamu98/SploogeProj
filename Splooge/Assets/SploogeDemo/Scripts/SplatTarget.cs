using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplatTarget : MonoBehaviour 
{
	public void Init() 
	{
		Renderer renderer = gameObject.GetComponent<Renderer>();
		if (renderer != null) 
		{
			SplatManager.Instance.AddRenderer(renderer);
		}
	}

}
