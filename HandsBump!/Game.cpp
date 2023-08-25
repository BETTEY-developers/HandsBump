#include "Game.h"
#include "Player.h"
#include <vector>

// ����������ѡ���ʼ����Ҽ���
// count: �������
// option:
//   key: �������
//   value: ��ʼ��ѡ��:
//     [0]:  hand����
//     [1]:  Ŀ�����
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

// ��ȡ��ǰ��Ҷ�������
handsbump::game::Player* handsbump::game::Game::GetCurrentPlayerRefInstance()
{
	return &players[Status::currentPlayer];
}

// ����������ȡ��Ҷ�������
handsbump::game::Player* handsbump::game::Game::GetPlayerRefInstance(int index) 
{
	return &players[index];
}

// �л���һ�����
// �������������
void handsbump::game::Game::NextPlayer()
{
	Status::currentPlayer++;
}

// ��ʼ��״̬
void handsbump::game::Game::InitStatus()
{
	Status::currentPlayer = 0;
	Status::WinPlayer = nullptr;
	players = std::vector<handsbump::game::Player>{};
}

// �Ƿ������Ӯ��
// return: Ӯ��������ô�����Game::Status::WinPlayer�ֶ�
//   1: �����Ӯ��
//   0: û�����Ӯ
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
