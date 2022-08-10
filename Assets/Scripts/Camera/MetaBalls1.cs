using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class MetaBalls1 : MonoBehaviour
{
	/// Blur iterations - larger number means more blur.
	public int iterations = 3;

	/// Blur spread for each iteration. Lower values
	/// give better looking blur, but require more iterations to
	/// get large blurs. Value is usually between 0.5 and 1.0.
	public float blurSpread = 2f;


	// The blur iteration shader.
	// Basically it just takes 4 texture samples and averages them.
	// By applying it repeatedly and spreading out sample locations
	// we get a Gaussian blur approximation.

	public Shader blurShader = null;

	
	private Camera _camera;
	private RenderTexture particlesTargetTexture;
	private string dirKey = "dir",
		strengthKey = "strength";
	
	public Camera particlesCamera;
	public Material cutoutMaterial;
	private Material _material;
	
	
	void OnEnable() {
		Set();
	}

	public void Set()
	{
		
		_camera = GetComponent<Camera>();
		
		
		if (_camera != null && Screen.width > 0 && Screen.height > 0) {
			if (particlesTargetTexture != null) {
				particlesTargetTexture.Release();
			}
			if (Screen.width > 1080) {
				particlesTargetTexture = new RenderTexture (Screen.width / 2, Screen.height / 2, 24, RenderTextureFormat.ARGB32);
			} else {
				particlesTargetTexture = new RenderTexture (Screen.width / 2, Screen.height / 2, 24, RenderTextureFormat.ARGB32);
			}
			particlesTargetTexture.filterMode = FilterMode.Bilinear;
			particlesCamera.targetTexture = particlesTargetTexture;	
			particlesCamera.backgroundColor = Color.clear;
			//GetComponent<Camera>().forceIntoRenderTexture = true;
		}
		
		if (!_material) {
			_material = new Material(blurShader);
			_material.hideFlags = HideFlags.HideAndDontSave;
		}
	}
		

	protected void OnDisable()
	{
		if (_material)
		{
			DestroyImmediate(_material);
		}
	}

	public void Start()
	{
	}
	
	public void Update() {
	}
	
	void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		int rtW = source.width / 4;
		int rtH = source.height / 4;
		
		
		Graphics.Blit(source, destination);		
		
		RenderTexture buffer = RenderTexture.GetTemporary(rtW, rtH, 0,  RenderTextureFormat.ARGB32);
		
		for (int i = 0; i < iterations; i++)
		{
			// horizontal blur
			_material.SetVector(dirKey, new Vector2(1.0f, 0.0f));
			_material.SetFloat(strengthKey, (i + 1) * blurSpread / rtW);
			Graphics.Blit(particlesTargetTexture, buffer, _material);
			// vertical blur
			_material.SetVector(dirKey, new Vector2(0.0f, 1.0f));
			_material.SetFloat(strengthKey, (i + 1) * blurSpread / rtH);
			Graphics.Blit(buffer, particlesTargetTexture, _material);
		}

		Graphics.Blit(particlesTargetTexture, destination, cutoutMaterial);
		
		RenderTexture.ReleaseTemporary (buffer);
	}
}

