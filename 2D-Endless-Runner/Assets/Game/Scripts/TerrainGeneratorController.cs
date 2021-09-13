using System.Collections.Generic;
using UnityEngine;

public class TerrainGeneratorController : MonoBehaviour
{
    #region Declare
    
    [Header("Templates")]
    public List<TerrainTemplateController> terrainTemplates;
    public float terrainTemplateWidth;

    [Header("Generator Area")]
    public Camera gameCamera;
    public float areaStartOffset;
    public float areaEndOffset;

    [Header("Force Early Template")]
    public List<TerrainTemplateController> earlyTerrainTemplates;

    private List<GameObject> spawnedTerrain;

    private Dictionary<string, List<GameObject>> pool;

    private const float debugLineHeight = 10;

    private float lastGeneratedPositionX;

    private float lastRemovePositionX;

    #endregion

    #region Start

    private void Start()
    {
        //mengisi variable pool agar tidak null/kosong
        pool = new Dictionary<string, List<GameObject>>();

        spawnedTerrain = new List<GameObject>();

        lastGeneratedPositionX = GetHorizontalPositionStart();
        lastRemovePositionX = lastGeneratedPositionX - terrainTemplateWidth;

        foreach(TerrainTemplateController terrain in earlyTerrainTemplates)
        {
            GenerateTerrain(lastGeneratedPositionX, terrain);
            lastGeneratedPositionX += terrainTemplateWidth;
        }
        while (lastGeneratedPositionX < GetHorizontalPositionEnd())
        {
            GenerateTerrain(lastGeneratedPositionX);
            lastGeneratedPositionX += terrainTemplateWidth;
        }
    }

    #endregion

    #region Update

    private void Update()
    {
        while(lastGeneratedPositionX < GetHorizontalPositionEnd())
        {
            GenerateTerrain(lastGeneratedPositionX);
            lastGeneratedPositionX += terrainTemplateWidth;
        }
        while(lastRemovePositionX + terrainTemplateWidth < GetHorizontalPositionStart())
        {
            lastRemovePositionX += terrainTemplateWidth;
            RemoveTerrain(lastRemovePositionX);
        }
    }

    #endregion

    //Terrain Pooling
    #region Generate From Pool Method

    //Spawn berdasarkan prefabs
    private GameObject GenerateFromPool(GameObject item, Transform parent)
    {
        if (pool.ContainsKey(item.name))
        {
            //aktifkan objek dalam pooling dibandingkan harus melakukan spawn
            if(pool[item.name].Count > 0)
            {
                GameObject newItemFromPool = pool[item.name][0];
                pool[item.name].Remove(newItemFromPool);
                newItemFromPool.SetActive(true);
                newItemFromPool.transform.position = new Vector2(lastGeneratedPositionX, 0);
                return newItemFromPool;
            }
        }
        else
        {
            //apabila pool tidak memiliki item dalam list, maka tambahkan item tersebut kedalam list
            spawnedTerrain.Add(item);
            pool.Add(item.name, new List<GameObject>());
        }

        //spawn item apabila item sedang aktif/tidak dapat digunakan
        GameObject newItem = Instantiate(item, parent);
        newItem.transform.position = new Vector2(lastGeneratedPositionX, 0);
        newItem.name = item.name;
        return newItem;
    }

    #endregion
    #region Return To Pool Method

    private void ReturnToPool(GameObject item)
    {
        //apabila pool tidak memiliki item tersebut maka debug log error
        if(!pool.ContainsKey(item.name))
        {
            Debug.Log(item.name);
            Debug.LogError("INVALID POOL ITEM!");
        }

        //non aktifkan item yang udah terpakai
        pool[item.name].Add(item);
        item.SetActive(false);
    }

    #endregion

    //Terrain spawn and destroy
    #region Generate Terrain Method

    private void GenerateTerrain(float posX, TerrainTemplateController forceterrain = null)
    {
        GameObject newTerrain = null;
        if(forceterrain == null)
        {
            newTerrain = Instantiate(terrainTemplates[Random.Range(0, terrainTemplates.Count)].gameObject, transform);
        }
        else
        {
            newTerrain = Instantiate(forceterrain.gameObject, transform);
        }

        newTerrain.transform.position = new Vector3(posX, 0);

        spawnedTerrain.Add(newTerrain);
    }

    #endregion
    #region Remove Terrain Method

    private void RemoveTerrain(float posX)
    {
        GameObject terrainToRemove = null;

        foreach(GameObject item in spawnedTerrain)
        {
            //mencari terrain berdasarkan posisi x sebelumnya
            if(item.transform.position.x == posX)
            {
                terrainToRemove = item;
                break;
            }
        }

        if(spawnedTerrain != null)
        {
            spawnedTerrain.Remove(terrainToRemove);
            Destroy(terrainToRemove);
        }
    }

    #endregion


    #region Get Horizontal Position Method

    private float GetHorizontalPositionStart()
    {
        return gameCamera.ViewportToWorldPoint(new Vector2(0f, 0f)).x + areaStartOffset;
    }

    private float GetHorizontalPositionEnd()
    {
        return gameCamera.ViewportToWorldPoint(new Vector2(1f, 0f)).x + areaEndOffset;
    }

    #endregion

    #region OnDrawGizmos

    private void OnDrawGizmos()
    {
        Vector3 areaStartPosition = transform.position;
        Vector3 areaEndPosition = transform.position;

        areaStartPosition.x = GetHorizontalPositionStart();
        areaEndPosition.x = GetHorizontalPositionEnd();

        Debug.DrawLine(areaStartPosition + Vector3.up * debugLineHeight / 2,
            areaStartPosition + Vector3.down * debugLineHeight / 2, Color.red);

        Debug.DrawLine(areaEndPosition + Vector3.up * debugLineHeight / 2,
            areaEndPosition + Vector3.down * debugLineHeight / 2, Color.red);
    }

    #endregion
}
