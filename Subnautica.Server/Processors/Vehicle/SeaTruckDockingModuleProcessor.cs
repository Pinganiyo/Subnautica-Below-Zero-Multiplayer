namespace Subnautica.Server.Processors.Vehicle
{
    using Server.Core;

    using Subnautica.Network.Models.Core;
    using Subnautica.Network.Models.Storage.World.Childrens;
    using Subnautica.Server.Abstracts.Processors;
    using Subnautica.Server.Extensions;

    using ServerModel      = Subnautica.Network.Models.Server;
    using WorldEntityModel = Subnautica.Network.Models.WorldEntity.DynamicEntityComponents;

    public class SeaTruckDockingModuleProcessor : NormalProcessor
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
            var packet = networkPacket.GetPacket<ServerModel.SeaTruckDockingModuleArgs>();
            if (packet == null)
            {
                return this.SendEmptyPacketErrorLog(networkPacket);
            }

            var dockingModule = Server.Instance.Storages.World.GetDynamicEntity(packet.UniqueId);
            if (dockingModule == null || dockingModule.TechType != TechType.SeaTruckDockingModule)
            {
                API.Features.Log.Info($"SeaTruckDockingModule.Server: dock rejected. bay={packet.UniqueId} found={dockingModule != null}");
                return false;
            }

            var component = dockingModule.Component?.GetComponent<WorldEntityModel.SeaTruckDockingModule>();
            if (component == null)
            {
                // Repair: bay entity exists but carries no (or a wrong) docking component.
                API.Features.Log.Info($"SeaTruckDockingModule.Server: repairing docking component. bay={packet.UniqueId}");
                dockingModule.SetComponent(new WorldEntityModel.SeaTruckDockingModule().Initialize(null));
                component = dockingModule.Component.GetComponent<WorldEntityModel.SeaTruckDockingModule>();
            }

            if (component == null)
            {
                API.Features.Log.Info($"SeaTruckDockingModule.Server: dock rejected. bay={packet.UniqueId} has no docking component.");
                return false;
            }

            if (packet.IsDocking)
            {
                var exosuitModule = Server.Instance.Storages.World.GetDynamicEntity(packet.VehicleId);
                if (exosuitModule == null)
                {
                    // Repair: prawn exists physically but was never registered (e.g. after id churn).
                    API.Features.Log.Info($"SeaTruckDockingModule.Server: registering missing exosuit. vehicle={packet.VehicleId}");
                    exosuitModule = Server.Instance.Logices.World.CreateDynamicEntity(packet.VehicleId, TechType.Exosuit, profile.Position, profile.Rotation, profile.UniqueId);
                    if (exosuitModule != null)
                    {
                        exosuitModule.SetComponent(new WorldEntityModel.Exosuit().Initialize(null));
                    }
                }

                if (exosuitModule == null || exosuitModule.TechType != TechType.Exosuit)
                {
                    API.Features.Log.Info($"SeaTruckDockingModule.Server: dock rejected. vehicle={packet.VehicleId} found={exosuitModule != null}");
                    return false;
                }

                if (component.IsDocked() && !this.IsDockedVehicleInUse(component))
                {
                    // Stale dock state (e.g. after id churn or a missed undock): reset it so docking can proceed.
                    API.Features.Log.Info($"SeaTruckDockingModule.Server: clearing stale dock on bay={packet.UniqueId}");
                    component.Undock(out _);
                }

                if (component.Dock(exosuitModule))
                {
                    if (Server.Instance.Storages.World.RemoveDynamicEntity(exosuitModule.UniqueId))
                    {
                        if (exosuitModule.IsUsingByPlayer)
                        {
                            var player = Server.Instance.GetPlayer(exosuitModule.OwnershipId);
                            if (player != null)
                            {
                                player.SetInterior(null);
                                player.SetVehicle(null);

                                Server.Instance.Logices.Interact.RemoveBlockByPlayerId(player.UniqueId);
                            }
                        }

                        profile.SendPacketToAllClient(packet);
                    }
                }
            }
            else
            {
                if (Server.Instance.Logices.Interact.IsBlocked(packet.VehicleId))
                {
                    return false;
                }

                if (component.Undock(out var exosuit))
                {
                    exosuit.SetPositionAndRotation(packet.UndockPosition, packet.UndockRotation);

                    this.UndockExosuit(exosuit, profile, packet.IsEnterUndock);

                    packet.Vehicle = exosuit;

                    profile.SendPacketToAllClient(packet);
                }
            }

            return true;
        }

        /**
         *
         * Kenetli aracın gerçekten kullanımda olup olmadığını döner.
         * Kayıp kimlikler ve kullanım-dışı araçlar eski kayıt sayılır.
         *
         */
        private bool IsDockedVehicleInUse(WorldEntityModel.SeaTruckDockingModule component)
        {
            var dockedVehicle = component.Vehicle;
            if (dockedVehicle == null)
            {
                return false;
            }

            if (Server.Instance.Storages.World.GetDynamicEntity(dockedVehicle.UniqueId) == null)
            {
                return false;
            }

            if (dockedVehicle.IsUsingByPlayer)
            {
                return true;
            }

            return Server.Instance.Logices.Interact.IsBlocked(dockedVehicle.UniqueId);
        }

        /**
         *
         * Demirlemeyi çözer.
         *
         * @author Ismail <ismaiil_0234@hotmail.com>
         *
         */
        private void UndockExosuit(WorldDynamicEntity vehicle, AuthorizationProfile profile, bool isEnterUndock)
        {
            vehicle.RenewId();

            if (isEnterUndock)
            {
                vehicle.IsUsingByPlayer = true;

                profile.SetVehicle(vehicle.UniqueId);

                Server.Instance.Logices.Interact.AddBlock(profile.UniqueId, vehicle.UniqueId, true);
            }
            else
            {
                vehicle.IsUsingByPlayer = false;
            }

            Server.Instance.Logices.EntityWatcher.ChangeEntityOwnership(vehicle, profile.UniqueId);
            Server.Instance.Storages.World.AddWorldDynamicEntity(vehicle);
        }
    }
}
