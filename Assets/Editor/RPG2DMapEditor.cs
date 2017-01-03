/**
 * RPG2DMapEditor.cs
 * 
 * 2D RPG用のマップエディタクラス. 
 *
 * @author ys.ohta
 * @version 1.0
 * @date 2016/10/10
 */
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

/**
 * <summary>
 * RPG2DMapEditor
 * </summary>
 **/
public class RPG2DMapEditor : EditorWindow
{
//===========================================================
// 変数宣言
//===========================================================

    //---------------------------------------------------
    // public
    //---------------------------------------------------

    public Texture2D selectedImage;
    
    //---------------------------------------------------
    // private
    //---------------------------------------------------

    // 画像ファイルパス
    private string imgFilename;
    // 出力先ディレクトリ(nullだとAssets下に出力します)
    private Object outputDirectory;
    // マップエディタのマスの数
    private int mapSize = 10;
    // グリッドの大きさ、小さいほど細かくなる
    private float gridSize = 48.0f;
    // 出力ファイル名
    private string outputFileName;
    // 選択した画像パス
    private string selectedImagePath;
    // マップ編集サブウィンドウ
    private MapEditor subWindow;

    //---------------------------------------------------
    // others
    //---------------------------------------------------

    Vector2 leftScrollPos = Vector2.zero;

//===========================================================
// 関数定義
//===========================================================
    //---------------------------------------------------
    // public
    //---------------------------------------------------

    /**
     * <summary>
     * 出力先パスを生成
     * For the future upgrade
     * </summary>
     * @param 
     * @return 出力先パス
     **/
    public string OutputFilePath()
    {
        string resultPath = "";
        if (outputDirectory != null)
        {
            resultPath = AssetDatabase.GetAssetPath(outputDirectory);
        }
        else
        {
            resultPath = Application.dataPath;
        }
        return resultPath + "/" + outputFileName + ".txt";
    }

    /**
     * <summary>
     * selectedImagePathのGetter
     * For the future upgrade
     * </summary>
     **/
    public string SelectedImagePath
    {
        get { return "test"; }
    }

    /**
     * <summary>
     * mapSizeのGetter
     * </summary>
     **/
    public int MapSize
    {
        get { return mapSize; }
    }

    /**
     * <summary>
     * gridSizeのGetter
     * </summary>
     **/
    public float GridSize
    {
        get { return gridSize; }
    }

    /**
     * <summary>
     * マップタイル一覧をボタン選択できる形にしてGUI表示
     * <summary>
     * @param
     * @return
     **/
    private void DrawImageParts()
    {
        if (imgFilename != null)
        {
            //表示するマップタイルの位置とサイズ
            float x = 0.0f;
            float y = 0.0f;
            float w = 50.0f;
            float h = 50.0f;

            //マップタイルを並べる際の横幅の最大値
            float maxW = 300.0f;

            //マップタイルのサイズ
            int mapTileSize = 48;

            //マップタイル用の画像データ
            string path = imgFilename;
            Regex regex = new Regex(".*Assets");
            path = regex.Replace(path, "Assets");
            selectedImagePath = path;

            //画像データからテクスチャデータ取得
            Texture2D tex = (Texture2D)AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D));

            //画像ファイルがなければ終了
            if (tex == null)
            {
                return;
            }

            //マップタイル用の画像データの縦横のサイズ
            int imageMaxW = tex.width;
            int imageMaxH = tex.height;

            //マップタイル用の画像データの縦横のサイズ
            int imageX = 0;
            int imageY = 0;
            
            EditorGUILayout.BeginVertical();
            
