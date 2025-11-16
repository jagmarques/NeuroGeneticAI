using System;
using System.Collections.Generic;

namespace UnityEngine
{
    public class Object
    {
        public static void Destroy(Object obj) { }
        public static void DestroyImmediate(Object obj) { }
        public static T Instantiate<T>(T original, Vector3 position, Quaternion rotation) => original;
        public static void DontDestroyOnLoad(Object target) { }
    }

    public class Component : Object
    {
        public bool enabled { get; set; } = true;
        public GameObject gameObject { get; } = new GameObject();
        public Transform transform { get; } = new Transform();
        public string tag { get; set; } = string.Empty;

        public T GetComponent<T>() => default;
        public T GetComponentInChildren<T>() => default;
    }

    public class MonoBehaviour : Component { }

    public class GameObject : Object
    {
        public Transform transform { get; } = new Transform();

        public T GetComponent<T>() => default;
        public T GetComponentInChildren<T>() => default;
        public Transform Find(string name) => new Transform();
        public string tag { get; set; } = string.Empty;
    }

    public class Transform
    {
        public Vector3 position { get; set; }
        public Vector3 localPosition { get; set; }
        public Vector3 up { get; set; } = Vector3.up;
        public Vector3 forward { get; set; } = Vector3.forward;

        public Transform Find(string name) => new Transform();
        public GameObject gameObject { get; } = new GameObject();

        public Vector3 InverseTransformDirection(Vector3 direction) => direction;
        public Vector3 InverseTransformDirection(Vector2 direction) => new Vector3(direction.x, direction.y, 0f);
    }

    public class Rigidbody
    {
        public Vector3 velocity { get; set; }
        public Vector3 angularVelocity { get; set; }

        public void AddForce(Vector3 force) { }
    }

    public class Camera : Component
    {
        public Rect rect { get; set; }
    }

    public class Collider : Component
    {
        public GameObject gameObject { get; } = new GameObject();

        public Vector3 ClosestPointOnBounds(Vector3 position) => position;
    }

    public class Collision
    {
        public Collider collider { get; set; } = new Collider();
    }

    public struct Rect
    {
        public float x;
        public float y;
        public float width;
        public float height;

        public Rect(float x, float y, float width, float height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }
    }

    public struct Vector3
    {
        public float x;
        public float y;
        public float z;

        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public float magnitude => (float)Math.Sqrt(x * x + y * y + z * z);

        public Vector3 normalized
        {
            get
            {
                var mag = magnitude;
                return mag > 0 ? new Vector3(x / mag, y / mag, z / mag) : new Vector3(0, 0, 0);
            }
        }

        public static Vector3 operator +(Vector3 a, Vector3 b) => new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
        public static Vector3 operator -(Vector3 a, Vector3 b) => new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
        public static Vector3 operator *(Vector3 a, float d) => new Vector3(a.x * d, a.y * d, a.z * d);
        public static Vector3 operator *(float d, Vector3 a) => new Vector3(a.x * d, a.y * d, a.z * d);

        public static Vector3 zero => new Vector3(0, 0, 0);
        public static Vector3 up => new Vector3(0, 1, 0);
        public static Vector3 forward => new Vector3(0, 0, 1);

        public static float Distance(Vector3 a, Vector3 b)
        {
            return (a - b).magnitude;
        }
    }

    public struct Vector2
    {
        public float x;
        public float y;

        public Vector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public float magnitude => (float)Math.Sqrt((x * x) + (y * y));

        public Vector2 normalized
        {
            get
            {
                var mag = magnitude;
                return mag > 0 ? new Vector2(x / mag, y / mag) : new Vector2(0, 0);
            }
        }

        public static Vector2 operator -(Vector2 a, Vector2 b) => new Vector2(a.x - b.x, a.y - b.y);

        public static Vector2 right => new Vector2(1, 0);

        public static float Distance(Vector2 a, Vector2 b)
        {
            var dx = a.x - b.x;
            var dy = a.y - b.y;
            return (float)Math.Sqrt((dx * dx) + (dy * dy));
        }

        public static float Angle(Vector2 from, Vector2 to)
        {
            var dot = (from.x * to.x) + (from.y * to.y);
            var mag = from.magnitude * to.magnitude;
            if (mag == 0)
            {
                return 0f;
            }

            var cos = Math.Clamp(dot / mag, -1f, 1f);
            return (float)(Math.Acos(cos) * Mathf.Rad2Deg);
        }
    }

    public struct Quaternion
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public Quaternion(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public static Quaternion identity => new Quaternion(0, 0, 0, 1);

        public static Quaternion AngleAxis(float angle, Vector3 axis) => identity;
        public static Vector3 operator *(Quaternion rotation, Vector3 point) => point;
    }

    public static class Mathf
    {
        public static int Min(int a, int b) => Math.Min(a, b);
        public static float Sqrt(float f) => (float)Math.Sqrt(f);
        public static int CeilToInt(float f) => (int)Math.Ceiling(f);
        public static float Round(float f) => (float)Math.Round(f);
        public static float Atan2(float y, float x) => (float)Math.Atan2(y, x);
        public static float Rad2Deg => 57.29578f;
        public static float Log(float f) => (float)Math.Log(f);
        public static float Exp(float f) => (float)Math.Exp(f);
        public static float Max(float a, float b) => Math.Max(a, b);
    }

    public static class Time
    {
        public static float deltaTime { get; set; } = 0.02f;
        public static float time { get; set; }
        public static float timeScale { get; set; } = 1f;
    }

    public static class Application
    {
        public static int targetFrameRate { get; set; }
    }

    public static class QualitySettings
    {
        public static int vSyncCount { get; set; }
    }

    public static class Debug
    {
        public static void Log(string message) => Console.WriteLine(message);
        public static void DrawRay(Vector3 start, Vector3 dir, Color color) { }
        public static void LogError(string message) => Console.Error.WriteLine(message);
    }

    public struct Color
    {
        public float r;
        public float g;
        public float b;
        public float a;

        public Color(float r, float g, float b, float a = 1f)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }

        public static Color black => new Color(0, 0, 0);
    }

    public static class Random
    {
        private static System.Random _random = new System.Random();

        public static void InitState(int seed)
        {
            _random = new System.Random(seed);
        }

        public static int Range(int min, int max) => _random.Next(min, max);
        public static float Range(float min, float max) => (float)(_random.NextDouble() * (max - min) + min);
    }

    public static class Physics
    {
        public static Collider[] OverlapSphere(Vector3 position, float radius) => Array.Empty<Collider>();
    }

    public static class Input
    {
        public static bool GetKeyDown(KeyCode keyCode) => false;
    }

    public enum KeyCode
    {
        Mouse0
    }

    public class Text
    {
        public string text { get; set; } = string.Empty;
    }

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class HideInInspector : Attribute { }

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class HeaderAttribute : Attribute
    {
        public HeaderAttribute(string header) { }
    }
}

namespace UnityEngine.UI
{
    public class Text
    {
        public string text { get; set; } = string.Empty;
    }
}
