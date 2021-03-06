﻿using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;
using System.Collections.Generic;

public class SplatManager : MonoBehaviour 
{
	[Header("Splats Atlas")]
	[SerializeField] private Texture2D m_splatTexture;
	[SerializeField] private Vector2Int m_splatCount;

	[Header("Settings")]
	[SerializeField] private Vector2Int m_splatMapTexSize;

	public static SplatManager Instance { get; private set; }

	public Vector4 Scores { get { return m_scores; } }



	// This just picks a random scale and bias for a 4x4 splat atlas
	// You could use a larger atlas of splat textures and pick a scale and offset for the specific splat you want to use
	public void CreateSplatAtPoint(Vector3 point, Vector3 normal, Vector4 mask, float scale)
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
		m_transformMakerObj.transform.localScale = new Vector3(randScale, randScale * 0.5f, randScale) * scale;

		Splat newSplat;
		newSplat.SplatMatrix = m_transformMakerObj.transform.worldToLocalMatrix;
		newSplat.ChannelMask = mask;

		float splatscaleX = 1.0f / m_splatCount.x;
		float splatscaleY = 1.0f / m_splatCount.y;
		float splatsBiasX = Mathf.Floor(Random.Range(0, m_splatCount.x * 0.99f)) / m_splatCount.x;
		float splatsBiasY = Mathf.Floor(Random.Range(0, m_splatCount.y * 0.99f)) / m_splatCount.y;

		newSplat.ScaleBias = new Vector4(splatscaleX, splatscaleY, splatsBiasX, splatsBiasY);

