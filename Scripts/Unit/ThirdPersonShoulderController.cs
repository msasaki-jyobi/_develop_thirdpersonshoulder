using Cinemachine;
using develop_common;
using develop_tps;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

// �ړ��ł��Ȃ����R�FApplyRootmotion & State��Locomotion�ɂȂ��ĂȂ�

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
        public bool IsNotRootMotion;

        static int _hashFront = Animator.StringToHash("Front");
        static int _hashSide = Animator.StringToHash("Side");

        private float _inputX;
        private float _inputY;
        private CinemachinePOV _currentPOV;
        private bool _inputNone;

        private bool _canJump;
        private bool _isJump;

        // �ǉ�
        private Camera _camera;
        private Vector3 _tpsVelocity;
        private Quaternion _targetRotation;

        private EUnitStatus _unitStatus;


        private void Start()
        {
            _inputReader.MoveEvent += OnMoveHandle;
            _inputReader.PrimaryActionCrossEvent += OnJumpHandle;

            _camera = Camera.main;

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
            // �ǉ��d��
            if (_addGravity > 0)
                _rigidBody.AddForce(Vector2.down * _addGravity * Time.fixedDeltaTime, ForceMode.Acceleration);
            //if (_unitStatus.UnitState != EUnitState.Play) return;
            // �ړ�����
            if (IsNotRootMotion)
                if (CheckSloopAngle())
                    _rigidBody.velocity = new Vector3(_tpsVelocity.x * _moveSpeed, _rigidBody.velocity.y, _tpsVelocity.z * _moveSpeed);
        }

        private void Move()
        {

            if (_inputNone) return;

            // ���͂��擾
            var leftStick = new Vector3(_inputX, 0, _inputY).normalized;

            if (IsNotRootMotion)
            {
                // �J�������猩�ĕ��p�����߂�(Tps����ڐA�jRootMotion���g�p���Ȃ��ړ��p
                var tpsHorizontalRotation = Quaternion.AngleAxis(_camera.transform.rotation.eulerAngles.y, Vector3.up);
                _tpsVelocity = tpsHorizontalRotation * new Vector3(_inputX, 0, _inputY).normalized;
            }

            // �ړ����x���v�Z
            var velocity = Speed * leftStick;

            // �������X�V
            var horizontalRotation = Quaternion.AngleAxis(_currentPOV.m_HorizontalAxis.Value, Vector3.up);
            var verticalRotation = Quaternion.AngleAxis(_currentPOV.m_VerticalAxis.Value, Vector3.right);
            transform.rotation = horizontalRotation;
            _eye.localRotation = verticalRotation;

            // Animator�ɔ��f
            _animator.SetFloat(_hashFront, velocity.z, 0.1f, Time.deltaTime);
            _animator.SetFloat(_hashSide, velocity.x, 0.1f, Time.deltaTime);
        }

        private void Jump()
        {
            if (_isJump)
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
        // private Slope Parameter
        private float _slopeAngle;
        private float _staminaTimer;
        [Header("Slope Parameter")]
        [SerializeField] private float _maxSlopeAngle = 45f;
        [SerializeField] private float _slopeDistance = 0.2f;

        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private float _dashRange = 1.5f;
        [SerializeField] private float _stamina = 10f;
        [SerializeField] private float _staminaHealSpeed = 1f;
        [SerializeField] private float _addGravity = 600f;
        private void OnJumpHandle(bool flg, EInputReader key)
        {
            _isJump = flg;
        }
        /// <summary>
        /// �p�x�����m���Ĉړ��ł��邩����
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public bool CheckSloopAngle()
        {
            float maxSlopeAngle = _maxSlopeAngle;
            float downwardAngle = 25f; // ���̒l�𒲐����āARay�̉������̊p�x��ύX
            Vector3 forwardDown = (transform.forward - Vector3.up * Mathf.Tan(downwardAngle * Mathf.Deg2Rad)).normalized;
            Ray ray = new Ray(transform.position + Vector3.up * 0.1f, forwardDown);

            float rayDistance = _slopeDistance; // Ray�̒���
            Color rayColor = Color.blue; // Ray�̐F
            Debug.DrawRay(ray.origin, ray.direction * rayDistance, rayColor);

            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, rayDistance))
            {
                float angle = Vector3.Angle(hit.normal, Vector3.up);
                _slopeAngle = angle;
                return angle <= maxSlopeAngle ? true : false;
            }
            else
            {
                _slopeAngle = 0;
                return true;
            }
        }
        public bool IsCheckInputControl()
        {
            bool check = true;
            //if (IsNotInputReader) check = false; // ����s��
            //if (_unitActionLoader.UnitStatus.Value != EUnitStatus.Ready) check = false;
            return check;
        }
    }

}