            //マップタイル用のデータからマップタイルを読み込んでボタン生成
            while(imageY < imageMaxH)
            {
                //水平に並べるタイル数が最大数を超えた場合は次の行から並べる
                if (x > maxW)
                {
                    x = 0.0f;
                    y += h;
                    EditorGUILayout.EndHorizontal();
                }
                if (x == 0.0f)
                {
                    EditorGUILayout.BeginHorizontal();
                }
                
                //テクスチャから読み込むマップタイルの位置が画像の右端に達した場合次の行から読み込む
                if (imageX > imageMaxW-mapTileSize)
                {
                    imageX = 0;
                    imageY += mapTileSize;
                }
                //テクスチャから読み込むマップタイルがすべて読み込まれたら終了
                if (imageY > imageMaxH-mapTileSize)
                {
                    break;
                }

                GUILayout.FlexibleSpace();

                //テクスチャからマップタイル取得
                Color[] pixel;
                pixel = tex.GetPixels(imageX, imageY, mapTileSize, mapTileSize);
                Texture2D clipTex = new Texture2D(mapTileSize, mapTileSize);
                clipTex.SetPixels(pixel);
                clipTex.Apply();

                imageX += mapTileSize;

                //読み込んだマップタイル画像からボタン生成
                if (GUILayout.Button(clipTex, GUILayout.MaxWidth(w), GUILayout.MaxHeight(h), GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false)))
                {
                    selectedImage = clipTex;
                }
                GUILayout.FlexibleSpace();
                x += w;
            }
            EditorGUILayout.EndVertical();
            
        }
    }

    /**
     * <summary>
     * マップタイルとして選択した画像データを表示
     * </summary>
     * @param 
     * @return 
     **/
    // 選択した画像データを表示
    private void DrawSelectedImage()
    {
        if (selectedImage != null)
        {
            EditorGUILayout.BeginVertical();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Current Selected Tile Image");
            GUILayout.Box(selectedImage);
            EditorGUILayout.EndVertical();
        }
    }

    /**
     * <summary>
     * マップ編集ウィンドウを開くボタンを生成
     * </summary>
     * @param 
     * @return 
     **/
    // マップウィンドウを開くボタンを生成
    private void DrawMapWindowButton()
    {
        EditorGUILayout.BeginVertical();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Open Editor"))
        {
            if (subWindow == null)
            {
                subWindow = MapEditor.WillAppear(this);
            }
            else
            {
                subWindow.Focus();
            }
        }
        EditorGUILayout.EndVertical();
    }

    //---------------------------------------------------
    // other
    //---------------------------------------------------

    /**
     * UnityのメニューWindowにMapEditorアイテムを追加
     * メニュー選択時のウィンドウ表示処理
     * @param
     * @return
     **/
    [UnityEditor.MenuItem("Window/MapEditor")]
    static void ShowTestMainWindow()
    {
        EditorWindow.GetWindow(typeof(RPG2DMapEditor));
    }

    /**
     * Unityの拡張Editor用のGUI表示処理
     * @param
     * @return
     **/
    void OnGUI()
    {
        // 左側のスクロールビュー(横幅300px)
        leftScrollPos = EditorGUILayout.BeginScrollView(leftScrollPos, GUI.skin.box);

        // GUI
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Open Map Data file"))
        {
            imgFilename = EditorUtility.OpenFilePanel("select file", Application.dataPath+"MapImages/tilesets", "png");
        }
        GUILayout.FlexibleSpace();

        GUILayout.EndHorizontal();
        EditorGUILayout.Space();

        DrawImageParts();

        DrawSelectedImage();

        DrawMapWindowButton();

        EditorGUILayout.EndScrollView();
    }

}

/**
 * <summary>
 * マップ編集ウィンドウ用クラス
 * </summary>
 **/
public class MapEditor : EditorWindow
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

    // マップウィンドウのサイズ
    private const float WINDOW_W = 750.0f;
    private const float WINDOW_H = 750.0f;
    // マップのグリッド数
    private int mapSize = 0;
    // グリッドサイズ. 親から値をもらう
    private float gridSize = 0.0f;
    // マップデータ
    private Texture2D[,] map;
    // グリッドの四角
    private Rect[,] gridRect;
    // 親ウィンドウの参照
    private RPG2DMapEditor parent;

