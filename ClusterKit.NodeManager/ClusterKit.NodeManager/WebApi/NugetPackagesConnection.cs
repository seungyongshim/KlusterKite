 // --------------------------------------------------------------------------------------------------------------------
// <copyright file="NugetPackagesConnection.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The connection to the nuget server
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.WebApi
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    using ClusterKit.API.Client;
    using ClusterKit.NodeManager.Client.ApiSurrogates;

    using NuGet;

    /// <summary>
    /// The connection to the nuget server
    /// </summary>
    public class NugetPackagesConnection : INodeConnection<PackageDescription>
    {
        /// <summary>
        /// The nuget server url
        /// </summary>
        private string nugetServerUrl;

        /// <summary>
        /// Initializes a new instance of the <see cref="NugetPackagesConnection"/> class.
        /// </summary>
        /// <param name="nugetServerUrl">
        /// The nuget server url.
        /// </param>
        public NugetPackagesConnection(string nugetServerUrl)
        {
            this.nugetServerUrl = nugetServerUrl;
        }

        /// <inheritdoc />
        public Task<QueryResult<PackageDescription>> Query(
            Expression<Func<PackageDescription, bool>> filter,
            IEnumerable<SortingCondition> sort,
            int? limit,
            int? offset,
            ApiRequest apiRequest)
        {
            var nugetRepository = PackageRepositoryFactory.Default.CreateRepository(this.nugetServerUrl);

            IQueryable<PackageDescription> query;
            if (apiRequest.Fields.Where(f => f.FieldName == "items").SelectMany(f => f.Fields).Any(f => f.FieldName == "availableVersions"))
            {
                query = nugetRepository.Search(string.Empty, true).ToList().GroupBy(p => p.Id).Select(
                    g =>
                        {
                            var package = g.FirstOrDefault(p => p.IsLatestVersion) ?? g.First();
                            return new PackageDescription
                                       {
                                           Name = package.Id,
                                           Version = package.Version.ToString(),
                                           AvailableVersions =
                                               g.OrderByDescending(p => p.Version)
                                                   .Select(p => p.Version.ToString())
                                                   .ToList()
                                       };
                        }).AsQueryable();
            }
            else
            {
                query =
                    nugetRepository.Search(string.Empty, true)
                        .Where(p => p.IsLatestVersion)
                        .ToList()
                        .Select(p => new PackageDescription { Name = p.Id, Version = p.Version.ToString() })
                        .AsQueryable();
            }

            if (filter != null)
            {
                query = query.Where(filter);
            }

            var count = query.Count();

            if (sort != null)
            {
                query = query.ApplySorting(sort);
            }

            if (offset != null)
            {
                query = query.Skip(offset.Value);
            }

            if (limit != null)
            {
                query = query.Take(limit.Value);
            }

            return Task.FromResult(new QueryResult<PackageDescription> { Count = count, Items = query });
        }

        /// <inheritdoc />
        public Task<MutationResult<PackageDescription>> Create(PackageDescription newNode)
        {
            throw new InvalidOperationException();
        }

        /// <inheritdoc />
        public Task<MutationResult<PackageDescription>> Update(object id, PackageDescription newNode, ApiRequest request)
        {
            throw new InvalidOperationException();
        }

        /// <inheritdoc />
        public Task<MutationResult<PackageDescription>> Delete(object id)
        {
            throw new InvalidOperationException();
        }
    }
}
