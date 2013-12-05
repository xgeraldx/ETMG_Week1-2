using UnityEngine;
using System.Collections;

public static class GameState {

	public enum State{
		Running,
		Pause,
		GameOver
	};

	public static State gameState = State.Running;


}
