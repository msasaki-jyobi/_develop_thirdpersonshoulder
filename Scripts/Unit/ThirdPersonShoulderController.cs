using Cinemachine;
using develop_common;
using develop_tps;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

// 移動できない理由：ApplyRootmotion & StateがLocomotionになってない

namespace develop_ThirdPersonShoulder
{

    public class ThirdPersonShoulderController : MonoBehaviour
    {
        [SerializeField] private InputReader _inputReader;
        [SerializeField] private UnitHealth _health;
        [SerializeField] private Animator _animator;
        [SerializeField] private Rigidbody _rigidBody;
        [SerializeField] private Transform _eye;
        [SerializeField] private CinemachineVirtualCamera _vcam;
        [SerializeField] private LineData _ground;
        public float Speed = 5f;
        public float JumpPower = 7f;

        static int _hashFront = Animator.StringToHash("Front");
        static int _hashSide = Animator.StringToHash("Side");

        private float _inputX;
        private float _inputY;
        private CinemachinePOV _currentPOV;
        private bool _inputNone;

        private bool _canJump;
        private bool _isJump;

        private EUnitStatus _unitStatus;


        private void Start()
        {
            _inputReader.MoveEvent += OnMoveHandle;
            _inputReader.PrimaryActionCrossEvent += OnJumpHandle;

            _currentPOV = _vcam.GetCinemachineComponent<CinemachinePOV>();

            _health.UnitStatus
                .Subscribe((x) => 
                {
                    _unitStatus = x;
                    _inputNone = x == EUnitStatus.Executing ? true : false;
                });
        }

        private void Update()
        {
            _canJump = UtilityFunction.CheckLineData(_ground, transform);
            Move();
        }

        private void FixedUpdate()
        {
            Jump();
        }

        private void Move()
        {

            if (_inputNone) return;

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
            _animator.SetFloat(_hashFront, velocity.z, 0.1f, Time.deltaTime);
            _animator.SetFloat(_hashSide, velocity.x, 0.1f, Time.deltaTime);
        }

        private void Jump()
        {
            if(_isJump)
            {
                _rigidBody.AddForce(transform.up * JumpPower, ForceMode.Impulse);
                _isJump = false;
            }
        }

        private void OnMoveHandle(Vector2 movement)
        {
            _inputX = movement.x;
            _inputY = movement.y;
        }

        private void OnJumpHandle(bool flg, EInputReader key)
        {
            _isJump = flg;
        }
    }
}

