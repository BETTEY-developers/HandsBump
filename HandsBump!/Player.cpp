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

// 初始化
// handscount: hand数量
// target: 目标点数
handsbump::game::Player::Player(int handscount, int target = 5)
{
	hands = Initer(vector<int>);
	for (int i = 0; i < handscount; i++) {
		hands.push_back(0);
	}

	this->target = target;
}

// [[废弃]]
handsbump::game::Player::~Player()
{
}

// 设置昵称
void handsbump::game::Player::Name(std::string name)
{
	this->name = name;
}

// 获取昵称
std::string handsbump::game::Player::Name()
{
	return this->name;
}

// 将点数相加
// src: 相加点数的来源
// srchand: 与哪只hand相加的索引
// dsthand: 要相加的hand索引
int handsbump::game::Player::Add(Player src, int srchand, int dsthand)
{
	if (srchand >= src.hands.size() || dsthand >= hands.size() ||
		std::min(srchand, dsthand) < 0) {
		throw std::exception{"IndexOutOfRangeException: 你们可没有这么多只手哦~"};
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

// 获取hand集合
std::vector<int> handsbump::game::Player::GetHands()
{
	return hands;
}

