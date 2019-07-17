using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Osakana4242 {
	public sealed class MainPart : MonoBehaviour {
		public Data data;

		// 操作
		// 左右によける、前に進む.
		// 左右にかき分ける

		// 人にぶつかると後ろに戻される

		// スタート
		// 
		// 終了

		StateMachine<MainPart> sm_;
		List<MyObject> objectList_;
		public ResourceBank resource;
		public GameObject cameraGo;
		public int playerId;
		public WaveData waveData;
		public float startWaveZ_ = 3f;
		public int blockI_;
		public int waveI_;

		public sealed class MyObject : MonoBehaviour {
			public int id;
			public string category;
			public Player player;
			public Enemy enemy;
		}

		public sealed class Player {

		}

		public sealed class Enemy {
			public Quaternion targetRot;
		}

		void Awake() {
			sm_ = new StateMachine<MainPart>(stateInit_g_);
			objectList_ = new List<MyObject>();
			Application.logMessageReceived += OnLog;
		}
		public void OnLog(string condition, string stackTrace, LogType type) {
			switch (type) {
				case LogType.Exception:
				Debug.Break();
				GameObject.Destroy(gameObject);
				Application.Quit();
				break;
			}
		}

		void OnDestroy() {
			Application.logMessageReceived -= OnLog;
			sm_ = null;
			objectList_ = null;
		}
		void FixedUpdate() {

			{
				var player = GetPlayer();
				if (player != null) {
					var rb = player.GetComponent<Rigidbody>();
					var v = rb.velocity;
					// if (0f < v.y) {
					// 	v.y = 0f;
					// 	rb.velocity = v;
					// }
					// var pos = rb.position;
					// if (0f < pos.y) {
					// 	pos.y = 0f;
					// 	rb.position = pos;
					// }

				}
			}

			foreach (var item in objectList_) {
				var enemy = item.enemy;
				if (enemy == null) continue;
				var rb = item.GetComponent<Rigidbody>();
				var speed = 1f;
				var trot = enemy.targetRot;
				rb.rotation = Quaternion.RotateTowards(rb.rotation, trot, 180f * Time.deltaTime);
				var forward = rb.rotation * Vector3.forward;
				forward.y = 0f;
				forward.Normalize();
				var deltaAngle = Quaternion.Angle(rb.rotation, trot);

				if (forward != Vector3.zero && deltaAngle < 5f) {
					rb.position += forward * speed * Time.deltaTime;
				}
			}
		}

		void Update() {
			sm_.Update(this);
		}

		public MyObject FindObjectById(int id) {
			foreach (var item in objectList_) {
				if (item.id == id) return item;
			}
			return null;
		}

		public MyObject GetPlayer() {
			return FindObjectById(playerId);
		}
		int autoincrement;
		public int CreateObjectId() {
			return ++autoincrement;
		}


		static StateMachine<MainPart>.StateFunc stateExit_g_ = (_evt) => {
			switch (_evt.type) {
				case StateMachineEventType.Enter: {
						UnityEngine.SceneManagement.SceneManager.LoadScene("main");
						return null;
					}
				default:
				return null;
			}
		};

		static StateMachine<MainPart>.StateFunc stateInit_g_ = (_evt) => {
			switch (_evt.type) {
				case StateMachineEventType.Update: {
						var self = _evt.owner;

						{
							var waveJson = self.resource.Get<TextAsset>("wave");
							self.waveData = JsonUtility.FromJson<WaveData>(waveJson.text);
						}

						{
							var playerPrefab = self.resource.Get<GameObject>("player");
							var go = GameObject.Instantiate(playerPrefab, Vector3.zero, Quaternion.identity, self.transform);
							var obj = go.AddComponent<MyObject>();
							obj.id = self.CreateObjectId();
							obj.category = "player";
							obj.player = new Player();
							self.objectList_.Add(obj);
							self.playerId = obj.id;
							var camera = self.cameraGo.GetComponent<CameraController>();
							camera.target = go;
						}

						{
							var prefab = self.resource.Get<GameObject>("stone");

							var basePos = new Vector3(0f, 0f, 3f);
							var count = 4096;
							Random.InitState(1);
							var list = new List<Lib.MaterialPropertyBlockComponent>();
							var colors = new Color32[] {
								new Color32(0xff, 0xff, 0x00, 0xff),
								new Color32(0xff, 0x00, 0x00, 0xff),
								new Color32(0x00, 0xff, 0x00, 0xff),
								new Color32(0x00, 0x00, 0xff, 0xff),
							};
							for (var i = 0; i < count; i++) {
								var pos = basePos + new Vector3(
									Random.Range(-1f, 1f),
									i * 1f,
									Random.Range(-1f, 1f)
								);
								var rot = Quaternion.Euler(
									Random.Range(0f, 360f),
									Random.Range(0f, 360f),
									Random.Range(0f, 360f)
								);

								var scale = new Vector3(
									Random.Range(1f, 3f),
									Random.Range(1f, 3f),
									Random.Range(1f, 3f)
								);
								var go = GameObject.Instantiate(prefab, pos, Quaternion.identity, self.transform);
								var tr = go.transform;
								tr.localScale = Vector3.Scale(tr.localScale, scale);
								go.GetComponents<Lib.MaterialPropertyBlockComponent>(list);
								var color = colors[Random.Range(0,colors.Length)];
								foreach (var comp in list) {
									comp.color = color;
								}
							}

						}

						return stateMain_g_;
					}
				default:
				return null;
			}
		};

		void StepWave() {
			var self = this;

			var cellSize = 1f;
			var targetZ = self.GetPlayer().transform.position.z + 20f;

			var enemyPrefab = self.resource.Get<GameObject>("enemy");
			var waveData = self.waveData;
			var lastZ = 0f;
			var items = waveData.blocks[blockI_].items;
			var enemyCount = items.Length / 2;
			for (var i = self.waveI_; i < enemyCount; i++) {
				self.waveI_ = i;
				var j = i * 2;
				var offset = i * 3;
				var pos = new Vector3(items[j + 0], 0f, items[j + 1]) * cellSize;
				pos.z += self.startWaveZ_;
				lastZ = pos.z;
				if (targetZ < pos.z) return;

				var go = GameObject.Instantiate(enemyPrefab, pos, Quaternion.LookRotation(Vector3.back), self.transform);
				var obj = go.AddComponent<MyObject>();
				obj.id = self.CreateObjectId();
				obj.category = "enemy";
				obj.enemy = new Enemy();
				obj.enemy.targetRot = Quaternion.LookRotation(Vector3.back) * Quaternion.Euler(0f, Random.Range(-10f, 10f), 0f);
				self.objectList_.Add(obj);
			}
			self.waveI_ = 0;
			self.blockI_ = (self.blockI_ + 1) % waveData.blocks.Length;
			startWaveZ_ = lastZ + 1f * cellSize;
		}

		static StateMachine<MainPart>.StateFunc stateMain_g_ = (_evt) => {
			var self = _evt.owner;
			// self.StepWave();

			var player = self.GetPlayer();
			var playerPos = player.transform.position;
			self.data.distance = Mathf.Max(self.data.distance, playerPos.z);

			var z = playerPos.z - 20;
			for (var i = self.objectList_.Count - 1; 0 <= i; i--) {
				var obj = self.objectList_[i];
				if (obj.enemy == null) continue;
				var pos = obj.transform.position;
				if (z < pos.z) continue;
				self.objectList_.RemoveAt(i);
				GameObject.Destroy(obj.gameObject);
			}

			var isFall = playerPos.y < -5;
			if (isFall) {
				return stateFall_g_;
			}

			if (Input.GetKeyDown(KeyCode.R)) {
				return stateExit_g_;
			}

			return null;
		};

		/** 落下 */
		static StateMachine<MainPart>.StateFunc stateFall_g_ = (_evt) => {
			var self = _evt.owner;
			switch (_evt.type) {
				case StateMachineEventType.Enter:
				self.cameraGo.GetComponent<CameraController>().target = null;
				break;
			}

			if (3f <= _evt.sm.time) {
				return stateExit_g_;
			}

			return null;
		};

		[System.Serializable]
		public class Data {
			/** 経過時間 */
			public float time;
			/** 制限時間 */
			public float duration;
			/** 走行距離 */
			public float distance;
			public float speed;
			public float speedMax;
		}

	}
}
