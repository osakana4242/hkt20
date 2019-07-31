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

		public TMPro.TextMeshProUGUI progressTextUI;
		public TMPro.TextMeshProUGUI centerTextUI;

		StateMachine<MainPart> sm_;
		List<MyObject> objectList_;
		public ResourceBank resource;
		public GameObject cameraGo;
		public int playerId;
		public int goalId;
		public WaveData waveData;
		public float startWaveZ_ = 3f;
		public int blockI_;
		public int waveI_;
		public Vector3 goalPosition = new Vector3(-15, 0f, -15f);
		public Vector3[] stonePositions = {
			new Vector3(-15f, 0f, 15f),
			new Vector3(15f, 0f, 15f),
			new Vector3(-15f, 0f, 0f),
			new Vector3(15f, 0f, 0f),
		};

		public sealed class MyObject : MonoBehaviour {
			public bool hasDestroy;
			public int id;
			public string category;
			public Player player;
			public Enemy enemy;
			public Goal goal;
			public Stone stone;

			public void Destroy() {
				hasDestroy = true;
			}
		}

		public struct CollisionInfo {
			public Collider collider;
			public Collision collision;
		}

		public class CollilsionObserver : MonoBehaviour {
			public System.Action<CollisionInfo> onEvent;
			public void OnDestroy() {
				onEvent = null;
			}
			public void OnTriggerEnter(Collider collider) {
				if (onEvent == null) return;
				onEvent(new CollisionInfo() {
					collider = collider,
				});
			}
			public void OnCollisionEnter(Collision collision) {
				if (onEvent == null) return;
				onEvent(new CollisionInfo() {
					collision = collision,
				});
			}
		}

		public sealed class Goal {
			public float time;
			public float rotateDuration = 4f;
			public int score = 0;
		}

		public sealed class Player {
			public int score;
		}

		public sealed class Stone {
			public int score = 1;
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
			for (var i = objectList_.Count - 1; 0 <= i; i--) {
				var obj = objectList_[i];
				{
					var player = obj.player;
					if (player != null) {
						UpdatePlayer(obj, player);
					}
				}
				{
					var goal = obj.goal;
					if (goal != null) {
						UpdateGoal(obj, goal);
					}
				}
				{
					var stone = obj.stone;
					if (stone != null) {
						UpdateStone(obj, stone);
					}
				}

			}


			for (var i = objectList_.Count - 1; 0 <= i; i--) {
				var obj = objectList_[i];
				if (!obj.hasDestroy) continue;
				objectList_.RemoveAt(i);
				GameObject.Destroy(obj.gameObject);
			}

			if (data.isPlaying) {
				data.time += Time.deltaTime;
			}
		}

		void UpdatePlayer(MyObject obj, Player player) {
			var rb = obj.GetComponent<Rigidbody>();
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
			player.score = GetGoal().goal.score;
		}

		void UpdateGoal(MyObject obj, Goal goal) {
			var y = 360f * goal.time / goal.rotateDuration;
			obj.transform.localRotation = Quaternion.Euler(0f, y, 0f);
			goal.time += Time.deltaTime;
		}

		void UpdateStone(MyObject obj, Stone stone) {
			if (obj.transform.position.y <= -5f) {
				obj.Destroy();
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
		public MyObject GetGoal() {
			return FindObjectById(goalId);
		}
		int autoincrement;
		public int CreateObjectId() {
			return ++autoincrement;
		}


		static StateMachine<MainPart>.StateFunc stateExit_g_ = (_evt) => {
			var self = _evt.owner;
			switch (_evt.type) {
				case StateMachineEventType.Enter: {
						self.data.isPlaying = false;
						UnityEngine.SceneManagement.SceneManager.LoadScene("main");
						return null;
					}
				default:
				return null;
			}
		};

		void addStone(Vector3 basePos, int count) {
			var self = this;
			var prefabs = new GameObject[] {
					self.resource.Get<GameObject>("stone_01"),
					self.resource.Get<GameObject>("stone_01"),
					self.resource.Get<GameObject>("stone_02"),
					self.resource.Get<GameObject>("stone_02"),
				};

			var list = new List<Lib.MaterialPropertyBlockComponent>();
			var colors = new Color32[] {
								new Color32(0xff, 0xff, 0x00, 0xff),
								new Color32(0xff, 0x00, 0x00, 0xff),
								new Color32(0x00, 0xff, 0x00, 0xff),
								new Color32(0x00, 0x00, 0xff, 0xff),
							};
			for (var i = 0; i < count; i++) {
				var pos = basePos + new Vector3(
					Random.Range(-3f, 3f),
					i * 1f,
					Random.Range(-3f, 3f)
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
				var prefab = prefabs[i & (prefabs.Length - 1)];
				var go = GameObject.Instantiate(prefab, pos, Quaternion.identity, self.transform);
				var tr = go.transform;
				tr.localScale = Vector3.Scale(tr.localScale, scale);
				go.GetComponents<Lib.MaterialPropertyBlockComponent>(list);
				var color = colors[Random.Range(0, colors.Length)];
				foreach (var comp in list) {
					comp.color = color;
				}
				var obj = go.AddComponent<MyObject>();
				obj.id = self.CreateObjectId();
				obj.stone = new Stone();
				self.objectList_.Add(obj);
			}
		}

		static StateMachine<MainPart>.StateFunc stateInit_g_ = (_evt) => {
			switch (_evt.type) {
				case StateMachineEventType.Enter: {
						var self = _evt.owner;
						self.progressTextUI.text = "";
						self.centerTextUI.text = "READY";

						{
							var waveJson = self.resource.Get<TextAsset>("wave");
							self.waveData = JsonUtility.FromJson<WaveData>(waveJson.text);
						}


						{
							var prefab = self.resource.Get<GameObject>("goal");
							var go = GameObject.Instantiate(prefab, self.goalPosition, Quaternion.identity, self.transform);
							var obj = go.AddComponent<MyObject>();
							var colObserver = go.AddComponent<CollilsionObserver>();
							colObserver.onEvent = (_info) => {
								if (!self.data.isPlaying) return;
								var otherObj = _info.collider.GetComponentInParent<MyObject>();
								if (otherObj == null) return;
								if (otherObj.stone == null) return;
								obj.goal.score += otherObj.stone.score;
								otherObj.Destroy();
							};
							obj.id = self.CreateObjectId();
							obj.category = "goal";
							obj.goal = new Goal();
							self.objectList_.Add(obj);
							self.goalId = obj.id;
						}


						{
							var prefab = self.resource.Get<GameObject>("player");
							var goal = self.GetGoal();
							var go = GameObject.Instantiate(prefab, goal.transform.position + new Vector3(0f, 10f, 0f), Quaternion.identity, self.transform);
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
							Random.InitState(1);
							self.addStone(self.stonePositions[0], 1024);
							self.addStone(self.stonePositions[1], 1024);
							self.addStone(self.stonePositions[2], 512);
							self.addStone(self.stonePositions[3], 512);
						}
						return null;
					}
				case StateMachineEventType.Update: {
						if (1f <= _evt.sm.time) {
							return stateMain_g_;
						}
						return null;
					}

				default:
				return null;
			}
		};

		static StateMachine<MainPart>.StateFunc stateMain_g_ = (_evt) => {
			var self = _evt.owner;
			// self.StepWave();
			self.data.isPlaying = true;

			var player = self.GetPlayer();

			{
				var sb = new System.Text.StringBuilder();
				sb.AppendFormat("SCORE: {0:F0}\n", player.player.score);
				sb.AppendFormat("TIME: {0:F2}\n", self.data.RestTime);
				self.progressTextUI.text = sb.ToString();
			}
			{
				self.centerTextUI.text = "";
			}

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

			var hasTimeOver = self.data.RestTime <= 0f;
			if (hasTimeOver) {
				return stateTimeOver_g_;
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

		/** タイムオーバー */
		static StateMachine<MainPart>.StateFunc stateTimeOver_g_ = (_evt) => {
			var self = _evt.owner;
			switch (_evt.type) {
				case StateMachineEventType.Enter:
				self.centerTextUI.text = "TIME OVER";
				self.data.isPlaying = false;
				break;
			}

			if (3f <= _evt.sm.time) {
				return stateResult_g_;
			}

			return null;
		};

		/** 落下 */
		static StateMachine<MainPart>.StateFunc stateFall_g_ = (_evt) => {
			var self = _evt.owner;
			switch (_evt.type) {
				case StateMachineEventType.Enter:
				self.centerTextUI.text = "FALL";
				self.data.isPlaying = false;
				self.cameraGo.GetComponent<CameraController>().target = null;
				break;
			}

			if (3f <= _evt.sm.time) {
				return stateResult_g_;
			}

			return null;
		};

		static StateMachine<MainPart>.StateFunc stateResult_g_ = (_evt) => {
			var self = _evt.owner;
			switch (_evt.type) {
				case StateMachineEventType.Enter:
				self.centerTextUI.text = "PRESS Z KEY";
				self.data.isPlaying = false;
				self.cameraGo.GetComponent<CameraController>().target = null;
				break;
			}

			if (Input.GetKeyDown(KeyCode.Z)) {
				return stateExit_g_;
			}

			return null;
		};

		[System.Serializable]
		public class Data {
			public bool isPlaying;
			/** 経過時間 */
			public float time;
			/** 制限時間 */
			public float duration = 90f;
			/** 走行距離 */
			public float distance;
			public float speed;
			public float speedMax;
			public float RestTime => Mathf.Max(0f, duration - time);
		}

	}
}
