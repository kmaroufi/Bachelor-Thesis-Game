using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class AntiAliasing : MonoBehaviour
{
	public Shader fxaaShader = null;
	
	private GameManager gm;
	
	private CommandBuffer _cBuffer;
	private CameraEvent _cameraEvent = CameraEvent.AfterEverything;
	private Camera _camera;
	private Material _material;
	private string blurredIDKey = "_Temp1";
	private FilterMode _filterMode = FilterMode.Bilinear;

	public void Set() {
		//return;
		gm = GameManager.Instance;
		
		_camera = GetComponent<Camera>();
		
		if (!_material) {
			_material = new Material(fxaaShader);
			_material.hideFlags = HideFlags.HideAndDontSave;
		}
		
		int _blurredID = Shader.PropertyToID(blurredIDKey);
		
		DestroyCommandBuffer();
		_cBuffer = new CommandBuffer();
		_cBuffer.name = "AntiAliasing Cm";
		_cBuffer.Clear();
		
		
		_cBuffer.GetTemporaryRT(_blurredID, -1, -1, 0, _filterMode);

		
		float rcpWidth = 1.0f / Screen.width;
		float rcpHeight = 1.0f / Screen.height;

		_material.SetVector( "_rcpFrame", new Vector4( rcpWidth, rcpHeight, 0, 0 ) );
		_material.SetVector( "_rcpFrameOpt", new Vector4( rcpWidth * 2, rcpHeight * 2, rcpWidth * 0.5f, rcpHeight * 0.5f ) );
		

		_cBuffer.Blit(BuiltinRenderTextureType.CurrentActive, _blurredID);
		_cBuffer.Blit(_blurredID, BuiltinRenderTextureType.CameraTarget, _material);
		
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
			if (cBuffer.name == "AntiAliasing Cm") {
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
}
