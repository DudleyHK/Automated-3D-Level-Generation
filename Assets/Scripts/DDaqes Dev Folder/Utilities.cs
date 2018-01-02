using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utilities : ScriptableObject 
{
    /// <summary>
    /// Function which can convert any grid index to the coordinates and 
    ///     return an ID. 
    ///  Will return the same ID if the current width is passed in and a new
    ///     ID if a new width is passed in.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="width"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public static int Get2DGridID(int id, int width, int height, out int x, out int y)
    {
        x = id % width;
        y = id / height;
        
        return (x * width) + y;
    }

    public static int Get3DGridID(int id, int width, int height, int depth, out int x, out int y, out int z)
    {
        x = id % width;
        y = (id / width) % height;
        z = id / (width * height);

        return (x * width) + y;
    }

}
