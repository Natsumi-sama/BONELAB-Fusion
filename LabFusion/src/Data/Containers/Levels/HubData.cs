﻿using LabFusion.Network;
using LabFusion.Senders;
using LabFusion.SDK.Points;

using Il2CppSLZ.Bonelab;

using UnityEngine;
using LabFusion.Extensions;

namespace LabFusion.Data
{
    public class HubData : LevelDataHandler
    {
        public override string LevelTitle => "02 - BONELAB Hub";

        public static GameControl_Hub GameController;
        public static FunicularController Funicular;

        public static readonly Vector3 PointShopPosition = new Vector3(-5.69f, -0.013f, 39.79f);
        public static readonly Quaternion PointShopRotation = Quaternion.Euler(0f, 90f, 0f);

        protected override void MainSceneInitialized()
        {
            GameController = GameObject.FindObjectOfType<GameControl_Hub>(true);
            Funicular = GameObject.FindObjectOfType<FunicularController>(true);

            if (GameController != null)
            {
                PointShopHelper.SpawnBitMart(PointShopPosition, PointShopRotation);
            }

            if (NetworkInfo.IsServer && Funicular != null)
            {
                PropSender.SendPropCreation(Funicular.gameObject);
            }
        }

        public static bool IsInHub() => GameController != null;
    }
}
