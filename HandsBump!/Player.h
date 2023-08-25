#pragma once
#include<vector>
#include<string>

namespace handsbump {
    namespace game {
		class Player
		{
		public:
			Player(int handscount, int target);
			~Player();
			void Name(std::string name);
			std::string Name();
			int Add(Player src, int srchand, int dsthand);
			std::vector<int> GetHands();
		private:
			std::vector<int> hands;
			int target;
			std::string name;
		};
	}
}
