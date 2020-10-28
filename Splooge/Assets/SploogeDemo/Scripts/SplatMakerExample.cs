using UnityEngine;

public class SplatMakerExample : MonoBehaviour 
{
	[SerializeField] private float m_splatScale = 1.0f;

	private void Awake()
    {
		m_transformMakerObj = new GameObject();
		m_splatCounts = SplatManager.Instance.SplatCount;
	}
	
	void Update () 
	{
		if(Input.GetKeyDown(KeyCode.Alpha1))
		{
			m_channelMask = new Vector4(1,0,0,0);
		}
		else if(Input.GetKeyDown(KeyCode.Alpha2))
		{
			m_channelMask = new Vector4(0,1,0,0);
		}
		else if(Input.GetKeyDown(KeyCode.Alpha3))
		{
			m_channelMask = new Vector4(0,0,1,0);
		}
		else if(Input.GetKeyDown(KeyCode.Alpha4))
		{
			m_channelMask = new Vector4(0,0,0,1);
		}
			
		// Cast a ray from the camera to the mouse pointer and draw a splat there.
		// This just picks a rendom scale and bias for a 4x4 splat atlas
		// You could use a larger atlas of splat textures and pick a scale and offset for the specific splat you want to use
		if (Input.GetMouseButton(0)) 
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if(Physics.Raycast(ray, out hit, 10000))
			{
				CreateSplatAtPoint(hit.point, hit.normal);
			}
		}	
	}

	private void CreateSplatAtPoint(Vector3 point, Vector3 normal)
    {
		// Generate our rotation
		Vector3 leftVec = Vector3.Cross(normal, Vector3.up);
		float randScale = Random.Range(0.5f, 1.5f);

		m_transformMakerObj.transform.position = point;

		if (leftVec.magnitude > 0.001f)
		{
			m_transformMakerObj.transform.rotation = Quaternion.LookRotation(leftVec, normal);
		}
		else
		{
			m_transformMakerObj.transform.rotation = Quaternion.LookRotation(Vector3.left, Vector3.up);
		}

		m_transformMakerObj.transform.RotateAround(point, normal, Random.Range(-180, 180));
		m_transformMakerObj.transform.localScale = new Vector3(randScale, randScale * 0.5f, randScale) * m_splatScale;

		Splat newSplat;
		newSplat.SplatMatrix = m_transformMakerObj.transform.worldToLocalMatrix;
		newSplat.ChannelMask = m_channelMask;

		float splatscaleX = 1.0f / m_splatCounts.x;
		float splatscaleY = 1.0f / m_splatCounts.y;
		float splatsBiasX = Mathf.Floor(Random.Range(0, m_splatCounts.x * 0.99f)) / m_splatCounts.x;
		float splatsBiasY = Mathf.Floor(Random.Range(0, m_splatCounts.y * 0.99f)) / m_splatCounts.y;

		newSplat.ScaleBias = new Vector4(splatscaleX, splatscaleY, splatsBiasX, splatsBiasY);

		SplatManager.Instance.AddSplat(newSplat);
	}

	private Vector4 m_channelMask = new Vector4(1, 0, 0, 0);
	private GameObject m_transformMakerObj;
	private Vector2Int m_splatCounts;
}
