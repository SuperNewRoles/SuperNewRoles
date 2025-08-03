using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sentry.Unity.NativeUtils;
using SuperNewRoles.Roles;

namespace SuperNewRoles.Modules
{
    /// <summary>
    /// Represents the aggregated task option data with three integers: Short, Long, and Common.
    /// </summary>
    public class TaskOptionData
    {
        public int Short { get; set; }
        public int Long { get; set; }
        public int Common { get; set; }

        public TaskOptionData(int shortOption, int longOption, int commonOption)
        {
            Short = shortOption;
            Long = longOption;
            Common = commonOption;
        }

        public int Total => Short + Long + Common;
    }

    /// <summary>
    /// Custom option attribute that aggregates three CustomOptionIntAttribute instances.
    /// When any internal option is updated, the corresponding property in TaskOptionData is updated.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class CustomOptionTaskAttribute : CustomOptionBaseAttribute
    {
        // ここでは、個別オプション（Short/Long/Common）の型をintではなくCustomOptionIntAttributeとし、後からアタッチできるようにしています。
        public CustomOptionIntAttribute ShortOption { get; set; }
        public int ShortValue;
        public CustomOptionIntAttribute LongOption { get; set; }
        public int LongValue;
        public CustomOptionIntAttribute CommonOption { get; set; }
        public int CommonValue;

        public TaskOptionData TaskData { get; private set; }

        public event Action<int> ValueChanged;

        public CustomOptionTaskAttribute(string id, int shortDefault, int longDefault, int commonDefault, string? translationName = null, string? parentFieldName = null, DisplayModeId displayMode = DisplayModeId.Default, object? parentActiveValue = null)
            : base(id, translationName, parentFieldName, displayMode, parentActiveValue)
        {
            // TaskDataのみを初期化して、個別オプションは後付（AttachOptionsメソッド経由）とします
            TaskData = new TaskOptionData(shortDefault, longDefault, commonDefault);
            ShortOption = new CustomOptionIntAttribute(id + "_Short", 0, 20, 1, shortDefault, "CustomOptionTask_Short", parentFieldName, displayMode, parentActiveValue);
            LongOption = new CustomOptionIntAttribute(id + "_Long", 0, 20, 1, longDefault, "CustomOptionTask_Long", parentFieldName, displayMode, parentActiveValue);
            CommonOption = new CustomOptionIntAttribute(id + "_Common", 0, 20, 1, commonDefault, "CustomOptionTask_Common", parentFieldName, displayMode, parentActiveValue);
        }
        public void SetupAttributes(FieldInfo meField, ref HashSet<string> fieldNames, List<CustomOption> customOptions, Dictionary<string, CustomOptionBaseAttribute> customOptionAttributes)
        {
            if (!fieldNames.Add(ShortOption.Id))
            {
                throw new InvalidOperationException($"フィールド名が重複しています: {ShortOption.Id}");
            }
            if (!fieldNames.Add(LongOption.Id))
            {
                throw new InvalidOperationException($"フィールド名が重複しています: {LongOption.Id}");
            }
            if (!fieldNames.Add(CommonOption.Id))
            {
                throw new InvalidOperationException($"フィールド名が重複しています: {CommonOption.Id}");
            }

            // FieldInfoを取得するために、このクラスのTypeを取得
            Type type = this.GetType();

            // 各オプションのフィールドを取得
            FieldInfo shortField = type.GetField(nameof(ShortValue));
            FieldInfo longField = type.GetField(nameof(LongValue));
            FieldInfo commonField = type.GetField(nameof(CommonValue));

            // カスタムオプション属性を辞書に追加
            customOptionAttributes[ShortOption.Id] = ShortOption;
            customOptionAttributes[LongOption.Id] = LongOption;
            customOptionAttributes[CommonOption.Id] = CommonOption;
            Logger.Info($"shortField: {shortField == null} {longField == null} {commonField == null}");
            var shortOption = SetupAttribute(ShortOption, shortField, meField);
            var longOption = SetupAttribute(LongOption, longField, meField);
            var commonOption = SetupAttribute(CommonOption, commonField, meField);

            // 通常、ロング、ショートの順にタスクオプションを追加
            customOptions.Add(commonOption);
            customOptions.Add(longOption);
            customOptions.Add(shortOption);
            Logger.Info($"TaskOptionData初期値: Common={TaskData.Common}, Long={TaskData.Long}, Short={TaskData.Short}");
        }
        private CustomOption SetupAttribute(CustomOptionIntAttribute attribute, FieldInfo field, FieldInfo meField)
        {
            attribute.SetFieldInfo(field);
            RoleId? role = null;
            if (meField.DeclaringType.GetInterfaces().Contains(typeof(IRoleBase)))
            {
                var baseSingletonType = typeof(BaseSingleton<>).MakeGenericType(meField.DeclaringType);
                var instanceProperty = baseSingletonType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
                var roleInstance = instanceProperty.GetValue(null);
                role = ((IRoleBase)roleInstance).Role;
            }
            // カスタムオプションを作成し、リストに追加
            CustomOption option = new(attribute, field, role, isTaskOption: true);
            meField.SetValue(null, TaskData);

            // ValueChangedイベントハンドラーを設定
            if (attribute == ShortOption)
            {
                attribute.ValueChanged += OnShortOptionValueChanged;
            }
            else if (attribute == LongOption)
            {
                attribute.ValueChanged += OnLongOptionValueChanged;
            }
            else if (attribute == CommonOption)
            {
                attribute.ValueChanged += OnCommonOptionValueChanged;
            }

            return option;
        }

        private void OnShortOptionValueChanged(int val)
        {
            Logger.Info($"ShortOption値が変更されました: {val}");
            TaskData.Short = val;
            ValueChanged?.Invoke(val);
        }

        private void OnLongOptionValueChanged(int val)
        {
            Logger.Info($"LongOption値が変更されました: {val}");
            TaskData.Long = val;
            ValueChanged?.Invoke(val);
        }

        private void OnCommonOptionValueChanged(int val)
        {
            Logger.Info($"CommonOption値が変更されました: {val}");
            TaskData.Common = val;
            ValueChanged?.Invoke(val);
        }

        public override byte GenerateDefaultSelection()
        {
            return 0;
        }

        public override object[] GenerateSelections()
        {
            return new object[] { TaskData };
        }
    }
}
