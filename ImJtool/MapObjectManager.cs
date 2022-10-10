using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace ImJtool
{
    /// <summary>
    /// Manage objects in the map and provide some collision detection methods
    /// </summary>
    public class MapObjectManager
    {
        public static MapObjectManager Instance => Jtool.Instance.MapObjectManager;
        public MapObjectManager()
        {
            // Generate a "subclass info list" to use if needed
            var subclassTypes = Assembly.GetAssembly(typeof(MapObject)).GetTypes().Where(t => t.IsSubclassOf(typeof(MapObject)));
            foreach (var type in subclassTypes)
            {
                var types = Assembly.GetAssembly(type).GetTypes().Where(t => t.IsSubclassOf(type));
                MapObject.ChildTypes[type] = types.ToList();
            }
        }
        /// <summary>
        /// List of all objects in the map.
        /// This is also the sum of objects in all object pools.
        /// </summary>
        public List<MapObject> Objects { get; set; } = new();

        /// <summary>
        /// List of objects by type.
        /// It can be traversed on demand to improve the efficiency of collision detection, etc.
        /// </summary>
        Dictionary<Type, List<MapObject>> objectPools = new();

        /// <summary>
        /// Call the step method of all map objects
        /// </summary>
        public void DoStep()
        {
            foreach (var obj in Objects.ToArray())
            {
                if (!obj.NeedDestroy)
                {
                    obj.BeforeStep();
                    obj.Step();
                    obj.HandleImageIndex();
                    obj.HandleMovement();
                    obj.AfterMovement();
                }
            }
        }
        /// <summary>
        /// Call the draw method of all map objects
        /// </summary>
        public void DoDraw()
        {
            Objects = Objects.OrderBy(o => o.Depth).Reverse().ToList();

            foreach (var obj in Objects.ToArray())
            {
                if (!obj.NeedDestroy)
                {
                    obj.Draw();
                }
            }

            // Clean up useless objects
            Objects.RemoveAll(o => o.NeedDestroy);

            foreach (var (_, pool) in objectPools)
            {
                pool.RemoveAll(o => o.NeedDestroy);
            }
        }
        /// <summary>
        /// Add map object to their respective type pools. 
        /// It is convenient to perform collision detection methods.
        /// </summary>
        public void AddToPool(MapObject obj)
        {
            GetPool(obj.GetType()).Add(obj);
        }
        /// <summary>
        /// Create an object with type name, and call the "Create" method of the object.
        /// It will also add the object to both "objects" and "objectPools[typename]".
        /// </summary>
        public MapObject CreateObject(float x, float y, Type type)
        {
            var obj = (MapObject)Activator.CreateInstance(type);
            obj.X = x;
            obj.Y = y;
            Objects.Add(obj);
            AddToPool(obj);
            obj.Create();
            Gui.Log("MapObjectManager", $"Created object \"{type}\" at ({x}, {y})");
            return obj;
        }
        /// <summary>
        /// Check if a point collides with an object of the specified type
        /// </summary>
        public MapObject CollisionPoint(float x, float y, Type type = null)
        {
            var pool = GetTypeObjectsWithChildren(type);

            foreach (var obj in pool)
            {
                if (obj.NeedDestroy || obj.MaskSprite == null)
                    continue;

                var item = obj.MaskSprite.GetItem(obj.ImageIndex);

                if (!MapObject.PointCollision(item, x, y, obj.X, obj.Y, 1.0f / obj.XScale, 1.0f / obj.YScale, MathF.Sin(obj.Rotation * MathF.PI / 180f), MathF.Cos(obj.Rotation * MathF.PI / 180f), obj.MaskSprite.XOrigin, obj.MaskSprite.YOrigin))
                    continue;

                return obj;

            }
            return null;
        }
        /// <summary>
        /// Checks if a point collides with an object of the specified type and returns a list of objects
        /// </summary>
        public List<MapObject> CollisionPointList(float x, float y, Type type = null)
        {
            var list = new List<MapObject>();

            var pool = GetTypeObjectsWithChildren(type);

            foreach (var obj in pool)
            {
                if (obj.NeedDestroy || obj.MaskSprite == null)
                    continue;

                var item = obj.MaskSprite.GetItem(obj.ImageIndex);

                if (!MapObject.PointCollision(item, x, y, obj.X, obj.Y, 1.0f / obj.XScale, 1.0f / obj.YScale, MathF.Sin(obj.Rotation * MathF.PI / 180f), MathF.Cos(obj.Rotation * MathF.PI / 180f), obj.MaskSprite.XOrigin, obj.MaskSprite.YOrigin))
                    continue;

                list.Add(obj);
            }
            return list;
        }
        /// <summary>
        /// Get all objects that a line collides with.
        /// </summary>
        public List<MapObject> CollisionLineList(float x1, float y1, float x2, float y2, Type type = null)
        {
            var pool = GetTypeObjectsWithChildren(type);

            var xmin = MathF.Min(x1, x2);
            var xmax = MathF.Max(x1, x2);

            var ymin = MathF.Min(y1, y2);
            var ymax = MathF.Max(y1, y2);

            float k, b;
            int mode;

            if (x1 == x2 && y1 == y2)
            {
                // Just a point...
                return CollisionPointList(x1, y1, type);
            }
            else if (MathF.Abs(y1 - y2) < MathF.Abs(x1 - x2))
            {
                // Δy<Δx, do y=kx+b check
                k = (y1 - y2) / (x1 - x2);
                b = y1 - k * x1;
                mode = 0;
            }
            else
            {
                // Δy≥Δx, do x=ky+b check
                k = (x1 - x2) / (y1 - y2);
                b = x1 - k * y1;
                mode = 1;
            }

            var list = new List<MapObject>();

            foreach (var obj in pool)
            {
                if (obj.NeedDestroy || obj.MaskSprite == null)
                    continue;

                obj.GetBoundingBox();

                // First check if the line is completely out of bounding box
                if (xmax < obj.BBoxLeft || xmin > obj.BBoxRight || ymax < obj.BBoxTop || ymin > obj.BBoxBottom)
                    continue;

                // Then check if the line is inside the bounding box
                var inBox = xmin >= obj.BBoxLeft && xmax <= obj.BBoxRight && ymin >= obj.BBoxTop && ymax <= obj.BBoxBottom;
                if (!inBox)
                {
                    // Check if the line intersects the "boundary"
                    float x3, y3, x4, y4;

                    if (mode == 0) // y=kx+b, x=(y-b)/k
                    {
                        y3 = k * obj.BBoxLeft + b;
                        y4 = k * obj.BBoxRight + b;
                        x3 = (obj.BBoxTop - b) / k;
                        x4 = (obj.BBoxRight - b) / k;
                    }
                    else // x=ky+b, y=(x-b)/k
                    {
                        x3 = k * obj.BBoxTop + b;
                        x4 = k * obj.BBoxBottom + b;
                        y3 = (obj.BBoxLeft - b) / k;
                        y4 = (obj.BBoxRight - b) / k;
                    }

                    var leftok = y3 > obj.BBoxTop && y3 < obj.BBoxBottom;
                    var rightok = y4 > obj.BBoxTop && y4 < obj.BBoxBottom;
                    var topok = x3 > obj.BBoxLeft && x3 < obj.BBoxRight;
                    var bottomok = x4 > obj.BBoxLeft && x4 < obj.BBoxRight;

                    if (!leftok && !rightok && !topok && !bottomok)
                        continue;
                }

                // Finally, check each pixel in the line...

                var item = obj.MaskSprite.GetItem(obj.ImageIndex);

                var ss = MathF.Sin(obj.Rotation * MathF.PI / 180f);
                var cc = MathF.Cos(obj.Rotation * MathF.PI / 180f);

                var rxs = 1.0f / obj.XScale;
                var rys = 1.0f / obj.YScale;

                if (mode == 0)
                {
                    // _____
                    //      ----- <- Line
                    // ^^^^^^^^^^ <- Check
                    var left = MathF.Max(xmin, obj.BBoxLeft);
                    var right = MathF.Min(xmax, obj.BBoxRight);

                    for (var xx = left; xx <= right; xx++)
                    {
                        var yy = k * xx + b;
                        if (MapObject.PointCollision(item, xx, yy, obj.X, obj.Y, rxs, rys, ss, cc, obj.MaskSprite.XOrigin, obj.MaskSprite.YOrigin))
                        {
                            list.Add(obj);
                            break;
                        }
                    }
                }
                else
                {
                    //    ↓ Check
                    // |  <
                    // |  <
                    // |  <
                    //  | <         
                    //  | <
                    // ^---Line
                    var top = MathF.Max(ymin, obj.BBoxTop);
                    var bottom = MathF.Min(ymax, obj.BBoxBottom);
                    for (var yy = top; yy <= bottom; yy++)
                    {
                        var xx = k * yy + b;
                        if (MapObject.PointCollision(item, xx, yy, obj.X, obj.Y, rxs, rys, ss, cc, obj.MaskSprite.XOrigin, obj.MaskSprite.YOrigin))
                        {
                            list.Add(obj);
                            break;
                        }
                    }
                }
            }
            return list;
        }
        /// <summary>
        /// Get all objects at a point coordinate
        /// </summary>
        public List<MapObject> AtPosition(float x, float y, Type type = null)
        {
            var list = new List<MapObject>();

            var pool = GetTypeObjectsWithChildren(type);

            foreach (var obj in pool)
            {
                if (obj.NeedDestroy)
                    continue;

                if (obj.X == x && obj.Y == y)
                {
                    list.Add(obj);
                }
            }
            return list;
        }
        /// <summary>
        /// Marks all objects of the specified type as "Need Destroy".
        /// </summary>
        public void DestroyByType(Type type)
        {
            if (objectPools.ContainsKey(type))
            {
                foreach (var o in objectPools[type])
                {
                    o.Destroy();
                }
            }
        }
        /// <summary>
        /// Gets the total number of objects of the specified type.
        /// </summary>
        public int GetCount(Type type)
        {
            if (objectPools.ContainsKey(type))
            {
                return objectPools[type].Count;
            }
            return 0;
        }
        /// <summary>
        /// Gets a pool of objects of the specified type. 
        /// If the type is null, an object pool containing all objects is returned.
        /// </summary>
        public List<MapObject> GetPool(Type type)
        {
            if (type == null)
            {
                return Objects;
            }
            else if (!objectPools.ContainsKey(type))
            {
                objectPools[type] = new();
            }
            return objectPools[type];
        }
        public List<MapObject> GetTypeObjectsWithChildren(Type type)
        {
            if (type == null)
            {
                return Objects;
            }

            var result = new List<MapObject>();
            result.AddRange(GetPool(type));
            foreach (var child in MapObject.ChildTypes[type])
            {
                result.AddRange(GetPool(child));
            }

            return result;
        }
    }
}
