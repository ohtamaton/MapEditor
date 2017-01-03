/**
 * DangionMapEditor.cs
 * 
 * ローグライクゲーム用のダンジョンマップ生成クラス. 
 *
 * @author ys.ohta
 * @version 1.0
 * @date 2016/10/07
 */

using UnityEngine;
using UnityEditor;
using NUnit.Framework;

/**
 * <summery>
 * DangionMapEditor
 * </summery>
 */
public class DangionMapEditor : EditorWindow
{
//===========================================================
// 変数宣言
//===========================================================

    //---------------------------------------------------
    // public
    //---------------------------------------------------

    //None.

    //---------------------------------------------------
    // private
    //---------------------------------------------------

    //ダンジョンマップエディタウィンドウ
    private DangionMapEditWindow _subWindow;

    //===========================================================
    // 関数宣言
    //===========================================================

    //---------------------------------------------------
    // public
    //---------------------------------------------------

    //None.

    //---------------------------------------------------
    // private
    //---------------------------------------------------

    //None.

    //---------------------------------------------------
    // other
    //---------------------------------------------------

    /**
     * UnityのメニューWindowにDangionMapEditorアイテムを追加
     * メニュー選択時のウィンドウ表示処理
     * @param
     * @return
     **/
    [UnityEditor.MenuItem("Window/DangionMapEditor")]
    static void ShowTestMainWindow()
    {
        EditorWindow.GetWindow(typeof(DangionMapEditor));
    }

    /**
     * Unityの拡張Editor用のGUI表示処理
     * @param
     * @return
     **/
    void OnGUI()
    {
        GUILayout.BeginHorizontal();

        //ダンジョンマップエディタ用のサブウィンドウ生成ボタン生成
        if (GUILayout.Button("Open Editor"))
        {
            if (_subWindow == null)
            {
                _subWindow = DangionMapEditWindow.WillAppear(this);
            }
            else
            {
                _subWindow.Focus();
            }
        }

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }
}

/**
 * <summery>
 * DangionMapEdit SubWindow
 * </summery>
 */
public class DangionMapEditWindow : EditorWindow
{
//===========================================================
// 変数宣言
//===========================================================

    //---------------------------------------------------
    // public
    //---------------------------------------------------

    public DangionMapEditor parent { get; private set; }

    //---------------------------------------------------
    // private
    //---------------------------------------------------

    private int _mapHeight = 32;
    private int _mapWidth = 48;

    //部屋のサイズの最小値
    private int roomMin = 4;

    // マップウィンドウのサイズ
    private const float WINDOW_W = 750.0f;
    private const float WINDOW_H = 750.0f;

    private const float gridSize = 24.0f;

    private int[,] _mapValue = null;

    //---------------------------------------------------
    // other
    //---------------------------------------------------

    enum M_DIRECTON
    {
        NONE, LEFT, RIGHT, UP, DOWN
    };

//===========================================================
// 変数宣言
//===========================================================
    //---------------------------------------------------
    // public
    //---------------------------------------------------

    /**
     * <summary>
     * ダンジョンマップサブウィンドウの生成時の処理
     * </summary>
     * @param _parent 親ウィンドウ
     * @return 
     **/
    public static DangionMapEditWindow WillAppear(DangionMapEditor _parent)
    {
        DangionMapEditWindow window = (DangionMapEditWindow)EditorWindow.GetWindow(typeof(DangionMapEditWindow), false);
        window.Show();
        window.minSize = new Vector2(WINDOW_W, WINDOW_H);
        window.parent = _parent;
        window.init();
        return window;
    }

    /**
     * <summary>
     * ダンジョンマップサブウィンドウの初期化
     * </summary>
     * @param
     * @return 
     **/
    public void init()
    {
        //マップデータの初期化
        _mapValue = new int[_mapWidth,_mapHeight];
        for (int i=0; i<_mapWidth; i++) {
            for (int j=0; j<_mapHeight; j++)
            _mapValue[i,j] = 0;
        }
    }

    //---------------------------------------------------
    // private
    //---------------------------------------------------

