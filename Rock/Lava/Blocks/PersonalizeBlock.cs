﻿// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Lava.Blocks
{
    /// <summary>
    /// A Lava Block that provides a means of rendering content based on a set of personalization filters.
    /// Segments and filters can be used to determine the visibility of the block for the current user and request.
    /// </summary>
    public class PersonalizeBlock : LavaBlockBase
    {
        #region Filter Parameter Names

        /// <summary>
        /// Parameter name for specifying maximum occurrences. If not specified, the default value is 100.
        /// </summary>
        public static readonly string ParameterMatchType = "matchtype";
        /// <summary>
        /// Parameter name for specifying a filter for the intended audiences of the Event Occurrences. If not specified, all audiences are considered.
        /// </summary>
        public static readonly string ParameterSegments = "segment";
        /// <summary>
        /// Parameter name for specifying a filter for the campus of the Event Occurrences. If not specified, all campuses are considered.
        /// </summary>
        public static readonly string ParameterRequestFilters = "requestfilter";
        /// <summary>
        /// Parameter name for specifying the Person for whom the block should be evaluated.
        /// This parameter is most useful for testing purposes.
        /// </summary>
        public static readonly string ParameterPersonIdentifier = "person";

        #endregion

        /// <summary>
        /// The name of the element as it is used in the source document.
        /// </summary>
        public static readonly string TagSourceName = "personalize";

        private string _attributesMarkup;
        private bool _renderErrors = true;

        LavaElementAttributes _settings = new LavaElementAttributes();

        /// <summary>
        /// Initializes the specified tag name.
        /// </summary>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="markup">The markup.</param>
        /// <param name="tokens">The tokens.</param>
        public override void OnInitialize( string tagName, string markup, List<string> tokens )
        {
            _attributesMarkup = markup;

            base.OnInitialize( tagName, markup, tokens );
        }

        /// <summary>
        /// Renders the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="result">The result.</param>
        public override void OnRender( ILavaRenderContext context, TextWriter result )
        {
            try
            {
                _settings.ParseFromMarkup( _attributesMarkup, context );

                var showContent = ShowContentForCurrentRequest( context );
                if ( showContent )
                {
                    base.OnRender( context, result );
                }
            }
            catch ( Exception ex )
            {
                var message = "Personalization block not available. " + ex.Message;

                if ( _renderErrors )
                {
                    result.Write( message );
                }
                else
                {
                    ExceptionLogService.LogException( ex );
                }
            }
        }

        /// <summary>
        /// Determine if the block content should be shown for the current request and user.
        /// </summary>
        /// <param name="context"></param>
        /// <remarks>
        /// Changes to this method should remain synchronised with the Personalize.ShowContentForCurrentRequest() method.
        /// </remarks>
        /// <returns></returns>
        private bool ShowContentForCurrentRequest( ILavaRenderContext context )
        {
            var matchType = _settings.GetStringValue( ParameterMatchType, "any" ).ToLower();

            // Apply the request filters if we are processing a HTTP request.
            // Do this first because we may have the opportunity to exit early and avoid retrieving personalization segments.
            bool? isMatchForRequestFilters = null;
            var requestFilterParameterString = _settings.GetStringValue( ParameterRequestFilters );
            if ( !string.IsNullOrWhiteSpace( requestFilterParameterString ) )
            {
                var currentFilterIdList = LavaPersonalizationHelper.GetPersonalizationRequestFilterIdList();
                if ( currentFilterIdList != null )
                {

                    var requiredRequestIdList = RequestFilterCache.GetByKeys( requestFilterParameterString )
                        .Select( ps => ps.Id )
                        .ToList();
                    if ( requiredRequestIdList.Any() )
                    {
                        if ( matchType == "all" )
                        {
                            // All of the specified filters must be matched, so we need to fail for any invalid keys.
                            var requestFilterParameterCount = requestFilterParameterString.SplitDelimitedValues( ",", StringSplitOptions.RemoveEmptyEntries ).Count();
                            isMatchForRequestFilters = ( requiredRequestIdList.Count == requestFilterParameterCount )
                                && requiredRequestIdList.All( id => currentFilterIdList.Contains( id ) );
                        }
                        else if ( matchType == "none" )
                        {
                            isMatchForRequestFilters = !requiredRequestIdList.Any( id => currentFilterIdList.Contains( id ) );
                        }
                        else
                        {
                            // Apply default match type of "any".
                            isMatchForRequestFilters = requiredRequestIdList.Any( id => currentFilterIdList.Contains( id ) );
                        }
                    }
                }
            }
            // If request filters exist and are not matched, do not show the content.
            if ( isMatchForRequestFilters != null && !isMatchForRequestFilters.Value )
            {
                return false;
            }

            // Determine if the current block segments match the segments for the user in the current context.
            bool? isMatchForSegments = null;
            var segmentParameterString = _settings.GetStringValue( ParameterSegments );
            if ( !string.IsNullOrWhiteSpace( segmentParameterString ) )
            {
                // Get personalization segments for the target person.
                var personReference = _settings.GetStringValue( "person" );
                Person person;
                if ( !string.IsNullOrEmpty( personReference ) )
                {
                    person = LavaHelper.GetPersonFromInputParameter( personReference, context );
                }
                else
                {
                    person = LavaHelper.GetCurrentPerson( context );
                }

                List<int> personSegmentIdList;
                var rockContext = LavaHelper.GetRockContextFromLavaContext( context );
                if ( person != null )
                {
                    personSegmentIdList = LavaPersonalizationHelper.GetPersonalizationSegmentIdListForRequest( person,
                        rockContext,
                        System.Web.HttpContext.Current?.Request );
                }
                else
                {
                    personSegmentIdList = LavaPersonalizationHelper.GetPersonalizationSegmentIdListForPerson( person,
                        rockContext );
                }

                var requiredSegmentIdList = PersonalizationSegmentCache.GetByKeys( segmentParameterString )
                    .Select( ps => ps.Id )
                    .ToList();
                if ( requiredSegmentIdList != null )
                {
                    if ( matchType == "all" )
                    {
                        // All of the specified segments must be matched, so we need to fail for any invalid keys.
                        var segmentParameterCount = segmentParameterString.SplitDelimitedValues( ",", StringSplitOptions.RemoveEmptyEntries ).Count();
                        isMatchForSegments = ( requiredSegmentIdList.Count == segmentParameterCount )
                            && requiredSegmentIdList.All( id => personSegmentIdList.Contains( id ) );
                    }
                    else if ( matchType == "none" )
                    {
                        isMatchForSegments = !requiredSegmentIdList.Any( id => personSegmentIdList.Contains( id ) );
                    }
                    else
                    {
                        // Apply default match type of "any".
                        isMatchForSegments = requiredSegmentIdList.Any( id => personSegmentIdList.Contains( id ) );
                    }
                }
            }
            // If segments exist and are not matched, do not show the content.
            if ( isMatchForSegments != null && !isMatchForSegments.Value )
            {
                return false;
            }

            // If no parameters are specified for the block, do not show the content.
            if ( isMatchForRequestFilters == null && isMatchForSegments == null )
            {
                return false;
            }
            return true;
        }
    }
}
