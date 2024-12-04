using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Assets.Scripts;
using Assets.Scripts.MapObjects;

public class MapLogicTest
{
    [Test]
    /*public void CreateMap_NoExceptionsThrown()
    {
        long ticks = DateTime.Now.Ticks;
        int seed = (int)(ticks % int.MaxValue);
        var gameObject = new GameObject();
        var mapLogic = gameObject.AddComponent<MapLogic>();

        for (int i = 0; i < 1000; i++)
        {
            try
            {
                mapLogic.CreateMap(seed);
            }
            catch (Exception e)
            {
                Assert.Fail($"Exception thrown on iteration {i} with seed {mapLogic.seed}: {e.Message}");
            }
        }

        Assert.Pass("No exceptions thrown after 1000 iterations of CreateMap.");
    }*/

    public void CreateConnected_Map()
    {
        var gameObject = new GameObject();
        var mapLogic = gameObject.AddComponent<MapLogic>();
        var mapgen = gameObject.AddComponent<MapGenerator>();
		Debug.Log(mapLogic.height);
        Debug.Log(mapLogic.width);
        
        for(int i = 0; i < 10; i++)
        {
            try
            {
                long ticks = DateTime.Now.Ticks;
                int seed = (int)(ticks % int.MaxValue);
                var realseed = mapLogic.CreateMap(seed);
                var mapObjects=mapLogic.corridorList.Count+mapLogic.roomList.Count;
                var visitedobjects = BfsBuildGraph(mapLogic).Count;
                if (visitedobjects != mapObjects)
                {
                    Assert.Fail($"Map not connected with seed {realseed}.");
                }
                mapgen.GenerateMap();
			}
            catch (Exception e)
            {
                Debug.LogException(e);
                Debug.Log("Exception thrown on seed " + mapLogic.seed);
            }
        }
    }

    public HashSet<IMapObject> BfsBuildGraph(MapLogic map)
    {
        var visited = new HashSet<IMapObject>();
        var queue = new Queue<IMapObject>();

        visited.Add(map.startRoom);
        queue.Enqueue(map.startRoom);

        while (queue.Count > 0)
        {
            var currentMapObject = queue.Dequeue();

            foreach (var exitDirection in currentMapObject.ExitList)
            {
                IMapObject nextMapObject = map.GetNeighbour(currentMapObject, exitDirection);
                if (nextMapObject != null && !visited.Contains(nextMapObject))
                {
                    visited.Add(nextMapObject);
                    queue.Enqueue(nextMapObject);
                }
            }
        }
        return visited;
    }



}