    /**
     * <summary>
     * ダンジョンマップのランダム生成処理
     * </summary>
     * @param
     * @return 
     **/
    private void DangionGenerate()
    {
        //ダンジョン内の最大1つの部屋を有する区画の縦横の数
        int m_areaHeight = 2;
        int m_areaWidth = Random.Range(2, 4+1);
        int m_roomCnt = m_areaWidth * m_areaHeight;

        //各区画の部屋の有無
        bool[,] m_roomExist = new bool[m_areaWidth, m_areaHeight];
        bool[,] m_routeReachable = new bool[m_areaWidth * m_areaHeight, m_areaWidth * m_areaHeight];

        //区画内の部屋外の余白
        int m_space = 2;

        //区画ごとがもつ部屋の左端、上端、横縦幅
        Rect[,] m_roomRect = new Rect[m_areaWidth, m_areaHeight];

        //生成する部屋の最大の縦横幅
        int m_roomHeightMax = _mapHeight / m_areaHeight - 2 * m_space;
        int m_roomWidthMax = _mapWidth / m_areaWidth - 2 * m_space;

        if ((roomMin > m_roomHeightMax) || (roomMin > m_roomWidthMax))
        {
            throw new System.Exception("roomHeight/WidthMax is too small");
        }

        ///////////////////////////////////////////////////
        ///初期化処理
        ///////////////////////////////////////////////////
        for (int i = 0; i < _mapWidth * _mapHeight; i++)
        {
            _mapValue[(int)(i % _mapWidth), (int)(i / _mapWidth)] = 0;
        }

        for (int i = 0; i < m_areaWidth * m_areaHeight; i++)
        {
            m_roomExist[(int)(i % m_areaWidth), (int)(i / m_areaWidth)] = true;

            for (int j = 0; j < m_areaWidth * m_areaHeight; j++)
            {
                m_routeReachable[i, j] = false;
            }
        }

        ///////////////////////////////////////////////////
        ///各区画の処理
        ///////////////////////////////////////////////////
        for (int i = 0; i < m_areaWidth; i++)
        {        
            for(int j=0; j < m_areaHeight; j++)
            {
                //(1/区画数)の確率でその区画には部屋をつくらない場合に使用
                /*if (Random.Range(0, m_areaHeight * m_areaWidth) == 0)
                {
                    if (m_areaHeight > 1)
                    {
                        //部屋はないが、通路として通る地点を1つ設定する(以降1x1の部屋とみなす).
                        int x = i * (_mapWidth / m_areaWidth) + Random.Range(0, _mapWidth / m_areaWidth);
                        int y = j * (_mapHeight / m_areaHeight) + Random.Range(0, _mapHeight / m_areaHeight) ;
                        _mapValue[x, y] = 1;
                        m_roomRect[i, j] = new Rect(x, y, 1, 1);
                        continue;
                    }     
                }*/

                int m_roomWidth = Random.Range(roomMin, m_roomWidthMax+1);
                int m_roomHeight = Random.Range(roomMin, m_roomHeightMax+1);

                int m_roomPosX = i * (_mapWidth / m_areaWidth) + Random.Range(0, m_roomWidthMax - m_roomWidth+1) + m_space;
                int m_roomPosY = j * (_mapHeight / m_areaHeight) + Random.Range(0, m_roomHeightMax - m_roomHeight+1) + m_space;

                m_roomRect[i, j] = new Rect(m_roomPosX, m_roomPosY, m_roomWidth, m_roomHeight);

                for (int k = m_roomPosY; k < m_roomPosY + m_roomHeight; k++)
                {
                    for (int l= m_roomPosX; l < m_roomPosX + m_roomWidth; l++)
                    {
                       _mapValue[l, k] = 1;
                    }
                }
            }
        }

        ///////////////////////////////////////////////////
        ///区画間の通路作成処理
        ///////////////////////////////////////////////////

        //現在の通路描写位置
        int m_currentAreaX = 0;
        int m_currentAreaY = 0;

        while (true) 
        {
            ///
            ///現在の区画から通路をつなげる方向の決定.
            ///

            //通路を進める方向
            M_DIRECTON direction = M_DIRECTON.NONE;

            //次の区画がすでに到達済の区画かどうかを判定するフラグ
            bool reachable = false;
            if (Random.Range(0, 2) == 0)
            {
                ///
                ///横にフロアをつなげる処理
                ///                
                if (m_currentAreaX == 0) 
                {
                    //現在が左端の区画の場合は右方向へ通路を伸ばす
                    if (!m_routeReachable[m_currentAreaX + m_currentAreaY*m_areaWidth, (m_currentAreaX + 1) + m_currentAreaY * m_areaWidth])
                    {
                        direction = M_DIRECTON.RIGHT;
                    } else
                    {
                        reachable = true;
                    }                     
                }
                else if (m_currentAreaX == m_areaWidth - 1)
                {
                    //現在が右端の区画の場合は左方向へ通路を伸ばす
                    if (!m_routeReachable[(m_currentAreaX - 1) + m_currentAreaY * m_areaWidth, m_currentAreaX + m_currentAreaY * m_areaWidth])
                    {
                        direction = M_DIRECTON.LEFT;
                    }
                    else
                    {
                        reachable = true;
                    }
                }
                else
                {
                    //現在が端でない区画の場合は右か左にへ通路を伸ばす
                    //左か右かはランダムに決定
                    if (Random.Range(0, 2) == 0)
                    {
                        if (!m_routeReachable[m_currentAreaX + m_currentAreaY * m_areaWidth, (m_currentAreaX + 1) + m_currentAreaY * m_areaWidth])
                        {
                            direction = M_DIRECTON.RIGHT;
                        }
                        else
                        {
                            reachable = true;
                        }
                    } else
                    {
                        if (!m_routeReachable[(m_currentAreaX - 1) + m_currentAreaY * m_areaWidth, m_currentAreaX + m_currentAreaY * m_areaWidth])
                        {
                            direction = M_DIRECTON.LEFT;
                        }
                        else
                        {
                            reachable = true;
                        }
                    }
                }
            }
            else
            {
                ///
                ///縦方向にフロアをつなげる処理
                ///
                if (m_currentAreaY == 0)
                {
                    //現在が上端の区画の場合は下方向へ通路を伸ばす
                    if (!m_routeReachable[m_currentAreaX + m_currentAreaY * m_areaWidth, m_currentAreaX + (m_currentAreaY + 1) * m_areaWidth])
                    {
                        direction = M_DIRECTON.DOWN;
                    }
                    else
                    {
                        reachable = true;
                    }
                }
                else if (m_currentAreaY == m_areaHeight - 1)
                {
                    //現在が下端の区画の場合は上方向へ通路を伸ばす
                    if (!m_routeReachable[m_currentAreaX + (m_currentAreaY - 1) * m_areaWidth, m_currentAreaX + m_currentAreaY * m_areaWidth])
                    {
                        direction = M_DIRECTON.UP;
                    }
                    else
                    {
                        reachable = true;
                    }
                }
            } 

            //次に移動予定の区画が既に到達済の区画であった場合、次の区画を選択
            if (reachable)
            {
                m_currentAreaX++;
                if (m_currentAreaX >= m_areaWidth)
                {
                    if (m_currentAreaY == m_areaHeight - 1)
                    {
                        break;
                    }
                    m_currentAreaX -= m_areaWidth;
                    m_currentAreaY++;
                }
            }

            //次の区画への方向が未決定の場合は再度決定しなおす
            if (direction == M_DIRECTON.NONE)
            {
                continue;
            }

            
            if (direction == M_DIRECTON.RIGHT)
            {
                ///
                ///右方向の区画への通路生成処理
                ///
                int m_currentX = (int)m_roomRect[m_currentAreaX, m_currentAreaY].x + (int)m_roomRect[m_currentAreaX, m_currentAreaY].width;
                int m_currentY = Random.Range((int)m_roomRect[m_currentAreaX, m_currentAreaY].y, (int)m_roomRect[m_currentAreaX, m_currentAreaY].y + (int)m_roomRect[m_currentAreaX, m_currentAreaY].height);

                int m_startX = m_currentX;
                int m_startY = m_currentY;

                int targetX = (int)m_roomRect[m_currentAreaX + 1, m_currentAreaY].x;
                int targetY = Random.Range((int)m_roomRect[m_currentAreaX + 1, m_currentAreaY].y, (int)m_roomRect[m_currentAreaX + 1, m_currentAreaY].y + (int)m_roomRect[m_currentAreaX + 1, m_currentAreaY].height);

                //区画内まで次のターゲットへ通路を伸ばす.
                for (m_currentX = m_startX; m_currentX < (m_currentAreaX + 1) * (_mapWidth / m_areaWidth); m_currentX++)
                {
                    _mapValue[m_currentX, m_currentY] = 1;
                }

                if (targetY > m_currentY)
                {
                    for (m_currentY = m_startY; m_currentY < targetY + 1; m_currentY++)
                    {
                        _mapValue[m_currentX, m_currentY] = 1;
                    }
                }
                else
                {
                    for (m_currentY = m_startY; m_currentY > targetY - 1; m_currentY--)
                    {
                        _mapValue[m_currentX, m_currentY] = 1;
                    }
                }

                m_currentY = targetY;
                m_startX = m_currentX;

                for (m_currentX = m_startX; m_currentX < targetX + 1; m_currentX++)
                {
                    _mapValue[m_currentX, m_currentY] = 1;
                }
                m_routeReachable[m_currentAreaX + m_currentAreaY*m_areaWidth, (m_currentAreaX + 1) + m_currentAreaY * m_areaWidth] = true;

            } else if (direction == M_DIRECTON.LEFT)
            {
                ///
                ///左方向の区画への通路生成処理
                ///
                int m_currentX = (int)m_roomRect[m_currentAreaX, m_currentAreaY].x;
                int m_currentY = Random.Range((int)m_roomRect[m_currentAreaX, m_currentAreaY].y, (int)m_roomRect[m_currentAreaX, m_currentAreaY].y + (int)m_roomRect[m_currentAreaX, m_currentAreaY].height);

                int m_startX = m_currentX;
                int m_startY = m_currentY;

                int targetX = (int)m_roomRect[m_currentAreaX - 1, m_currentAreaY].x + (int)m_roomRect[m_currentAreaX - 1, m_currentAreaY].width;
                int targetY = Random.Range((int)m_roomRect[m_currentAreaX - 1, m_currentAreaY].y, (int)m_roomRect[m_currentAreaX - 1, m_currentAreaY].y + (int)m_roomRect[m_currentAreaX - 1, m_currentAreaY].height);

                //区画内まで次のターゲットへ通路を伸ばす.
                for (m_currentX = m_startX; m_currentX > (m_currentAreaX) * (_mapWidth / m_areaWidth); m_currentX--)
                {
                    _mapValue[m_currentX, m_currentY] = 1;
                }

                if (targetY > m_currentY)
                {
                    for (m_currentY = m_startY; m_currentY < targetY + 1; m_currentY++)
                    {
                        _mapValue[m_currentX, m_currentY] = 1;
                    }
                }
                else
                {
                    for (m_currentY = m_startY; m_currentY > targetY - 1; m_currentY--)
                    {
                        _mapValue[m_currentX, m_currentY] = 1;
                    }
                }

                m_currentY = targetY;
                m_startX = m_currentX;

                for (m_currentX = m_startX; m_currentX > targetX - 1; m_currentX--)
                {
                    _mapValue[m_currentX, m_currentY] = 1;
                }
                m_routeReachable[(m_currentAreaX-1) + m_currentAreaY * m_areaWidth, m_currentAreaX + m_currentAreaY * m_areaWidth] = true;

            } else if (direction == M_DIRECTON.DOWN)
            {
                ///
                ///下方向の区画への通路生成処理
                ///
                int m_currentX = Random.Range((int)m_roomRect[m_currentAreaX, m_currentAreaY].x, (int)m_roomRect[m_currentAreaX, m_currentAreaY].x + (int)m_roomRect[m_currentAreaX, m_currentAreaY].width);
                int m_currentY = (int)m_roomRect[m_currentAreaX, m_currentAreaY].y + (int)m_roomRect[m_currentAreaX, m_currentAreaY].height;                

                int m_startX = m_currentX;
                int m_startY = m_currentY;
 
                int targetX = Random.Range((int)m_roomRect[m_currentAreaX, m_currentAreaY + 1].x + 1, (int)m_roomRect[m_currentAreaX, m_currentAreaY + 1].x + (int)m_roomRect[m_currentAreaX, m_currentAreaY + 1].width - 1);
                int targetY = (int)m_roomRect[m_currentAreaX, m_currentAreaY+1].y;
                
                //区画内まで次のターゲットへ通路を伸ばす.
                for (m_currentY = m_startY; m_currentY < (m_currentAreaY + 1) * (_mapHeight / m_areaHeight); m_currentY++)
                {
                    _mapValue[m_currentX, m_currentY] = 1;
                }

                if (targetX > m_currentX)
                {
                    for (m_currentX = m_startX; m_currentX < targetX + 1; m_currentX++)
                    {
                        _mapValue[m_currentX, m_currentY] = 1;
                    }
                }
                else
                {
                    for (m_currentX = m_startX; m_currentX > targetX - 1; m_currentX--)
                    {
                        _mapValue[m_currentX, m_currentY] = 1;
                    }
                }

                m_currentX = targetX;
                m_startY = m_currentY;

                for (m_currentY = m_startY; m_currentY < targetY + 1; m_currentY++)
                {
                    _mapValue[m_currentX, m_currentY] = 1;
                }

                m_routeReachable[m_currentAreaX + m_currentAreaY * m_areaWidth, m_currentAreaX + (m_currentAreaY+1) * m_areaWidth] = true;

            } else if (direction == M_DIRECTON.UP)
            {
                ///
                ///上方向の区画への通路生成処理
                ///
                int m_currentX = Random.Range((int)m_roomRect[m_currentAreaX, m_currentAreaY].x, (int)m_roomRect[m_currentAreaX, m_currentAreaY].x + (int)m_roomRect[m_currentAreaX, m_currentAreaY].width);
                int m_currentY = (int)m_roomRect[m_currentAreaX, m_currentAreaY].y;

                int m_startX = m_currentX;
                int m_startY = m_currentY;

                int targetX = Random.Range((int)m_roomRect[m_currentAreaX, m_currentAreaY - 1].x, (int)m_roomRect[m_currentAreaX, m_currentAreaY - 1].x + (int)m_roomRect[m_currentAreaX, m_currentAreaY - 1].width);
                int targetY = (int)m_roomRect[m_currentAreaX, m_currentAreaY - 1].y + (int)m_roomRect[m_currentAreaX, m_currentAreaY - 1].height;

                //区画内まで次のターゲットへ通路を伸ばす.
                for (m_currentY = m_startY; m_currentY > (m_currentAreaY - 1) * (_mapHeight / m_areaHeight); m_currentY--)
                {
                    _mapValue[m_currentX, m_currentY] = 1;
                }

                if (targetX > m_currentX)
                {
                    for (m_currentX = m_startX; m_currentX < targetX + 1; m_currentX++)
                    {
                        _mapValue[m_currentX, m_currentY] = 1;
                    }
                }
                else
                {
                    for (m_currentX = m_startX; m_currentX > targetX - 1; m_currentX--)
                    {
                        _mapValue[m_currentX, m_currentY] = 1;
                    }
                }

                m_currentX = targetX;
                m_startY = m_currentY;

                for (m_currentY = m_startY; m_currentY > targetY - 1; m_currentY--)
                {
                    _mapValue[m_currentX, m_currentY] = 1;
                }

                m_routeReachable[m_currentAreaX + (m_currentAreaY - 1) * m_areaWidth, m_currentAreaX + m_currentAreaY * m_areaWidth] = true;
                
            }
            m_currentAreaX++;
            if (m_currentAreaX >= m_areaWidth)
            {
                if (m_currentAreaY == m_areaHeight - 1)
                {
                    break;
                }
                m_currentAreaX -= m_areaWidth;
                m_currentAreaY++;
            }
        }

        ///
        ///縦方向の区間のつながりチェック
        ///
        bool m_reacheable = true;

        for (int i = 0; i < m_areaWidth; i++)
        {
            m_reacheable &= m_routeReachable[i, i + m_areaWidth];
        }

        if (!m_reacheable)
        {
            ///
            ///縦方向の区間のつながりがない場合はつなげる区間をランダムに選択してつなげる
            ///
            m_currentAreaX = Random.Range(0, m_areaWidth);
            m_currentAreaY = 0;

            int m_currentX = Random.Range((int)m_roomRect[m_currentAreaX, m_currentAreaY].x, (int)m_roomRect[m_currentAreaX, m_currentAreaY].x + (int)m_roomRect[m_currentAreaX, m_currentAreaY].width);
            int m_currentY = (int)m_roomRect[m_currentAreaX, m_currentAreaY].y + (int)m_roomRect[m_currentAreaX, m_currentAreaY].height;

            int m_startX = m_currentX;
            int m_startY = m_currentY;

            int targetX = Random.Range((int)m_roomRect[m_currentAreaX, m_currentAreaY + 1].x + 1, (int)m_roomRect[m_currentAreaX, m_currentAreaY + 1].x + (int)m_roomRect[m_currentAreaX, m_currentAreaY + 1].width - 1);
            int targetY = (int)m_roomRect[m_currentAreaX, m_currentAreaY + 1].y;

            //区画内まで次のターゲットへ通路を伸ばす.
            for (m_currentY = m_startY; m_currentY < (m_currentAreaY + 1) * (_mapHeight / m_areaHeight); m_currentY++)
            {
                _mapValue[m_currentX, m_currentY] = 1;
            }

            if (targetX > m_currentX)
            {
                for (m_currentX = m_startX; m_currentX < targetX + 1; m_currentX++)
                {
                    _mapValue[m_currentX, m_currentY] = 1;
                }
            }
            else
            {
                for (m_currentX = m_startX; m_currentX > targetX - 1; m_currentX--)
                {
                    _mapValue[m_currentX, m_currentY] = 1;
                }
            }

            m_currentX = targetX;
            m_startY = m_currentY;

            for (m_currentY = m_startY; m_currentY < targetY + 1; m_currentY++)
            {
                _mapValue[m_currentX, m_currentY] = 1;
            }

            m_routeReachable[m_currentAreaX + m_currentAreaY * m_areaWidth, m_currentAreaX + (m_currentAreaY + 1) * m_areaWidth] = true;
        }

        ///
        ///横方向の区間のつながりチェック
        ///
        for (int i = 0; i < m_areaWidth-1; i++)
        {
            m_reacheable = true;
            for (int j=0; j < m_areaHeight; j++)
            {
                m_reacheable &= m_routeReachable[i + j * m_areaWidth, (i + 1) + j * m_areaWidth];
            }
            if (!m_reacheable)
            {
                ///
                ///横方向の区間のつながりがない場合はつなげる区間をランダムに選択してつなげる
                ///
                m_currentAreaX = i;
                m_currentAreaY = Random.Range(0, m_areaHeight);


                int m_currentX = (int)m_roomRect[m_currentAreaX, m_currentAreaY].x + (int)m_roomRect[m_currentAreaX, m_currentAreaY].width;
                int m_currentY = Random.Range((int)m_roomRect[m_currentAreaX, m_currentAreaY].y, (int)m_roomRect[m_currentAreaX, m_currentAreaY].y + (int)m_roomRect[m_currentAreaX, m_currentAreaY].height);

                int m_startX = m_currentX;
                int m_startY = m_currentY;

                int targetX = (int)m_roomRect[m_currentAreaX + 1, m_currentAreaY].x;
                int targetY = Random.Range((int)m_roomRect[m_currentAreaX + 1, m_currentAreaY].y, (int)m_roomRect[m_currentAreaX + 1, m_currentAreaY].y + (int)m_roomRect[m_currentAreaX + 1, m_currentAreaY].height);

                //区画内まで次のターゲットへ通路を伸ばす.
                for (m_currentX = m_startX; m_currentX < (m_currentAreaX + 1) * (_mapWidth / m_areaWidth); m_currentX++)
                {
                    _mapValue[m_currentX, m_currentY] = 1;
                }

                if (targetY > m_currentY)
                {
                    for (m_currentY = m_startY; m_currentY < targetY + 1; m_currentY++)
                    {
                        _mapValue[m_currentX, m_currentY] = 1;
                    }
                }
                else
                {
                    for (m_currentY = m_startY; m_currentY > targetY - 1; m_currentY--)
                    {
                        _mapValue[m_currentX, m_currentY] = 1;
                    }
                }

                m_currentY = targetY;
                m_startX = m_currentX;

                for (m_currentX = m_startX; m_currentX < targetX + 1; m_currentX++)
                {
                    _mapValue[m_currentX, m_currentY] = 1;
                }
                m_routeReachable[m_currentAreaX + m_currentAreaY * m_areaWidth, (m_currentAreaX + 1) + m_currentAreaY * m_areaWidth] = true;

            }
        }
    }

