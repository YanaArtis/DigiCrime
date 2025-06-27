using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.XR.CoreUtils;

public class Root : MonoBehaviour
{
    public static Root Instance;
    public XROrigin xrOrigin;

    public static void Init(bool enableXR)
    {
        Instance = (new GameObject("Root")).AddComponent<Root>();
        Instance.name = "Root";
        Instance.transform.position = Vector3.zero;
        Instance.transform.localScale = Vector3.one;
        Instance.transform.rotation = Quaternion.identity;

        if (enableXR)
            Instance.EnableXR();
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.LogError("Root already exists!");
            Destroy(this);
        }
    }

    void EnableXR()
    {
        if (xrOrigin == null)
            xrOrigin = CreateXROrigin();
    }

    XROrigin CreateXROrigin()
    {
        GameObject obj = Instantiate(Resources.Load<GameObject>("XRController"));
        obj.transform.SetParent(Instance.transform);
        obj.transform.position = Vector3.zero;
        obj.transform.rotation = Quaternion.identity;
        obj.transform.localScale = Vector3.one;

        return obj.GetComponentInChildren<XROrigin>();
    }
}
