using UnityEngine;
using System.Collections;

public static class GameState {

	public enum State{
		Running,
		Pause,
		GameOver
	};
	public static int NUMROUNDS = 3;
	public static int round = 1;
	public static State gameState = State.Running;


	public static void NextRound()
	{
		round++;
		if(round  > NUMROUNDS)
		{
			gameState = State.GameOver;
		}
	}
}
