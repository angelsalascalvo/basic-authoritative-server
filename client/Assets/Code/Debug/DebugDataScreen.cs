using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DebugDataScreen : MonoBehaviour {
	//// REF PUB
	[Header("Player Position")]
	public GameObject player;
	[Header("Screen Texts")]
	public Text textFPS, textPosition;

	//// VAR PRI
	private float deltaTime = 0.0f;
	private string text;
	private Rect rect;

	void Update() {
		deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;

		//FPS
		float msec = deltaTime * 1000.0f;
		float fps = 1.0f / deltaTime;
		text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
		textFPS.text = text;

		//Posicion jugador
		text = "X: " + player.transform.position.x + " | Y: " + player.transform.position.y;
		textPosition.text = text;
	}
}