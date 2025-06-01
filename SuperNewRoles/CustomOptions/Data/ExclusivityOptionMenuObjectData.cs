using System.Collections.Generic;
using UnityEngine;
using TMPro;
using SuperNewRoles.Roles;
using SuperNewRoles.Modules;

namespace SuperNewRoles.CustomOptions.Data
{
    public class ExclusivityOptionMenuObjectData : OptionMenuBase
    {
        public static ExclusivityOptionMenuObjectData Instance { get; private set; }

        /// <summary>
        /// メニューのGameObjectを取得
        /// </summary>
        public GameObject ExclusivityOptionMenu { get; }
        public Scroller MainAreaScroller { get; }
        public GameObject MainAreaInner { get; }
        public GameObject ExclusivityOptionButtonContainer { get; set; }
        public GameObject ExclusivityEditMenu { get; set; }
        public Scroller ExclusivityEditRightAreaScroller { get; set; }
        public GameObject ExclusivityEditRightAreaInner { get; set; }
        public GameObject RoleDetailButtonContainer { get; set; }
        public int CurrentEditingIndex { get; set; } = -1;

        public ExclusivityOptionMenuObjectData(GameObject exclusivityOptionMenu) : base()
        {
            Instance = this;
            ExclusivityOptionMenu = exclusivityOptionMenu;
            MainAreaScroller = ExclusivityOptionMenu.transform.Find("Scroller").GetComponent<Scroller>();
            MainAreaInner = MainAreaScroller.transform.Find("Inner").gameObject;
        }

        public override void Hide()
        {
            if (ExclusivityOptionMenu != null)
                ExclusivityOptionMenu.SetActive(false);
            if (ExclusivityEditMenu != null)
                ExclusivityEditMenu.SetActive(false);
        }

        public override void UpdateOptionDisplay()
        {
            // オプションの表示を更新
        }
    }
}