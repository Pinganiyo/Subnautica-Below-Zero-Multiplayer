namespace Subnautica.Client.Synchronizations.Processors.WorldEntities.DynamicEntities
{
    using System.Linq;

    using Subnautica.API.Features;
    using Subnautica.Client.Abstracts.Processors;
    using Subnautica.Network.Core.Components;

    using UnityEngine;

    using WorldEntityModel = Subnautica.Network.Models.WorldEntity.DynamicEntityComponents;

    public class ExosuitProcessor : WorldDynamicEntityProcessor
    {
        /**
         *
         * Dünya yüklenip nesne doğduğunda çalışır.
         *
         * @author Ismail <ismaiil_0234@hotmail.com>
         *
         */
        public override bool OnWorldLoadItemSpawn(NetworkDynamicEntityComponent packet, bool isDeployed, Pickupable pickupable, GameObject gameObject)
        {

            gameObject.SetActive(true);

            var component = packet.GetComponent<WorldEntityModel.Exosuit>();
            if (component == null)
            {
                return false;
            }

            var vehicle = gameObject.GetComponentInChildren<global::Vehicle>();
            if (vehicle == null)
            {
                return false;
            }

            var uniqueId = Network.Identifier.GetIdentityId(gameObject);
            if (string.IsNullOrEmpty(uniqueId))
            {
                uniqueId = Network.Identifier.GetIdentityId(vehicle.gameObject);
            }

            if (string.IsNullOrEmpty(uniqueId))
            {
                return false;
            }

            Network.Identifier.SetIdentityId(vehicle.gameObject, uniqueId);
            Network.Identifier.SetIdentityId(gameObject, uniqueId);

            vehicle.LazyInitialize();

            if (component.PowerCells == null || component.PowerCells.Count < 2)
            {
                component.PowerCells = new System.Collections.Generic.List<Subnautica.Network.Models.WorldEntity.DynamicEntityComponents.Shared.PowerCell> 
                { 
                    new Subnautica.Network.Models.WorldEntity.DynamicEntityComponents.Shared.PowerCell(), 
                    new Subnautica.Network.Models.WorldEntity.DynamicEntityComponents.Shared.PowerCell() 
                };
            }
            for (int i = 0; i < component.PowerCells.Count; i++)
            {
                var cell = component.PowerCells[i];
                if (cell == null)
                {
                    cell = component.PowerCells[i] = new Subnautica.Network.Models.WorldEntity.DynamicEntityComponents.Shared.PowerCell();
                }

                if (string.IsNullOrEmpty(cell.UniqueId))
                {
                    cell.UniqueId = $"{uniqueId}_PowerCell{i + 1}";
                }

                // JUST IN PRAWN SUITS: always have installed powercells
                if (cell.Charge == -1f || cell.TechType == TechType.None)
                {
                    cell.TechType = TechType.PowerCell;
                    cell.Capacity = 200f;
                    cell.Charge   = 200f;
                }
            }

            Vehicle.ApplyModules(component.Modules, vehicle.upgradesInput.equipment, TechType.Exosuit);
            Vehicle.ApplyBatterySlotIds(gameObject, TechType.Exosuit, component.PowerCells.ElementAt(0).UniqueId, component.PowerCells.ElementAt(1).UniqueId);
            Vehicle.ApplyPowerCells(uniqueId, component.PowerCells);
            Vehicle.EnsurePrawnSuitHasPowerCells(gameObject.GetComponent<global::Exosuit>() ?? vehicle as global::Exosuit);
            Vehicle.ApplyStorageContainer(uniqueId, component.StorageContainer);
            Vehicle.ApplyLiveMixin(vehicle.liveMixin, component.LiveMixin.Health);
            Vehicle.ApplyColorCustomizer(component.ColorCustomizer, vehicle.colorNameControl);
            return true;
        }
    }
}