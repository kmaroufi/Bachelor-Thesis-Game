using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class MetaBalls4 : MonoBehaviour
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
	public Shader copyShader = null;
	public Shader colorShader = null;
	
	private CommandBuffer _cBuffer;
	private Camera _camera;
	private CameraEvent _cameraEvent = CameraEvent.BeforeImageEffects;
	private RenderTexture cameraTargetTexture;
	private FilterMode _filterMode = FilterMode.Bilinear;
	private string blurredIDKey = "_Temp1",
		blurredID2Key = "_Temp2",
		offsetsKey = "offsets",
		dirKey = "dir",
		strengthKey = "strength";
	
	public Material cutoutMaterial;
	private Material _material;
	public GameObject plane;
	
	
	void OnEnable() {
		Set();
	}

	public void Set()
	{	
		_camera = GetComponent<Camera>();
		
		
		if (_camera != null && Screen.width > 0 && Screen.height > 0) {
			if (cameraTargetTexture != null) {
				cameraTargetTexture.Release();
			}
			if (Screen.width > 1080) {
				cameraTargetTexture = new RenderTexture (Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32);
			} else {
				cameraTargetTexture = new RenderTexture (Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32);
			}
			cameraTargetTexture.filterMode = FilterMode.Bilinear;
			_camera.targetTexture = cameraTargetTexture;	
			_camera.backgroundColor = Color.clear;
			//GetComponent<Camera>().forceIntoRenderTexture = true;
		}
		
		if (!_material) {
			_material = new Material(blurShader);
			_material.hideFlags = HideFlags.HideAndDontSave;
		}
		
		
		Material copyMaterial = new Material(copyShader);
		Material blitColorMaterial = new Material(colorShader);
		
		int _blurredID = Shader.PropertyToID(blurredIDKey);
		int _blurredID2 = Shader.PropertyToID(blurredID2Key);
		
		DestroyCommandBuffer();
		_cBuffer = new CommandBuffer();
		_cBuffer.name = "MetaBalls Cm";
		_cBuffer.Clear();
		
		_cBuffer.GetTemporaryRT(_blurredID, -3, -3, 0, _filterMode);
		_cBuffer.GetTemporaryRT(_blurredID2, -2, -2, 0, _filterMode);
		
		_cBuffer.Blit(cameraTargetTexture, _blurredID2, copyMaterial);
		
		for (int i = 0; i < iterations; i++)
		{
			// horizontal blur
			_cBuffer.SetGlobalVector(dirKey, new Vector2(1.0f, 0.0f));
			_cBuffer.SetGlobalFloat(strengthKey, (i + 1) * blurSpread / Screen.width * 4);
			_cBuffer.Blit(_blurredID2, _blurredID, _material);
			// vertical blur
			_cBuffer.SetGlobalVector(dirKey, new Vector2(0.0f, 1.0f));
			_cBuffer.SetGlobalFloat(strengthKey, (i + 1) * blurSpread / Screen.height * 4);
			_cBuffer.Blit(_blurredID, _blurredID2, _material);
		}
		
		_cBuffer.Blit(null, cameraTargetTexture, blitColorMaterial);
		_cBuffer.Blit(_blurredID2, cameraTargetTexture, cutoutMaterial);
		
		_cBuffer.ReleaseTemporaryRT(_blurredID);
		_cBuffer.ReleaseTemporaryRT(_blurredID2);

		_camera.AddCommandBuffer(_cameraEvent, _cBuffer);
		Debug.Log(_cBuffer.name + " Added");
		
		var height = 2.0f * Mathf.Tan(0.5f * _camera.fieldOfView * Mathf.Deg2Rad) * transform.position.z;
		var width = height * _camera.aspect;
		plane.transform.localScale = new Vector3(width / 10, 1, height / 10);
		plane.transform.position = transform.position - new Vector3(0, 0, transform.position.z);
		plane.GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_MainTex", cameraTargetTexture);
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

