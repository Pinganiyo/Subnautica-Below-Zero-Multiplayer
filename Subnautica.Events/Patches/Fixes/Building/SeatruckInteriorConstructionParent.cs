namespace Subnautica.Events.Patches.Fixes.Building
{
    using HarmonyLib;

    using Subnautica.API.Features;

    using System;

    using UnityEngine;

    /**
     *
     * Seatruck içi inşa edilen yapıları segmente ebeveynler ve çarpışmaları ayarlar.
     *
     */
    [HarmonyPatch(typeof(global::Constructable), nameof(global::Constructable.Construct))]
    public static class SeatruckInteriorConstructionParent
    {
        /**
         *
         * Fonksiyonu yamalar.
         *
         */
        private static void Postfix(global::Constructable __instance)
        {
            if (!Network.IsMultiplayerActive)
            {
                return;
            }

            try
            {
                var gameObject = __instance.gameObject;
                if (gameObject == null)
                {
                    return;
                }

                if (gameObject.GetComponent<global::Vehicle>() != null)
                {
                    return;
                }

                if (gameObject.GetComponentInParent<global::Base>() != null)
                {
                    return;
                }

                var existingSegment = gameObject.GetComponentInParent<global::SeaTruckSegment>();
                if (existingSegment != null)
                {
                    SeatruckInteriorConstructionParent.SetupSeatruckInterior(gameObject, existingSegment);
                    return;
                }

                var segment = SeatruckInteriorConstructionParent.FindContainingSegment(gameObject.transform.position);
                if (segment != null)
                {
                    gameObject.transform.SetParent(segment.transform, true);
                    SeatruckInteriorConstructionParent.SetupSeatruckInterior(gameObject, segment);
                    Log.Info($"Building.SeatruckInteriorConstructionParent: '{gameObject.name}' parented to SeaTruck segment.");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Building.SeatruckInteriorConstructionParent: {ex}");
            }
        }

        /**
         *
         * Seatruck içi yapı için Rigidbody ve çarpışma ayarlarını yapar.
         *
         */
        public static void SetupSeatruckInterior(GameObject gameObject, global::SeaTruckSegment segment)
        {
            if (gameObject == null || segment == null)
            {
                return;
            }

            // 1. Kendi kinematik Rigidbody'sini ekler, böylece SeaTruck Rigidbody'sinin bileşik çarpışanı (compound collider) olmaz.
            var rb = gameObject.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();
            }
            rb.isKinematic = true;
            rb.interpolation = RigidbodyInterpolation.None;

            // 2. Mobilya kontrol bileşenini ekler.
            var furniture = gameObject.GetComponent<SeaTruckInteriorFurniture>();
            if (furniture == null)
            {
                furniture = gameObject.AddComponent<SeaTruckInteriorFurniture>();
            }
            furniture.Initialize(segment);
        }

        /**
         *
         * Verilen konumu içeren SeaTruckSegment değerini döner.
         *
         */
        private static global::SeaTruckSegment FindContainingSegment(Vector3 position)
        {
            global::SeaTruckSegment bestSegment = null;

            var bestDistance = float.MaxValue;
            foreach (var collider in Physics.OverlapSphere(position, 0.5f, ~0, QueryTriggerInteraction.Ignore))
            {
                if (collider == null)
                {
                    continue;
                }

                var segment = collider.GetComponentInParent<global::SeaTruckSegment>();
                if (segment == null)
                {
                    continue;
                }

                var distance = (segment.transform.position - position).sqrMagnitude;
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestSegment  = segment;
                }
            }

            return bestSegment;
        }
    }

    /**
     *
     * Seatruck içi mobilyanın SeaTruck ve Base ile çarpışmalarını yoksayar.
     *
     */
    public class SeaTruckInteriorFurniture : MonoBehaviour
    {
        private global::SeaTruckSegment segment;
        private Collider[] myColliders;
        private float nextCheckTime;

        public void Initialize(global::SeaTruckSegment segment)
        {
            this.segment = segment;
            this.myColliders = this.GetComponentsInChildren<Collider>(true);
            this.UpdateIgnoredCollisions();
        }

        public void UpdateIgnoredCollisions()
        {
            if (this.segment == null || this.myColliders == null || this.myColliders.Length == 0)
            {
                return;
            }

            var firstSegment = this.segment.GetFirstSegment() ?? this.segment;
            var truckColliders = firstSegment.GetComponentsInChildren<Collider>(true);

            foreach (var ic in this.myColliders)
            {
                if (ic == null) continue;

                foreach (var tc in truckColliders)
                {
                    if (tc != null && tc != ic)
                    {
                        Physics.IgnoreCollision(ic, tc, true);
                    }
                }
            }

            // Dünyadaki mevcut Base parçaları ile çarpışmayı yoksay
            var bases = UnityEngine.Object.FindObjectsOfType<global::Base>();
            foreach (var ic in this.myColliders)
            {
                if (ic == null) continue;

                foreach (var b in bases)
                {
                    if (b == null) continue;
                    foreach (var bc in b.GetComponentsInChildren<Collider>(true))
                    {
                        if (bc != null && bc != ic)
                        {
                            Physics.IgnoreCollision(ic, bc, true);
                        }
                    }
                }
            }
        }

        private void Update()
        {
            if (Time.time > this.nextCheckTime)
            {
                this.nextCheckTime = Time.time + 3.0f;
                this.UpdateIgnoredCollisions();
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.collider != null && !collision.gameObject.GetComponent<global::Player>())
            {
                if (this.myColliders != null)
                {
                    foreach (var ic in this.myColliders)
                    {
                        if (ic != null && ic != collision.collider)
                        {
                            Physics.IgnoreCollision(ic, collision.collider, true);
                        }
                    }
                }
            }
        }
    }
}
