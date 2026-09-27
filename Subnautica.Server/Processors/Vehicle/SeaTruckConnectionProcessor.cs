namespace Subnautica.Server.Processors.Vehicle
{
    using System.Linq;

    using Server.Core;

    using Subnautica.API.Extensions;
    using Subnautica.Network.Core.Components;
    using Subnautica.Network.Models.Core;
    using Subnautica.Server.Abstracts.Processors;

    using ServerModel      = Subnautica.Network.Models.Server;
    using MetadataModel    = Subnautica.Network.Models.Metadata;
    using WorldEntityModel = Subnautica.Network.Models.WorldEntity.DynamicEntityComponents;
    using Subnautica.API.Features;

    public class SeaTruckConnectionProcessor : NormalProcessor
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
            var packet = networkPacket.GetPacket<ServerModel.SeaTruckConnectionArgs>();
            if (packet == null)
            {
                return this.SendEmptyPacketErrorLog(networkPacket);
            }

            if (packet.IsEject)
            {
                if (Server.Instance.Logices.Interact.IsBlocked(packet.FrontModuleId))
                {
                    return false;
                }

                packet.BackModuleId = Server.Instance.Storages.World.RemoveSeaTruckConnection(packet.FrontModuleId);

                if (packet.BackModuleId.IsNotNull())
                {
                    var backModule = Core.Server.Instance.Storages.World.GetDynamicEntity(packet.BackModuleId);
                    if (backModule != null)
                    {
                        packet.ModuleId = backModule.Id;
                        packet.Position = backModule.Position;
                        packet.Rotation = backModule.Rotation;
                    }

                    profile.SendPacketToAllClient(packet);
                }
            }
            else if (packet.IsMoonpoolExpansion)
            {
                var construction = Server.Instance.Storages.Construction.GetConstruction(packet.FrontModuleId);
                if (construction == null)
                {
                    return false;
                }

                var component = construction.EnsureComponent<MetadataModel.BaseMoonpool>();
                if (component == null)
                {
                    return false;
                }

                if (packet.IsConnect)
                {
                    this.EnsureModuleEntity(packet, packet.BackModuleId, profile.UniqueId);

                    if (!component.ExpansionManager.IsTailDocked() && Server.Instance.Storages.World.AddSeaTruckConnection(packet.BackModuleId, packet.FirstModuleId, false))
                    {
                        component.ExpansionManager.DockTail(packet.BackModuleId);

                        profile.SendPacketToAllClient(packet);
                    }
                }
                else
                {
                    if (component.ExpansionManager.IsTailDocked())
                    {
                        Server.Instance.Storages.World.RemoveSeaTruckConnection(packet.FirstModuleId, false);

                        component.ExpansionManager.UndockTail();

                        profile.SendPacketToAllClient(packet);
                    }
                }
            }
            else if (packet.IsConnect)
            {
                this.EnsureModuleEntity(packet, packet.FrontModuleId, profile.UniqueId);

                if (Server.Instance.Storages.World.AddSeaTruckConnection(packet.FrontModuleId, packet.BackModuleId))
                {
                    profile.SendPacketToAllClient(packet);
                }
            }
            else
            {
                packet.BackModuleId = Server.Instance.Storages.World.RemoveSeaTruckConnection(packet.FrontModuleId);

                if (packet.BackModuleId.IsNotNull())
                {
                    profile.SendPacketToAllClient(packet);
                }
            }

            return true;
        }

        /**
         *
         * Bağlanan modül sunucuda bilinmiyorsa üretir.
         * Üretim sırasında üretilen modüller ancak böyle kaydedilir.
         *
         */
        private void EnsureModuleEntity(ServerModel.SeaTruckConnectionArgs packet, string moduleId, string ownershipId)
        {
            if (moduleId.IsNull() || !packet.ModuleTechType.IsSeaTruckModule() || packet.Position == null)
            {
                return;
            }

            if (Server.Instance.Storages.World.GetDynamicEntity(moduleId) != null)
            {
                return;
            }

            var entity = Server.Instance.Logices.World.CreateDynamicEntity(moduleId, packet.ModuleTechType, packet.Position, packet.Rotation, ownershipId);
            if (entity == null)
            {
                return;
            }

            entity.SetComponent(this.GetModuleComponent(packet.ModuleTechType));
        }

        /**
         *
         * Modül bileşenini döner.
         *
         */
        private NetworkDynamicEntityComponent GetModuleComponent(TechType techType)
        {
            switch (techType)
            {
                case TechType.SeaTruckFabricatorModule:    return new WorldEntityModel.SeaTruckFabricatorModule().Initialize(this.OnModuleComponentInitialized);
                case TechType.SeaTruckStorageModule:       return new WorldEntityModel.SeaTruckStorageModule().Initialize(this.OnModuleComponentInitialized);
                case TechType.SeaTruckAquariumModule:      return new WorldEntityModel.SeaTruckAquariumModule().Initialize(this.OnModuleComponentInitialized);
                case TechType.SeaTruckDockingModule:       return new WorldEntityModel.SeaTruckDockingModule().Initialize(this.OnModuleComponentInitialized);
                case TechType.SeaTruckSleeperModule:       return new WorldEntityModel.SeaTruckSleeperModule().Initialize(this.OnModuleComponentInitialized);
                case TechType.SeaTruckTeleportationModule: return new WorldEntityModel.SeaTruckTeleportationModule().Initialize(this.OnModuleComponentInitialized);
            }

            return null;
        }

        /**
         *
         * Modül bileşen sınıfı oluşturulduğunda tetiklenir.
         *
         */
        public void OnModuleComponentInitialized(NetworkDynamicEntityComponent entityComponent)
        {
            if (entityComponent is WorldEntityModel.SeaTruckFabricatorModule)
            {
                var component = entityComponent.GetComponent<WorldEntityModel.SeaTruckFabricatorModule>();
                if (component != null)
                {
                    Server.Instance.Storages.Construction.AddConstructionItem(new Subnautica.Network.Models.Storage.Construction.ConstructionItem()
                    {
                        IsStatic = true,
                        UniqueId = component.FabricatorUniqueId,
                        TechType = TechType.Fabricator,
                        ConstructedAmount = 1f,
                    });
                }
            }
        }
    }
}
