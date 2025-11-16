using System.Collections.Generic;
using NeuroGeneticAI.Infrastructure.Diagnostics;
using NeuroGeneticAI.Controllers;
using UnityEngine;

namespace NeuroGeneticAI.Utils
{
    
    // Utility MonoBehaviour that lets the user cycle through different simulations by
    // clicking the mouse.
    
    public class CameraController : MonoBehaviour
    {
        private readonly ILogger _logger = LogManager.Logger;
        private Vector3 target;

        // Simulations currently visible on screen.
        public List<SimulationInfo> simsInfo = new();

        private int viewIndex;

        private void Start()
        {
            target = Vector3.zero;
            viewIndex = 0;
        }

        private void Update()
        {
            SwitchTarget();
        }

        private void LateUpdate()
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, target.z);
        }

        private void SwitchTarget()
        {
            if (!Input.GetKeyDown(KeyCode.Mouse0) || simsInfo == null || simsInfo.Count == 0)
            {
                return;
            }

            target = simsInfo[viewIndex].sim.transform.position;
            _logger.LogInformation(
                $"Camera locked to Blue #{simsInfo[viewIndex].individualIndexBlue} vs Red #{simsInfo[viewIndex].individualIndexRed}");
            viewIndex++;
            if (viewIndex == simsInfo.Count)
            {
                viewIndex = 0;
            }
        }
    }
}
