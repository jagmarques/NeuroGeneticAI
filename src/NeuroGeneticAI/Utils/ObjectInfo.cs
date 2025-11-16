using System;
using UnityEngine;

namespace NeuroGeneticAI.Utils
{
    
    // Lightweight data transfer object describing a sensed element.  Comparisons are
    // based on the distance to guarantee deterministic ordering.
    
    public class ObjectInfo : IEquatable<ObjectInfo>, IComparable<ObjectInfo>
    {
        // Distance to the object expressed in Unity units.
        public float distance { get; }

        // Angle relative to the sensor.
        public float angle { get; }

        // Position stored as a 2D vector.
        public Vector2 pos { get; }

        // Initializes a new instance of the  class.
        public ObjectInfo(float distance, float angle, Vector2 pos)
        {
            this.distance = distance;
            this.angle = angle;
            this.pos = pos;
        }

        
        public bool Equals(ObjectInfo other)
        {
            if (other == null)
            {
                return false;
            }

            return Math.Abs(distance - other.distance) < float.Epsilon &&
                   Math.Abs(angle - other.angle) < float.Epsilon;
        }

        
        public int CompareTo(ObjectInfo other)
        {
            if (other == null)
            {
                return 1;
            }

            if (distance < other.distance)
            {
                return 1;
            }

            if (Math.Abs(distance - other.distance) < float.Epsilon)
            {
                return 0;
            }

            return -1;
        }
    }
}
