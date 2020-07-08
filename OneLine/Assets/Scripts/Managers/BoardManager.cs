﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// This class is the one that will control the board of the game and 
/// manage all the changes in the tiles. Calculates the space between 
/// the panels to locate the game. Calculates how many tiles are needed
/// for the current level. Sets the space for the current configuration
/// and scales everything keeping aspect ratio. 
/// </summary>
public class BoardManager : MonoBehaviour
{
    // Space that the board will take
    Vector2 resolution;
    
    // Margins
    public int margenSuperior = 5;
    public int margenLateral = 45;

    // Object that will contain the Board
    public Transform panelDePrueba;
    GameObject[,] board;

    // Tile's prefab
    public GameObject tile;
    public GameObject playerPath;
    public GameObject colorTile;
    public GameObject pathColor;

    // Panels to calculate the space for the board
    private float panelSuperior;
    private float panelInferior;

    // How many tiles will be in the current level (WidthxHeight)
    public Vector2 dimensiones =  new Vector2(); // Cuantos tiles hay a lo alto y a lo ancho

    
    /// <summary>
    /// Converts a pixel meassure to Unity Units.
    /// </summary>
    /// <param name="pixel">Pixels</param>
    /// <returns>Unity Units</returns>
    float PixelToUnityPosition(float pixel)
    {
        return pixel /= GameManager.GetInstance().GetScaling().UnityUds();
    }

