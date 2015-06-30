﻿using System;
using System.Web.Services.Description;
using Umbraco.Core.Models.Membership;
using Umbraco.Web.Cache;

namespace Umbraco.Web.PublishedCache
{
    /// <summary>
    /// Creates and manages IPublishedCaches.
    /// </summary>
    internal interface IPublishedCachesService : IDisposable
    {
        #region PublishedCaches

        /* Various places (such as Node) want to access the XML content, today as an XmlDocument
         * but to migrate to a new cache, they're migrating to an XPathNavigator. Still, they need
         * to find out how to get that navigator.
         * 
         * Because a cache such as the DrippingCache is contextual ie it has a "snapshot" nothing
         * and remains consistent over the snapshot, the navigator should come from the "current"
         * snapshot.
         * 
         * The service creates those snapshots in IPublishedCaches objects.
         * 
         * Places such as Node need to be able to find the "current" one so the factory has a
         * notion of what is "current". In most cases, the IPublishedCaches object is created
         * and registered against an UmbracoContext, and that context is then used as "current".
         * 
         * But for tests we need to have a way to specify what's the "current" object & preview.
         * Which is defined in PublishedCachesServiceBase.
         * 
         */

        /// <summary>
        /// Creates a set of published caches.
        /// </summary>
        /// <param name="previewToken">A preview token, or <c>null</c> if not previewing.</param>
        /// <returns>A set of published caches.</returns>
        IPublishedCaches CreatePublishedCaches(string previewToken);

        /// <summary>
        /// Gets the current set of published caches.
        /// </summary>
        /// <returns>The current set of published caches.</returns>
        /// <remarks></remarks>
        IPublishedCaches GetPublishedCaches();

        #endregion

        #region Preview

        /* Later on we can imagine that EnterPreview would handle a "level" that would be either
         * the content only, or the content's branch, or the whole tree + it could be possible
         * to register filters against the factory to filter out which nodes should be preview
         * vs non preview.
         * 
         * EnterPreview() returns the previewToken. It is up to callers to store that token
         * wherever they want, most probably in a cookie.
         * 
         */

        /// <summary>
        /// Enters preview for specified user and content.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="contentId">The content identifier.</param>
        /// <returns>A preview token.</returns>
        /// <remarks>
        /// <para>Tells the caches that they should prepare any data that they would be keeping
        /// in order to provide preview to a give user. In the Xml cache this means creating the Xml
        /// file, though other caches may do things differently.</para>
        /// <para>Does not handle the preview token storage (cookie, etc) that must be handled separately.</para>
        /// </remarks>
        string EnterPreview(IUser user, int contentId);

        /// <summary>
        /// Refreshes preview for a specified content.
        /// </summary>
        /// <param name="previewToken">The preview token.</param>
        /// <param name="contentId">The content identifier.</param>
        /// <remarks>Tells the caches that they should update any data that they would be keeping
        /// in order to provide preview to a given user. In the Xml cache this means updating the Xml
        /// file, though other caches may do things differently.</remarks>
        void RefreshPreview(string previewToken, int contentId);

        /// <summary>
        /// Exits preview for a specified preview token.
        /// </summary>
        /// <param name="previewToken">The preview token.</param>
        /// <remarks>
        /// <para>Tells the caches that they can dispose of any data that they would be keeping
        /// in order to provide preview to a given user. In the Xml cache this means deleting the Xml file,
        /// though other caches may do things differently.</para>
        /// <para>Does not handle the preview token storage (cookie, etc) that must be handled separately.</para>
        /// </remarks>
        void ExitPreview(string previewToken);

        #endregion

        #region Changes

        /* An IPublishedCachesService implementation can rely on transaction-level events to update
         * its internal, database-level data, as these events are purely internal. However, it cannot
         * rely on cache refreshers CacheUpdated events to update itself, as these events are external
         * and the order-of-execution of the handlers cannot be guaranteed, which means that some
         * user code may run before Umbraco is finished updating itself. Instead, the cache refreshers
         * explicitely notify the service of changes.
         * 
         */

        /// <summary>
        /// Notifies of content cache refresher changes.
        /// </summary>
        /// <param name="payloads">The changes.</param>
        /// <param name="draftChanged">A value indicating whether draft contents have been changed in the cache.</param>
        /// <param name="publishedChanged">A value indicating whether published contents have been changed in the cache.</param>
        void Notify(ContentCacheRefresher.JsonPayload[] payloads, out bool draftChanged, out bool publishedChanged);

        /// <summary>
        /// Notifies of media cache refresher changes.
        /// </summary>
        /// <param name="payloads">The changes.</param>
        /// <param name="anythingChanged">A value indicating whether medias have been changed in the cache.</param>
        void Notify(MediaCacheRefresher.JsonPayload[] payloads, out bool anythingChanged);

        // there is no NotifyChanges for MemberCacheRefresher because we're not caching members.

        /// <summary>
        /// Notifies of content type refresher changes.
        /// </summary>
        /// <param name="payloads">The changes.</param>
        void Notify(ContentTypeCacheRefresher.JsonPayload[] payloads);

        /// <summary>
        /// Notifies of data type refresher changes.
        /// </summary>
        /// <param name="payloads">The changes.</param>
        void Notify(DataTypeCacheRefresher.JsonPayload[] payloads);

        /// <summary>
        /// Notifies of domain refresher changes.
        /// </summary>
        /// <param name="payloads">The changes.</param>
        void Notify(DomainCacheRefresher.JsonPayload[] payloads);

        #endregion
    }
}
