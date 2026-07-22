using System;
using FluentAssertions;
using SuperNewRoles.CustomCosmetics;
using SuperNewRoles.Modules;
using Xunit;

namespace SuperNewRoles.Tests;

public class CustomCosmeticsJsonNodeTests
{
    [Fact]
    public void Parse_ReadsMetadataWithoutNewtonsoftJson()
    {
        CustomCosmeticsJsonNode root = CustomCosmeticsJsonNode.Parse(
            """
            {
              "package": { "name": "テスト", "parseversion": 2 },
              "hats": [
                { "name": "帽子A", "adaptive": true, "bounce": false },
                // 外部コスメ定義との互換性のため、コメントと末尾カンマを許容する。
                { "name": "帽子B", "adaptive": false },
              ],
            }
            """);

        root["package"]["name"].ToString().Should().Be("テスト");
        ((int)root["package"]["parseversion"]).Should().Be(2);

        CustomCosmeticsJsonNode firstHat = root["hats"].First;
        firstHat["name"].ToString().Should().Be("帽子A");
        ((bool)firstHat["adaptive"]).Should().BeTrue();
        firstHat.Next["name"].ToString().Should().Be("帽子B");
        firstHat.Next.Next.Should().BeNull();
    }

    [Fact]
    public void Parse_RejectsMalformedJson()
    {
        Action parse = () => CustomCosmeticsJsonNode.Parse("{ invalid json }");

        parse.Should().Throw<JsonParseException>();
    }

    [Fact]
    public void HatOptions_UseManagedJsonValues()
    {
        CustomCosmeticsJsonNode optionsJson = CustomCosmeticsJsonNode.Parse(
            """{ "adaptive": true, "bounce": true, "behind": true, "hideBody": true }""");

        var options = new CustomCosmeticsHatOptions(optionsJson);

        options.front.Should().HaveFlag(HatOptionType.Adaptive);
        options.front.Should().HaveFlag(HatOptionType.Bounce);
        options.front.Should().HaveFlag(HatOptionType.Behind);
        options.HideBody.Should().BeTrue();
    }
}
