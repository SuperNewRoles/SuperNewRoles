namespace SuperNewRoles.CustomCosmetics
{
    public static class CustomPet
    {
        public static bool isAdded = false;
        /*
        [HarmonyPatch(typeof(HatManager), nameof(HatManager.GetPetById))]
        class UnlockedPetPatch
        {
            public static void Postfix(HatManager __instance)
            {
                if (isAdded) return;
                isAdded = true;
                SuperNewRolesPlugin.Logger.LogInfo("ペット読み込み処理開始");
                var AllPlates = __instance.allPets;

                var plateDir = new DirectoryInfo("SuperNewRoles\\CustomPetChache");
                if (!plateDir.Exists) plateDir.Create();
                var Files = plateDir.GetFiles("*.png").ToList();
                Files.AddRange(plateDir.GetFiles("*.jpg"));
                var CustomPlates = new List<NamePlateData>();
                foreach (var file in Files)
                {
                    try
                    {
                        SuperNewRolesPlugin.Logger.LogInfo("ファイル名:"+file.Name);
                        var pet = ScriptableObject.CreateInstance<PetData>();
                        var FileName = file.Name.Substring(0, file.Name.Length - 4);
                        //var Data = DownLoadClass.platedetails.FirstOrDefault(data => data.resource.Replace(".png", "") == FileName);
                        pet.name = "名前";//Data.name + "\nby " + Data.author;
                        pet.ProductId = "CustomPet_" + FileName.Replace(".png", "").Replace(".jpg", "");
                        pet.BundleId = "CustomPet_" + FileName.Replace(".png", "").Replace(".jpg", "");
                        pet.displayOrder = 99;
                        pet.ChipOffset = new Vector2(0f, 0.2f);
                        pet.Free = true;
                        SuperNewRolesPlugin.Logger.LogInfo("あ");
                        pet.viewData.viewData = new PetBehaviour();
                        SuperNewRolesPlugin.Logger.LogInfo("え");
                        pet.viewData.viewData.rend = new SpriteRenderer();
                        SuperNewRolesPlugin.Logger.LogInfo("い");
                        var sp = LoadTex.loadSprite("SuperNewRoles\\CustomPetChache\\" + file.Name);
                        SuperNewRolesPlugin.Logger.LogInfo("お");
                        pet.viewData.viewData.rend.sprite = sp;
                        SuperNewRolesPlugin.Logger.LogInfo("う");
                        //CustomPlates.Add(plate);
                        //AllPlates.Add(plate);
                        __instance.allPets.Add(pet);
                        SuperNewRolesPlugin.Logger.LogInfo("ペット完了:" + file.Name);
                    }
                    catch (Exception e)
                    {
                        SuperNewRolesPlugin.Logger.LogError("エラー:CustomPetの読み込みに失敗しました:" + file.FullName);
                        SuperNewRolesPlugin.Logger.LogError(file.FullName + "のエラー内容:" + e);
                    }
                }
                SuperNewRolesPlugin.Logger.LogInfo("ペット読み込み処理終了");
            }
        }*/
    }
}