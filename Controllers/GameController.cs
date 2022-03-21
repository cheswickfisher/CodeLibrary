using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static Transform playerTransform;
    [SerializeField]
    private Transform _playerTransform;

    void Awake()
    {
        playerTransform = _playerTransform;
    }

}
