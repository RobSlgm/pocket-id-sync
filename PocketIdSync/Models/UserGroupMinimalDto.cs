using System;
using Generator.Equals;

namespace PocketIdSync.Models;

[Equatable]
partial class UserGroupMinimalDto
{
    [IgnoreEquality]
    public DateTime? CreatedAt { get; set; }

    [UnorderedEquality]
    public CustomClaimDto[] CustomClaims { get; set; } = [];

    public string? FriendlyName { get; set; }

    [IgnoreEquality]
    public string? Id { get; set; }

    [IgnoreEquality]
    public string? LdapId { get; set; }

    public string? Name { get; set; }

    [IgnoreEquality]
    public int? UserCount { get; set; }
}
