namespace Subnautica.Server.Processors.Vehicle
{
    using System.Linq;

    using Server.Core;

    using Subnautica.API.Features;
    using Subnautica.Network.Models.Core;
    using Subnautica.Network.Models.WorldEntity.DynamicEntityComponents.Shared;
    using Subnautica.Server.Abstracts.Processors;

    using ServerModel      = Subnautica.Network.Models.Server;
    using WorldEntityModel = Subnautica.Network.Models.WorldEntity.DynamicEntityComponents;

    public class BatteryProcessor : NormalProcessor
    {
        /**
         *
         * Gelen veriyi işler
         *
         * @author Ismail <ismaiil_0234@hotmail.com>
         *
         */
        public override bool OnExecute(AuthorizationProfile profile, NetworkPacket networkPacket)
        {
            var packet = networkPacket.GetPacket<ServerModel.VehicleBatteryArgs>();
            if (packet == null)
            {
                return this.SendEmptyPacketErrorLog(networkPacket);
            }

            if (packet.IsOpening)
            {
                if (Server.Instance.Logices.Interact.IsBlocked(packet.BatterySlotId, profile.UniqueId))
                {
                    return false;
                }

                Server.Instance.Logices.Interact.AddBlock(profile.UniqueId, packet.BatterySlotId, true);

                profile.SendPacket(packet);
            }
            else
            {
                Server.Instance.Logices.Interact.RemoveBlockByConstruction(packet.BatterySlotId);
                
                var entity = Server.Instance.Storages.World.GetVehicle(packet.UniqueId);
                if (entity == null)
                {
                    Log.Error($"[VehicleBattery] Vehicle not found in world storage: {packet.UniqueId}");
                    return false;
                }

                entity.IsDeployed = true;

                PowerCell powerCell = null;
                System.Collections.Generic.List<PowerCell> powerCells = null;
                if (entity.TechType == TechType.SeaTruck)
                {
                    var component = entity.Component.GetComponent<WorldEntityModel.SeaTruck>();
                    if (component != null)
                    {
                        powerCells = component.PowerCells;
                    }
                }
                else if (entity.TechType == TechType.Exosuit)
                {
                    var component = entity.Component.GetComponent<WorldEntityModel.Exosuit>();
                    if (component != null)
                    {
                        powerCells = component.PowerCells;
                    }
                }

                if (powerCells == null)
                {
                    powerCells = new System.Collections.Generic.List<PowerCell>() { new PowerCell(), new PowerCell() };
                    if (entity.TechType == TechType.SeaTruck)
                    {
                        var component = entity.Component.GetComponent<WorldEntityModel.SeaTruck>();
                        if (component != null)
                        {
                            component.PowerCells = powerCells;
                        }
                    }
                    else if (entity.TechType == TechType.Exosuit)
                    {
                        var component = entity.Component.GetComponent<WorldEntityModel.Exosuit>();
                        if (component != null)
                        {
                            component.PowerCells = powerCells;
                        }
                    }
                }

                powerCell = powerCells.Where(q => q.UniqueId == packet.BatterySlotId).FirstOrDefault();
                if (powerCell == null)
                {
                    bool isSlot2 = !string.IsNullOrEmpty(packet.BatterySlotId) && (packet.BatterySlotId.Contains("PowerCell2") || packet.BatterySlotId.Contains("Right") || packet.BatterySlotId.Contains("Slot2"));
                    powerCell = isSlot2 ? powerCells.ElementAtOrDefault(1) : powerCells.ElementAtOrDefault(0);
                    if (powerCell == null)
                    {
                        powerCell = new PowerCell();
                        powerCells.Add(powerCell);
                    }

                    if (!string.IsNullOrEmpty(packet.BatterySlotId))
                    {
                        powerCell.UniqueId = packet.BatterySlotId;
                    }
                }

                if (packet.IsAdding)
                {
                    var type = packet.BatteryType == TechType.None ? TechType.PowerCell : packet.BatteryType;
                    powerCell.SetBatteryType(type);
                    powerCell.Charge = packet.Charge >= 0f ? packet.Charge : powerCell.Capacity;
                }
                else
                {
                    powerCell.Charge = -1f;
                }

                Log.Info($"[VehicleBattery] PowerCell updated on server: Vehicle={packet.UniqueId}, Slot={powerCell.UniqueId}, TechType={powerCell.TechType}, Charge={powerCell.Charge}, IsAdding={packet.IsAdding}");

                profile.SendPacketToOtherClients(packet);
            }

            return true;
        }
    }
}