		m_splatsBacklog.Add(newSplat);
	}

	public void ClearAllSplats()
    {
		Graphics.Blit(Texture2D.blackTexture, m_splatTexPing, m_splatBlitMaterial, 1);
		Graphics.Blit(Texture2D.blackTexture, m_splatTexPong, m_splatBlitMaterial, 1);
		m_splatsBacklog.Clear();
	}

	private void Awake() 
	{
		if (Instance != null) 
		{
			if (Instance != this) 
			{
				Destroy(this);
			}
		} 
		else 
		{
			Instance = this;
		}
	}

	private void Start() 
	{
		m_splatBlitMaterial = new Material(Shader.Find("Splatoonity/SplatBlit"));
		
		m_splatTexPing = new RenderTexture(m_splatMapTexSize.x, m_splatMapTexSize.y, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
		m_splatTexPing.Create();
		m_splatTexPong = new RenderTexture(m_splatMapTexSize.x, m_splatMapTexSize.y, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
		m_splatTexPong.Create();
		m_worldPosTex = new RenderTexture(m_splatMapTexSize.x, m_splatMapTexSize.y, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
		m_worldPosTex.Create();
		m_worldTangentTex = new RenderTexture(m_splatMapTexSize.x, m_splatMapTexSize.y, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
		m_worldTangentTex.Create();
		m_worldBinormalTex = new RenderTexture(m_splatMapTexSize.x, m_splatMapTexSize.y, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
		m_worldBinormalTex.Create();

		Shader.SetGlobalTexture("_SplatTex", m_splatTexPing);
		Shader.SetGlobalTexture("_WorldPosTex", m_worldPosTex);
		Shader.SetGlobalTexture("_WorldTangentTex", m_worldTangentTex);
		Shader.SetGlobalTexture("_WorldBinormalTex", m_worldBinormalTex);
		Shader.SetGlobalVector("_SplatTexSize", new Vector4 (m_splatMapTexSize.x, m_splatMapTexSize.y, 0, 0));

		// Textures for tallying scores 
		// needs to be higher precision because it will be mipped down to 4x4 ldr texture for final score keeping
		m_scoreTex = new RenderTexture(m_splatMapTexSize.x/8, m_splatMapTexSize.y/ 8, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
		m_scoreTex.autoGenerateMips = true;
		m_scoreTex.useMipMap = true;
		m_scoreTex.Create();
		m_rt4 = new RenderTexture(4, 4, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
		m_rt4.Create();
		m_tex4 = new Texture2D(4, 4, TextureFormat.ARGB32, false);

		SplatTarget[] allSplatTargets = FindObjectsOfType<SplatTarget>();
		foreach(var target in allSplatTargets)
        {
			Renderer newRenderer = target.GetRenderer();
			if (newRenderer == null) continue;
			m_renderers.Add(newRenderer);
        }

		RenderTextures();
		BleedTextures ();
		StartCoroutine( UpdateScores() );

		m_transformMakerObj = new GameObject();
		m_transformMakerObj.name = "SplatTransformMaker";
	}

	/*
	// Render textures using shader replacement.
	// This will render all objects in the scene though.
	// You could cull based on layers though.
	void RenderTextures() {

		Material worldPosMaterial = new Material(Shader.Find("Splatoonity/WorldPosUnwrap"));
		Material worldNormalMaterial = new Material(Shader.Find("Splatoonity/WorldNormalUnwrap"));

		rtCamera.targetTexture = worldPosTex;
		rtCamera.RenderWithShader(Shader.Find("Splatoonity/WorldPosUnwrap"), null);

		rtCamera.targetTexture = worldTangentTex;
		rtCamera.RenderWithShader(Shader.Find("Splatoonity/WorldTangentUnwrap"), null);

		rtCamera.targetTexture = worldBinormalTex;
		rtCamera.RenderWithShader(Shader.Find("Splatoonity/WorldBinormalUnwrap"), null);
	}
	*/

	// Render textures with a command buffer.
	// This is more flexible as you can explicitly add more objects to render without worying about layers.
	// You could also have multiple instances for chunks of a scene.
	private void RenderTextures() 
	{
		// Set up rendering camera
		GameObject newCamObject = new GameObject();
		newCamObject.name = "rtCameraObject";
		newCamObject.transform.position = Vector3.zero;
		newCamObject.transform.rotation = Quaternion.identity;
		newCamObject.transform.localScale = Vector3.one;
		Camera newCam = newCamObject.AddComponent<Camera>();
		newCam.renderingPath = RenderingPath.Forward;
		newCam.clearFlags = CameraClearFlags.SolidColor;
		newCam.backgroundColor = new Color(0, 0, 0, 0);
		newCam.orthographic = true;
		newCam.nearClipPlane = 0.0f;
		newCam.farClipPlane = 1.0f;
		newCam.orthographicSize = 1.0f;
		newCam.aspect = 1.0f;
		newCam.useOcclusionCulling = false;
		newCam.enabled = false;

		// Set the culling mask to Nothing so we can draw renderers explicitly
		newCam.cullingMask = LayerMask.NameToLayer("Nothing");

		Material worldPosMaterial = new Material (Shader.Find ("Splatoonity/WorldPosUnwrap"));
		Material worldTangentMaterial = new Material (Shader.Find ("Splatoonity/WorldTangentUnwrap"));
		Material worldBiNormalMaterial = new Material (Shader.Find ("Splatoonity/WorldBinormalUnwrap"));

		// You could collect all objects you want rendererd and loop through DrawRenderer
		// but for this example I'm just drawing the one renderer.
		//Renderer envRenderer = this.gameObject.GetComponent<Renderer> ();

		int rendererCount = m_renderers.Count;

		// You could also use a multi render target and only have to draw each renderer once.
		CommandBuffer cb = new CommandBuffer();
		cb.SetRenderTarget(m_worldPosTex);
		cb.ClearRenderTarget(true, true, new Color(0,0,0,0) );
		//cb.DrawRenderer(envRenderer, worldPosMaterial);
		for (int i = 0; i < rendererCount; i++) {
			cb.DrawRenderer (m_renderers[i], worldPosMaterial);
		}

		cb.SetRenderTarget(m_worldTangentTex);
		cb.ClearRenderTarget(true, true, new Color(0,0,0,0) );
		//cb.DrawRenderer(envRenderer, worldTangentMaterial);
		for (int i = 0; i < rendererCount; i++) {
			cb.DrawRenderer (m_renderers[i], worldTangentMaterial);
		}

		cb.SetRenderTarget(m_worldBinormalTex);
		cb.ClearRenderTarget(true, true, new Color(0,0,0,0) );
		//cb.DrawRenderer(envRenderer, worldBiNormalMaterial);
		for (int i = 0; i < rendererCount; i++) {
			cb.DrawRenderer (m_renderers[i], worldBiNormalMaterial);
		}

		// Only have to render the camera once!
		newCam.AddCommandBuffer (CameraEvent.AfterEverything, cb);
		newCam.Render ();

		// Destroy the rendering camera
		Destroy(newCamObject);
	}


	private void BleedTextures() 
	{
		Graphics.Blit(Texture2D.blackTexture, m_splatTexPing, m_splatBlitMaterial, 1);		
		Graphics.Blit(Texture2D.blackTexture, m_splatTexPong, m_splatBlitMaterial, 1);

		m_splatBlitMaterial.SetVector("_SplatTexSize", new Vector2( m_splatMapTexSize.x, m_splatMapTexSize.y) );

		RenderTexture tempTex = new RenderTexture(m_splatMapTexSize.x, m_splatMapTexSize.y, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
		tempTex.Create();

		// Bleed the world position out 2 pixels
		Graphics.Blit(m_worldPosTex, tempTex, m_splatBlitMaterial, 2);
		Graphics.Blit(tempTex, m_worldPosTex, m_splatBlitMaterial, 2);

		// Don't need this guy any more
		tempTex.Release();
	}


	// Blit the splats
	// This is similar to how a deferred decal would work
	// except instead of getting the world position from the depth
	// use the world position that is stored in the texture.
	// Each splat is tested against the entire world position texture.
	private void PaintSplats() 
	{
		if (m_splatsBacklog.Count > 0) 
		{
			Matrix4x4[] SplatMatrixArray = new Matrix4x4[10];
			Vector4[] SplatScaleBiasArray = new Vector4[10];
			Vector4[] SplatChannelMaskArray = new Vector4[10];

			// Render up to 10 splats per frame.
			int i = 0;
			while(m_splatsBacklog.Count > 0 && i < 10 )
			{
				SplatMatrixArray [i] = m_splatsBacklog[0].SplatMatrix;
				SplatScaleBiasArray [i] = m_splatsBacklog[0].ScaleBias;
				SplatChannelMaskArray [i] = m_splatsBacklog[0].ChannelMask;
				m_splatsBacklog.RemoveAt(0);
				++i;
			}
			m_splatBlitMaterial.SetMatrixArray ( "_SplatMatrix", SplatMatrixArray );
			m_splatBlitMaterial.SetVectorArray ( "_SplatScaleBias", SplatScaleBiasArray );
			m_splatBlitMaterial.SetVectorArray ( "_SplatChannelMask", SplatChannelMaskArray );

			m_splatBlitMaterial.SetInt ( "_TotalSplats", i );

			m_splatBlitMaterial.SetTexture ("_WorldPosTex", m_worldPosTex);

			// Ping pong between the buffers to properly blend splats.
			// If this were a compute shader you could just update one buffer.
			if (m_isPingOrPong) 
			{
				m_splatBlitMaterial.SetTexture("_LastSplatTex", m_splatTexPong);
				Graphics.Blit (m_splatTexture, m_splatTexPing, m_splatBlitMaterial, 0);
				Shader.SetGlobalTexture ("_SplatTex", m_splatTexPing);
			} 
			else 
			{
				m_splatBlitMaterial.SetTexture ("_LastSplatTex", m_splatTexPing);
				Graphics.Blit (m_splatTexture, m_splatTexPong, m_splatBlitMaterial, 0);
				Shader.SetGlobalTexture ("_SplatTex", m_splatTexPong);
			}
			m_isPingOrPong = !m_isPingOrPong;
		}

	}

	// Update the scores by mipping the splat texture down to a 4x4 texture and sampling the pixels.
	// Space the whole operation out over a few frames to keep everything running smoothly.
	// Only update the scores once every second.
	private IEnumerator UpdateScores() 
	{
		while( true )
		{
			yield return new WaitForEndOfFrame();

			Graphics.Blit (m_splatTexPing, m_scoreTex, m_splatBlitMaterial, 3);
			Graphics.Blit (m_scoreTex, m_rt4, m_splatBlitMaterial, 4);

			RenderTexture.active = m_rt4;
			m_tex4.ReadPixels (new Rect (0, 0, 4, 4), 0, 0);
			m_tex4.Apply ();

			yield return new WaitForSeconds(0.01f);

			Color scoresColor = new Color(0,0,0,0);
			scoresColor += m_tex4.GetPixel(0,0);
			scoresColor += m_tex4.GetPixel(0,1);
			scoresColor += m_tex4.GetPixel(0,2);
			scoresColor += m_tex4.GetPixel(0,3);

			yield return new WaitForSeconds(0.01f);

			scoresColor += m_tex4.GetPixel(1,0);
			scoresColor += m_tex4.GetPixel(1,1);
			scoresColor += m_tex4.GetPixel(1,2);
			scoresColor += m_tex4.GetPixel(1,3);

			yield return new WaitForSeconds(0.01f);

			scoresColor += m_tex4.GetPixel(2,0);
			scoresColor += m_tex4.GetPixel(2,1);
			scoresColor += m_tex4.GetPixel(2,2);
			scoresColor += m_tex4.GetPixel(2,3);

			yield return new WaitForSeconds(0.01f);

			scoresColor += m_tex4.GetPixel(3,0);
			scoresColor += m_tex4.GetPixel(3,1);
			scoresColor += m_tex4.GetPixel(3,2);
			scoresColor += m_tex4.GetPixel(3,3);

			m_scores.x = scoresColor.r;
			m_scores.y = scoresColor.g;
			m_scores.z = scoresColor.b;
			m_scores.w = scoresColor.a;

			yield return new WaitForSeconds (1.0f);

		}

	}
	
	// Update is called once per frame
	void Update() 
	{
		PaintSplats();
	}

	// List of renderers to draw splats to
	private List<Renderer> m_renderers = new List<Renderer>();

	// List of splats to be drawn
	internal List<Splat> m_splatsBacklog = new List<Splat>();

	private RenderTexture m_splatTexPing;
	private RenderTexture m_splatTexPong;
	private bool m_isPingOrPong = false;

	private RenderTexture m_worldPosTex;
	private RenderTexture m_worldTangentTex;
	private RenderTexture m_worldBinormalTex;

	private Material m_splatBlitMaterial;

	private GameObject m_transformMakerObj;

	private RenderTexture m_scoreTex;
	private RenderTexture m_rt4;
	private Texture2D m_tex4;

	private Vector4 m_scores = Vector4.zero;



	public struct Splat
	{
		public Matrix4x4 SplatMatrix;
		public Vector4 ChannelMask;
		public Vector4 ScaleBias;
	}
}