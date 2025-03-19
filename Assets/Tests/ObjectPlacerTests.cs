using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using NUnit.Framework.Internal;
using UnityEngine;
using UnityEngine.TestTools;

public class ObjectPlacerTests
{

    private TerrainData InitializeTerrainData()
    {
        TerrainData td = ScriptableObject.CreateInstance<TerrainData>();

        td.waterLevel = 0;
        td.groundLevel = 0;
        td.smoothLevel = 0;
        td.lod = 1;
        td.isoLevel = 0.9f;
        td.seed = 0;
        td.octaves = 3;
        td.persistence = 0.7f;
        td.lacunarity = 2;
        td.scale = 10;
        td.offsetX = 0;
        td.offsetZ = 0;
        td.objectDensity = 0;
        td.gradient = new();

        return td;
    }


    private GameObject InitializeGameObject()
    {
        // test game object is the default unity flat plane - it consists of 200 triangles (for possible object placement)
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Plane);
        go.AddComponent<ObjectPlacer>();
        go.transform.position = new Vector3(0, 10, 0);
        return go;
    }


    [Test]
    public void ObjectPlacer_PlaceObjects_HandlesEmptyMesh()
    {
        GameObject go = InitializeGameObject();
        ObjectPlacer objectPlacer = go.GetComponent<ObjectPlacer>();
        TerrainData terrainData = InitializeTerrainData();

        go.GetComponent<MeshFilter>().sharedMesh = null;

        Assert.DoesNotThrow(() => objectPlacer.PlaceObjects(terrainData));
    }


    [Test]
    public void ObjectPlacer_PlaceObjects_HandlesEmptyMeshFilterComponent()
    {
        GameObject go = new(); // no mesh filter in new game object
        go.AddComponent<ObjectPlacer>();
        ObjectPlacer objectPlacer = go.GetComponent<ObjectPlacer>();
        TerrainData terrainData = InitializeTerrainData();

        Assert.DoesNotThrow(() => objectPlacer.PlaceObjects(terrainData));
    }


    [Test]
    public void ObjectPlacer_PlaceObjects_GeneratesObjects()
    {
        GameObject go = InitializeGameObject();
        ObjectPlacer objectPlacer = go.GetComponent<ObjectPlacer>();
        TerrainData terrainData = InitializeTerrainData();
        terrainData.objectDensity = 1000; // guarantee object placement

        objectPlacer.PlaceObjects(terrainData);

        Assert.AreNotEqual(0, go.transform.childCount);
    }


    [Test]
    public void ObjectPlacer_PlaceObjects_DoesntPlaceObjectsUnderWater()
    {
        GameObject go = InitializeGameObject();
        ObjectPlacer objectPlacer = go.GetComponent<ObjectPlacer>();
        TerrainData terrainData = InitializeTerrainData();
        terrainData.objectDensity = 1000;
        terrainData.waterLevel = 20;

        objectPlacer.PlaceObjects(terrainData);

        Assert.AreEqual(0, go.transform.childCount);
    }


    [Test]
    public void ObjectPlacer_PlaceObjects_DoesntPlaceObjectsOnSlopes()
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube); // initializingcube instead of plane for horizontal surfaces
        go.AddComponent<ObjectPlacer>();
        go.transform.position = new Vector3(0, 10, 0);
        ObjectPlacer objectPlacer = go.GetComponent<ObjectPlacer>();
        TerrainData terrainData = InitializeTerrainData();
        terrainData.objectDensity = 1000;

        objectPlacer.PlaceObjects(terrainData);

        Assert.AreEqual(2, go.transform.childCount); // objects should be placed only on top side of cube
    }


    [Test]
    public void ObjectPlacer_PlaceObjects_NumberOfObjectsDependsOnObjectDensity()
    {
        GameObject go = InitializeGameObject();
        ObjectPlacer objectPlacer = go.GetComponent<ObjectPlacer>();
        TerrainData terrainData = InitializeTerrainData();
        terrainData.objectDensity = 100;

        objectPlacer.PlaceObjects(terrainData);
        int denseObjectCount = go.transform.childCount;

        terrainData.objectDensity = 10;
        objectPlacer.PlaceObjects(terrainData);
        int sparseObjectCount = go.transform.childCount;

        Assert.IsTrue(denseObjectCount > sparseObjectCount);
    }


    [Test]
    public void ObjectPlacer_DestroyObjectsInArea_DestroysObjects()
    {
        GameObject go = InitializeGameObject();
        ObjectPlacer objectPlacer = go.GetComponent<ObjectPlacer>();
        TerrainData terrainData = InitializeTerrainData();
        terrainData.objectDensity = 1000;
        Bounds bounds = new(new Vector3(0, 10, 0), Vector3.one);

        objectPlacer.PlaceObjects(terrainData);
        int objectCountBefore = go.transform.childCount;

        objectPlacer.DestroyObjectsInArea(bounds);
        int objectCountAfter = go.transform.childCount;

        Assert.IsTrue(objectCountBefore > objectCountAfter);
    }


    [Test]
    public void ObjectPlacer_DestroyObjectsInArea_LeavesObjectsOutsideArea()
    {
        GameObject go = InitializeGameObject();
        ObjectPlacer objectPlacer = go.GetComponent<ObjectPlacer>();
        TerrainData terrainData = InitializeTerrainData();
        terrainData.objectDensity = 1000;
        Bounds bounds = new(new Vector3(100, 10, 100), Vector3.one); // bounds far away from game object

        objectPlacer.PlaceObjects(terrainData);
        int objectCountBefore = go.transform.childCount;

        objectPlacer.DestroyObjectsInArea(bounds);
        int objectCountAfter = go.transform.childCount;

        Assert.AreEqual(objectCountBefore, objectCountAfter);
    }

}
