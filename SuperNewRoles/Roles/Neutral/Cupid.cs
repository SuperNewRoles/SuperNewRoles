namespace SuperNewRoles.Roles.Neutral
{
    public static class Cupid
    {
        //ここにコードを書きこんでください
        public static void FixedUpdate()
        {
            if (RoleClass.Cupid.Created) {
                if (RoleClass.Cupid.currentLovers is null ||
                    !RoleClass.Cupid.currentLovers.IsLovers())
                {
                    RoleClass.Cupid.Created = false;
                }
            } else
            {
                if (RoleClass.Cupid.currentTarget != null)
                {
                    Patches.PlayerControlFixedUpdatePatch.SetPlayerOutline(RoleClass.Cupid.currentTarget, RoleClass.Cupid.color);
                }
            }
        }
    }
}