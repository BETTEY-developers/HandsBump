using System;
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
        private int target = 0;
        private bool targetseted = false;
        private int stepcount = 0;
        private string name = string.Empty;

        public int Target
        {
            get => target;
            set
            {
                if (!targetseted)
                    target = value;
                targetseted = true;
            }
        }
        public int StepCount { get => stepcount; }
        public List<int> Hands { get => hands; }
        public string Name { get => name; set => name = value; }

        public Player(int handscount) 
        {
            for(int i = 0; i < handscount; i++)
            {
                hands.Add(0);
            }
        }

        public void SetStartNumber(int number)
        {
            for(int i = 0; i < hands.Count; i++)
            {
                hands[i] = number;
            }
        }

        public void Add(Player srcPlayer, int srchandindex, int dsthandindex )
        {
            if (dsthandindex >= hands.Count || dsthandindex < 0)
            {
                throw new IndexOutOfRangeException("hand数量没有这么多哦~");
            }
            else if(srchandindex >= srcPlayer.hands.Count || 0 < srchandindex)
            {
                throw new IndexOutOfRangeException("对方hand数量没有这么多哦~");
            }

            hands[dsthandindex] = (hands[dsthandindex] + srcPlayer.hands[srchandindex]) % target;
            stepcount++;

            if (hands[dsthandindex] == 0)
            {
                hands.RemoveAt(dsthandindex);
            }
        }
    }
}
