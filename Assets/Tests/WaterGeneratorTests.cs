using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class WaterGeneratorTests
{

    [Test]
    public void WaterGenerator_GenerateWater_CreatesWaterObject()
    {
        GameObject go = new();
        WaterGenerator waterGenerator = new();

        waterGenerator.GenerateWater(go.transform, Vector2.zero, 10, 10, 1);

        Assert.AreEqual(1, go.transform.childCount);
    }


    [Test]
    public void WaterGenerator_GenerateWater_ReplacesExistingWaterObject()
    {
        GameObject go = new();
        WaterGenerator waterGenerator = new();

        waterGenerator.GenerateWater(go.transform, Vector2.zero, 10, 10, 1);
        waterGenerator.GenerateWater(go.transform, Vector2.zero, 10, 10, 1);

        Assert.AreEqual(1, go.transform.childCount);
    }


    [Test]
    public void WaterGenerator_GenerateWater_ReturnsIfWaterLevelNotHighEnough()
    {
        GameObject go = new();
        WaterGenerator waterGenerator = new();
        float waterLevel = 0;

        waterGenerator.GenerateWater(go.transform, Vector2.zero, 10, waterLevel, 1);

        Assert.AreEqual(0, go.transform.childCount);
    }


    [Test]
    public void WaterGenerator_GenerateWater_HandlesNullParent()
    {
        GameObject go = new();
        WaterGenerator waterGenerator = new();

        Assert.DoesNotThrow(() => waterGenerator.GenerateWater(null, Vector2.zero, 10, 10, 1));
    }


    [Test]
    public void WaterGenerator_GenerateWater_HandlesNegativeWidth()
    {
        GameObject go = new();
        WaterGenerator waterGenerator = new();

        waterGenerator.GenerateWater(go.transform, Vector2.zero, -1, 10, 1);

        Assert.AreEqual(0, go.transform.childCount);
    }

}
