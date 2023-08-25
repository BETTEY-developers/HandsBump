#include "Game.h"
#include "Player.h"
#include <vector>

// 根据数量与选项初始化玩家集合
// count: 玩家数量
// option:
//   key: 玩家索引
//   value: 初始化选项:
//     [0]:  hand数量
//     [1]:  目标点数
void handsbump::game::Game::InitPlayersInstance(int count, std::map<int, std::vector<int>> option)
{
	InitStatus();
	std::vector<int> keys{};
	for (std::map<int, std::vector<int>>::iterator iter = option.begin(); iter != option.end(); iter++)
		keys.push_back(iter->first);
	for (int i = 0; i < count; i++) {
		if (*std::find(keys.begin(), keys.end(), i) != -1) {
			players.push_back(Player{ option[i][0], option[i][1] });
		}
		else {
			players.push_back(Player{ 2, 5 });
		}
	}
}

// 获取当前玩家对象引用
handsbump::game::Player* handsbump::game::Game::GetCurrentPlayerRefInstance()
{
	return &players[Status::currentPlayer];
}

// 根据索引获取玩家对象引用
handsbump::game::Player* handsbump::game::Game::GetPlayerRefInstance(int index) 
{
	return &players[index];
}

// 切换下一个玩家
// 将玩家索引自增
void handsbump::game::Game::NextPlayer()
{
	Status::currentPlayer++;
}

// 初始化状态
void handsbump::game::Game::InitStatus()
{
	Status::currentPlayer = 0;
	Status::WinPlayer = nullptr;
	players = std::vector<handsbump::game::Player>{};
}

// 是否有玩家赢了
// return: 赢的玩家引用储存在Game::Status::WinPlayer字段
//   1: 有玩家赢了
//   0: 没有玩家赢
int handsbump::game::Game::IsWin()
{
	for (std::vector<handsbump::game::Player>::iterator iter = players.begin(); iter != players.end(); iter++) {
		if (iter->GetHands().empty()) {
			Status::WinPlayer = &*iter;
			return 1;
		}
	}
	return -1;
}
