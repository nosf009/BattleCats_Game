using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton : MonoBehaviour
{

    #region Singleton
    private static Singleton _Instance;
    public static Singleton Instance
    {
        get
        {
            if (_Instance == null)
                _Instance = FindObjectOfType<Singleton>();
            return _Instance;
        }
    }
    #endregion


    public List<GameObject> confettiPS = new List<GameObject>();
    public List<GameObject> onHitFX = new List<GameObject>();
}
