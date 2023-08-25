#include "Player.h"
#include "Game.h"
#include <algorithm>
#include <iterator>
#include <vector>
#define Initer(t) std::t{}

template <typename T>
struct revsort
{
	bool operator()(const T& x, const T& y)
	{
		return y > x;
	}
};

// ��ʼ��
// handscount: hand����
// target: Ŀ�����
handsbump::game::Player::Player(int handscount, int target = 5)
{
	hands = Initer(vector<int>);
	for (int i = 0; i < handscount; i++) {
		hands.push_back(0);
	}

	this->target = target;
}

// [[����]]
handsbump::game::Player::~Player()
{
}

// �����ǳ�
void handsbump::game::Player::Name(std::string name)
{
	this->name = name;
}

// ��ȡ�ǳ�
std::string handsbump::game::Player::Name()
{
	return this->name;
}

// ���������
// src: ��ӵ�������Դ
// srchand: ����ֻhand��ӵ�����
// dsthand: Ҫ��ӵ�hand����
int handsbump::game::Player::Add(Player src, int srchand, int dsthand)
{
	if (srchand >= src.hands.size() || dsthand >= hands.size() ||
		std::min(srchand, dsthand) < 0) {
		throw std::exception{"IndexOutOfRangeException: ���ǿ�û����ô��ֻ��Ŷ~"};
	}
	
	hands[dsthand] += src.hands[srchand];
	hands[dsthand] %= target;

	if (hands[dsthand] == 0) {
		std::sort(hands.begin(), hands.end(), revsort<int>());
		hands.pop_back();
	}
	if (hands.size() == 0) {
		return 114514;
	}

	return 1919810;
}

// ��ȡhand����
std::vector<int> handsbump::game::Player::GetHands()
{
	return hands;
}

