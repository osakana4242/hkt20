# memo

ステージに点在する宝石の山をブルードーザーで崩して、
自分の陣地に運び込む

実は対戦ゲームにできそう

構成要素
* ワールド
	* プレイヤー(ブルドーザー)
	* 宝石 (いろんな色、形状がある)
	* 地面
	* 自分の陣地
* HUD
	* タイム
	* 獲得宝石数


```csharp

selaed class StageProgress {
	public float time;
	public float duration;
}

sealed class Player {
	public int score;
}

sealed class Stone {
	public int score;
}

sealed class Home {
	public int ownerId;
}


```
