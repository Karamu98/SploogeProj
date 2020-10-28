using UnityEngine;

public class SplatMakerExample : MonoBehaviour 
{
	[SerializeField] private float m_splatScale = 1.0f;

	
	void Update () 
	{
		if(Input.GetKeyDown(KeyCode.Alpha1))
		{
			m_channelMask = new Vector4(1, 0, 0, 0);
		}
		else if(Input.GetKeyDown(KeyCode.Alpha2))
		{
			m_channelMask = new Vector4(0, 1, 0, 0);
		}
		else if(Input.GetKeyDown(KeyCode.Alpha3))
		{
			m_channelMask = new Vector4(0, 0, 1, 0);
		}
		else if(Input.GetKeyDown(KeyCode.Alpha4))
		{
			m_channelMask = new Vector4(0, 0, 0, 1);
		}
			
		// Cast a ray from the camera to the mouse pointer and draw a splat there.
		if (Input.GetMouseButton(0)) 
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if(Physics.Raycast(ray, out hit, 10000))
			{
				SplatManager.Instance.CreateSplatAtPoint(hit.point, hit.normal, m_channelMask, m_splatScale);
			}
		}	
	}

    private void OnGUI()
    {
        if(GUILayout.Button("Clear all splats"))
        {
			SplatManager.Instance.ClearAllSplats();
        }
    }

	private Vector4 m_channelMask = new Vector4(1, 0, 0, 0);
}
