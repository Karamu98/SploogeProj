using UnityEngine;

public class DemoCamera : MonoBehaviour 
{
	private Camera m_controlledCam;
	[SerializeField] float m_swayAmount = 0.5f;
	[SerializeField] float m_swaySpeed = 0.5f;
	[SerializeField] float m_swaySmoothness = 10f;
	[SerializeField] float m_speed = 10.0f;
	
	private Quaternion targetRotation = Quaternion.identity;
	
	private Transform targetTransform;
	
	private Vector3 lastMousePosition;
	
	// Use this for initialization
	void Start () 
	{
		m_controlledCam = GetComponent<Camera>();	

		if(Application.isPlaying == true )
		{
			targetTransform = new GameObject().transform;
			targetTransform.position = m_controlledCam.transform.position;
			targetTransform.rotation = m_controlledCam.transform.rotation;
		}
	}
	
	// Update is called once per frame
	void Update () 
	{
		float deltaTime = Time.deltaTime;

		Vector3 mouseDelta = Vector3.zero;
		if(Input.GetMouseButton(1))
		{
			mouseDelta = (lastMousePosition - Input.mousePosition) * 0.1f;
		}

		lastMousePosition = Input.mousePosition;
			
		float horizontal = mouseDelta.x;
		float vertical = mouseDelta.y;

		float motionRight = 0;
		float motionForward = 0;
		float motionUp = 0;

		if(Input.GetKey(KeyCode.W))
		{
			motionForward += m_speed;
		}
		if(Input.GetKey(KeyCode.S))
		{
			motionForward -= m_speed;
		}

		if(Input.GetKey(KeyCode.A))
		{
			motionRight -= m_speed;
		}
		if(Input.GetKey(KeyCode.D))
		{
			motionRight += m_speed;
		}

		if(Input.GetKey(KeyCode.LeftShift))
		{
			motionRight *= 3.0f;
			motionForward *= 3.0f;
		}
		
		targetTransform.RotateAround(targetTransform.position, Vector3.up, horizontal * -200.0f * deltaTime);
		targetTransform.RotateAround(targetTransform.position, targetTransform.right, vertical * 150.0f * deltaTime);
			
		targetTransform.position += targetTransform.right * motionRight * deltaTime;
		targetTransform.position += targetTransform.forward * motionForward * deltaTime;
			
		targetTransform.position += new Vector3( 0.0f, motionUp * deltaTime, 0.0f );
			
		float randRotX = Mathf.Sin(Time.fixedTime * 2.17f * m_swaySpeed ) + Mathf.Sin( Time.fixedTime * 0.73f * m_swaySpeed);
		float randRotY = Mathf.Sin(Time.fixedTime * 2.73f * m_swaySpeed ) + Mathf.Sin( Time.fixedTime * 1.17f * m_swaySpeed);
		float randRotZ = Mathf.Sin(Time.fixedTime * 3.17f * m_swaySpeed ) + Mathf.Sin( Time.fixedTime * 1.31f * m_swaySpeed);
			
		targetRotation = targetTransform.rotation * Quaternion.Euler(randRotX * m_swayAmount, randRotY * m_swayAmount, randRotZ * m_swayAmount);

		m_controlledCam.transform.rotation = Quaternion.Slerp(m_controlledCam.transform.rotation, targetRotation, m_swaySmoothness * deltaTime);
		m_controlledCam.transform.position += (targetTransform.position - m_controlledCam.transform.position) * m_swaySmoothness * deltaTime;	
	}
}
