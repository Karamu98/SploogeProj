using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplatTarget : MonoBehaviour 
{
	// Need to add all the renderers before Start of Splat Manager
	void Awake () 
	{
		Renderer renderer = gameObject.GetComponent<Renderer>();
		if (renderer != null) 
		{
			SplatManager.Instance.AddRenderer(renderer);
		}
	}

}
