using System;
using System.Collections.Generic;
using GeodesicGrid;
using Kethane.UserInterface;
using UnityEngine;

namespace Kethane.PartModules
{
    public class ResourceManagementModule : PartModule
    {
        [KSPField(guiName = "Resource Management Status", isPersistant = false, guiActive = true)]
        public string resourceManagementStatus = "Active";

        public bool isActive;

        public List<String> supportedResources = new List<string>();
        public Dictionary<String, float> ResourceNameToEfficiency = new Dictionary<string, float>();
        public bool setupDoneAtLeastOnce = false;

        
        public override void OnLoad(ConfigNode config)
        {
            Debug.Log("ResourceManagementModule loaded");
            config = Misc.Parse(config.ToString()).GetNode("MODULE");
            doSetup();
        }
        
        public override void OnStart(PartModule.StartState state)
        {
            if (state != StartState.Editor)
            {
                // Module is always active
                this.enabled = true;
                this.isActive = false;
                this.part.force_activate();
                doSetup();
                Debug.Log("ResourceManagementModule - Activated");
            }
        }
        
        public override void OnInitialize()
        {
            doSetup();
        }

        private void doSetup()
        {
            var resourceHarvesters = part.FindModulesImplementing<ModuleResourceHarvester>();
            supportedResources.Clear();
            foreach (var harvester in resourceHarvesters)
            {
                supportedResources.Add(harvester.ResourceName);
                ResourceNameToEfficiency[harvester.ResourceName] = harvester.Efficiency;
            }
            setupDoneAtLeastOnce = true;
            Debug.Log("ResourceManagementModule - Setup complete");
        }

        public override string GetInfo()
        {
            return String.Format("Manages Resource extraction and resource harvesters. Will disable resource harvesters if no valid or supported resource exists where mining is occuring");
        }

        public override void OnFixedUpdate()
        {
            if (!setupDoneAtLeastOnce)
            {
                doSetup();
            }
            var resourceHarvesters = part.FindModulesImplementing<ModuleResourceHarvester>();
            bool isAnyModuleActive = false;
            
            foreach (var harvester in resourceHarvesters)
            {
                if (harvester != null &&
                    harvester.IsActivated)
                {
                    isAnyModuleActive = true;
                }
            }
            
            foreach (var harvester in resourceHarvesters)
            {
                if (harvester != null &&
                    harvester.isEnabled &&
                    harvester.hasEInput &&
                    harvester.IsSituationValid() &&
                    harvester.CheckForImpact() &&
                    isAnyModuleActive)
                {
                    checkAndDoMining(harvester);
                }
            }
        }

        private void checkAndDoMining(ModuleResourceHarvester harvester)
        {
            IBodyResources bodyResources = this.getBodyResources(harvester.ResourceName);
            if (bodyResources != null)
            {
                Cell cellUnder = this.getCellUnder();
                double? quantity = bodyResources.GetQuantity(cellUnder);
                if (quantity != null)
                {
                    double harvestRate = getHarvestRate(harvester.ResourceName);
                    bodyResources.Extract(cellUnder, -part.RequestResource(harvester.ResourceName, -Math.Min(harvestRate, quantity.Value)));
                }
            }
        }

        private Cell getCellUnder()
        {
            return MapOverlay.GetCellUnder(base.vessel.mainBody, base.vessel.transform.position);
        }

        // Token: 0x06000150 RID: 336 RVA: 0x0000905A File Offset: 0x0000725A
        private IBodyResources getBodyResources(string resourceName)
        {
            if (KethaneData.Current == null || !KethaneData.Current.resourceExists(resourceName))
            {
                return null;
            }
            return KethaneData.Current[resourceName][base.vessel.mainBody].Resources;
        }

        private double getHarvestRate(string resourceName)
        {
            return TimeWarp.fixedDeltaTime * (ResourceNameToEfficiency[resourceName] * 0.10);
        }
    }
}
