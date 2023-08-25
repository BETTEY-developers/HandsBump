#include <iostream>
#include <string>
#include <Windows.h>
#include "Player.h"
#include "Game.h"

using namespace std;

#define delegate(type) (type (*)())



namespace handsbump {
	namespace ui {
		void gotoxy(int xpos, int ypos)
		{
			COORD scrn;
			HANDLE hOuput = GetStdHandle(STD_OUTPUT_HANDLE);
			scrn.X = xpos; scrn.Y = ypos;
			SetConsoleCursorPosition(hOuput, scrn);
		}
		void Logo() {
			cout << "Hands Bump!  Copyright Elipese 2023-114514" << endl;
		}
		void Clear() {
			gotoxy(0, 0);
		}
		int Menu(string* menuitems, int length, bool idfromone = true, int (*startcontent)() = nullptr, int (*endcontent)() = nullptr) {
			while (true) {
				Clear();
				if (startcontent) {
					startcontent();
				}
				for (int i = 0; i < length; i++)
				{
					string item = menuitems[i];
					cout << i + (idfromone?1:0) << ". " << item << endl;
				}
				if (endcontent) {
					endcontent();
				}
				int index;
				cin >> index;
				if (idfromone) {
					if (index <= length && index > 0) {
						return index;
					}
					else {
						Clear();
					}
				}
				else {
					if (index < length && index > -1) {
						return index;
					}
					else {
						Clear();
					}
				}
				
			}
		}
	}
}

void StartMenu() {
	int mi = handsbump::ui::Menu(new string[5]{ "普通模式","10点模式","15点模式","自定义模式","退出菜单" }, 5, true, delegate(int)handsbump::ui::Logo);
	
}

int main() {
	handsbump::game::Game::InitStatus();
	int mi = handsbump::ui::Menu(new string[2]{ "开始游戏","退出游戏" }, 2, true, delegate(int)handsbump::ui::Logo);
	if (mi == 1) {
		StartMenu();
	}
}