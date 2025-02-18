using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperNewRoles.Modules;

/// <summary>
/// 1つしかインスタンスが不要なクラスに使用
/// 継承してabstract classを作成する場合：
/// 　abstract class InheritClass<T> : BaseSingleton<T> where T : BaseSingleton<T>, new()
/// 継承での使用例：
/// 　class InheritClass2 : InheritClass<InheritClass2>
/// </summary>
public abstract class BaseSingleton<T> where T : BaseSingleton<T>, new()
{
    private static T m_Instance;

    public static T Instance
    {
        get { return GetOrCreateInstance<T>(); }
    }
    protected static bool IsCreated
    {
        get { return m_Instance != null; }
    }

    protected static V GetOrCreateInstance<V>() where V : class, T, new()
    {
        if (m_Instance is V _instance) return _instance;

        if (IsCreated)
        {
            //ここに来たということは、m_Instanceが継承先ではなく基底クラスで作られている
            //古いインスタンスはGCされるはずなので、基本は何もしなくてよいはず
            //ただしdisposeが必要なもの(FileStreamとか)抱えてる場合は必ずリリースが必要なので一応準備だけしておく
            if (m_Instance is IDisposable) ((IDisposable)m_Instance).Dispose();
        }

        //問答無用でインスタンスを再作成する
        m_Instance = new V();

        //if (IsCreated)
        //{
        //    // 基底クラスから呼ばれた後に継承先から呼ばれるとエラーになる。先に継承先から呼ぶ
        //    if (!typeof(V).IsAssignableFrom(m_Instance.GetType()))
        //    {
        //        Logger.Info($"{m_Instance.GetType().ToString()}が{typeof(V).ToString()}を継承していません");
        //    }
        //}
        //else
        //{
        //    m_Instance = new V();
        //}
        return m_Instance as V;
    }

    /// <summary>
    ///
    /// </summary>
    protected BaseSingleton() => Init();

    protected virtual void Init() { }
}