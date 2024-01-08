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
        
        private List<Resource> resources;
        
        [KSPField(guiActive = true, guiActiveEditor = false, guiName = "Resource Management Status")]
        public string ResourceManagementStatus = "Inactive";
        
        public override void OnLoad(ConfigNode config)
        {
            config = Misc.Parse(config.ToString()).GetNode("MODULE");
            this.resources = (from n in config.GetNodes("Resource")
                select new Resource(n)).ToList<Resource>();
        }

        public override void OnStart(PartModule.StartState state)
        {
            if (state != StartState.Editor && FlightGlobals.fetch != null)
            {
                // Module is always active
                this.enabled = true;
                this.part.force_activate();
            }
        }

        public override void OnFixedUpdate()
        {
            var resourceHarvesters = part.FindModulesImplementing<ModuleResourceHarvester>();
            foreach (var harvester in resourceHarvesters)
            {
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
            foreach (Resource resource in this.resources)
            {
                if (resource.Name == harvester.ResourceName)
                {
                    Cell cellUnder = this.getCellUnder();
                    IBodyResources bodyResources = this.getBodyResources(resource.Name);
                    if (bodyResources != null)
                    {
                        double? quantity = bodyResources.GetQuantity(cellUnder);
                        if (quantity != null)
                        {
                            harvester.EnableModule();
                            //double num3 = (double)(TimeWarp.fixedDeltaTime * harvester._resFlow);
                            //num3 = Math.Min(harvester._resFlow, quantity.Value);
                            bodyResources.Extract(cellUnder, -base.part.RequestResource(resource.Name, -Math.Min(harvester._resFlow, quantity.Value)));
                        } else {
                            harvester.DisableModule();
                        }
                    } else {
                        harvester.DisableModule();
                    }
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
            if (KethaneData.Current == null)
            {
                return null;
            }
            return KethaneData.Current[resourceName][base.vessel.mainBody].Resources;
        }
        
        public class Resource
        {
            public string Name { get; private set; }

            public Resource(ConfigNode node)
            {
                this.Name = node.GetValue("Name");
            }
        }
        
    }
}