    /**
     * <summary>
     * ダンジョンマップをランダム生成されたマップデータに従って描画する.
     * </summary>
     * @param
     * @return
     **/
    private void DrawBaseGrid()
    {
        for (int i = 0; i < _mapWidth; i++)
        {
            for (int j = 0; j < _mapHeight; j++)
            {
                Rect rect = new Rect(i * gridSize, j * gridSize, gridSize, gridSize);
                DrawGridLine(rect);
                
                if(_mapValue[i,j] == 1)
                {
                    Color oldColor = GUI.color;
                    GUI.color = Color.blue;
                    GUI.Box(rect, "");
                    GUI.color = oldColor;
                }
                
            }
        }
    }

    /**
     * <summary>
     * ダンジョンマップ用の1タイル分のグリッド線を描画.
     * Rectデータにしたがって1タイル分のグリッド線を描画する.
     * ダンジョンマップ生成試験用.
     * </summary>
     * @param r 描画するタイルのRectデータ
     * @return
     **/
    private void DrawGridLine(Rect r)
    {

        Color tmp = Handles.color;

        //描画するグリッド線の色を設定
        Handles.color = new Color(1f, 1f, 1f, 0.5f);

        //上側のグリッド線を描画
        Handles.DrawLine(
            new Vector2(r.position.x, r.position.y),
            new Vector2(r.position.x + r.size.x, r.position.y));

        //下側のグリッド線を描画
        Handles.DrawLine(
            new Vector2(r.position.x, r.position.y + r.size.y),
            new Vector2(r.position.x + r.size.x, r.position.y + r.size.y));

        //左側のグリッド線を描画
        Handles.DrawLine(
            new Vector2(r.position.x, r.position.y),
            new Vector2(r.position.x, r.position.y + r.size.y));

        //右側のグリッド線を描画
        Handles.DrawLine(
            new Vector2(r.position.x + r.size.x, r.position.y),
            new Vector2(r.position.x + r.size.x, r.position.y + r.size.y));

        Handles.color = tmp;
    }

    //---------------------------------------------------
    // other
    //---------------------------------------------------

    /**
     * <summary>
     * ダンジョンマップサブウィンドウの描画処理
     * </summary>
     * @param
     * @return 
     **/
    void OnGUI()
    {
        //ダンジョンマップデータ描画
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        DrawBaseGrid();
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        EditorGUILayout.Space();

        //ダンジョンマップ生成ボタンの描画
        GUILayout.BeginVertical();
        GUILayout.FlexibleSpace();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Generate Dangion Test"))
        {
            //ダンジョンマップ生成ボタンが押された際にダンジョンを自動生成
            DangionGenerate();
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
    }

}