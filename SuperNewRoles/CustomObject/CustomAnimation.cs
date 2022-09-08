using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.CustomObject
{
    [Il2CppRegister]
    public sealed class CustomAnimation : MonoBehaviour
    {
        private List<Sprite> sprites;
        private float UpdateTime;
        private float DefaultUpdateTime => 1f / freamrate;
        private int freamrate;
        private int index;
        private bool IsLoop;
        private bool Playing;
        private SpriteRenderer render;
        public void Awake()
        {
            sprites = new();
            index = 0;
            if ((render = gameObject.GetComponent<SpriteRenderer>()) == null)
            {
                render = gameObject.AddComponent<SpriteRenderer>();
            }
        }

        public void Init(List<Sprite> _sprites, bool _isLoop, int _freamrate)
        {
            freamrate = _freamrate;
            sprites = _sprites;
            index = 0;
            IsLoop = _isLoop;
            Playing = false;
            UpdateTime = DefaultUpdateTime;
        }

        public void FixedUpdate()
        {
        }
    }
}
