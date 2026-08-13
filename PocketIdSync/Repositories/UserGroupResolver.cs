using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PocketIdSync.Apis;
using PocketIdSync.Models;

namespace PocketIdSync.Repositories;

sealed class UserGroupResolver
{
    private Dictionary<string, UserGroupMinimalDto> UserGroups { get; } = new Dictionary<string, UserGroupMinimalDto>(StringComparer.OrdinalIgnoreCase);

    private void AddUserGroups(UserGroupMinimalDto[] userGroups)
    {
        foreach (var ug in userGroups ?? [])
        {
            UserGroups.TryAdd(ug.Name ?? ug.Id!, ug);
        }
    }

    public async Task<ApiResult<UserGroupMinimalDto[]>> Initialize(PocketIdClient pocketId, CancellationToken ct)
    {
        var response = await pocketId.UserGroups.ListAsync(ct);
        if (!response.IsSuccessful)
        {
            return response;
        }
        if (response.Data is not null)
        {
            UserGroups.Clear();
            AddUserGroups(response.Data ?? []);
        }
        return response;
    }

    public UserGroupMinimalDto? Find(string id)
    {
        if (UserGroups.TryGetValue(id, out var ug))
        {
            return ug;
        }
        return null;
    }
}

static class UserGroupResolverExtensions
{
    extension(UserGroupResolver resolver)
    {
        public UserGroupMinimalDto[] ToGroups(string[]? groupNames)
        {
            if (groupNames is null || groupNames.Length == 0)
            {
                return [];
            }
            var permissions = new List<UserGroupMinimalDto>();
            foreach (var uid in groupNames)
            {
                var permission = resolver.Find(uid);
                if (permission is not null)
                {
                    permissions.Add(permission);
                }
            }
            return [.. permissions];
        }
    }
}
