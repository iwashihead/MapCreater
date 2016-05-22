using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace WindowsFormsApplication1
{
    /// <summary>
    /// ダンジョンクラス
    /// </summary>
    class cDungeon
    {
        // 定数
        #region
        /// <summary>道のマップ値</summary>
        public const int ROAD = 0;
        /// <summary>壁のマップ値</summary>
        public const int WALL = 1;
        /// <summary>水路のマップ値</summary>
        public const int WATER = 2;
        /// <summary>障害物1のマップ値</summary>
        public const int OBSTACLE1 = 3;
        /// <summary>障害物2のマップ値</summary>
        public const int OBSTACLE2 = 4;
        #endregion
        /// <summary>初期シード</summary>
        public int seed;
        /// <summary>ダンジョン配列</summary>
        public int[,] cell;
        /// <summary>ダンジョンの横幅</summary>
        public int Width {
            get {
                if (cell == null) return 0;
                return cell.GetLength(0);
            }
        }
        /// <summary>ダンジョンの縦幅</summary>
        public int Height {
            get {
                if (cell == null) return 0;
                return cell.GetLength(1);
            }
        }
        /// <summary>始点</summary>
        public Point start;
        /// <summary>終点</summary>
        public Point end;
        /// <summary>イベント</summary>
        public List<KeyValuePair<Point, cEvent>> events;
        /// <summary>ランダム</summary>
        public Random rand;

        private int eventcnt;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="seed">初期シード</param>
        /// <param name="w">横幅</param>
        /// <param name="h">縦幅</param>
        public cDungeon(int seed, int w, int h)
        {
#if DEBUG
            if (w <= 0 || h <= 0) return;
#endif
            cell = new int[w, h];
            events = new List<KeyValuePair<Point, cEvent>>();
            this.seed = seed;
            rand = new Random(seed);
        }

        /// <summary>
        /// ダンジョンランダム生成
        /// </summary>
        public void generateRandom(int type)
        {
            eventcnt = 0;
            int i, j, k, l;
            // 初期化
            for (i = 0; i < Width; i++)
            {
                for (j = 0; j < Height; j++)
                {
                    cell[i, j] = WALL;
                }
            }

            // 部屋生成
            k = Width / 7;
            l = Height / 7;
            List<Point> starts = new List<Point>();

            while (starts.Count <= 0)
            {
                starts.Clear();
                for (i = 0; i < k; i++)
                {
                    for (j = 0; j < l; j++)
                    {
                        if (rand.Next(3) == 0) continue;

                        int roomtype = rand.Next(100);
                        Rectangle room = new Rectangle(
                            7 * i + rand.Next(2) * 2 + 1,
                            7 * j + rand.Next(2) * 2 + 1,
                            3,
                            3);
                        Console.WriteLine(room.ToString());
                        for (int x = room.X; x < room.X + room.Width + 1; x++)
                        {
                            for (int y = room.Y; y < room.Y + room.Height + 1; y++)
                            {
                                if (roomtype < 1)
                                {
                                    if (x > room.X && x < room.X + room.Width && y > room.Y && y < room.Y + room.Height)
                                    {
                                        cell[x, y] = ROAD;
                                    }
                                    else
                                    {
                                        cell[x, y] = WATER;
                                    }
                                }
                                else if (roomtype < 20)
                                {
                                    cell[x, y] = WATER;
                                }
                                else
                                {
                                    cell[x, y] = ROAD;
                                }
                            }
                        }

                        // 部屋の入口を追加
                        int ix, iy;
                        do
                        {
                            ix = rand.Next(room.X, room.X + room.Width);
                            iy = rand.Next(room.Y, room.Y + room.Height);
                        }
                        while (ix % 2 == 0 || iy % 2 == 0);

                        if (iy - 2 <= 0) continue;
                        if (iy + 2 >= cell.GetLongLength(1)) continue;
                        if (ix - 2 <= 0) continue;
                        if (ix + 2 >= cell.GetLongLength(0)) continue;

                        if ((cell[ix, iy - 2] != WALL) && (cell[ix, iy + 2] != WALL)
                            && (cell[ix - 2, iy] != WALL) && (cell[ix + 2, iy] != WALL))
                        {
                            continue;
                        }

                        starts.Add(new Point(ix, iy));
                        Console.WriteLine("start:({0},{1})", ix, iy);
                    }
                }
            }

            recarcive(ref cell, starts[0].X, starts[0].Y);
            cell[starts[0].X, starts[0].Y] = 10;
        }

        /// <summary>
        /// 迷路生成
        /// </summary>
        public void generateMaze(Rectangle rect){
            int[,] maze;
            maze = new int[rect.Width, rect.Height];

            // 初期化
            for (int w = 0; w < rect.Width; w++)
            {
                for (int h = 0; h < rect.Height; h++)
                {
                    maze[w, h] = WALL;
                }
            }

            // 起点を設定する
            int startX, startY;
            do
            {// ランダム奇数
                startX = rand.Next(rect.Width);
            }
            while (startX % 2 == 0);
            do
            {// ランダム奇数
                startY = rand.Next(rect.Height);
            }
            while (startY % 2 == 0);

            // 再帰法で迷路を生成していく
            recarcive(ref maze, startX, startY);

            // ダンジョンマップ上書き
            for (int w = 0; w < Width; w++)
            {
                if (w < rect.X || w >= rect.X + rect.Width)
                    continue;
                for (int h = 0; h < Height; h++)
                {
                    if (h < rect.Y || h >= rect.Y + rect.Height)
                        continue;

                    cell[w, h] = maze[w - rect.X, h - rect.Y];
                }
            }
        }
        /// <summary>
        /// 迷路再帰呼び出し
        /// </summary>
        /// <param name="map">編集中の二次元配列の参照</param>
        /// <param name="x">起点X座標</param>
        /// <param name="y">起点Y座標</param>
        private void recarcive(ref int[,] map, int x, int y)
        {
            int[] dir = randomDir();

            // 順番にその方向に進む
            for (int i = 0; i < dir.Length; i++)
            {
                switch (dir[i])
                {
                    case 0:// 上
                        if (y - 2 <= 0) continue;
                        if (map[x, y - 2] == WALL)
                        {
                            map[x, y - 1] = ROAD;
                            map[x, y - 2] = ROAD;
                            recarcive(ref map, x, y - 2);
                        }
                        break;
                    case 1:// 下
                        if (y + 2 >= map.GetLongLength(1)) continue;
                        if (map[x, y + 2] == WALL)
                        {
                            map[x, y + 1] = ROAD;
                            map[x, y + 2] = ROAD;
                            recarcive(ref map, x, y + 2);
                        }
                        break;
                    case 2:// 左
                        if (x - 2 <= 0) continue;
                        if (map[x - 2, y] == WALL)
                        {
                            map[x - 1, y] = ROAD;
                            map[x - 2, y] = ROAD;
                            recarcive(ref map, x - 2, y);
                        }
                        break;
                    case 3:// 右
                        if (x + 2 >= map.GetLongLength(0)) continue;
                        if (map[x + 2, y] == WALL)
                        {
                            map[x + 1, y] = ROAD;
                            map[x + 2, y] = ROAD;
                            recarcive(ref map, x + 2, y);
                        }
                        break;
                }
            }

            // 行き止まりかどうか調べる
            if ((x - 1 < 0)
                || (x + 1 >= map.GetLongLength(0))
                || (y - 1 < 0)
                || (y + 1 >= map.GetLongLength(1)))
                return;

            int cnt = 0;
            if (map[x - 1, y] == ROAD) cnt++;
            if (map[x + 1, y] == ROAD) cnt++;
            if (map[x, y + 1] == ROAD) cnt++;
            if (map[x, y - 1] == ROAD) cnt++;
            if (cnt == 1)
            {
                // 行き止まり
                map[x, y] = 11;

                // イベント登録( イベントはnullで、とりあえず位置情報を登録しておく )
                KeyValuePair<Point, cEvent> pv = 
                    new KeyValuePair<Point, cEvent>(
                        new Point(x, y),
                        new cEvent(rand.Next(1000)));
                events.Add(pv);
            }
            else if (cnt == 2)
            {
                // 通路
            }
            else
            {
                eventcnt++;
                // 分岐点
                if (eventcnt % 5 == 0)
                    map[x, y] = 10;
            }
        }

        /// <summary>ランダムに方向を生成する</summary>
        private int[] randomDir()
        {
            //シャッフルする配列
            int[] ary = new int[] { 0, 1, 2, 3 };

            //Fisher-Yatesアルゴリズムでシャッフルする
            int n = ary.Length;
            while (n > 1)
            {
                n--;
                int k = rand.Next(n + 1);
                int tmp = ary[k];
                ary[k] = ary[n];
                ary[n] = tmp;
            }

            return ary;
        }
    }

    /// <summary>
    /// ダンジョンイベントクラス
    /// </summary>
    public class cEvent{
        /// <summary>イベント種別</summary>
        public enum EventType {
            /// <summary>起点</summary>
            Start,
            /// <summary>終点</summary>
            End,
            /// <summary>宝箱</summary>
            Treasure,
            /// <summary>トラップ</summary>
            Trap,
            /// <summary>戦闘</summary>
            Battle,
            /// <summary>採取ポイント</summary>
            Gather,
            /// <summary>会話</summary>
            Talk,
            /// <summary>露店</summary>
            Shop,
            /// <summary>休憩ポイント</summary>
            Rest,
        }
        
        /// <summary>イベント種別実体</summary>
        public EventType type;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public cEvent(int value)
        {
        }
    }
}
