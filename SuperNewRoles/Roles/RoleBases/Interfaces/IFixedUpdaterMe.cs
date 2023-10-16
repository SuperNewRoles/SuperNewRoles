namespace SuperNewRoles.Roles.RoleBases.Interfaces;
public interface IFixedUpdaterMe
{
    public void FixedUpdateMeDefaultAlive();
    public virtual void FixedUpdateMeDefaultDead() { }
    public virtual void FixedUpdateMeDefault() { }
    public virtual void FixedUpdateMeSHR() { }
    public virtual void FixedUpdateMeSHRAlive() { }
    public virtual void FixedUpdateMeSHRDead() { }
}