namespace Subnautica.Server.Processors.Vehicle
{
    using System;
    using System.Collections.Generic;

    using Server.Core;

    using Subnautica.API.Extensions;
    using Subnautica.Network.Models.Core;
    using Subnautica.Network.Models.WorldEntity.DynamicEntityComponents.Shared;
    using Subnautica.Server.Abstracts.Processors;

    using ServerModel      = Subnautica.Network.Models.Server;
    using WorldEntityModel = Subnautica.Network.Models.WorldEntity.DynamicEntityComponents;

    public class UpgradeConsoleProcessor : NormalProcessor
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
            var packet = networkPacket.GetPacket<ServerModel.VehicleUpgradeConsoleArgs>();
            if (packet == null)
            {
                return this.SendEmptyPacketErrorLog(networkPacket);
            }

            if (packet.IsOpening)
            {
                if (!Server.Instance.Logices.Interact.IsBlocked(packet.UniqueId))
                {
                    Server.Instance.Logices.Interact.AddBlock(profile.UniqueId, packet.UniqueId, true);

                    profile.SendPacket(packet);
                }
            }
            else
            {
                if (packet.ItemId.IsNull())
                {
                    return false;
                }

                var entity = Server.Instance.Storages.World.GetVehicle(packet.UniqueId);
                if (entity == null)
                {
                    return false;
                }

                var isSendPacket = false;
                var slotId       = this.GetSlotNumber(packet.SlotId);

                switch (entity.TechType)
                {
                    case TechType.Hoverbike:

                        var hoverbikeComp = entity.Component.GetComponent<WorldEntityModel.Hoverbike>();
                        if (packet.IsAdding)
                        {
                            isSendPacket = this.AddModule(hoverbikeComp.Modules, slotId, packet.ModuleType, packet.ItemId, packet.SlotId);
                        }
                        else
                        {
                            isSendPacket = this.RemoveModule(hoverbikeComp.Modules, slotId, packet.SlotId);
                        }

                        break;
                    case TechType.Exosuit:

                        var exosuitComp = entity.Component.GetComponent<WorldEntityModel.Exosuit>();
                        if (packet.IsAdding)
                        {
                            isSendPacket = this.AddModule(exosuitComp.Modules, slotId, packet.ModuleType, packet.ItemId, packet.SlotId);
                        }
                        else
                        {
                            isSendPacket = this.RemoveModule(exosuitComp.Modules, slotId, packet.SlotId);
                        }

                        break;
                    case TechType.SeaTruck:

                        var seaTruckComp = entity.Component.GetComponent<WorldEntityModel.SeaTruck>();
                        if (packet.IsAdding)
                        {
                            isSendPacket = this.AddModule(seaTruckComp.Modules, slotId, packet.ModuleType, packet.ItemId, packet.SlotId);
                        }
                        else
                        {
                            isSendPacket = this.RemoveModule(seaTruckComp.Modules, slotId, packet.SlotId);
                        }

                        break;
                }


                if (isSendPacket)
                {
                    profile.SendPacketToOtherClients(packet);
                }
            }

            return true;
        }
        
        /**
         *
         * Modülü ekler.
         *
         * @author Ismail <ismaiil_0234@hotmail.com>
         *
         */
        private bool AddModule(List<UpgradeConsoleItem> modules, int slotId, TechType moduleType, string itemId, string slotName = null)
        {
            if (!string.IsNullOrEmpty(slotName))
            {
                var existing = modules.Find(m => m.SlotId == slotName);
                if (existing != null)
                {
                    existing.ModuleType = moduleType;
                    existing.ItemId     = itemId;
                    return true;
                }
            }

            if (slotId < 0)
            {
                modules.Add(new UpgradeConsoleItem() { SlotId = slotName, ModuleType = moduleType, ItemId = itemId });
                return true;
            }

            while (modules.Count <= slotId)
            {
                modules.Add(new UpgradeConsoleItem());
            }

            if (modules[slotId].ModuleType == TechType.None || modules[slotId].SlotId == slotName)
            {
                modules[slotId].SlotId     = slotName;
                modules[slotId].ModuleType = moduleType;
                modules[slotId].ItemId     = itemId;
                
                return true;
            }

            modules.Add(new UpgradeConsoleItem() { SlotId = slotName, ModuleType = moduleType, ItemId = itemId });
            return true;
        }
        
        /**
         *
         * Modülü kaldırır.
         *
         * @author Ismail <ismaiil_0234@hotmail.com>
         *
         */
        private bool RemoveModule(List<UpgradeConsoleItem> modules, int slotId, string slotName = null)
        {
            if (!string.IsNullOrEmpty(slotName))
            {
                var existing = modules.Find(m => m.SlotId == slotName);
                if (existing != null)
                {
                    existing.ModuleType = TechType.None;
                    existing.ItemId     = null;
                    return true;
                }
            }

            if (slotId >= 0 && slotId < modules.Count)
            {
                if (modules[slotId].ModuleType != TechType.None)
                {
                    modules[slotId].ModuleType = TechType.None;
                    modules[slotId].ItemId     = null;
                    
                    return true;
                }
            }

            return false;
        }

        /**
         *
         * Slot numarasını döner.
         *
         * @author Ismail <ismaiil_0234@hotmail.com>
         *
         */
        private int GetSlotNumber(string slotId)
        {
            if (string.IsNullOrEmpty(slotId))
            {
                return -1;
            }

            if (slotId == "ExosuitArmLeft")
            {
                return 4;
            }

            if (slotId == "ExosuitArmRight")
            {
                return 5;
            }

            if (slotId == "SeaTruckArmLeft")
            {
                return 12;
            }

            if (slotId == "SeaTruckArmRight")
            {
                return 13;
            }

            var cleaned = slotId.Replace("ExosuitModule", "").Replace("HoverbikeModule", "").Replace("SeaTruckModule", "");
            if (int.TryParse(cleaned, out var num))
            {
                return num - 1;
            }

            return -1;
        }
    }
}
