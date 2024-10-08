using Cinemachine;
using develop_tps;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

namespace develop_ThirdPersonShoulder
{

    public class ThirdPersonShoulderController : MonoBehaviour
    {
        [SerializeField] private InputReader _inputReader;
        [SerializeField] private Transform _eye;
        [SerializeField] private CinemachineVirtualCamera _vcam;
        public Animator Animator;
        public float Speed = 5f;
        [SerializeField] private AxisState _vertical;
        [SerializeField] private AxisState _horizontal;

        static int _hashFront = Animator.StringToHash("Front");
        static int _hashSide = Animator.StringToHash("Side");

        private float _inputX;
        private float _inputY;
        private CinemachinePOV _currentPOV;

        private void Start()
        {
            _inputReader.MoveEvent += OnMoveHandle;
            _currentPOV = _vcam.GetCinemachineComponent<CinemachinePOV>();
        }

        private void Update()
        {
            // 入力を取得
            var leftStick = new Vector3(_inputX, 0, _inputY).normalized;

            // 移動速度を計算
            var velocity = Speed * leftStick;

            // 向きを更新
            var horizontalRotation = Quaternion.AngleAxis(_currentPOV.m_HorizontalAxis.Value, Vector3.up);
            var verticalRotation = Quaternion.AngleAxis(_currentPOV.m_VerticalAxis.Value, Vector3.right);
            transform.rotation = horizontalRotation;
            _eye.localRotation = verticalRotation;

            // Animatorに反映
            Animator.SetFloat(_hashFront, velocity.z, 0.1f, Time.deltaTime);
            Animator.SetFloat(_hashSide, velocity.x, 0.1f, Time.deltaTime);
        }

        private void OnMoveHandle(Vector2 movement)
        {
            _inputX = movement.x;
            _inputY = movement.y;
        }
    }
}

