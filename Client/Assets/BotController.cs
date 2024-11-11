using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class BotController : MonoBehaviour
{

	private bool l = false;
	private int i = -1;
	private Avatar _avatar;
	private Vector3 _targetPosition;
	private float _targetRotation;

	private List<Vector3> _points = new List<Vector3>() {
		new Vector3(-5,0,-5),new Vector3(5,0,5),
		new Vector3(5,0,-5),new Vector3(-5,0,-5)
	};

	private void Awake() {

		transform.position = new Vector3(Random.Range(-15f, 15f), 5f, Random.Range(-5f, 5f));
		l = true;//Random.Range(0, 2) > 0;
		i = Random.Range(0, 4);
		_avatar = GetComponent<Avatar>();
		if(_avatar != null)
			_avatar.Randomize();
	}

	private void Update() {

		if (i == -1) {
			return;
		}

		transform.position = Vector3.MoveTowards(transform.position, _targetPosition, 5f * Time.deltaTime);
		transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, _targetRotation, 0), 5f * Time.deltaTime);


		if (Math.Abs(transform.position.x - _targetPosition.x) < 0.1f && Math.Abs(transform.position.z - _targetPosition.z) < 0.1f) {
			Next();
		}

	}

	private void Next()
	{

		i = l ? i - 1 : i + 1;
		i = i < 0 ? 3 : i > 3 ? 0 : i;

		_targetPosition = _points[i];

		if (transform.position.z == _targetPosition.z) {
			_targetRotation = transform.position.x > _targetPosition.x ? -90 : transform.position.x < _targetPosition.x ? 90 : 0;
		} else if(transform.position.x == _targetPosition.x) {
			_targetRotation = transform.position.z > _targetPosition.z ? 180 : transform.position.z < _targetPosition.z ? 0 : 0;
		}
	}
	

}