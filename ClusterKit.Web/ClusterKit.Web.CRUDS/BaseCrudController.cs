﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BaseCrudController.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Base class to provide basic CRUD operation for REST service
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.CRUDS
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Http;

    using Akka.Actor;

    using ClusterKit.Core.Rest.ActionMessages;

    using JetBrains.Annotations;

    /// <summary>
    /// Base class to provide basic CRUD operation for REST service
    /// </summary>
    /// <typeparam name="TObject">
    /// The type of ef object
    /// </typeparam>
    /// <typeparam name="TId">
    /// The type of object identity field
    /// </typeparam>
    [UsedImplicitly]
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Reviewed. Suppression is OK here.")]
    public abstract class BaseCrudController<TObject, TId> : ApiController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseCrudController{TObject,TId}"/> class.
        /// </summary>
        /// <param name="system">
        /// The system.
        /// </param>
        protected BaseCrudController(ActorSystem system)
        {
            this.System = system;
            this.AkkaTimeout = ConfigurationUtils.GetRestTimeout(system);
        }

        /// <summary>
        /// Gets timeout for actor system requests
        /// </summary>
        protected virtual TimeSpan AkkaTimeout { get; }

        /// <summary>
        /// Gets the actor system
        /// </summary>
        protected ActorSystem System { get; }

        /// <summary>
        /// Updates or creates new Node template
        /// </summary>
        /// <param name="request">Node template data</param>
        /// <returns>Updated node template</returns>
        [HttpPut]
        [Route("")]
        public virtual async Task<TObject> Create(TObject request)
        {
            var template =
               await
               this.System.ActorSelection(this.GetDbActorProxyPath())
                   .Ask<TObject>(
                       new RestActionMessage<TObject, TId> { ActionType = EnActionType.Create, Request = request },
                       this.AkkaTimeout);

            if (template == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return template;
        }

        /// <summary>
        /// Removes Node template
        /// </summary>
        /// <param name="id">Node template unique id</param>
        /// <returns>Execution task</returns>
        [HttpDelete]
        [Route("{id}/")]
        public virtual async Task Delete(TId id)
        {
            var result =
                await
                this.System.ActorSelection(this.GetDbActorProxyPath())
                    .Ask<bool>(
                        new RestActionMessage<TObject, TId> { ActionType = EnActionType.Delete, Id = id },
                        this.AkkaTimeout);

            if (!result)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
        }

        /// <summary>
        /// Gets node template by its id
        /// </summary>
        /// <param name="id">Node template unique id</param>
        /// <returns>Node template</returns>
        [HttpGet]
        [Route("{id}/")]
        public virtual async Task<TObject> Get(TId id)
        {
            var template =
                await
                this.System.ActorSelection(this.GetDbActorProxyPath())
                    .Ask<TObject>(
                        new RestActionMessage<TObject, TId> { ActionType = EnActionType.Get, Id = id },
                        this.AkkaTimeout);

            if (template == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return template;
        }

        /// <summary>
        /// Gets the list of all node templates
        /// </summary>
        /// <param name="count">
        /// The count of elements to return.
        /// </param>
        /// <param name="skip">
        /// The count of elements to skip.
        /// </param>
        /// <returns>
        /// list of node templates
        /// </returns>
        [Route("")]
        [HttpGet]
        public virtual async Task<List<TObject>> GetList(int count = 100, int skip = 0)
        {
            return
                await
                this.System.ActorSelection(this.GetDbActorProxyPath())
                    .Ask<List<TObject>>(new CollectionRequest<TObject> { Count = count, Skip = skip }, this.AkkaTimeout);
        }

        /// <summary>
        /// Updates or creates new Node template
        /// </summary>
        /// <param name="request">Node template data</param>
        /// <returns>Updated node template</returns>
        [HttpPatch]
        [Route("{id}/")]
        public virtual async Task<TObject> Update(TObject request)
        {
            var template =
               await
               this.System.ActorSelection(this.GetDbActorProxyPath())
                   .Ask<TObject>(
                       new RestActionMessage<TObject, TId> { ActionType = EnActionType.Update, Request = request },
                       this.AkkaTimeout);

            if (template == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return template;
        }

        /// <summary>
        /// Gets akka actor path for database worker
        /// </summary>
        /// <returns>Akka actor path</returns>
        protected abstract string GetDbActorProxyPath();
    }
}