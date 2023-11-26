﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace HandsBump
{
    internal class Player
    {
        private List<int> hands = new();
        private List<int> noremovedhands = new();
        private int target = 0;
        private int stepcount = 0;
        private string name = string.Empty;
        private int handcount = 0;

        public int Target { init => target = value; get => target; }
        public int StepCount { get => stepcount; }
        public List<int> Hands { get => hands; }
        public string Name { get => name; set => name = value; }


        public int HandCount 
        { 
            init
            {
                for(int i = 0; i < value; i++)
                {
                    hands.Add(0);
                    noremovedhands.Add(0);
                }
                handcount = value;
            }
            get => handcount;
        }

        public int StartupNumber
        {
            init
            {
                for(int i = 0; i < hands.Count; i++)
                {
                    hands[i] = value;
                    noremovedhands[i] = value;
                }
            }
        }

        public Player() { } 

        public void Bump(Player srcPlayer, int srchandindex, int dsthandindex )
        {
            if (dsthandindex >= hands.Count || dsthandindex < 0)
            {
                throw new IndexOutOfRangeException("hand数量没有这么多哦~");
            }
            else if(srchandindex >= srcPlayer.hands.Count || srchandindex < 0)
            {
                throw new IndexOutOfRangeException("对方hand数量没有这么多哦~");
            }

            hands[dsthandindex] = (hands[dsthandindex] + srcPlayer.hands[srchandindex]) % target;
            noremovedhands[dsthandindex] = hands[dsthandindex] + srcPlayer.hands[srchandindex];
            stepcount++;

            if (hands[dsthandindex] == 0)
            {
                hands.RemoveAt(dsthandindex);
            }
        }

        public int GetSum()
        {
            return noremovedhands.Sum();
        }
    }
}
