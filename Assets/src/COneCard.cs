using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class COneCard : MonoBehaviour {

	public GameObject canvas;

	//カード種別（列挙体で作りたかったけど対応していないので仕方なく変数で定義する）
	public enum CardType {
		Spade,
		Heart,
		Diamond,
		Clover,
		Joker,
		Count
	}

	public const int MaxCount = 4 * 13 + 1;
	public Texture[] trumpTextures = new Texture[MaxCount];
	public UnityEngine.UI.RawImage mainGraph;
	public UnityEngine.UI.RawImage hideGraph;
	public int Type;                //カードの種別
	public int Number = 0;          //カードの数字
	public bool Cleared = false;	//完成されたかどうか
	public bool Hide = false;       //裏返しになっているかどうか
	public bool MotionOpen = false;     //オモテ面にするアニメーション中かどうか
	public bool MotionClose = false;    //裏返しにするアニメーション中かどうか
	public Vector2 Location;        //表示座標


	// Use this for initialization
	void Start() {

	}

	// Update is called once per frame
	void Update() {
		if((int)this.transform.position.x != (int)this.Location.x || (int)this.transform.position.y != (int)this.Location.y) {
			//表示座標に向かって直線移動
			this.transform.position = new Vector3(
				(int)this.transform.position.x +
					((this.transform.position.x < this.Location.x && System.Math.Abs(this.transform.position.x - this.Location.x) > 100) ? 20
					: ((this.transform.position.x < this.Location.x && System.Math.Abs(this.transform.position.x - this.Location.x) > 10) ? 10
					: ((this.transform.position.x < this.Location.x) ? 1
					: ((this.transform.position.x > this.Location.x && System.Math.Abs(this.transform.position.x - this.Location.x) > 100) ? -20
					: ((this.transform.position.x > this.Location.x && System.Math.Abs(this.transform.position.x - this.Location.x) > 10) ? -10
					: ((this.transform.position.x > this.Location.x) ? -1
					: 0)))))),
				(int)this.transform.position.y +
					((this.transform.position.y < this.Location.y && System.Math.Abs(this.transform.position.y - this.Location.y) > 100) ? 20
					: ((this.transform.position.y < this.Location.y && System.Math.Abs(this.transform.position.y - this.Location.y) > 10) ? 10
					: ((this.transform.position.y < this.Location.y) ? 1
					: ((this.transform.position.y > this.Location.y && System.Math.Abs(this.transform.position.y - this.Location.y) > 100) ? -20
					: ((this.transform.position.y > this.Location.y && System.Math.Abs(this.transform.position.y - this.Location.y) > 10) ? -10
					: ((this.transform.position.y > this.Location.y) ? -1
					: 0)))))),
				0
			);
		}

		if(this.MotionOpen == true) {
			if(this.hideGraph.transform.localScale.x > 0) {
				this.hideGraph.transform.localScale = new Vector3(this.hideGraph.transform.transform.localScale.x - 0.2f, 1.0f, 0);
			} else if(this.mainGraph.transform.localScale.x < 1.0f) {
				this.hideGraph.transform.localScale = new Vector3(0, 1.0f, 0);
				this.mainGraph.transform.localScale = new Vector3(this.mainGraph.transform.transform.localScale.x + 0.2f, 1.0f, 0);
			} else {
				this.hideGraph.transform.localScale = new Vector3(0, 1.0f, 0);
				this.mainGraph.transform.localScale = new Vector3(1.0f, 1.0f, 0);
				this.MotionOpen = false;
			}
		}

		if(this.MotionClose == true) {
			if(this.mainGraph.transform.localScale.x > 0) {
				this.mainGraph.transform.localScale = new Vector3(this.mainGraph.transform.transform.localScale.x - 0.2f, 1.0f, 0);
			} else if(this.hideGraph.transform.localScale.x < 1.0f) {
				this.mainGraph.transform.localScale = new Vector3(0, 1.0f, 0);
				this.hideGraph.transform.localScale = new Vector3(this.hideGraph.transform.transform.localScale.x + 0.2f, 1.0f, 0);
			} else {
				this.mainGraph.transform.localScale = new Vector3(0, 1.0f, 0);
				this.hideGraph.transform.localScale = new Vector3(1.0f, 1.0f, 0);
				this.MotionClose = false;
			}
		}
	}


	//カードの属性をセットする
	public void SetCard(int type, int num, bool opened) {
		this.Type = type;
		if(1 <= num && num <= 13 || type == (int)CardType.Joker) {
			this.Number = num;
		} else {
			Debug.LogError("不正なカードの数値 [" + num + "] です。");
		}
		if(opened == true) {
			this.Open();
		} else {
			this.Close();
		}
	}


	//カードをオモテにする
	public void Open() {
		if(canvas.GetComponent<CMainRoutine>().OpenCount >= 2) {
			Debug.Log("これ以上カードを表にできません。");
			return;
		}
		this.Hide = false;

		//オモテ面を見せる
		this.mainGraph.texture = this.trumpTextures[this.Type * 13 + (this.Number - 1)];
		this.mainGraph.transform.localScale = new Vector3(0, 1, 0);
		this.MotionOpen = true;

		canvas.GetComponent<CMainRoutine>().OpenCount++;
		StartCoroutine(canvas.GetComponent<CMainRoutine>().Judge());
	}


	//カードを裏返しにする
	public void Close() {
		this.Hide = true;
		this.MotionClose = true;
		canvas.GetComponent<CMainRoutine>().OpenCount--;
	}


	//カードを反転させる
	public void Turn() {
		if(this.Cleared == true) {
			return;     //確定されたカードは変更できない
		}

		if(this.Hide == false) {
			if(canvas.GetComponent<CMainRoutine>().OpenCount >= 1) {
				Debug.Log("カードが開いているときはプレイヤーによって閉じることはできません。");
				return;
			}
			this.Close();
		} else {
			this.Open();
		}
	}
}
