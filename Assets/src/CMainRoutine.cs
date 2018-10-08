using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CMainRoutine : MonoBehaviour {

	/// <summary>
	/// カード情報
	/// </summary>
	public class CardData {
		public int ID;              //カード番号
		public GameObject obj;      //配置されたオブジェクトのインスタンス
		public Vector2 Position;    //位置
	}

	//定数: WebGL出力の都合で変数にしている
	public const int WindowWidth = 640;
	public const int WindowHeight = 480;
	public const int MarginX = 30;
	public const int MarginY = 30;

	public int LimitTimeSecond;     //制限時間: マイナスのとき無制限
	public int NGMax;               //間違えられる最大数

	public int OpenCount = 0;       //場に開かれている枚数
	public int OpenedID = -1;       //既に開かれているカードのID
	private int NGCounter = 0;      //間違えた回数
	private bool gameCleared = false;   //ゲームクリアしたかどうか

	public int Width = 6;           //初期値
	public int Height = 3;          //初期値
	public float TimeSecond = -1.0f;    //残り時間
	public CardData[,] CardList;        //配置されているカードの情報

	//エディター側で設定
	public GameObject gameClear;        //クローン生成元: 最前面表示にするために動的にクローンを生成する
	public GameObject gameClearClone;
	public GameObject startButton;
	public GameObject card;
	public GameObject canvas;
	public UnityEngine.UI.Text txtCount;
	public UnityEngine.UI.Text txtTime;


	// Use this for initialization
	void Start() {
		txtTime.text = "";
		txtCount.text = "";
	}


	// Update is called once per frame
	void Update() {
		if(this.IsGamingNow() == true) {
			//ゲームクリア表示
			if(this.gameCleared == true) {
				//ゲームクリア表示を生成
				this.gameCleared = false;
				gameClearClone = Instantiate(
					this.gameClear,
					new Vector3(0, 0, 0),
					transform.rotation
				);
				gameClearClone.transform.localScale = new Vector3(1, 0.1f, 0);
				gameClearClone.transform.SetParent(this.canvas.transform, false);
			} else if(gameClearClone != null) {
				//ゲームクリア表示のアニメーション
				if(0.0f < gameClearClone.transform.localScale.y && gameClearClone.transform.localScale.y < 1.0f) {
					gameClearClone.transform.localScale = new Vector3(
						gameClearClone.transform.localScale.x,
						gameClearClone.transform.localScale.y + 0.1f,
						0
					);
				}
			}

			//制限時間表示
			if(LimitTimeSecond > 0) {
				this.TimeSecond -= Time.deltaTime;
				if(this.TimeSecond < 0) {
					this.txtTime.text = "制限時間  タイムアップ";
					this.EndGame();
				} else {
					this.txtTime.text = "制限時間  " + (int)this.TimeSecond + " 秒";
				}
			}
		}
	}


	//間違えた回数をセットしてテキストを更新する
	public void SetNGCounter(int value) {
		this.NGCounter = value;
		if(this.NGCounter < 0) {
			txtCount.text = "";
		} else {
			txtCount.text = "あと " + this.NGCounter + " 回 まで";
		}
	}


	//ゲーム中であるかどうかを取得する
	public bool IsGamingNow() {
		return (this.CardList != null);
	}


	//ゲームを終了させる
	public void EndGame() {
		//ゲームクリア表示を消去
		Destroy(gameClearClone);

		//カードを初期化
		if(this.CardList != null) {
			txtCount.text = "";
			txtTime.text = "";
			for(var x = 0; x < this.Width; x++) {
				for(var y = 0; y < this.Height; y++) {
					this.deleteCard(x, y);
				}
			}
			this.CardList = null;
		}
		this.startButton.SetActive(true);
	}


	//ゲームを初期化して開始する
	public void Setup() {
		this.Reset(this.Width, this.Height);
		this.OpenCount = 0;
		this.SetNGCounter(NGMax);
		this.TimeSecond = LimitTimeSecond;
		this.startButton.SetActive(false);
	}


	//カードを並べる
	public void Reset(int width, int height) {
		if((width * height) % 2 == 1) {
			Debug.LogError("奇数枚ではカードが並べられません。");
			return;
		}

		//カードをランダムに生成して並べる
		this.Width = width;
		this.Height = height;

		//初期化
		this.EndGame();
		this.CardList = new CardData[this.Width, this.Height];

		//配置処理
		for(var i = 0; i < width * height / 2; i++) {
			int type, num;

			//既に存在するカードの場合はNG
			do {
				type = Random.Range(0, (int)COneCard.CardType.Count);
				num = Random.Range(1, 13 + 1);
				if(type == (int)COneCard.CardType.Joker) {
					num = 0;    //ジョーカーは数字がない
				}
			} while(this.existsCardID(type * 13 + num) == true);

			//空いているところをランダムに２箇所配置する
			this.setCard(type, num);
			this.setCard(type, num);
		}
	}


	//空いているところに指定したカードを配置し、その位置を返す
	private void setCard(int type, int num) {
		while(this.existsEmptyCard() == true) {
			var x = Random.Range(0, this.Width);
			var y = Random.Range(0, this.Height);

			//ランダム生成した位置にまだカードが置かれていなければ配置処理を行う
			if(this.CardList[x, y] == null) {
				var data = new CardData();
				data.ID = type * 13 + num;
				data.Position = new Vector2(x, y);
				data.obj = Instantiate(
					this.card,
					new Vector3(0, 0, 0),
					transform.rotation
				);
				data.obj.GetComponent<COneCard>().Location = new Vector2(
					105 + (WindowWidth - MarginX * 2 - 150) * x / (this.Width - 1),
					80 + (WindowHeight - MarginY * 2 - 150) * y / (this.Height - 1)
				);
				data.obj.transform.SetParent(this.canvas.transform, false);
				data.obj.GetComponent<COneCard>().SetCard(type, num, false);
				this.CardList[x, y] = data;
				return;
			}
		}
		Debug.LogError("新たにカードを配置できるスペースがありません。");
		return;
	}


	//指定した位置のカードを削除する
	private void deleteCard(int x, int y) {
		if(this.CardList[x, y] != null) {
			Destroy(this.CardList[x, y].obj);       //オブジェクトを破棄
			this.CardList[x, y] = null;
		}
	}


	//空のカードがまだ存在するかどうかを取得する
	private bool existsEmptyCard() {
		if(this.CardList != null) {
			for(var x = 0; x < this.Width; x++) {
				for(var y = 0; y < this.Height; y++) {
					if(this.CardList[x, y] == null) {
						return true;
					}
				}
			}
		}
		return false;
	}


	//カードの残り枚数を取得する
	private int GetCardCount() {
		int cnt = 0;
		if(this.CardList != null) {
			for(var x = 0; x < this.Width; x++) {
				for(var y = 0; y < this.Height; y++) {
					if(this.CardList[x, y] != null && this.CardList[x, y].obj.GetComponent<COneCard>().Cleared == false) {
						cnt++;
					}
				}
			}
		}
		return cnt;
	}


	//指定したIDのカードが既に存在するかどうかを取得する
	private bool existsCardID(int id) {
		if(this.CardList != null) {
			for(var x = 0; x < this.Width; x++) {
				for(var y = 0; y < this.Height; y++) {
					if(this.CardList[x, y] != null && this.CardList[x, y].ID == id) {
						return true;
					}
				}
			}
		}
		return false;
	}


	//開かれたカードを比較する
	public IEnumerator Judge() {
		if(this.OpenCount < 2) {
			yield break;     //２枚開かれていない場合は処理しない
		}

		//２枚の開かれたカードを探す
		var pair = new CardData[2];
		var n = 0;
		for(var x = 0; x < this.Width && n < 2; x++) {
			for(var y = 0; y < this.Height; y++) {
				if(this.CardList[x, y] != null
				&& this.CardList[x, y].obj.GetComponent<COneCard>().Cleared == false
				&& this.CardList[x, y].obj.GetComponent<COneCard>().Hide == false) {
					pair[n] = this.CardList[x, y];
					n++;
					continue;
				}
			}
		}

		//両者を比較する
		if(pair[0].ID == pair[1].ID) {
			//当たり
			//カードを確定する
			this.OpenCount = 0;
			this.CardList[(int)pair[0].Position.x, (int)pair[0].Position.y].obj.GetComponent<COneCard>().Cleared = true;
			this.CardList[(int)pair[1].Position.x, (int)pair[1].Position.y].obj.GetComponent<COneCard>().Cleared = true;

			//カードを消去する
			//this.deleteCard((int)pair[0].Position.x, (int)pair[0].Position.y);
			//this.deleteCard((int)pair[1].Position.x, (int)pair[1].Position.y);

			//終了判定
			if(GetCardCount() == 0) {
				//ゲームクリア
				yield return new WaitForSeconds(0.5f);
				this.gameCleared = true;
			}
		} else {
			//ハズレ: カードを伏せる
			yield return new WaitForSeconds(1.0f);
			pair[0].obj.GetComponent<COneCard>().Close();
			pair[1].obj.GetComponent<COneCard>().Close();
			this.SetNGCounter(this.NGCounter - 1);

			//終了判定
			if(this.NGCounter < 0) {
				//ゲームオーバー
				this.EndGame();
			}
		}
	}
}
