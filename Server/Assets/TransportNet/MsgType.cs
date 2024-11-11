
namespace Transport.Messaging {

	using UnityEngine;
	using System.Text;
	using System;
	using System.Collections.Generic;

	public class MsgType {

		public const short TstMessage = 1;        // Просто строка для тестов
												  //public const short BigMessage = 2;        // Большой меседж который приходит кусками
												  //public const short BigMessage_End = 3;    // Завершение большого меседжа

		public const short MasterIdRequest = 2;

		public const short Request_NetworkObjs = 4;
		public const short Create_NetworkObjs = 5;      
		public const short Destroy_NetworkObjs = 6;     
		public const short Refresh_NetworkObjs = 7;     
		public const short Synchronize_NetworkObjs = 8; 

		public const short JoinToLobby = 9;    // Попытка подключения к лобби
		public const short SynsWithInstance = 10; // Синхронизировать инстанс

		public const short PlayerInput = 11;      // Отправка ввода игрока на сервер

		public const short TickSyns = 12;       // Синхронизация системы тиков
		public const short TickCalibaration = 13;

		public const short SetClientId = 20;      // ClientManager - SetClientID

		public const short GetConnectInfo = 21;   // Переадрес. клиентов
		public const short SetConnectInfo = 22;   

		public const short SetClientInfo = 23;          
		public const short OnClientJoinToServer = 24;   // Когда клиент присоединился к боевому серверу
		public const short OnClientLeftFromServer = 25;

		public const short ChatMessage = 55;

	}
	
}