//===========================================================
// 関数宣言
//===========================================================
    //---------------------------------------------------
    // public
    //---------------------------------------------------

    /**
     * <summary>
     * マップ編集ウィンドウ生成時処理
     * </summary>
     * @param _parent 親ウィンドウ
     * @return マップ編集ウィンドウ
     **/
    public static MapEditor WillAppear(RPG2DMapEditor _parent)
    {
        MapEditor window = (MapEditor)EditorWindow.GetWindow(typeof(MapEditor), false);
        window.Show();
        window.minSize = new Vector2(WINDOW_W, WINDOW_H);
        window.SetParent(_parent);
        window.init();
        return window;
    }

    /**
     * <summary>
     * マップ編集ウィンドウの初期化
     * </summary>
     * @param
     * @return 
     **/
    public void init()
    {
        //マップサイズ、マップエディタ用のグリッドサイズを親からもらう
        mapSize = parent.MapSize;
        gridSize = parent.GridSize;

        // マップデータを初期化
        map = new Texture2D[mapSize, mapSize];
        for (int i = 0; i < mapSize; i++)
        {
            for (int j = 0; j < mapSize; j++)
            {
                map[i, j] = null;
            }
        }
        // グリッドデータを生成
        gridRect = CreateGrid(mapSize);
    }

    //---------------------------------------------------
    // private
    //---------------------------------------------------

    /**
     * <summary>
     * 2Dマップ用のグリッドデータを生成
     * </summary>
     * @param div 縦横のグリッド数
     * @return gridデータ
     **/
    private Rect[,] CreateGrid(int div)
    {
        //縦横のグリッド数
        int sizeW = div;
        int sizeH = div;
                
        //グリッドの位置と縦横幅
        float x = 0.0f;
        float y = 0.0f;
        float w = gridSize; // gridSizeは親からもらう
        float h = gridSize; // gridSizeは親からもらう

        //生成したグリッドを入れる配列
        Rect[,] resultRects = new Rect[sizeH, sizeW];

        //div, gridSizeにもどついてグリッド生成
        for (int yy = 0; yy < sizeH; yy++)
        {
            x = 0.0f;
            for (int xx = 0; xx < sizeW; xx++)
            {
                Rect r = new Rect(new Vector2(x, y), new Vector2(w, h));
                resultRects[yy, xx] = r;
                x += w;
            }
            y += h;
        }

        return resultRects;
    }

    /**
     * <summary>
     * 2Dマップ用の1タイル分のグリッド線を描画.
     * Rectデータにしたがって1タイル分のグリッド線を描画する.
     * </summary>
     * @param r 描画するタイルのRectデータ
     * @return
     **/
    private void DrawGridLine(Rect r)
    {

        Color tmp = Handles.color;
        
        //描画するグリッド線の色を設定
        Handles.color = new Color(0f, 0f, 0f, 1f);

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

    /**
     * <summary>
     * parentのSetter
     * </summary>
     **/
    private void SetParent(RPG2DMapEditor _parent)
    {
        parent = _parent;
    }

    //---------------------------------------------------
    // other
    //---------------------------------------------------

    /**
     * <summary>
     * マップ編集ウィンドウの描画処理
     * </summary>
     * @param
     * @return 
     **/
    void OnGUI()
    {
        //マップエディタ用のグリッド線を描画する
        for (int yy = 0; yy < mapSize; yy++)
        {
            for (int xx = 0; xx < mapSize; xx++)
            {
                DrawGridLine(gridRect[yy, xx]);
            }
        }

        //クリックされた位置を探して、その場所に画像データを入れる
        Event e = Event.current;
        if (e.type == EventType.MouseDown)
        {
            //マウスクリック時のマウス位置
            Vector2 pos = Event.current.mousePosition;
            int xx;
        
            //x位置を先に計算して、計算回数を減らす
            for (xx = 0; xx < mapSize; xx++)
            {
                Rect r = gridRect[0, xx];
                if (r.x <= pos.x && pos.x <= r.x + r.width)
                {
                    break;
                }
            }

            if (xx == mapSize)
            {
                xx -= 1;
            }

            // 後はy位置だけ探す
            for (int yy = 0; yy < mapSize; yy++)
            {
                if (gridRect[yy, xx].Contains(pos))
                {
                    //消しゴムの時はデータを消す
                    //For the future upgrade 
                    if (parent.SelectedImagePath.IndexOf("000") > -1)
                    {
                        map[yy, xx] = null;
                    }
                    else
                    {
                        //選択された位置に画像を設定
                        map[yy, xx] = parent.selectedImage;
                    }
                    //ウィンドウの再描画処理
                    Repaint();
                    break;
                }
            }
        }

        //現在のマップを再描画する
        for (int yy = 0; yy < mapSize; yy++)
        {
            for (int xx = 0; xx < mapSize; xx++)
            {
                if (map[yy, xx] != null)
                {
                    GUI.DrawTexture(gridRect[yy, xx], map[yy, xx]);
                }
            }
        }

        // 出力ボタン
        // For the future upgrade
        /*
        Rect rect = new Rect(0, WINDOW_H - 50, 300, 50);
        GUILayout.BeginArea(rect);
        if (GUILayout.Button("output file", GUILayout.MinWidth(300), GUILayout.MinHeight(50)))
        {
            OutputFile();
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndArea();
        */
    }
}
