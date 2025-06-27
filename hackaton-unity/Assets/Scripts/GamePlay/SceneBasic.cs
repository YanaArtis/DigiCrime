using System.Collections.Generic;
using UnityEngine;

public class SceneBasic : MonoBehaviour
{
	public bool enableXR = true;

	virtual protected void Awake()
	{
		Debug.Log(name + " Awake");

		if (Root.Instance == null)
			Root.Init(enableXR);
	}

	virtual protected void Start()
	{
		Debug.Log(name + " Start");

		RemoveExtraCameras();

		if (enableXR)
		{
			VRHeadTracker.onSafeZoneChanged += OnSafeZoneChanged;
			VRHeadTracker.onHeadTrackedEvent += OnReady;

			if (VRHeadTracker.IsHeadTracked)
				OnReady();
		}
		else OnReady();
	}

	virtual protected void OnDestroy()
	{
		Debug.Log(name + " OnDestroy");

		VRHeadTracker.onHeadTrackedEvent -= OnReady;
		VRHeadTracker.onSafeZoneChanged -= OnSafeZoneChanged;
	}


	public void RemoveExtraCameras()
	{
		var cams = GameObject.FindGameObjectsWithTag("MainCamera");
		if (cams.Length > 1)
		{
			foreach (var cam in cams)
				if (cam != Root.Instance.xrOrigin.Camera.gameObject)
					Destroy(cam.gameObject);
		}
	}

	virtual protected void OnReady()
	{
		Debug.Log(name + " OnReady");
	}

	virtual protected void OnSafeZoneChanged(List<Vector3> safeZone)
	{
		Debug.Log(name + " OnSafeZoneChanged");
	}
}
