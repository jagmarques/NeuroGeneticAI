using System.Collections.Generic;
using NeuroGeneticAI.Infrastructure.Diagnostics;
using NeuroGeneticAI.Utils;
using UnityEngine;

namespace NeuroGeneticAI.Sensors
{
    
    // Simple radial detector that collects objects around the robot and stores the
    // closest instance per tag.  The returned map is later consumed by the neural
    // controller.
    
    public class DetectorScript : MonoBehaviour
    {
        private readonly ILogger _logger = LogManager.Logger;

        protected Vector3 initialTransformUp;
        protected Vector3 initialTransformFwd;

        // Indicates whether debugging output should be produced.
        public bool debug_mode;

        // Tag assigned to the adversary robot.
        public string AdversaryTag = string.Empty;

        // Tag assigned to the opponent goal.
        public string AdversaryGoal = string.Empty;

        // Tag assigned to the friendly goal.
        public string MyGoal = string.Empty;

        // Map of known objects keyed by friendly identifier.
        public Dictionary<string, ObjectInfo> objectsInformation = new();

        private void Start()
        {
            initialTransformUp = transform.up;
            initialTransformFwd = transform.forward;
        }

        
        // Scans the environment and reports the closest object for every tag of
        // interest.
        
        // Returns: Dictionary of object descriptors.
        public Dictionary<string, ObjectInfo> GetVisibleObjects()
        {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, 80);
            objectsInformation = new Dictionary<string, ObjectInfo>();

            foreach (Collider col in hitColliders)
            {
                Vector2 sensorPos = new Vector2(transform.position.x, transform.position.z);
                Vector3 temp = col.ClosestPointOnBounds(transform.position);
                Vector2 objectPos = new Vector2(temp.x, temp.z);
                Vector2 objectLocalPos = new Vector2(col.gameObject.transform.localPosition.x, col.gameObject.transform.localPosition.z);
                Vector2 dir = sensorPos - objectPos;
                dir = transform.InverseTransformDirection(dir);
                float angle = Mathf.Round(Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + 180);
                float dist = Mathf.Round(Vector2.Distance(sensorPos, objectPos));

                ObjectInfo info = new ObjectInfo(dist, angle, objectLocalPos);
                RegisterObject(col, info);
            }

            if (!objectsInformation.ContainsKey("DistanceToBall"))
            {
                _logger.LogWarning("Ball was not detected by the sensors.");
            }

            return objectsInformation;
        }

        private void RegisterObject(Collider collider, ObjectInfo info)
        {
            if (collider.tag.Equals(AdversaryGoal))
            {
                StoreClosest("AdversaryGoal", info);
            }
            else if (collider.tag.Equals(MyGoal))
            {
                StoreClosest("MyGoal", info);
            }
            else if (collider.tag.Equals("ball"))
            {
                StoreClosest("DistanceToBall", info);
            }
            else if (collider.tag.Equals("Wall"))
            {
                StoreClosest("Wall", info);
            }
            else if (collider.tag.Equals(AdversaryTag))
            {
                StoreClosest("Adversary", info);
            }
        }

        private void StoreClosest(string key, ObjectInfo info)
        {
            if (!objectsInformation.ContainsKey(key) || info.distance < objectsInformation[key].distance)
            {
                objectsInformation[key] = info;
            }
        }
    }
}
