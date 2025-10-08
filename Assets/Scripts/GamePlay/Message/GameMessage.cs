using MessagePipe;
using PlayingCard.GamePlay.Configuration;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace PlayingCard.GamePlay.Message
{
	public struct QuitGameMessage { }

	public struct StartGameMessage { }

	public struct SelectGameMessage
	{
		public Game game;
	}
}