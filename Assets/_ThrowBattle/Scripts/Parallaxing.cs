using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallaxing : MonoBehaviour {

    [HideInInspector]
    public List<Transform> backgrounds;
    private float[] parallaxScale;
    public float smooting = 1f;
    private Transform mainCamera;
    private Vector3 previousCameraPosition;
    float parallax;
    float backgroundTargetPositionX;
    Vector3 backgroundDestination;
    bool isParallaxing = false;

    private void OnEnable()
    {
        mainCamera = Camera.main.transform;
    }

    public void InitialSetting()
    {
        previousCameraPosition = mainCamera.position;
        parallaxScale = new float[backgrounds.Count];
        for (int i = 0; i < backgrounds.Count; i++)
            parallaxScale[i] = backgrounds[i].position.z * -1;
        isParallaxing = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (isParallaxing)
        {
            for (int i = 0; i < backgrounds.Count; i++)
            {
                parallax = (previousCameraPosition.x - mainCamera.position.x) * parallaxScale[i];
                backgroundTargetPositionX = backgrounds[i].position.x + parallax;
                backgroundDestination = new Vector3(backgroundTargetPositionX, backgrounds[i].position.y, backgrounds[i].position.z);
                backgrounds[i].position = Vector3.Lerp(backgrounds[i].position, backgroundDestination, smooting * Time.deltaTime);
            }
            previousCameraPosition = mainCamera.transform.position;
        }
    }
}
