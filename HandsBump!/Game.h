#pragma once

#include <vector>
#include <map>
#include "Player.h"

namespace handsbump {
	namespace game {

		class Status {
		public:
			static int currentPlayer;
			static Player* WinPlayer;
		};

		class Game {
		public:
			static std::vector<handsbump::game::Player> players;
			static void InitStatus();
			static void InitPlayersInstance(int count, std::map<int,std::vector<int>> option);
			static handsbump::game::Player* GetPlayerRefInstance(int index);
			static handsbump::game::Player* GetCurrentPlayerRefInstance();
			static void NextPlayer();
			static int IsWin();
		};
	}
}