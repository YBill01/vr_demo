using Transport.Client;
using Transport.Messaging;
using UnityEngine;

public class PlayerNetworkObject : NetworkObject {

	[SerializeField] private GameObject _usernameLabel;

	private bool _thisLocalPlayer;

	public bool ThisLocalPlayer {

		get => _thisLocalPlayer;

		set {
#if UNITY_EDITOR || UNITY_STANDALONE
			var controller = GetComponent<CharacterController>();
			if (controller)
				controller.enabled = false;
#endif
			foreach (var c in Components) 
			{
				if (c is NetworkTransform t) {
					t.EnableSyncing = value;
				}
			}
			print("DisableCanvas " + value);
			if (value)
			{
				GetNetworkComponent<NetworkEmoji>().DisableCanvas();
				InitNetworkComponent();
				// TODO - TESTING DEFINE
				if (ClientTypeHolder.Instance.ClientType == ClientType.Bot) 
				{
					gameObject.AddComponent(typeof(BotController));
					//GetNetworkComponent<NetworkAnimator>().SetTrigger("Walk");
				} 
				else 
				{
#if UNITY_EDITOR || UNITY_STANDALONE
						var playerInput = gameObject.AddComponent(typeof(SamplePlayerInput)) as SamplePlayerInput;
						playerInput.Animator = GetNetworkComponent<NetworkAnimator>();
#endif
				}
			}
			else 
			{
				Destroy(gameObject.GetComponent(typeof(Rigidbody)));
				Game.GetManager<Client_NetworkObjects>().UnsubscripeFromUpdate(this);
				_usernameLabel.SetActive(true);
			}

			_thisLocalPlayer = value;

			void InitNetworkComponent()
			{
				var battleClientCore = Game.GetManager<BattleClientCore>();
				var skinSetter = GetComponent<NetworkSkinSetter>();
				skinSetter.SetSkin(battleClientCore.SkinsName);
				var networkLabel = GetComponent<NetworkLabel>();
				networkLabel.ChangeLabel(battleClientCore.Username);
				Game.GetManager<Client_NetworkObjects>().SubscribeForUpdate(this);
				_usernameLabel.SetActive(false);
			}

		}

	}

	public override void ReadSyncData(MsgStreamReader reader, bool skipSync = false) {
		skipSync = skipSync ? skipSync : ThisLocalPlayer ? true : false;
		base.ReadSyncData(reader, skipSync);
	}

#if SERVER
	private void Awake() {
		Destroy(gameObject.GetComponent(typeof(Rigidbody)));
	}
#endif

}

public class SamplePlayerInput : MonoBehaviour
{

	[SerializeField] private float gravityDetectDistance = 0.5f;
	
	public NetworkAnimator Animator;
	private bool _isMove = false; // TODO - create normal input cheme

	private void Awake() {

		_cameraHolder = new GameObject("CameraHolder").transform;
		_cameraHolder.SetParent(transform);
		_cameraHolder.localPosition = Vector3.zero;
		_cameraHolder.localEulerAngles = new Vector3(20, 0, 0);

		_cameraTransform = Camera.main.transform; 
		_cameraTransform.SetParent(_cameraHolder);
		_cameraTransform.localPosition = new Vector3(0, 0.2f, -_cameraDistance);
		_cameraTransform.localEulerAngles = Vector3.zero;

	}

	float _moveSpeed = 5f;
	float _lookSens = 250f;
	float _wheelSens = 8f;

	bool _initJump = false;
	float _ySpeed = -1f;
	float _gravity = 9f;
	float _jumpPower = 120f;
	Rigidbody _rigi;

	Transform _cameraHolder;
	Transform _cameraTransform;
	float _lookAngleX = 20f;
	float _cameraDistance = 5f;

	private void Update()
	{

		if (Input.GetKeyDown(KeyCode.Space) && IsGrounded()) {
			_initJump = true;
		}
		if (Input.GetKeyDown(KeyCode.Escape)) {
			Cursor.visible = !Cursor.visible;
			Cursor.lockState = Cursor.visible ? CursorLockMode.None : CursorLockMode.Locked;
		}

	}

	private void FixedUpdate() {

		Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).normalized;

		if (IsGrounded()) {
			_ySpeed = 0f;
			if (_initJump) {
				_ySpeed = _jumpPower * Time.fixedDeltaTime;
				//Animator.SetTrigger("Jump"); 
				_isMove = false;
			}
		} else {
			_ySpeed -= _gravity * Time.fixedDeltaTime;
			_ySpeed = Mathf.Clamp(_ySpeed, -_gravity, _jumpPower);
		}
		
		_initJump = false;

		if (!Cursor.visible) {

			transform.Rotate(0, Input.GetAxis("Mouse X") * _lookSens * Time.fixedDeltaTime, 0);

			_lookAngleX -= Input.GetAxis("Mouse Y") * _lookSens * Time.fixedDeltaTime;
			_lookAngleX = Mathf.Clamp(_lookAngleX, 0f, 70f);
			_cameraHolder.localEulerAngles = new Vector3(_lookAngleX, 0, 0);

			_cameraDistance -= Input.GetAxis("Mouse ScrollWheel") * _wheelSens;
			_cameraDistance = Mathf.Clamp(_cameraDistance, 3f, 9.5f);
			_cameraTransform.localPosition = new Vector3(0, 0.95f, -_cameraDistance);

		}

		transform.Translate(new Vector3(input.x, _ySpeed, input.y) * _moveSpeed * Time.fixedDeltaTime);

		if (input != Vector2.zero) {
			if (!_isMove) {
				Animator.SetTrigger("Walk");
				_isMove = true;
			}
		} else if(input == Vector2.zero) {
			if (_isMove) {
				Animator.SetTrigger("Idle");
				_isMove = false;
			}
		}
	
	}

	private bool IsGrounded() {
		return Physics.Raycast(transform.position, Vector3.down, gravityDetectDistance);
	}

}