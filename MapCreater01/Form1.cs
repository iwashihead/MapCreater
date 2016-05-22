using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        #region 定数
        private const int MAX_WIDTH = 400;
        private const int MAX_HEIGHT = 400;

        // 色の対応表
        /// <summary>浅瀬、湖、川の色（ワールドマップ上）</summary>
        private static Color CELL_SEA = Color.DarkCyan;
        /// <summary>深い海の色（ワールドマップ上）</summary>
        private static Color CELL_DEEPSEA = Color.DarkCyan;
        /// <summary>砂漠、海岸の色（ワールドマップ上）</summary>
        private static Color CELL_SAND = Color.LightYellow;
        /// <summary>道の色（ワールドマップ上）</summary>
        private static Color CELL_ROAD = Color.SandyBrown;
        /// <summary>平原、草原の色</summary>
        private static Color CELL_GRASS = Color.Green;
        /// <summary>森の色（ワールドマップ上）</summary>
        private static Color CELL_FOREST = Color.DarkGreen;
        /// <summary>山岳の色（ワールドマップ上）</summary>
        private static Color CELL_MOUNTEN = Color.Brown;
        /// <summary>雪原の色（ワールドマップ上）</summary>
        private static Color CELL_SNOW = Color.White;
        /// <summary>雪山の色（ワールドマップ上）</summary>
        private static Color CELL_SNOWMOUNTEN = Color.LightGray;
        /// <summary>街の色（ワールドマップ上）</summary>
        private static Color CELL_TOWN = Color.Yellow;
        #endregion

        //◇ ワールドマップ生成に必要なパラメタ
        /// <summary>マップのセルの属性</summary>
        public enum MapAtr
        {
            /// <summary>深海</summary>
            DEEP_SEA = -5,
            /// <summary>海</summary>
            SEA = -1,
            /// <summary>平原、草原</summary>
            GRASS = 0,
            /// <summary>山</summary>
            MOUNTEN = 8,
            /// <summary>雪山</summary>
            SNOW_MOUNTEN = 12,



            /// <summary>街</summary>
            TOWN = 100,
            /// <summary>道</summary>
            ROAD,
            /// <summary>森</summary>
            FOREST,
            /// <summary>砂漠、海岸</summary>
            SAND,
            /// <summary>雪原</summary>
            SNOW,



            /// <summary>不明</summary>
            UNKNOWN = 255
        }
        
        //◇ 街マップ生成に必要なパラメタ
        /// <summary>都市区分</summary>
        public enum TownType
        {
            /// <summary>農村→街→大都市</summary>
            VILLAGE,
            /// <summary>港町→貿易都市</summary>
            MARIN,
            /// <summary>城→王都</summary>
            CASTLE,
            /// <summary>無法都市</summary>
            DARK,
            /// <summary>廃墟</summary>
            RUIN,
            /// <summary>雪の街</summary>
            SNOW,
            /// <summary>雪の城</summary>
            SNOW_CASTLE,

            MAX,
        }

        /// <summary>街の区域種別</summary>
        public enum AreaType
        {
            /// <summary>空き地</summary>
            NONE = 0,
            /// <summary>民家</summary>
            HOUSE,
            /// <summary>城</summary>
            CASTLE,
            /// <summary>店</summary>
            SHOP,
            /// <summary>闇市場</summary>
            BLACK_MARKET,
            /// <summary>魔法屋</summary>
            MAGIC,
            /// <summary>宿屋</summary>
            INN,
            /// <summary>酒場</summary>
            BAR,
            /// <summary>闘技場</summary>
            ARENA,
            /// <summary>港</summary>
            MARINE,
            /// <summary>牧場</summary>
            FARM,
            /// <summary>広場</summary>
            SQUARA,
            /// <summary>墓地</summary>
            GRAVE,
        }

        /// <summary>街クラス</summary>
        public class Town
        {
            /// <summary>タイプ</summary>
            public TownType type;
            /// <summary>ワールドマップ上の座標</summary>
            public Point worldPos;
            /// <summary>街の元々の地形情報</summary>
            public MapAtr srcMap;
            /// <summary>
            /// <para>区域 以下のような割り振り</para>
            /// <para>◎①②③</para>
            /// <para>④⑤⑥⑦</para>
            /// <para>⑧⑨⑩⑪</para>
            /// <para>⑫⑬⑭⑮</para>
            /// </summary>
            public AreaType[] area = new AreaType[16];
            public int[,] cell = new int[64, 64];

            public void Init()
            {
                for (int x = 0; x < cell.GetLength(0); x++)
                {
                    for (int y = 0; y < cell.GetLength(1); y++)
                    {
                        cell[x, y] = 0;
                    }
                }
                for (int i = 0; i < area.Length; i++)
                {
                    area[i] = AreaType.NONE;
                }
            }
        }
        public Town town = new Town();

        //◇ ダンジョンマップ生成に必要なパラメタ
        /// <summary>ダンジョン区分</summary>
        public enum DungeonType
        {
            /// <summary>ローグ（不思議のダンジョン風）</summary>
            ROGE,

            /// <summary>迷路（ほぼ一本道）</summary>
            MAZE,
            
            /// <summary>大部屋</summary>
            ONE_FLORE,

            /// <summary></summary>
            

            /// <summary></summary>
            MAX,
        }

        /// <summary>表示する画像データ</summary>
        private Bitmap bmp;
        /// <summary>セルデータ</summary>
        private int[,] cell = new int[MAX_WIDTH, MAX_HEIGHT];
        /// <summary>疑似乱数</summary>
        private Random rand = new Random();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Form1()
        {
            InitializeComponent();

            numericUpDownSeed.Value = rand.Next(int.MaxValue);
        }

        /// <summary>
        /// 生成ボタン押下
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            // 初期Seed設定
            rand = new Random((int)numericUpDownSeed.Value);
            if (checkBoxSeedFix.Checked == false)   // 固定するかどうか
                numericUpDownSeed.Value = rand.Next(int.MaxValue);


            if (radioButtonWorld.Checked){
                // ワールドマップ生成
                CreateWorld();
            }
            else if (radioButtonTown.Checked){
                // 街マップ生成
                CreateTown();
            }
            else if (radioButtonDungeon.Checked){
                // ダンジョンマップ生成
                CreateDungeon();
            }
            else{
                Console.WriteLine("生成タイプが選択されていません！");
            }
        }

        #region ワールドマップ関連
        /// <summary>
        /// ワールドマップ生成
        /// </summary>
        private void CreateWorld()
        {
            int v1 = (int)numericUpDown1.Value;    // １区画の大きさ
            int v2 = (int)numericUpDown2.Value;    // 最大影響力
            int v3 = (int)numericUpDown3.Value;    // マイナス率（陸地と海の領域に影響する）
            int v4 = (int)numericUpDown4.Value;    // 試行回数
            int v5 = (int)numericUpDown5.Value;    // 雪原境界(0~50%)
            int x, y, w, h, border;
            w = (int)(numericUpDownWidth.Value / v1);
            h = (int)(numericUpDownHeight.Value / v1);

            border = (int)((int)numericUpDownHeight.Value * v5 * 0.01f);    // 雪原境界線

            // 高さデータ
            int[,] hMap = new int[(int)numericUpDownWidth.Value, (int)numericUpDownHeight.Value];

            // 実生成部分------------------------------------
            for (int time = 0; time < v4; time++)
            {
                for (x = 0; x < w; x++)
                {
                    for (y = 0; y < h; y++)
                    {
                        // 影響点をランダム設定
                        int ix, iy;
                        ix = x * v1 + rand.Next(v1);
                        iy = y * v1 + rand.Next(v1);

                        // 影響力を設定
                        int ef;
                        ef = rand.Next(v2) - (int)(v2 * v3 * 0.01);
                        if (ef == 0)
                            continue;
                        if (time == 0)
                        {
                            if (ef > 0)
                            {
                                if (ef < (v2 - (int)(v2 * v3 * 0.01)) * 0.8f)
                                    continue;
                                ef = (int)(ef * 1.5f);
                                Console.WriteLine(string.Format("影響力は{0}です！", ef));
                            }
                            else
                            {
                                if (ef > -(int)(v2 * v3 * 0.01) * 0.5f)
                                    continue;
                            }
                        }

                        // 影響力データ（影響計算毎に高さデータに加算するテーブル）
                        int[,] eMap = new int[(int)numericUpDownWidth.Value, (int)numericUpDownHeight.Value];

                        // 影響力マップ作成
                        effectHeight(ref eMap, ix, iy, ef, (ef > 0 ? true : false));

                        // 高さマップに影響力マップを加算
                        addMap(ref hMap, ref eMap);
                    }
                }

                // 影響力を低下
                v3 = (int)(v3 * 0.5);

                Console.WriteLine(string.Format("{0}回目の演算終了", time));
            }
            //--------------------------------------------実生成終了

            for (int i = 0; i < 1; i++)
            {
                // 微修正（海に囲まれた箇所を取り除く）
                for (x = 0; x < numericUpDownWidth.Value; x++)
                {
                    // 雪原判定用の変数を決める
                    int b = (border <= 0) ? 0 : border + rand.Next(3);
                    int ub = (border <= 0) ? 0 : border + rand.Next(3);

                    for (y = 0; y < numericUpDownHeight.Value; y++)
                    {
                        // 陸に囲まれた海を変化
                        if (hMap[x, y] < 0)
                        {
                            int ix, iy;

                            // 上
                            ix = x;
                            iy = (y == 0) ? (int)numericUpDownHeight.Value - 1 : y - 1;
                            if (hMap[ix, iy] < 0)
                                continue;

                            // 下
                            iy = (y == (int)numericUpDownHeight.Value - 1) ? 0 : y + 1;
                            if (hMap[ix, iy] < 0)
                                continue;

                            // 左
                            ix = (x == 0) ? (int)numericUpDownWidth.Value - 1 : x - 1;
                            iy = y;
                            if (hMap[ix, iy] < 0)
                                continue;

                            // 右
                            ix = (x == (int)numericUpDownWidth.Value - 1) ? 0 : x + 1;
                            if (hMap[ix, iy] < 0)
                                continue;

                            setMapAtr(MapAtr.GRASS, ref hMap[x, y]);

                            // 雪原修正
                            if (y >= b && y <= (int)numericUpDownHeight.Value - ub)
                                continue;

                            MapAtr atr = getMapAtr(hMap[x, y]);
                            if (atr == MapAtr.GRASS)
                                setMapAtr(MapAtr.SNOW, ref hMap[x, y]);
                            else if (atr == MapAtr.MOUNTEN)
                                setMapAtr(MapAtr.SNOW_MOUNTEN, ref hMap[x, y]);
                        }
                        else
                        {
                            #region 海に囲まれた陸を変化
                            //    // 海に囲まれた陸を変化
                            //    int ix, iy;

                            //    // 上
                            //    ix = x;
                            //    iy = (y == 0) ? (int)numericUpDownHeight.Value - 1 : y - 1;
                            //    if (hMap[ix, iy] >= 0)
                            //        continue;

                            //    // 下
                            //    iy = (y == (int)numericUpDownHeight.Value - 1) ? 0 : y + 1;
                            //    if (hMap[ix, iy] >= 0)
                            //        continue;

                            //    // 左
                            //    ix = (x == 0) ? (int)numericUpDownWidth.Value - 1 : x - 1;
                            //    iy = y;
                            //    if (hMap[ix, iy] >= 0)
                            //        continue;

                            //    // 右
                            //    ix = (x == (int)numericUpDownWidth.Value - 1) ? 0 : x + 1;
                            //    if (hMap[ix, iy] >= 0)
                            //        continue;

                            //    hMap[x, y] = -1;
                            //    Console.WriteLine("B");
                            #endregion

                            #region 平地に囲まれた座標を探索
                            //int ix, iy;
                            //// 上
                            //ix = x;
                            //iy = (y == 0) ? (int)numericUpDownHeight.Value - 1 : y - 1;
                            //if (getMapAtr(hMap[ix, iy]) == MapAtr.GRASS)
                            //{// 下
                            //    iy = (y == (int)numericUpDownHeight.Value - 1) ? 0 : y + 1;
                            //    if (getMapAtr(hMap[ix, iy]) == MapAtr.GRASS)
                            //    {// 左
                            //        ix = (x == 0) ? (int)numericUpDownWidth.Value - 1 : x - 1;
                            //        iy = y;
                            //        if (getMapAtr(hMap[ix, iy]) == MapAtr.GRASS)
                            //        {// 右
                            //            ix = (x == (int)numericUpDownWidth.Value - 1) ? 0 : x + 1;
                            //            if (getMapAtr(hMap[ix, iy]) == MapAtr.GRASS)
                            //            {
                            //                setMapAtr(MapAtr.ROAD, ref hMap[ix, iy]);
                            //            }
                            //        }
                            //    }
                            //}
                            #endregion

                            // 雪原修正
                            if (y >= b && y <= (int)numericUpDownHeight.Value - ub)
                                continue;

                            MapAtr atr = getMapAtr(hMap[x, y]);
                            if (atr == MapAtr.GRASS)
                                setMapAtr(MapAtr.SNOW, ref hMap[x, y]);
                            else if (atr == MapAtr.MOUNTEN)
                                setMapAtr(MapAtr.SNOW_MOUNTEN, ref hMap[x, y]);
                        }
                    }
                }
            }

            #region 街の作成
            {
                int w2, h2, ix, iy;
                w2 = (int)(w * 0.25f);
                h2 = (int)(h * 0.25f);

                List<Point> points = new List<Point>();

            TownMake:
                points.Clear();

                // 設置する街を探す
                for (x = 0; x < w2; x++)
                {
                    for (y = 0; y < h2; y++)
                    {
                        ix = x * v1 * 4 + rand.Next(v1 * 4);
                        iy = y * v1 * 4 + rand.Next(v1 * 4);
                        MapAtr atr = getMapAtr(hMap[ix,iy]);
                        if (atr != MapAtr.DEEP_SEA
                            && atr != MapAtr.SEA)
                        {
                            // 街が隣接しないようにする
                            atr = getMapAtr(hMap[ix, (iy == 0) ? (int)numericUpDownHeight.Value - 1 : iy - 1]);
                            if (atr == MapAtr.TOWN) continue;
                            atr = getMapAtr(hMap[ix, (iy == (int)numericUpDownHeight.Value - 1) ? 0 : iy + 1]);
                            if (atr == MapAtr.TOWN) continue;
                            atr = getMapAtr(hMap[(ix == 0) ? (int)numericUpDownWidth.Value - 1 : ix - 1, iy]);
                            if (atr == MapAtr.TOWN) continue;
                            atr = getMapAtr(hMap[(ix == (int)numericUpDownWidth.Value - 1) ? 0 : ix + 1, iy]);
                            if (atr == MapAtr.TOWN) continue;

                            // 座標を追加
                            points.Add(new Point(ix, iy));
                        }
                    }
                }

                // 街が２つ以下の場合作りなおし
                if (points.Count < 3)
                    goto TownMake;

                // 街設置
                for (int i = 0; i < points.Count; i++)
                {
                    setMapAtr(MapAtr.TOWN, ref hMap[points[i].X, points[i].Y]);
                }

                // 道の作成
                if (checkBox1.Checked)
                {
                    for (int i = 1; i < points.Count; i++)
                    {
                        Point p = getTownPos(null, ref hMap, points[i - 1].X, points[i - 1].Y);
                        CreateRoad(points[i - 1].X, points[i - 1].Y,    // 元の街
                                   p.X, p.Y,                            // 狙い街
                                   0,                                   // 向き
                                   ref hMap);
                    }
                }
            }
            #endregion

            // ビットマップの生成
            bmp = new Bitmap((int)numericUpDownWidth.Value * 4, (int)numericUpDownHeight.Value * 4);


            for (int vx = 0; vx < (int)numericUpDownWidth.Value; vx++)
            {
                //Console.Write("\n");
                for (int vy = 0; vy < (int)numericUpDownHeight.Value; vy++)
                {
                    // ログ出力
                    //Console.Write(string.Format("[{0}],", hMap[vx, vy]));

                    // 色データを入れる
                    int i = hMap[vx, vy];
                    MapAtr atr = getMapAtr(i);

                    if (atr == MapAtr.DEEP_SEA)
                    {
                        //                        bmp.SetPixel(vx, vy, CELL_DEEPSEA);
                        DrawPixel(bmp, vx * 4, vy * 4, CELL_DEEPSEA);
                    }
                    else if (atr == MapAtr.SEA)
                    {
                        //                        bmp.SetPixel(vx, vy, CELL_SEA);
                        DrawPixel(bmp, vx * 4, vy * 4, CELL_SEA);
                    }
                    else if (atr == MapAtr.GRASS)
                    {
                        //                        bmp.SetPixel(vx, vy, CELL_GRASS);
                        DrawPixel(bmp, vx * 4, vy * 4, CELL_GRASS);
                    }
                    else if (atr == MapAtr.MOUNTEN)
                    {
                        //                        bmp.SetPixel(vx, vy, CELL_MOUNTEN);
                        DrawPixel(bmp, vx * 4, vy * 4, CELL_MOUNTEN);
                    }
                    else if (atr == MapAtr.SNOW_MOUNTEN)
                    {
                        //                        bmp.SetPixel(vx, vy, CELL_SNOWMOUNTEN);
                        DrawPixel(bmp, vx * 4, vy * 4, CELL_SNOWMOUNTEN);
                    }
                    else if (atr == MapAtr.SNOW)
                    {
                        DrawPixel(bmp, vx * 4, vy * 4, CELL_SNOW);
                    }
                    else if (atr == MapAtr.TOWN)
                    {
                        DrawPixel(bmp, vx * 4, vy * 4, CELL_TOWN);
                    }
                    else if (atr == MapAtr.ROAD)
                    {
                        DrawPixel(bmp, vx * 4, vy * 4, CELL_ROAD);
                    }
                    else
                    {
                        // 不明
                        DrawPixel(bmp, vx * 4, vy * 4, Color.Black);
                    }
                }
            }

            // 画像表示
            pictureBox1.Image = bmp;
        }

        /// <summary>
        /// 道を伸ばす
        /// </summary>
        /// <param name="x">現在地点X</param>
        /// <param name="y">現在地点Y</param>
        /// <param name="aimX">目的地X</param>
        /// <param name="aimY">目的地Y</param>
        /// <param name="dis">現在の方角（0:上 1:右 2:下 3:左）</param>
        /// <param name="hMap"></param>
        void CreateRoad(int x, int y, int aimX, int aimY, int dis, ref int[,] hMap)
        {
            MapAtr atr;
            int vx, vy;
            vx = aimX - x;
            vy = aimY - y;
            atr = getMapAtr(hMap[x, y]);
            List<byte> list = new List<byte>();

            // 現在地点が街かどうか判定
            if (atr != MapAtr.TOWN){
                // 道を設置する
                setMapAtr(MapAtr.ROAD, ref hMap[x, y]);
            }

            // 分岐、道停止の判定
            int r = rand.Next(100);
            if (r < 6) return;      // 道停止
            else if (r < 9)
            {
                list.Add(0); list.Add(1);
                list.Add(2); list.Add(3);

                while (list.Count > 0)
                {
                    int i = rand.Next(list.Count);
                    if (chkSetRoad(x, y, list[i], ref hMap))
                    {
                        // 進める
                        dis = list[i];
                        switch (dis)
                        {
                            case 0: y = (y - 1 < 0) ? y - 1 + (int)numericUpDownHeight.Value : y - 1; break;
                            case 1: x = (x + 1) % (int)numericUpDownWidth.Value; break;
                            case 2: y = (y + 1) % (int)numericUpDownHeight.Value; break;
                            case 3: x = (x - 1 < 0) ? x - 1 + (int)numericUpDownWidth.Value : x - 1; break;
                        }
                        List<Point> exclude = new  List<Point>();
                        exclude.Add(new Point(aimX, aimY));
                        Point town = getTownPos(exclude, ref hMap, x, y);
                        CreateRoad(x, y, town.X, town.Y, dis, ref hMap);
                    }
                    else
                    {
                        // 進めなかった
                        list.RemoveAt(i);
                    }
                }
                list.Clear();
            }

            // 方向を数値データとして格納しておく
            list.Add(0); list.Add(1);
            list.Add(2); list.Add(3);

            int t = rand.Next(2);
            if (t == 0)
            {
                if (vx > 0) dis = 1;
                else if (vx < 0) dis = 3;
                if (chkSetRoad(x, y, dis, ref hMap))
                {
                    // 進める
                    switch (dis)
                    {
                        case 0: y = (y - 1 < 0) ? y - 1 + (int)numericUpDownHeight.Value : y - 1; break;
                        case 1: x = (x + 1) % (int)numericUpDownWidth.Value; break;
                        case 2: y = (y + 1) % (int)numericUpDownHeight.Value; break;
                        case 3: x = (x - 1 < 0) ? x - 1 + (int)numericUpDownWidth.Value : x - 1; break;
                    }
                    CreateRoad(x, y, aimX, aimY, dis, ref hMap);
                    return;
                }
                else
                {
                    list.Remove((byte)dis);

                    if (vy > 0) dis = 2;
                    else if (vy < 0) dis = 0;
                    if (chkSetRoad(x, y, dis, ref hMap))
                    {
                        // 進める
                        switch (dis)
                        {
                            case 0: y = (y - 1 < 0) ? y - 1 + (int)numericUpDownHeight.Value : y - 1; break;
                            case 1: x = (x + 1) % (int)numericUpDownWidth.Value; break;
                            case 2: y = (y + 1) % (int)numericUpDownHeight.Value; break;
                            case 3: x = (x - 1 < 0) ? x - 1 + (int)numericUpDownWidth.Value : x - 1; break;
                        }
                        CreateRoad(x, y, aimX, aimY, dis, ref hMap);
                        return;
                    }
                    else
                    {
                        list.Remove((byte)dis);
                    }
                }
            }
            else
            {
                if (vy > 0) dis = 2;
                else if (vy < 0) dis = 0;
                if (chkSetRoad(x, y, dis, ref hMap))
                {
                    // 進める
                    switch (dis)
                    {
                        case 0: y = (y - 1 < 0) ? y - 1 + (int)numericUpDownHeight.Value : y - 1; break;
                        case 1: x = (x + 1) % (int)numericUpDownWidth.Value; break;
                        case 2: y = (y + 1) % (int)numericUpDownHeight.Value; break;
                        case 3: x = (x - 1 < 0) ? x - 1 + (int)numericUpDownWidth.Value : x - 1; break;
                    }
                    CreateRoad(x, y, aimX, aimY, dis, ref hMap);
                    return;
                }
                else
                {
                    list.Remove((byte)dis);

                    if (vx > 0) dis = 1;
                    else if (vx < 0) dis = 3;
                    if (chkSetRoad(x, y, dis, ref hMap))
                    {
                        // 進める
                        switch (dis)
                        {
                            case 0: y = (y - 1 < 0) ? y - 1 + (int)numericUpDownHeight.Value : y - 1; break;
                            case 1: x = (x + 1) % (int)numericUpDownWidth.Value; break;
                            case 2: y = (y + 1) % (int)numericUpDownHeight.Value; break;
                            case 3: x = (x - 1 < 0) ? x - 1 + (int)numericUpDownWidth.Value : x - 1; break;
                        }
                        CreateRoad(x, y, aimX, aimY, dis, ref hMap);
                        return;
                    }
                    else
                    {
                        list.Remove((byte)dis);
                    }
                }
            }

            while (list.Count > 0)
            {
                int i = rand.Next(list.Count);
                if (chkSetRoad(x, y, list[i], ref hMap))
                {
                    // 進める
                    dis = list[i];
                    switch (dis)
                    {
                        case 0: y = (y - 1 < 0) ? y - 1 + (int)numericUpDownHeight.Value : y - 1; break;
                        case 1: x = (x + 1) % (int)numericUpDownWidth.Value; break;
                        case 2: y = (y + 1) % (int)numericUpDownHeight.Value; break;
                        case 3: x = (x - 1 < 0) ? x - 1 + (int)numericUpDownWidth.Value : x - 1; break;
                    }
                    CreateRoad(x, y, aimX, aimY, dis, ref hMap);
                    return;
                }
                else
                {
                    // 進めなかった
                    list.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// 最寄りの街の位置を返す
        /// </summary>
        /// <param name="exclude">除外する位置情報</param>
        /// <param name="hMap">マップ</param>
        /// <returns></returns>
        Point getTownPos(List<Point> exclude, ref int[,] hMap, int startX, int startY)
        {
            List<Point> towns = new List<Point>();

            for (int x = 0; x < (int)numericUpDownWidth.Value; x++)
            {
                for (int y = 0; y < (int)numericUpDownHeight.Value; y++)
                {
                    MapAtr atr = getMapAtr(hMap[x, y]);
                    if (atr == MapAtr.TOWN)
                    {
                        if (startX == x && startY == y)
                            continue;

                        if (exclude != null)
                        {
                            for (int i = 0; i < exclude.Count; i++)
                            {
                                if (exclude[i] == new Point(x, y))
                                    continue;
                            }
                        }

                        towns.Add(new Point(x, y));
                    }
                }
            }

            // すべての街の中で一番近い街を見つける
            float least = float.MaxValue;
            int idx = -1;
            for (int i = 0; i < towns.Count; i++)
            {
                float dis;
                int vx, vy;
                vx = towns[i].X - startX;
                vy = towns[i].Y - startY;

                dis = (float)Math.Sqrt(vx * vx + vy * vy);
                if (least > dis) {
                    least = dis;
                    idx = i;
                }
            }

            if (idx >= 0)
                return towns[idx];

            return new Point(int.MaxValue, int.MaxValue);
        }

        void getTownPos(int startX, int startY, Point exclude, ref int aimX, ref int aimY, ref int[,]hMap)
        {
            MapAtr atr = getMapAtr(hMap[startX, startY]);
            if (atr == MapAtr.TOWN && exclude != new Point(startX, startY))
            {
                aimX = startX;
                aimY = startY;
            }
        }

        /// <summary>
        /// 道がひけるかチェックする
        /// </summary>
        /// <param name="x">開始点X</param>
        /// <param name="y">開始点Y</param>
        /// <param name="dis">方向</param>
        /// <param name="hMap">マップ</param>
        /// <returns></returns>
        bool chkSetRoad(int x, int y, int dis, ref int[,] hMap)
        {
            MapAtr atr;
            switch (dis)
            {
                case 0:// 上
                    atr = getMapAtr(hMap[x, (y - 1 < 0) ? (int)numericUpDownHeight.Value + y - 1 : y - 1]);
                    if (atr == MapAtr.GRASS || atr == MapAtr.MOUNTEN)
                    {
                        atr = getMapAtr(hMap[(x + 1) % (int)numericUpDownWidth.Value, (y - 1 < 0) ? (int)numericUpDownHeight.Value + y - 1 : y - 1]);
                        if (atr == MapAtr.ROAD) return false;
                        atr = getMapAtr(hMap[(x - 1 < 0) ? (int)numericUpDownWidth.Value + x - 1 : x - 1, (y - 1 < 0) ? (int)numericUpDownHeight.Value + y - 1 : y - 1]);
                        if (atr == MapAtr.ROAD) return false;
                        atr = getMapAtr(hMap[x, (y - 2 < 0) ? (int)numericUpDownHeight.Value + y - 2 : y - 2]);
                        if (atr == MapAtr.GRASS || atr == MapAtr.MOUNTEN)
                        {
                            return true;
                        }
                    }
                    break;
                case 1:
                    atr = getMapAtr(hMap[(x + 1) % (int)numericUpDownWidth.Value, y]);
                    if (atr == MapAtr.GRASS || atr == MapAtr.MOUNTEN)
                    {
                        atr = getMapAtr(hMap[(x + 1) % (int)numericUpDownWidth.Value, (y - 1 < 0) ? (int)numericUpDownHeight.Value + y - 1 : y - 1]);
                        if (atr == MapAtr.ROAD) return false;
                        atr = getMapAtr(hMap[(x + 1) % (int)numericUpDownWidth.Value, (y + 1) % (int)numericUpDownHeight.Value]);
                        if (atr == MapAtr.ROAD) return false;
                        atr = getMapAtr(hMap[(x + 2) % (int)numericUpDownWidth.Value, y]);
                        if (atr == MapAtr.GRASS || atr == MapAtr.MOUNTEN)
                        {
                            return true;
                        }
                    }
                    break;
                case 2:
                    atr = getMapAtr(hMap[x, (y + 1) % (int)numericUpDownHeight.Value]);
                    if (atr == MapAtr.GRASS || atr == MapAtr.MOUNTEN)
                    {
                        atr = getMapAtr(hMap[(x + 1) % (int)numericUpDownWidth.Value, (y + 1) % (int)numericUpDownHeight.Value]);
                        if (atr == MapAtr.ROAD) return false;
                        atr = getMapAtr(hMap[(x - 1 < 0) ? (int)numericUpDownWidth.Value + x - 1 : x - 1, (y + 1) % (int)numericUpDownHeight.Value]);
                        if (atr == MapAtr.ROAD) return false;
                        atr = getMapAtr(hMap[x, (y + 2) % (int)numericUpDownHeight.Value]);
                        if (atr == MapAtr.GRASS || atr == MapAtr.MOUNTEN)
                        {
                            return true;
                        }
                    }
                    break;
                case 3:
                    atr = getMapAtr(hMap[(x - 1 < 0) ? (int)numericUpDownWidth.Value + x - 1 : x - 1, y]);
                    if (atr == MapAtr.GRASS || atr == MapAtr.MOUNTEN)
                    {
                        atr = getMapAtr(hMap[(x - 1 < 0) ? (int)numericUpDownWidth.Value + x - 1 : x - 1, (y - 1 < 0) ? (int)numericUpDownHeight.Value + y - 1 : y - 1]);
                        if (atr == MapAtr.ROAD) return false;
                        atr = getMapAtr(hMap[(x - 1 < 0) ? (int)numericUpDownWidth.Value + x - 1 : x - 1, (y + 1) % (int)numericUpDownHeight.Value]);
                        if (atr == MapAtr.ROAD) return false;
                        atr = getMapAtr(hMap[(x - 2 < 0) ? (int)numericUpDownWidth.Value + x - 2 : x - 2, y]);
                        if (atr == MapAtr.GRASS || atr == MapAtr.MOUNTEN)
                        {
                            return true;
                        }
                    }
                    break;
            }

            return false;
        }

        /// <summary>
        /// 高低差を元にマップの基本属性を返す
        /// </summary>
        /// <returns></returns>
        MapAtr getMapAtr(int i)
        {
            if (i <= -5)
            {
                return MapAtr.DEEP_SEA;
            }
            else if (i < 0)
            {
                return MapAtr.SEA;
            }
            else if (i < 8)
            {
                return MapAtr.GRASS;
            }
            else if (i < 12)
            {
                return MapAtr.MOUNTEN;
            }
            else if (i < 100)
            {
                return MapAtr.SNOW_MOUNTEN;
            }
            else
            {
                switch (i)
                {
                    case (int)MapAtr.TOWN: return MapAtr.TOWN;
                    case (int)MapAtr.ROAD: return MapAtr.ROAD;
                    case (int)MapAtr.FOREST: return MapAtr.FOREST;
                    case (int)MapAtr.SAND: return MapAtr.SAND;
                    case (int)MapAtr.SNOW: return MapAtr.SNOW;
                    case (int)MapAtr.UNKNOWN: return MapAtr.UNKNOWN;
                    default: return MapAtr.UNKNOWN;
                }
            }
        }

        /// <summary>
        /// マップ属性をセットする
        /// </summary>
        /// <param name="atr"></param>
        /// <param name="i"></param>
        void setMapAtr(MapAtr atr, ref int i)
        {
            i = (int)atr;
        }

        /// <summary>
        /// 二次元配列を上書きする
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="col"></param>
        void DrawPixel(Bitmap bmp, int x, int y, Color col)
        {
            for (int ix = x; ix < x + 4; ix++)
            {
                for (int iy = y; iy < y + 4; iy++)
                {
                    bmp.SetPixel(ix, iy, col);
                }
            }
        }

        /// <summary>
        /// 影響力マップを作成する
        /// </summary>
        /// <param name="eMap">影響力マップ</param>
        /// <param name="startX">開始地点X</param>
        /// <param name="startY">開始地点Y</param>
        /// <param name="effect_value">影響力</param>
        void effectHeight(ref int[,] eMap, int startX, int startY, int effect_value, bool isAdd)
        {
            int i0, x, y;

            eMap[startX, startY] = effect_value;

            // 影響力の減り方
            if (isAdd)
                //                 i0 = effect_value - 1;
                i0 = (effect_value <= 2) ? 0 : (int)(effect_value * 0.75f);
            else
                //                 i0 = effect_value + 1;
                i0 = (effect_value >= -2) ? 0 : (int)(effect_value * 0.75f);

            // 終了
            if (i0 == 0)
                return;

            // 上方向
            if (startY - 1 < 0)
            {
                x = startX;
                y = (int)numericUpDownHeight.Value - 1;
            }
            else
            {
                x = startX;
                y = startY - 1;
            }
            effectHeight(ref eMap, x, y, i0, isAdd);

            // 下方向
            if (startY + 1 >= (int)numericUpDownHeight.Value)
            {
                x = startX;
                y = 0;
            }
            else
            {
                x = startX;
                y = startY + 1;
            }
            effectHeight(ref eMap, x, y, i0, isAdd);

            // 左方向
            if (startX - 1 < 0)
            {
                x = (int)numericUpDownWidth.Value - 1;
                y = startY;
            }
            else
            {
                x = startX - 1;
                y = startY;
            }
            effectHeight(ref eMap, x, y, i0, isAdd);

            // 右方向
            if (startX + 1 >= (int)numericUpDownWidth.Value)
            {
                x = 0;
                y = startY;
            }
            else
            {
                x = startX + 1;
                y = startY;
            }
            effectHeight(ref eMap, x, y, i0, isAdd);
        }

        /// <summary>
        /// ２次元配列の加算
        /// </summary>
        /// <param name="hMap"></param>
        /// <param name="eMap"></param>
        void addMap(ref int[,] hMap, ref int[,] eMap)
        {
            for (int x = 0; x < hMap.GetLength(0); x++)
            {
                if (x >= eMap.GetLength(0))
                    continue;
                for (int y = 0; y < hMap.GetLength(1); y++)
                {
                    if (y >= eMap.GetLength(1))
                        continue;

                    hMap[x, y] += eMap[x, y];
                }
            }
        }
        #endregion
        #region 街マップ関連
        /// <summary>
        /// 街マップ生成
        /// </summary>
        private void CreateTown()
        {
            // 初期化
            town.Init();

            // 街タイプの決定
            switch ((string)domainUpDown1.SelectedItem)
            {
                case "RANDOM": town.type = (TownType)rand.Next((int)TownType.MAX); break;
                case "VILLAGE": town.type = TownType.VILLAGE; break;
                case "MARIN": town.type = TownType.MARIN; break;
                case "CASTLE": town.type = TownType.CASTLE; break;
                case "DARK": town.type = TownType.DARK; break;
                case "RUIN": town.type = TownType.RUIN; break;
                case "SNOW": town.type = TownType.SNOW; break;
                case "SNOW_CASTLE": town.type = TownType.SNOW_CASTLE; break;
            }

            // 部屋の作成
            switch (town.type)
            {
                case TownType.CASTLE:
                startRondom:
                    int i0 = rand.Next(0, 12);
                    if (i0 % 4 == 3) { goto startRondom; }
                    createArea(town, AreaType.CASTLE, i0);
                    break;
            }

            // ビットマップの生成
            bmp = new Bitmap((int)numericUpDownWidth.Value * 4, (int)numericUpDownHeight.Value * 4);

            for (int vx = 0; vx < (int)numericUpDownWidth.Value; vx++)
            {
//                Console.Write("\n");
                for (int vy = 0; vy < (int)numericUpDownHeight.Value; vy++)
                {
                    // ログ出力
//                    Console.Write(string.Format("[{0}],", town.cell[vx, vy]));

                    // 色データを入れる
                    int i = town.cell[vx, vy];

                    if (i == 0)
                    {
                        DrawPixel(bmp, vx * 4, vy * 4, Color.LawnGreen);
                    }
                    else if (i == 10)
                    {
                        DrawPixel(bmp, vx * 4, vy * 4, Color.LightGray);
                    }
                    else if (i == 11)
                    {
                        DrawPixel(bmp, vx * 4, vy * 4, Color.Gray);
                    }
                }
            }

            // 画像表示
            pictureBox1.Image = bmp;
        }

        /// <summary>
        /// 街の区域を作成する
        /// </summary>
        /// <param name="area"></param>
        /// <param name="areaNo"></param>
        /// <returns></returns>
        void createArea(Town town, AreaType area, int areaNo)
        {
            if (area == AreaType.CASTLE)
            {
                if (areaNo % 4 == 3 || areaNo > 11) { MessageBox.Show("部屋ができない！"); return; }
                town.area[areaNo] =
                town.area[areaNo + 1] =
                town.area[areaNo + 4] =
                town.area[areaNo + 5] = area;

                // 部屋作成
                int x, y, w, h;
                w = rand.Next(20, 28);
                h = rand.Next(18, 28);
                x = 16 * (areaNo % 4) + 2 + rand.Next(2);
                y = 16 * (areaNo / 4) + 2;

                Rectangle rect = new Rectangle(x, y, w, h);
                DrawRect(ref town.cell, rect, 10, 11);
            }
        }

        /// <summary>
        /// 部屋を配置する
        /// </summary>
        /// <param name="map"></param>
        /// <param name="rect"></param>
        /// <param name="atr"></param>
        void DrawRect(ref int[,] map, Rectangle rect, int atr, int edgeatr)
        {
            int col;
            for (int x = rect.X; x < rect.X + rect.Width; x++)
            {
                for (int y = rect.Y; y < rect.Y + rect.Height; y++)
                {
                    col = atr;
                    if (x == rect.X || x == rect.X + rect.Width - 1
                        || y == rect.Y || y == rect.Y + rect.Height - 1)
                    { col = edgeatr; }

                    map[x, y] = col;
                }
            }
        }

        #endregion

        #region ダンジョンマップ関連
        /// <summary>
        /// ダンジョンマップ生成
        /// </summary>
        private void CreateDungeon()
        {
            DungeonType type = DungeonType.MAZE;

            // ダンジョンタイプの決定
            switch ((string)domainUpDown1.SelectedItem)
            {
                case "RANDOM": type = (DungeonType)rand.Next((int)DungeonType.MAX); break;
                case "MAZE": type = DungeonType.MAZE;
                    cDungeon dungeon = new cDungeon(rand.Next(), (int)numericUpDownWidth.Value, (int)numericUpDownHeight.Value);
                    dungeon.generateRandom(0);
//                    cell = generateMaze((int)numericUpDownWidth.Value, (int)numericUpDownHeight.Value, 1, 0);
                    cell = dungeon.cell;
                    break;
                case "ONE_FLORE": type = DungeonType.ONE_FLORE; break;
                case "ROGE": type = DungeonType.ROGE; break;
            }

            // ビットマップの生成
            bmp = new Bitmap((int)cell.GetLength(0) * 4, (int)cell.GetLength(1) * 4);

            for (int vx = 0; vx < (int)cell.GetLength(0); vx++)
            {
                //                Console.Write("\n");
                for (int vy = 0; vy < (int)cell.GetLength(1); vy++)
                {
                    // ログ出力
                    //                    Console.Write(string.Format("[{0}],", town.cell[vx, vy]));

                    // 色データを入れる
                    int i = cell[vx, vy];

                    if (i == cDungeon.ROAD)
                    {// 道
                        DrawPixel(bmp, vx * 4, vy * 4, Color.LightGray);
                    }
                    else if (i == cDungeon.WALL)
                    {// 壁
                        DrawPixel(bmp, vx * 4, vy * 4, Color.Black);
                    }
                    else if (i == cDungeon.WATER)
                    {// 水路
                        DrawPixel(bmp, vx * 4, vy * 4, Color.CornflowerBlue);
                    }
                    else if (i == cDungeon.OBSTACLE1)
                    {// 障害物１
                        DrawPixel(bmp, vx * 4, vy * 4, Color.Yellow);
                    }
                    else if (i == cDungeon.OBSTACLE2)
                    {// 障害物2
                        DrawPixel(bmp, vx * 4, vy * 4, Color.Orange);
                    }
                    else if (i == 10)
                    {// 分岐点
                        DrawPixel(bmp, vx * 4, vy * 4, Color.Yellow);
                    }
                    else if (i == 11)
                    {// 行き止まり
                        DrawPixel(bmp, vx * 4, vy * 4, Color.Magenta);
                    }
                }
            }

            // 画像表示
            pictureBox1.Image = bmp;
        }
        #endregion

        /// <summary>
        /// ラジオボタン "ワールドマップ生成" を選択
        /// </summary>
        private void radioButtonWorld_CheckedChanged(object sender, EventArgs e)
        {
            numericUpDownHeight.Minimum = 40;
            numericUpDownHeight.Maximum = 100;
            numericUpDownHeight.Value = 64;
            numericUpDownWidth.Minimum = 40;
            numericUpDownWidth.Maximum = 100;
            numericUpDownWidth.Value = 64;

            // ※ 4固定推奨
            label1.Text = "１区画";
            numericUpDown1.Minimum = 4;
            numericUpDown1.Maximum = 100;
            numericUpDown1.Value = 4;

            // ※ 20~36推奨（高すぎると負荷がかかり、処理落ちの原因となる）
            label2.Text = "最大影響力";
            numericUpDown2.Minimum = 10;
            numericUpDown2.Maximum = 200;
            numericUpDown2.Value = 28;

            // ※ 45~80推奨（低いと陸多め、高いと海多め）
            label3.Text = "影響率(海陸率)";
            numericUpDown3.Minimum = 0;
            numericUpDown3.Maximum = 500;
            numericUpDown3.Value = 65;

            // ※ 2固定！（負荷が全然違うので3以上にしない、１だとマップが雑すぎるため２固定）
            label4.Text = "試行回数";
            numericUpDown4.Minimum = 1;
            numericUpDown4.Maximum = 100;
            numericUpDown4.Value = 2;

            // ※ 0~50 自由でおk
            label5.Text = "雪原境界";
            numericUpDown5.Minimum = 0;
            numericUpDown5.Maximum = 50;
            numericUpDown5.Value = 10;

            checkBox1.Text = "道を作成する";
            checkBox1.Checked = false;
        }
        /// <summary>
        /// ラジオボタン "街マップ生成" を選択
        /// </summary>
        private void radioButtonTown_CheckedChanged(object sender, EventArgs e)
        {
            numericUpDownHeight.Maximum = 64;
            numericUpDownHeight.Minimum = 64;
            numericUpDownHeight.Value = 64;

            numericUpDownWidth.Maximum = 64;
            numericUpDownWidth.Minimum = 64;
            numericUpDownWidth.Value = 64;

            label1.Text = "１区画";
            numericUpDown1.Minimum = 16;
            numericUpDown1.Maximum = 16;
            numericUpDown1.Value = 16;

            label2.Text = "発展度";
            numericUpDown2.Minimum = 10;
            numericUpDown2.Maximum = 200;
            numericUpDown2.Value = 28;

            label3.Text = "";
            numericUpDown3.Minimum = 0;
            numericUpDown3.Maximum = 100;
            numericUpDown3.Value = 0;

            label4.Text = "";
            numericUpDown4.Minimum = 0;
            numericUpDown4.Maximum = 100;
            numericUpDown4.Value = 0;

            label5.Text = "";
            numericUpDown5.Minimum = 0;
            numericUpDown5.Maximum = 100;
            numericUpDown5.Value = 0;

            label6.Text = "";
            numericUpDown6.Minimum = 0;
            numericUpDown6.Maximum = 100;
            numericUpDown6.Value = 0;

            // 街タイプの選択欄
            domainUpDown1.Text = "RANDOM";
            domainUpDown1.Items.Clear();
            domainUpDown1.Items.Add("RANDOM");
            for (int i = 0; i < (int)TownType.MAX; i++)
            {
                domainUpDown1.Items.Add(((TownType)i).ToString());
            }
        }
        /// <summary>
        /// ラジオボタン "ダンジョンマップ生成" を選択
        /// </summary>
        private void radioButtonDungeon_CheckedChanged(object sender, EventArgs e)
        {
            // ダンジョンタイプの選択欄
            domainUpDown1.Text = "RANDOM";
            domainUpDown1.Items.Clear();
            domainUpDown1.Items.Add("RANDOM");
            for (int i = 0; i < (int)DungeonType.MAX; i++)
            {
                domainUpDown1.Items.Add(((DungeonType)i).ToString());
            }
        }

        /// <summary>
        /// 迷路生成　[ 穴掘り法 ]
        /// </summary>
        /// <param name="width">横幅</param>
        /// <param name="height">縦幅</param>
        /// <param name="wall">壁の値</param>
        /// <param name="road">道の値</param>
        /// <returns>生成された迷路の二次元配列を返す</returns>
        private int[,] generateMaze(int width, int height, int wall, int road)
        {
            // ローカル変数
            int[,] aCell;
            aCell = new int[width, height];

            // 初期化
            for (int w = 0; w < width; w++){
                for (int h = 0; h < height; h++){
                    aCell[w, h] = wall;
                }
            }

            // 奇数に調整
            if (width % 2 == 0) width--;
            if (height % 2 == 0) height--;

            // 起点を設定する
            int startX, startY;
            do
            {// ランダム奇数
                startX = rand.Next(width);
            }
            while (startX % 2 == 0);
            do
            {// ランダム奇数
                startY = rand.Next(height);
            }
            while (startY % 2 == 0);

            // 再帰法で迷路を生成していく
            recarcive(ref aCell, startX, startY, wall, road);

            return aCell;
        }

        /// <summary>
        /// 迷路生成再帰関数
        /// </summary>
        private void recarcive(ref int[,]map, int x, int y, int wall, int road)
        {
            // 4方向をランダムに生成
            int[] dir = generateRandomDirections();

            // 順番にその方向に進む
            for (int i = 0; i < dir.Length; i++)
            {
                switch (dir[i])
                {
                    case 0:// 上
                        if (y - 2 <= 0) continue;
                        if (map[x, y - 2] == wall)
                        {
                            map[x, y - 1] = road;
                            map[x, y - 2] = road;
                            recarcive(ref map, x, y - 2, wall, road);
                        }
                        break;
                    case 1:// 下
                        if (y + 2 >= map.GetLongLength(1)) continue;
                        if (map[x, y + 2] == wall)
                        {
                            map[x, y + 1] = road;
                            map[x, y + 2] = road;
                            recarcive(ref map, x, y + 2, wall, road);
                        }
                        break;
                    case 2:// 左
                        if (x - 2 <= 0) continue;
                        if (map[x - 2, y] == wall)
                        {
                            map[x - 1, y] = road;
                            map[x - 2, y] = road;
                            recarcive(ref map, x - 2, y, wall, road);
                        }
                        break;
                    case 3:// 右
                        if (x + 2 >= map.GetLongLength(0)) continue;
                        if (map[x + 2, y] == wall)
                        {
                            map[x + 1, y] = road;
                            map[x + 2, y] = road;
                            recarcive(ref map, x + 2, y, wall, road);
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
            if (map[x - 1, y] == road) cnt++;
            if (map[x + 1, y] == road) cnt++;
            if (map[x, y + 1] == road) cnt++;
            if (map[x, y - 1] == road) cnt++;
            if (cnt == 1)
            {
                // 行き止まり
                map[x, y] = 2;
            }
            else if (cnt == 2)
            {
                // 通路
            }
            else
            {
                // 分岐点
                map[x, y] = 3;
            }
        }

        /// <summary>
        /// ランダム方向生成
        /// </summary>
        /// <returns></returns>
        public int[] generateRandomDirections()
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

        /// <summary>
        /// 保存ボタンを押下
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonSave_Click(object sender, EventArgs e)
        {
            if (bmp == null)
                MessageBox.Show("保存する画像がありません！");
            else if (pictureBox1.Image != null)
            {
                //PNG形式で保存する
                DateTime now = DateTime.Now;
                bmp.Save("" +
                    /* タイムススタンプ */
                    now.Year +
                    ((now.Month < 10) ? ("0" + now.Month.ToString()) : now.Month.ToString()) +
                    ((now.Day < 10) ? ("0" + now.Day.ToString()) : now.Day.ToString()) +
                    ((now.Minute < 10) ? ("0" + now.Minute.ToString()) : now.Minute.ToString()) +
                    ((now.Second < 10) ? ("0" + now.Second.ToString()) : now.Second.ToString()) + 
                    ".png", System.Drawing.Imaging.ImageFormat.Png);

                MessageBox.Show("保存しました");
            }
        }
    }
}
