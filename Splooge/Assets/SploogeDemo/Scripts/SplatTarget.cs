using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplatTarget : MonoBehaviour 
{
	[SerializeField] Renderer m_renderer;
	public Renderer GetRenderer() 
	{
		if (m_renderer != null) 
		{
			return m_renderer;
		}
		return null;
	}

}
