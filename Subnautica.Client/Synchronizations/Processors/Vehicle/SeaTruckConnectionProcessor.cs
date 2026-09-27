namespace Subnautica.Client.Synchronizations.Processors.Vehicle
{
    using Subnautica.API.Enums;
    using Subnautica.API.Extensions;
    using Subnautica.API.Features;
    using Subnautica.API.Features.Helper;
    using Subnautica.Client.Abstracts;
    using Subnautica.Client.Core;
    using Subnautica.Client.Extensions;
    using Subnautica.Client.MonoBehaviours.World;
    using Subnautica.Events.EventArgs;
    using Subnautica.Network.Models.Core;
    using Subnautica.Network.Models.Storage.World.Childrens;
    using Subnautica.Network.Structures;

    using ServerModel = Subnautica.Network.Models.Server;

    public class SeaTruckConnectionProcessor : NormalProcessor
    {
        /**
         *
         * Gelen veriyi işler
         *
         * @author Ismail <ismaiil_0234@hotmail.com>
         *
         */
        public override bool OnDataReceived(NetworkPacket networkPacket)
        {
            var packet = networkPacket.GetPacket<ServerModel.SeaTruckConnectionArgs>();
            
            var action = new ItemQueueAction();
            action.RegisterProperty("FrontModuleId", packet.FrontModuleId);
            action.RegisterProperty("BackModuleId" , packet.BackModuleId);
            action.RegisterProperty("FirstModuleId", packet.FirstModuleId);
            action.RegisterProperty("IsConnect"    , packet.IsConnect);
            action.RegisterProperty("IsEject"      , packet.IsEject);
            action.RegisterProperty("PlayerId"     , packet.GetPacketOwnerId());
            action.RegisterProperty("ModuleId"       , packet.ModuleId);
            action.RegisterProperty("Position"       , packet.Position);
            action.RegisterProperty("Rotation"       , packet.Rotation);
            action.RegisterProperty("ModuleTechType" , packet.ModuleTechType);

            if (packet.IsMoonpoolExpansion)
            {
                action.OnProcessCompleted = this.OnMoonpoolProcessCompleted;
            }
            else
            {
                action.OnProcessCompleted = this.OnProcessCompleted;
            }

            Entity.ProcessToQueue(action);
            return true;
        }

        /**
         *
         * İşlem tamamlandığında tetiklenir.
         *
         * @author Ismail <ismaiil_0234@hotmail.com>
         *
         */
        private void OnMoonpoolProcessCompleted(ItemQueueProcess item)
        {
            var frontModuleId  = item.Action.GetProperty<string>("FrontModuleId");
            var backModuleId   = item.Action.GetProperty<string>("BackModuleId");
            var firstModuleId  = item.Action.GetProperty<string>("FirstModuleId");
            var playerId       = item.Action.GetProperty<byte>("PlayerId");
            var isConnect      = item.Action.GetProperty<bool>("IsConnect");
            var moduleTechType = item.Action.GetProperty<TechType>("ModuleTechType");
            var position       = item.Action.GetProperty<ZeroVector3>("Position");
            var rotation       = item.Action.GetProperty<ZeroQuaternion>("Rotation");

            var entity = Network.DynamicEntity.GetEntity(backModuleId);
            if (entity != null)
            {
                if (isConnect)
                {
                    entity.SetParent(firstModuleId);
                }
                else
                {
                    entity.SetParent(null);
                }
            }

            System.Action applyDockState = () =>
            {
                var dockingBay = Network.Identifier.GetComponentByGameObject<global::VehicleDockingBay>(frontModuleId);
                var backModule = Network.Identifier.GetComponentByGameObject<global::SeaTruckSegment>(backModuleId);
                if (dockingBay && backModule)
                {
                    var vehicleDockingBay = dockingBay.gameObject.EnsureComponent<MultiplayerVehicleDockingBay>();
                    if (vehicleDockingBay)
                    {
                        using (EventBlocker.Create(ProcessType.SeaTruckConnection))
                        {
                            if (isConnect)
                            {
                                vehicleDockingBay.ExpansionManager.DockTail(backModule, isConnection: true);
                            }
                            else
                            {
                                vehicleDockingBay.ExpansionManager.UndockTail();
                            }
                        }
                    }

                    if (ZeroPlayer.IsPlayerMine(playerId) == false)
                    {
                        backModule.rb.SetKinematic();
                    }
                }
            };

            if (isConnect)
            {
                if (!this.EnsureModuleSpawned(moduleTechType, position, rotation, playerId, backModuleId, firstModuleId, applyDockState))
                {
                    applyDockState();
                }
            }
            else
            {
                applyDockState();
            }
        }

        /**
         *
         * İşlem tamamlandığında tetiklenir.
         *
         * @author Ismail <ismaiil_0234@hotmail.com>
         *
         */
        private void OnProcessCompleted(ItemQueueProcess item)
        {
            var frontModuleId = item.Action.GetProperty<string>("FrontModuleId");
            var backModuleId  = item.Action.GetProperty<string>("BackModuleId");
            var playerId      = item.Action.GetProperty<byte>("PlayerId");
            var isConnect     = item.Action.GetProperty<bool>("IsConnect");
            var isReject      = item.Action.GetProperty<bool>("IsEject");
            var moduleId      = item.Action.GetProperty<ushort>("ModuleId");
            var position      = item.Action.GetProperty<ZeroVector3>("Position");
            var rotation      = item.Action.GetProperty<ZeroQuaternion>("Rotation");
            var moduleTechType = item.Action.GetProperty<TechType>("ModuleTechType");

            if (moduleId > 0)
            {
                Network.DynamicEntity.ChangePosition(moduleId, position, rotation);
            }

            global::SeaTruckSegment rearConnection = null;
            if (isConnect)
            {
                var entity = Network.DynamicEntity.GetEntity(frontModuleId);
                if (entity != null)
                {
                    entity.SetParent(backModuleId);
                }

                System.Action applyAttach = () =>
                {
                    var backModule  = Network.Identifier.GetComponentByGameObject<global::SeaTruckSegment>(frontModuleId);
                    var frontModule = Network.Identifier.GetComponentByGameObject<global::SeaTruckSegment>(backModuleId);

                    if (frontModule && frontModule)
                    {
                        var moonpoolExpansion = frontModule.GetFirstSegment().GetDockedMoonpoolExpansion();
                        if (moonpoolExpansion && global::Player.main.IsUnderwater() == false)
                        {
                            var underSeaTruckSegment = global::Player.main.GetUnderGameObject<SeaTruckSegment>();
                            if (underSeaTruckSegment && underSeaTruckSegment.GetFirstSegment() == backModule)
                            {
                                backModule.Exit(skipAnimations: true, newInterior: moonpoolExpansion.interior);
                            }
                        }

                        using (EventBlocker.Create(ProcessType.SeaTruckConnection))
                        {
                            backModule.frontConnection.SetConnectedTo(frontModule.rearConnection);

                            Utils.PlayFMODAsset(backModule.frontConnection.connectSound, backModule.frontConnection.connectionPoint);
                        }
                    }
                };

                if (!this.EnsureModuleSpawned(moduleTechType, position, rotation, playerId, frontModuleId, backModuleId, applyAttach))
                {
                    applyAttach();
                }
            }
            else
            {
                var entity = Network.DynamicEntity.GetEntity(backModuleId);
                if (entity != null)
                {
                    entity.SetParent(null);
                }

                var frontModule = Network.Identifier.GetComponentByGameObject<global::SeaTruckSegment>(frontModuleId);
                var backModule  = Network.Identifier.GetComponentByGameObject<global::SeaTruckSegment>(backModuleId);
                if (frontModule)
                {
                    using (EventBlocker.Create(ProcessType.SeaTruckConnection))
                    {
                        if (frontModule.rearConnection)
                        {
                            rearConnection = frontModule.rearConnection.GetConnection()?.truckSegment;

                            frontModule.rearConnection.Disconnect();

                            if (rearConnection && ZeroPlayer.IsPlayerMine(playerId) == false)
                            {
                                rearConnection.rb.SetKinematic();
                            }                                
                        }
                    }

                    var moonpoolExpansion = frontModule.GetFirstSegment().GetDockedMoonpoolExpansion();
                    if (moonpoolExpansion && global::Player.main.IsUnderwater() == false)
                    {
                        var underSeaTruckSegment = global::Player.main.GetUnderGameObject<SeaTruckSegment>();
                        if (underSeaTruckSegment && underSeaTruckSegment.GetFirstSegment() == backModule.GetFirstSegment())
                        {
                            global::Player.main.SetCurrentSub(null);
                            backModule.Enter(global::Player.main);
                        }
                    }
                }
            }

            var player = ZeroPlayer.GetPlayerById(playerId);
            if (player != null)
            {
                if (isReject)
                {
                    if (rearConnection)
                    {
                        rearConnection.player = null;
                        rearConnection.PropagatePlayer();
                    }

                    if (player.IsMine)
                    {
                        var seaTruckSegment = Network.Identifier.GetComponentByGameObject<global::SeaTruckSegment>(frontModuleId);
                        if (seaTruckSegment)
                        {
                            seaTruckSegment.seatruckanimation.currentAnimation = SeaTruckAnimation.Animation.EjectModules;
                        }
                    }
                    else
                    {
                        player.SeaTruckStartDetachCinematic(frontModuleId);
                    }
                }
            }
        }

        /**
         *
         * Bağlanan modül yerelde yoksa üretir, üretildikten sonra ekleme işlemini çalıştırır.
         * Üretim sırasında üretilen modüller ancak böyle görünür olur.
         *
         */
        private bool EnsureModuleSpawned(TechType moduleTechType, ZeroVector3 position, ZeroQuaternion rotation, byte ownerId, string moduleId, string parentModuleId, System.Action onSpawned)
        {
            if (Network.Identifier.GetGameObject(moduleId, true) != null)
            {
                return false;
            }

            if (!moduleTechType.IsSeaTruckModule() || position == null || rotation == null)
            {
                return false;
            }

            try
            {
                var owner = ZeroPlayer.GetPlayerById(ownerId);

                var entity = new WorldDynamicEntity()
                {
                    UniqueId    = moduleId,
                    TechType    = moduleTechType,
                    Position    = position,
                    Rotation    = rotation,
                    IsDeployed  = true,
                    OwnershipId = owner?.UniqueId,
                };
                entity.SetParent(parentModuleId);

                Network.DynamicEntity.Spawn(entity, (item, pickupable, spawnedGameObject) =>
                {
                    try
                    {
                        onSpawned?.Invoke();
                    }
                    catch (System.Exception ex)
                    {
                        Log.Error($"SeaTruckConnectionProcessor.EnsureModuleSpawned.Callback: {ex}");
                    }
                });

                return true;
            }
            catch (System.Exception ex)
            {
                Log.Error($"SeaTruckConnectionProcessor.EnsureModuleSpawned: {ex}");
                return false;
            }
        }

        /**
         *
         * Seatruck modülü bağlanırken/ayrılırken tetiklenir.
         *
         * @author Ismail <ismaiil_0234@hotmail.com>
         *
         */
        public static void OnSeaTruckConnecting(SeaTruckConnectingEventArgs ev)
        {
            ev.IsAllowed = false;

            if (ev.IsMoonpoolExpansion)
            {
                if (ev.IsConnect)
                {
                    var backModule = Network.DynamicEntity.GetEntity(ev.BackModuleId);
                    if (backModule != null && backModule.IsMine(ZeroPlayer.CurrentPlayer.UniqueId) && backModule.IsUsingByPlayer)
                    {
                        SeaTruckConnectionProcessor.SendPacketToServer(ev.IsConnect, frontModuleId: ev.FrontModuleId, backModuleId: ev.BackModuleId, firstModuleId: ev.FirstModuleId, isMoonpoolExpansion: ev.IsMoonpoolExpansion, spawnSubjectModuleId: ev.BackModuleId);
                    }
                }
                else
                {
                    if (Network.DynamicEntity.HasEntity(ev.BackModuleId))
                    {
                        SeaTruckConnectionProcessor.SendPacketToServer(ev.IsConnect, frontModuleId: ev.FrontModuleId, backModuleId: ev.BackModuleId, firstModuleId: ev.FirstModuleId, isMoonpoolExpansion: ev.IsMoonpoolExpansion);
                    }
                }
            }
            else
            {
                if (ev.IsConnect)
                {
                    var frontModule = Network.DynamicEntity.GetEntity(ev.FrontModuleId);
                    var backModule  = Network.DynamicEntity.GetEntity(ev.BackModuleId);
                    if (frontModule != null && backModule != null)
                    {
                        if ((frontModule.IsMine(ZeroPlayer.CurrentPlayer.UniqueId) && frontModule.IsUsingByPlayer) || (backModule.IsMine(ZeroPlayer.CurrentPlayer.UniqueId) && backModule.IsUsingByPlayer))
                        {
                            SeaTruckConnectionProcessor.SendPacketToServer(ev.IsConnect, frontModuleId: ev.FrontModuleId, backModuleId: ev.BackModuleId, firstModuleId: ev.FirstModuleId, isMoonpoolExpansion: ev.IsMoonpoolExpansion, spawnSubjectModuleId: ev.FrontModuleId);
                        }
                    }
                }
                else
                {
                    var frontModule = Network.DynamicEntity.GetEntity(ev.FrontModuleId);
                    if (frontModule != null)
                    {
                        SeaTruckConnectionProcessor.SendPacketToServer(ev.IsConnect, frontModuleId: ev.FrontModuleId, backModuleId: ev.BackModuleId, firstModuleId: ev.FirstModuleId, isMoonpoolExpansion: ev.IsMoonpoolExpansion);
                    }
                }
            }
        }

        /**
         *
         * SeaTruck modül bağlantı kesme animasyonunda tetiklenir.
         *
         * @author Ismail <ismaiil_0234@hotmail.com>
         *
         */
        public static void OnSeaTruckDetaching(SeaTruckDetachingEventArgs ev)
        {
            ev.IsAllowed = false;

            SeaTruckConnectionProcessor.SendPacketToServer(false, true, ev.UniqueId);
        }

        /**
         *
         * Sunucuya Paket Gönderir.
         *
         * @author Ismail <ismaiil_0234@hotmail.com>
         *
         */
        public static void SendPacketToServer(bool isConnect, bool isEject = false, string frontModuleId = null, string backModuleId = null, string firstModuleId = null, bool isMoonpoolExpansion = false, string spawnSubjectModuleId = null)
        {
            ServerModel.SeaTruckConnectionArgs request = new ServerModel.SeaTruckConnectionArgs()
            {
                IsConnect     = isConnect,
                IsEject       = isEject,
                FrontModuleId = frontModuleId,
                BackModuleId  = backModuleId,
                FirstModuleId = firstModuleId,
                IsMoonpoolExpansion = isMoonpoolExpansion
            };

            if (isConnect && !string.IsNullOrEmpty(spawnSubjectModuleId))
            {
                var subjectGameObject = Network.Identifier.GetGameObject(spawnSubjectModuleId, true);
                if (subjectGameObject != null)
                {
                    request.ModuleTechType = CraftData.GetTechType(subjectGameObject);
                    request.Position       = subjectGameObject.transform.position.ToZeroVector3();
                    request.Rotation       = subjectGameObject.transform.rotation.ToZeroQuaternion();
                }
            }

            NetworkClient.SendPacket(request);
        }
    }
}