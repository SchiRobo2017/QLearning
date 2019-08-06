using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Q_Leaning_CUI
{
    class PublicMembers
    {
        public static Random rnd = new Random(0);

        //迷路サイズ
        public const int SIZE_ROW = 10;
        public const int SIZE_COL = 10;

        //行動
        public const int UP = 0;
        public const int DOWN = 1;
        public const int LEFT = 2;
        public const int RIGHT = 3;

        //パラメータ
        public const int MAX_CYCLE = 10000;
        public const double ALPHA = 0.1;
        public const double GAMMA = 0.9;
        public const double EPS = 0.3;

        //Qテーブル
        public static State[,] Qtable = new State[SIZE_ROW, SIZE_COL];

        public static void PrintQtable()
        {
            for (int i = 0; i < PublicMembers.SIZE_ROW; i++)
            {
                for (int j = 0; j < PublicMembers.SIZE_COL; j++)
                {
                    if (i==3 & j==1) //start
                    {
                        Console.WriteLine(" START　");
                    }

                    if (PublicMembers.Qtable[i,j].isGoal) //goal
                    {
                        Console.Write(" GOAL ");
                        Console.Write(PublicMembers.Qtable[i,j].reward);
                        Console.Write(" ");
                    }
                    else
                    {
                        //Q値持つ
                        for (int k = 0; k < PublicMembers.Qtable[i, j].actSet.Count; k++)
                        {
                            Console.Write("{000:f1}", Qtable[i, j].actSet[k].qValue);
                            Console.Write(" ");
                        }

                        //maxQとその向き
                    }
                    Console.Write(" | ");
                }
                Console.WriteLine();
                for (int j = 0; j < 200; j++)
                {
                    Console.Write("-");
                }
                Console.WriteLine();
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            State s_0;      //初期状態
            State s;        //現状態
            State s_next;   //次状態

            //Qテーブルの初期化
            for (int i = 0; i < PublicMembers.SIZE_ROW; i++)
            {
                for (int j = 0; j < PublicMembers.SIZE_COL; j++)
                {
                    PublicMembers.Qtable[i, j] = new State(i, j, 0, false, false); //i行j列の状態s
                }
            }

            //ゴールと報酬の設定
            //PublicMembers.Qtable[0, 0] = new State(0, 0, -10, false, true);
            //PublicMembers.Qtable[0, 4] = new State(0, 4, 10, false, true);

            PublicMembers.Qtable[1, 6] = new State(0, 0, 10, false, true);
            PublicMembers.Qtable[4, 4] = new State(0, 0, -10, false, true);
            PublicMembers.Qtable[8, 3] = new State(0, 0, 15, false, true);
            PublicMembers.Qtable[7, 8] = new State(0, 0, 100, false, true);

            //初期状態の設定
            s_0 = PublicMembers.Qtable[3, 1];

            for (int i = 0; i < PublicMembers.MAX_CYCLE; i++)
            {
                s = s_0;　//現在の状態にスタート地点を設定

                while (!s.isGoal) //現状態sがゴールでない間繰り返す
                {
                    int a = s.ActionSelect();   //状態sにおける次の行動aを選択する
                    s_next = s.act(a);          //状態sから行動aによって遷移する状態s_next
                    s.updateQ(s_next, a);       //行動aの実施によって状態sのq値が更新される
                    s = s_next;                 //現在の状態をs_nextに設定する
                }
            }

            PublicMembers.PrintQtable();
        }
    }

    class Act //行動・Q値のセット
    {
        public int act;
        public double qValue;

        public Act(int act)
        {
            this.act = act;
            qValue = 0.0;
        }
    }

    class State
    {
        //迷路上での座標
        public int row;
        public int col;

        //選択可能な行動の集合
        public List<Act> actSet = new List<Act>();

        public double reward;

        public bool isWall;
        public bool isGoal;

        public State(int ROW, int COL, double reward,bool isWall, bool isGoal)
        {
            //座標
            row = ROW;
            col = COL;

            this.reward = reward;

            this.isWall = isWall;
            this.isGoal = isGoal;

            //行動を初期化して行動集合に追加
            actSet.Add(new Act(PublicMembers.UP));
            actSet.Add(new Act(PublicMembers.DOWN));
            actSet.Add(new Act(PublicMembers.LEFT));
            actSet.Add(new Act(PublicMembers.RIGHT));

            //隣接セル情報のチェック
            CheckNeighbors();

            //壁かゴールに設定されたら選択可能な行動無しにする
            if (isWall | isGoal)
            {
                actSet.Clear();
            }
            
            //壁に設定されたら周りのセルの情報を更新
            if (isWall==true)
            {
                //あとで
            }
        }

        //行けないセルを調べる
        private void CheckNeighbors()
        {
            if (row == 0) //UPいけない
            {
                actSet.Remove(actSet.Find(a => a.act == PublicMembers.UP));
            }
            if (row == PublicMembers.SIZE_ROW - 1) //DOWN行けない
            {
                actSet.Remove(actSet.Find(a => a.act == PublicMembers.DOWN));
            }
            if (col == 0) //LEFT行けない
            {
                actSet.Remove(actSet.Find(a => a.act == PublicMembers.LEFT));
            }
            if (col == PublicMembers.SIZE_COL - 1) //RIGHT行けない
            {
                actSet.Remove(actSet.Find(a => a.act == PublicMembers.RIGHT));
            }
        }

        public void updateQ(State s_next, int action)
        {
            Act a = actSet.Find(x => x.act == action);
            a.qValue = a.qValue + PublicMembers.ALPHA * (s_next.reward + PublicMembers.GAMMA * s_next.maxQ() - a.qValue);

        }

        //actionによる遷移先の状態s_nextを返す
        public State act(int action)
        {
            switch (action)
            {
                case PublicMembers.UP: //上に移動
                    return PublicMembers.Qtable[row-1, col];
                case PublicMembers.DOWN: //下に移動
                    return PublicMembers.Qtable[row+1, col];
                case PublicMembers.LEFT: //左に移動
                    return PublicMembers.Qtable[row, col-1];
                case PublicMembers.RIGHT: //右に移動
                    return PublicMembers.Qtable[row, col+1];
                default:
                    Console.WriteLine("値がおかしいよ");
                    return PublicMembers.Qtable[row,col];
            }
        }

        public double maxQ() //この状態の持つq値の最大値を返す
        {
            if (isGoal)
            {
                return 0.0;
            }
            else
            {
                return actSet.Max(a => a.qValue);
            }
        }

        public int ActionSelect()
        {
            //テスト　全部右にいく
            //return PublicMembers.RIGHT;

            if (PublicMembers.rnd.NextDouble() <= PublicMembers.EPS)
            {
                //グリーディ
                return GreedySelection();
            }
            else
            {
                //それ以外ならランダム
                int r = PublicMembers.rnd.Next(actSet.Count);
                return actSet[r].act;
            }
        }

        public int GreedySelection()
        {
            double max = actSet.Max(a => a.qValue);
            List<Act> tmp = actSet.FindAll(a => a.qValue == max); //Q値最大の行動全体

            int r = PublicMembers.rnd.Next(tmp.Count);
            return tmp[r].act;
        }
    }
}