    // Start is called before the first frame update
    void Start()
    {
        // Primero creamos el tablero de un tamaño concreto para que entren X tiles a lo largo y ancho
        // Le damos ese valor y luego, calculando el espacio disponible, lo ajustamos
        board = new GameObject[(int)dimensiones.y, (int)dimensiones.x];

        InitGameObjects(2);

        CalculateSpace();

        CalculatePosition();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    #region CalculateBoard
    void InitGameObjects(int color)
    {
        tile = GameManager.GetInstance().getSkins().LoadAsset<GameObject>("Tile");
        playerPath = GameManager.GetInstance().getSkins().LoadAsset<GameObject>("block_00_hint");
        colorTile = GameManager.GetInstance().getSkins().LoadAsset<GameObject>("block_0" + color);
        pathColor = GameManager.GetInstance().getSkins().LoadAsset<GameObject>("block_0" + color + "_hint");
    }

    /// <summary>
    /// Function that calculates the space available for the board and the game.  Uses the height 
    /// of the different panels to calculate this space in relation with the height of the screen.
    /// 
    /// </summary>
    void CalculateSpace()
    {
        // Calculamos el espacio ocupado por los paneles superior e inferior en píxeles
        panelSuperior = GameManager.GetInstance().panelSuperiorHeight() * GameManager.GetInstance().GetCanvas().scaleFactor;
        panelInferior = GameManager.GetInstance().panelInferiorHeight() * GameManager.GetInstance().GetCanvas().scaleFactor;
        
        Vector2 actRes = GameManager.GetInstance().getResolution();
        
        // Calculamos el espacio disponible en la resolución actual 
        float dispY = (actRes.y - (panelInferior + panelSuperior)) - (2 * GameManager.GetInstance().GetScaling().ResizeY(margenSuperior));
        float dipsX = actRes.x - (2 * GameManager.GetInstance().GetScaling().ResizeX(margenLateral));

        // Creamos el espacio disponible en pantalla (en píxeles) para el juego
        resolution = new Vector2(dipsX, dispY);

        Debug.Log("Disponemos de este espacio: " + resolution);

        resolution /= GameManager.GetInstance().GetScaling().UnityUds();

        DefineTileSize();
    }

    void DefineTileSize()
    {
        Vector2 resolutionTemp = resolution * GameManager.GetInstance().GetScaling().UnityUds();

        Vector3 medidasTablero = new Vector3 ();

        float tamFinal;
        float tamTilesX;
        float tamTilesY;

        if (dimensiones.y > 5)
        {
            tamTilesX = resolutionTemp.x / 6;
            tamTilesY = resolutionTemp.y / 8;            
        }
        else
        {
            tamTilesX = resolutionTemp.x / 6;
            tamTilesY = resolutionTemp.y / 5;
            
        }

        if (tamTilesY < tamTilesX)
        {
            tamFinal = tamTilesY;
        }
        else
        {
            tamFinal = tamTilesX;
        }

        medidasTablero.x = tamFinal * dimensiones.x;
        medidasTablero.y = tamFinal * dimensiones.y;

        medidasTablero /= GameManager.GetInstance().GetScaling().UnityUds();

        InstantiateTiles(medidasTablero, tamFinal);

        //Escalado del tablero con los tiles una vez que se han instanciado todos
        Vector2 nTam = new Vector2(tamFinal, tamFinal);
        panelDePrueba.transform.localScale = 
            GameManager.GetInstance().GetScaling().resizeObjectScaleKeepingAspectRatio(tile.GetComponent<SpriteRenderer>().bounds.size * GameManager.GetInstance().GetScaling().UnityUds(), 
            nTam, tile.transform.localScale);
    }

    void InstantiateTiles(Vector3 medidasTablero, float tamTile)
    {
        Debug.Log(medidasTablero);

        for(int i = 0; i < dimensiones.y; i++)
        {
            for (int j = 0; j < dimensiones.x; j++)
            {
                Vector3 position = new Vector3();

                position.z = -1;

                if(dimensiones.x % 2 == 0)
                {
                    position.x = (panelDePrueba.position.x + (dimensiones.x / 2) - 0.5f) - j;
                }
                else
                {
                    position.x = ((panelDePrueba.position.x - (int)(dimensiones.x / 2))) + j;
                }

                if (dimensiones.y % 2 == 0)
                {
                    position.y = (panelDePrueba.position.y + (dimensiones.y / 2) - 0.5f) - i;
                }
                else
                {
                    position.y = (panelDePrueba.position.y + (int)(dimensiones.y / 2)) - i;
                }
                
                ConfigTile(position, j, i);
            }
        }
    }

    void ConfigTile(Vector3 pos, int posX, int posY)
    {
        // Instantiate GameObjects needed
        GameObject nTile = Instantiate(tile, pos, Quaternion.identity);
        GameObject clTile = Instantiate(colorTile, pos, Quaternion.identity);
        GameObject pathPivot = new GameObject("PathPivot");
        pathPivot.transform.SetPositionAndRotation(pos, Quaternion.identity);
        GameObject plPath = Instantiate(playerPath, pos, Quaternion.identity);
        GameObject hintPivot = new GameObject("HintPivot");
        hintPivot.transform.SetPositionAndRotation(pos, Quaternion.identity);
        GameObject hnPath = Instantiate(pathColor, pos, Quaternion.identity);

        // Attacht them to parents
        nTile.transform.SetParent(panelDePrueba);
        clTile.transform.SetParent(nTile.transform);
        hintPivot.transform.SetParent(nTile.transform);
        pathPivot.transform.SetParent(nTile.transform);

        // Configure paths to rotate correctly
        plPath.transform.SetParent(pathPivot.transform);
        plPath.transform.SetPositionAndRotation(pathPivot.transform.position + new Vector3(0.5f, 0, 0), pathPivot.transform.rotation);

        hnPath.transform.SetParent(hintPivot.transform);
        hnPath.transform.SetPositionAndRotation(hintPivot.transform.position + new Vector3(0.5f, 0, 0),hintPivot.transform.rotation);

        // Configure Tile infor for later use
        nTile.transform.GetComponent<Tile>().SetTile(nTile, clTile, pathPivot, hintPivot, new Vector2(posX, posY));

        board[posY, posX] = nTile;
    }

    void CalculatePosition()
    {
        Vector3 position = new Vector3();

        float dispDistance = GameManager.GetInstance().getResolution().y - (panelInferior + panelSuperior);

        dispDistance /= 2; // Calculamos la distancia hasta el punto medio entre los dos paneles

        position.y = (GameManager.GetInstance().getResolution().y - panelSuperior) - dispDistance;
        
        // Ahora calcular la posición en unidades de Unity
        // Si la posición es mayor de la mitad, está en unidades de unity positivas
        if(position.y > (GameManager.GetInstance().getResolution().y / 2))
        {
            position.y -= (GameManager.GetInstance().getResolution().y / 2);

            position.y = PixelToUnityPosition(position.y);
        }
        // Si no, está en unidades negativas
        else if (position.y < (GameManager.GetInstance().getResolution().y / 2))
        {
            position.y = (GameManager.GetInstance().getResolution().y / 2) - position.y;

            position.y = (PixelToUnityPosition(position.y) * (-1));
        }
        // Por último, la posición 0, 0, 0
        else
        {
            position.y = 0;
        }

        panelDePrueba.SetPositionAndRotation(position, panelDePrueba.rotation);
    }
    #endregion
}
