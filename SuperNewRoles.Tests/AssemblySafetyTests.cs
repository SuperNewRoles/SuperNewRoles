using System.Linq;
using System.Reflection;
using FluentAssertions;
using SuperNewRoles.Modules;
using Xunit;

namespace SuperNewRoles.Tests;

public class AssemblySafetyTests
{
    [Fact]
    public void SuperNewRolesAssembly_DoesNotDeclarePInvokeMethods()
    {
        Assembly assembly = typeof(AnnouncementImageRendererAndroid).Assembly;
        MethodInfo[] pinvokeMethods = assembly.GetTypes()
            .SelectMany(type => type.GetMethods(BindingFlags.Public
                | BindingFlags.NonPublic
                | BindingFlags.Static
                | BindingFlags.Instance
                | BindingFlags.DeclaredOnly))
            .Where(method => (method.Attributes & MethodAttributes.PinvokeImpl) != 0)
            .ToArray();

        pinvokeMethods.Should().BeEmpty(
            "the same plugin DLL must pass Starlight's Android safety validation");
    }
}
