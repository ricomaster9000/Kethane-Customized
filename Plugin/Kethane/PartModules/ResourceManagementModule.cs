using System;
using System.Collections.Generic;
using System.Linq;
using GeodesicGrid;
using Kethane.UserInterface;
using UnityEngine;

namespace Kethane.PartModules
{
    public class ResourceManagementModule : PartModule
    {
        [KSPField(guiName = "Resource Management Status", isPersistant = false, guiActive = true)]
        public string ResourceManagementStatus = "Active";
        
        public override void OnLoad(ConfigNode config)
        {
            Debug.Log("ResourceManagementModule loaded");
            config = Misc.Parse(config.ToString()).GetNode("MODULE");
        }

        public override void OnStart(PartModule.StartState state)
        {
            if (state != StartState.Editor && FlightGlobals.fetch != null)
            {
                // Module is always active
                this.enabled = true;
                this.part.force_activate();
                Debug.Log("ResourceManagementModule - Activated");
            }
        }
        
        public override string GetInfo()
        {
            return String.Format("Manages Resource extraction and resource harvesters. Will disable resource harvesters if no valid or supported resource exists where mining is occuring");
        }


        public override void OnFixedUpdate()
        {
            var resourceHarvesters = part.FindModulesImplementing<ModuleResourceHarvester>();
            foreach (var harvester in resourceHarvesters)
            {
                Debug.Log("ResourceManagementModule - managing resource harvest");
                ManageResourceHarvester(harvester);
            }
        }

        private void ManageResourceHarvester(ModuleResourceHarvester harvester)
        {
            if (harvester == null || !harvester.isEnabled) return;
            
            // Logic to check resource availability and manage harvester
            // For example, deactivate the harvester if resource is not available
            CheckResourceAvailabilityAndDoMining(harvester);
        }

        private void CheckResourceAvailabilityAndDoMining(ModuleResourceHarvester harvester)
        {
            IBodyResources bodyResources = this.getBodyResources(harvester.ResourceName);
            if (bodyResources != null)
            {
                Cell cellUnder = this.getCellUnder();
                double? quantity = bodyResources.GetQuantity(cellUnder);
                if (quantity != null)
                {
                    Debug.Log("ResourceManagementModule - extracting resource " + harvester.ResourceName);
                    harvester.EnableModule();
                    //double num3 = (double)(TimeWarp.fixedDeltaTime * harvester._resFlow);
                    //num3 = Math.Min(harvester._resFlow, quantity.Value);
                    bodyResources.Extract(cellUnder, -base.part.RequestResource(harvester.ResourceName, -Math.Min(harvester._resFlow, quantity.Value)));
                } else {
                    harvester.DisableModule();
                }
            } else {
                harvester.DisableModule();
            }
        }
        
        private Cell getCellUnder()
        {
            return MapOverlay.GetCellUnder(base.vessel.mainBody, base.vessel.transform.position);
        }

        // Token: 0x06000150 RID: 336 RVA: 0x0000905A File Offset: 0x0000725A
        private IBodyResources getBodyResources(string resourceName)
        {
            if (KethaneData.Current == null)
            {
                return null;
            }
            return KethaneData.Current[resourceName][base.vessel.mainBody].Resources;
        }
    }
}
