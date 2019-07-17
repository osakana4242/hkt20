using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Osakana4242 {
	public class Dog : MonoBehaviour {

		public Rigidbody rigidBody;
		public float speed = 5f;
		public float rotSpeed = 180f;
		public Vector2 inputVec;

		// Start is called before the first frame update
		void Start() {

		}

		// Update is called once per frame
		void Update() {
			inputVec.x = Input.GetKey(KeyCode.LeftArrow) ?
					-1f :
					Input.GetKey(KeyCode.RightArrow) ?
							1f :
							0f;
			inputVec.y = Input.GetKey(KeyCode.DownArrow) ?
				-1f :
				Input.GetKey(KeyCode.UpArrow) ?
					1f :
					0f;
		}

		void FixedUpdate() {
			var x = inputVec.x;
			var y = inputVec.y;
			var dt = Time.fixedDeltaTime;
			var rot = rigidBody.rotation;
			rot *= Quaternion.Euler(0f, x * rotSpeed * dt, 0f);
			rigidBody.rotation = rot;

			var pos = rigidBody.position;
			var speed2 = y * speed; // * Time.deltaTime;
			var nextPos = pos + rot * new Vector3(0f, 0f, speed2);
			var deltaPos = nextPos - pos;
			var velocity = rigidBody.velocity;
			deltaPos.y = velocity.y;
			velocity = deltaPos;
			rigidBody.velocity = velocity;
		}
	}
}