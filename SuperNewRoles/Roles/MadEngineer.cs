using SuperNewRoles.CustomOption;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.Patch;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNewRoles.Roles
{
    class MadEngineer
    {
        public static List<byte> CheckedImpostor;
        public static bool CheckImpostor(PlayerControl p)
        {
            if (!RoleClass.MadEngineer.IsImpostorCheck) return false;
            if (!p.isRole(RoleId.MadEngineer)) return false;
            if (CheckedImpostor.Contains(p.PlayerId)) return true;
            /*
            SuperNewRolesPlugin.Logger.LogInfo("�C���|�X�^�[�`�F�b�N�^�X�N��:"+RoleClass.MadEngineer.ImpostorCheckTask);
            SuperNewRolesPlugin.Logger.LogInfo("�I���^�X�N��:"+TaskCount.TaskDate(p.Data).Item1);*/
            SuperNewRolesPlugin.Logger.LogInfo("�L����:" + (RoleClass.MadEngineer.ImpostorCheckTask <= TaskCount.TaskDate(p.Data).Item1));
            if (RoleClass.MadEngineer.ImpostorCheckTask <= TaskCount.TaskDate(p.Data).Item1)
            {
                SuperNewRolesPlugin.Logger.LogInfo("�L����Ԃ��܂���");
                return true;
            }
            // SuperNewRolesPlugin.Logger.LogInfo("��ԉ��܂Œʉ�");
            return false;
        }
    }
}