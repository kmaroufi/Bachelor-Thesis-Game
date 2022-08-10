using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class MetaBalls2 : MonoBehaviour
{
/// Blur iterations - larger number means more blur.
	public int iterations = 3;

	/// Blur spread for each iteration. Lower values
	/// give better looking blur, but require more iterations to
	/// get large blurs. Value is usually between 0.5 and 1.0.
	public float blurSpread = 0.6f;


	// The blur iteration shader.
	// Basically it just takes 4 texture samples and averages them.
	// By applying it repeatedly and spreading out sample locations
	// we get a Gaussian blur approximation.

	public Shader blurShader = null;
	
	private CommandBuffer _cBuffer;
	private Camera _camera;
	private CameraEvent _cameraEvent = CameraEvent.BeforeImageEffects;
	private RenderTexture particlesTargetTexture;
	private FilterMode _filterMode = FilterMode.Bilinear;
	private string blurredIDKey = "_Temp1",
		blurredID2Key = "_Temp2",
		offsetsKey = "offsets",
		dirKey = "dir",
		strengthKey = "strength";
	
	public Camera particlesCamera;
	public Material cutoutMaterial;
	public Material _material;
	
	
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
		
		int _blurredID = Shader.PropertyToID(blurredIDKey);
		
		DestroyCommandBuffer();
		
		_cBuffer = new CommandBuffer();
		_cBuffer.name = "MetaBalls Cm";
		_cBuffer.Clear();
		
		_cBuffer.GetTemporaryRT(_blurredID, -3, -3, 0, _filterMode);
		
		for (int i = 0; i < iterations; i++)
		{
			// horizontal blur
			_cBuffer.SetGlobalVector(dirKey, new Vector2(1.0f, 0.0f));
			_cBuffer.SetGlobalFloat(strengthKey, (i + 1) * blurSpread / Screen.width * 4);
			_cBuffer.Blit(particlesTargetTexture, _blurredID, _material);
			// vertical blur
			_cBuffer.SetGlobalVector(dirKey, new Vector2(0.0f, 1.0f));
			_cBuffer.SetGlobalFloat(strengthKey, (i + 1) * blurSpread / Screen.height * 4);
			_cBuffer.Blit(_blurredID, particlesTargetTexture, _material);
		}
		
		//cutoutMaterial.SetFloatArray("_Array", new float[] { 3/360.0f, 61/360.0f, 120/360.0f, 223/360.0f});
		_cBuffer.Blit(particlesTargetTexture, BuiltinRenderTextureType.CameraTarget, cutoutMaterial);
		
		_cBuffer.ReleaseTemporaryRT(_blurredID);

		_camera.AddCommandBuffer(_cameraEvent, _cBuffer);
		
		Debug.Log(_cBuffer.name + " Added");
	}
	
	private void DestroyCommandBuffer() {
		if (_cBuffer != null) {
			_camera.RemoveCommandBuffer(_cameraEvent, _cBuffer);
			_cBuffer.Clear();
			_cBuffer.Dispose();
			_cBuffer = null;
		}

		// Make sure we don't have any duplicates of our command buffer.
		CommandBuffer[] commandBuffers = _camera.GetCommandBuffers(_cameraEvent);
		foreach (CommandBuffer cBuffer in commandBuffers) {
			if (cBuffer.name == "MetaBalls Cm") {
				_camera.RemoveCommandBuffer(_cameraEvent, cBuffer);
				cBuffer.Clear();
				cBuffer.Dispose();
			}
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
}

