using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowObject : MonoBehaviour
{

    [SerializeField] Transform _playerTransform;
    [SerializeField] float _flipYRotationTime = 0.5f;

    private PlayerController _player;
    private bool isFacingRight;

    void Awake()
    {
        _player = _playerTransform.GetComponent<PlayerController>();
        isFacingRight = _player.isFacingRight;
    }

    void Update()
    {
        transform.position = _player.transform.position;
    }

    public void CallTurn()
    {
        LeanTween.rotateY(gameObject, DetermineRotation(), _flipYRotationTime).setEaseInOutSine();
    }

    private float DetermineRotation()
    {
        isFacingRight = !isFacingRight;
        return isFacingRight ? 0f : 180f;
    }

}
