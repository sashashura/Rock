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
using System.Linq;
using Http.TestLibrary;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Data;
using Rock.Lava;
using Rock.Model;
using Rock.Tests.Shared;

namespace Rock.Tests.Integration.Lava
{
    #region Personalize Block

    /// <summary>
    /// Tests for the Personalize Lava block.
    /// </summary>
    [TestClass]
    public class PersonalizationTests : LavaIntegrationTestBase
    {
        private const string SegmentAllMenGuid = "A8B006AF-1531-42B5-AD9A-570F97C8EFC1";
        private const string SegmentSmallGroupGuid = "F189099D-B1B7-4067-BD28-D354110AAB1D";
        private const string FilterMobileDeviceGuid = "F4F31B2A-F525-4E52-8A6E-ECA1E1EBD75B";
        private const string FilterQueryParameter1Guid = "674BD50E-DA57-495F-B2EC-B16BC34BE2FA";
        private const string FilterQueryParameter2Guid = "806079BE-768E-4707-9C86-47A73DB7A1C7";
        private const string FilterDesktopDeviceGuid = "58DEE912-6EF2-43C8-BE89-452A830BB7EF";

        [TestInitialize]
        public void TestInitialize()
        {
            var rockContext = new RockContext();

            // Create Personalization Segments.
            var personService = new PersonService( rockContext );
            var person = personService.GetByGuids( new List<Guid> { TestGuids.TestPeople.TedDecker.AsGuid() } ).FirstOrDefault();

            var personalizationService = new PersonalizationSegmentService( rockContext );

            var segmentAllMen = personalizationService.Get( SegmentAllMenGuid.AsGuid() );
            if ( segmentAllMen == null )
            {
                segmentAllMen = new PersonalizationSegment();
                personalizationService.Add( segmentAllMen );
            }

            segmentAllMen.SegmentKey = "ALL_MEN";
            segmentAllMen.Name = "All Men";
            segmentAllMen.Guid = SegmentAllMenGuid.AsGuid();

            var segmentSmallGroup = personalizationService.Get( SegmentSmallGroupGuid.AsGuid() );
            if ( segmentSmallGroup == null )
            {
                segmentSmallGroup = new PersonalizationSegment();
                personalizationService.Add( segmentSmallGroup );
            }

            segmentSmallGroup.SegmentKey = "IN_SMALL_GROUP";
            segmentSmallGroup.Name = "Small Group";
            segmentSmallGroup.Guid = SegmentSmallGroupGuid.AsGuid();
            rockContext.SaveChanges();

            // Add Ted Decker to segments: ALL_MEN, SMALL_GROUP
            AddOrUpdatePersonalizationSegmentForPerson( rockContext, SegmentAllMenGuid, TestGuids.TestPeople.TedDecker );
            AddOrUpdatePersonalizationSegmentForPerson( rockContext, SegmentSmallGroupGuid, TestGuids.TestPeople.TedDecker );
            // Add Bill Marble to segments: ALL_MEN
            AddOrUpdatePersonalizationSegmentForPerson( rockContext, SegmentAllMenGuid, TestGuids.TestPeople.BillMarble );

            rockContext.SaveChanges();

            // Create Request Filters.
            var filterService = new RequestFilterService( rockContext );

            // Create Request Filter: Desktop Device.
            var filterDesktop = filterService.Get( FilterDesktopDeviceGuid.AsGuid() );
            if ( filterDesktop == null )
            {
                filterDesktop = new RequestFilter();
                filterService.Add( filterDesktop );
            }

            filterDesktop.RequestFilterKey = "DESKTOP";
            filterDesktop.Name = "Desktop Device";
            filterDesktop.Guid = FilterDesktopDeviceGuid.AsGuid();

            // Create Request Filter: Mobile Device.
            var filterMobile = filterService.Get( FilterMobileDeviceGuid.AsGuid() );
            if ( filterMobile == null )
            {
                filterMobile = new RequestFilter();
                filterService.Add( filterMobile );
            }

            filterMobile.RequestFilterKey = "MOBILE";
            filterMobile.Name = "Mobile Device";
            filterMobile.Guid = FilterMobileDeviceGuid.AsGuid();

            rockContext.SaveChanges();

            // Create Request Filter: QueryStringParameter1.
            AddOrUpdateRequestFilterForQueryString( rockContext, FilterQueryParameter1Guid, "QUERY_1", "parameter1", "true" );
            AddOrUpdateRequestFilterForQueryString( rockContext, FilterQueryParameter2Guid, "QUERY_2", "parameter2", "true" );

            rockContext.SaveChanges();
        }

        private void AddOrUpdateRequestFilterForQueryString( RockContext rockContext, string guid, string filterKey, string parameterName, string parameterValue )
        {
            var filterService = new RequestFilterService( rockContext );

            var newFilter = filterService.Get( guid.AsGuid() );
            if ( newFilter == null )
            {
                newFilter = new RequestFilter();
                filterService.Add( newFilter );
            }

            newFilter.RequestFilterKey = filterKey;
            newFilter.Name = filterKey;
            newFilter.Guid = guid.AsGuid();

            var filterConfig = new Personalization.PersonalizationRequestFilterConfiguration();
            filterConfig.QueryStringRequestFilterExpressionType = FilterExpressionType.Filter;
            var queryStringFilter = new Personalization.QueryStringRequestFilter()
            {
                Key = parameterName,
                ComparisonType = ComparisonType.EqualTo,
                ComparisonValue = parameterValue,
            };
            filterConfig.QueryStringRequestFilters = new List<Personalization.QueryStringRequestFilter> { queryStringFilter };

            newFilter.FilterConfiguration = filterConfig;
        }

        private void AddOrUpdatePersonalizationSegmentForPerson( RockContext rockContext, string segmentGuid, string personGuid )
        {
            var pap = new PersonAliasPersonalization();
            pap.PersonalizationType = PersonalizationType.Segment;

            var personService = new PersonService( rockContext );
            var person = personService.GetByGuids( new List<Guid> { personGuid.AsGuid() } ).FirstOrDefault();
            pap.PersonAliasId = person.PrimaryAliasId.Value;

            var personalizationService = new PersonalizationSegmentService( rockContext );
            var segment = personalizationService.Get( segmentGuid.AsGuid() );
            pap.PersonalizationEntityId = segment.Id;

            var exists = rockContext.PersonAliasPersonalizations
                .Any( x => x.PersonAliasId == person.PrimaryAliasId.Value
                 && x.PersonalizationEntityId == segment.Id
                 && x.PersonalizationType == PersonalizationType.Segment );

            if ( !exists )
            {
                rockContext.PersonAliasPersonalizations.Add( pap );
            }
        }

        [TestMethod]
        public void PersonalizeBlock_WithNoParameters_IsHidden()
        {
            var input = @"
{% personalize %}
This content should not be visible to anyone, because this block is opt-in only.
{% endpersonalize %}
";
            var expectedOutput = @"";

            AssertOutputForPersonAndRequest( input, expectedOutput, TestGuids.TestPeople.TedDecker );
        }

        [TestMethod]
        public void PersonalizeBlock_ForSegmentsWithMatchAll_IsVisibleForPersonMatchingAllSegments()
        {
            var input = @"
{% personalize segment:'ALL_MEN,IN_SMALL_GROUP' matchtype:'all' %}
Hello Ted!
{% endpersonalize %}
";
            var expectedOutput = @"Hello Ted!";

            AssertOutputForPersonAndRequest( input, expectedOutput, TestGuids.TestPeople.TedDecker );
        }

        [TestMethod]
        public void PersonalizeBlock_ForSegmentsWithMatchAll_IsHiddenForPersonMatchingOnlySomeSegments()
        {
            var input = @"
{% personalize segment:'ALL_MEN,IN_SMALL_GROUP' matchtype:'all' %}
Bill should not see this because he only matches the 'ALL_MEN' segment.
{% endpersonalize %}
";
            var expectedOutput = @"";

            AssertOutputForPersonAndRequest( input, expectedOutput, TestGuids.TestPeople.BillMarble );
        }

        [TestMethod]
        public void PersonalizeBlock_ForSegmentsWithMatchAll_IsHiddenForInvalidSegmentKey()
        {
            var input = @"
{% personalize segment:'ALL_MEN,INVALID_KEY' matchtype:'all' %}
Bill should not see this because one of the personalize segments is invalid and cannot be matched.
{% endpersonalize %}
";
            var expectedOutput = @"";

            AssertOutputForPersonAndRequest( input, expectedOutput, TestGuids.TestPeople.BillMarble );
        }

        [TestMethod]
        public void PersonalizeBlock_ForSegmentsWithMatchAny_IsVisibleForPersonMatchingSomeSegments()
        {
            var input = @"
{% personalize segment:'ALL_MEN,IN_SMALL_GROUP' matchtype:'any' %}
Hello Bill!
{% endpersonalize %}
";
            var expectedOutput = @"Hello Bill!";

            AssertOutputForPersonAndRequest( input, expectedOutput, TestGuids.TestPeople.BillMarble );
        }

        [TestMethod]
        public void PersonalizeBlock_ForSegmentsWithMatchAny_IsHiddenForPersonMatchingNoSegments()
        {
            var input = @"
{% personalize segment:'ALL_MEN,IN_SMALL_GROUP' matchtype:'all' %}
Alisha should not see this because she does not match any of the specified segments.
{% endpersonalize %}
";
            var expectedOutput = @"";

            AssertOutputForPersonAndRequest( input, expectedOutput, TestGuids.TestPeople.AlishaMarble );
        }

        [TestMethod]
        public void PersonalizeBlock_ForSegmentsWithMatchNone_IsVisibleForPersonMatchingNoSegments()
        {
            var input = @"
{% personalize segment:'ALL_MEN,IN_SMALL_GROUP' matchtype:'none' %}
Hello Alisha!
{% endpersonalize %}
";
            var expectedOutput = @"Hello Alisha!";

            AssertOutputForPersonAndRequest( input, expectedOutput, TestGuids.TestPeople.AlishaMarble );
        }

        [TestMethod]
        public void PersonalizeBlock_ForSegmentsWithMatchNone_IsHiddenForPersonMatchingAnySegments()
        {
            var input = @"
{% personalize segment:'ALL_MEN,IN_SMALL_GROUP' matchtype:'none' %}
Bill should not see this because he matches the 'Small Group' segment.
{% endpersonalize %}
";
            var expectedOutput = @"";

            AssertOutputForPersonAndRequest( input, expectedOutput, TestGuids.TestPeople.BillMarble );
        }

        [TestMethod]
        public void PersonalizeBlock_ForRequestFiltersWithMatchAll_IsVisibleForRequestMatchingAllFilters()
        {
            var input = @"
{% personalize requestfilter:'QUERY_1,QUERY_2' matchtype:'all' %}
Request acknowledged!
{% endpersonalize %}
";
            var expectedOutput = @"Request acknowledged!";

            AssertOutputForPersonAndRequest( input,
                expectedOutput,
                inputUrl: "http://rock.rocksolidchurchdemo.com?parameter1=true&parameter2=true" );
        }

        [TestMethod]
        public void PersonalizeBlock_ForRequestFiltersWithMatchAll_IsHiddenForUnmatchedQueryParameter()
        {
            var input = @"
{% personalize requestfilter:'QUERY_1,QUERY_2' matchtype:'all' %}
This content should be hidden because the query parameter 'parameter2' does not match the required value 'true'.
{% endpersonalize %}
";
            var expectedOutput = @"";

            AssertOutputForPersonAndRequest( input,
                expectedOutput,
                inputUrl: "http://rock.rocksolidchurchdemo.com?parameter1=true&parameter2=xyzzy" );
        }

        [TestMethod]
        public void PersonalizeBlock_ForRequestFiltersWithMatchAll_IsHiddenForRequestMatchingOnlySomeFilters()
        {
            var input = @"
{% personalize requestfilter:'QUERY_1,QUERY_2' matchtype:'all' %}
Request does not match the block filter, does not include a match for QUERY_1.
{% endpersonalize %}
";
            var expectedOutput = @"";

            AssertOutputForPersonAndRequest( input,
                expectedOutput,
                inputUrl: "http://rock.rocksolidchurchdemo.com?parameter2=true" );
        }

        [TestMethod]
        public void PersonalizeBlock_ForRequestFiltersWithMatchAny_IsVisibleForRequestMatchingSomeFilters()
        {
            var input = @"
{% personalize requestfilter:'QUERY_1,QUERY_2' matchtype:'any' %}
Request matches filter: QUERY_2.
{% endpersonalize %}
";
            var expectedOutput = @"Request matches filter: QUERY_2.";

            AssertOutputForPersonAndRequest( input,
                expectedOutput,
                inputUrl: "http://rock.rocksolidchurchdemo.com?parameter2=true" );
        }

        [TestMethod]
        public void PersonalizeBlock_ForRequestFiltersWithMatchAny_IsHiddenForRequestMatchingNoFilters()
        {
            var input = @"
{% personalize requestfilter:'QUERY_1,QUERY_2' matchtype:'any' %}
Request does not match any filter.
{% endpersonalize %}
";
            var expectedOutput = @"";

            AssertOutputForPersonAndRequest( input,
                expectedOutput,
                inputUrl: "http://rock.rocksolidchurchdemo.com?parameter0=true" );
        }

        [TestMethod]
        public void PersonalizeBlock_ForRequestFiltersWithMatchNone_IsVisibleForRequestMatchingNoFilters()
        {
            var input = @"
{% personalize requestfilter:'QUERY_1,QUERY_2' matchtype:'none' %}
Request does not match any filter.
{% endpersonalize %}
";
            var expectedOutput = @"Request does not match any filter.";

            AssertOutputForPersonAndRequest( input,
                expectedOutput,
                inputUrl: "http://rock.rocksolidchurchdemo.com?parameter0=true" );
        }

        [TestMethod]
        public void PersonalizeBlock_ForRequestFiltersWithMatchNone_IsHiddenForRequestMatchingAnyFilters()
        {
            var input = @"
{% personalize requestfilter:'QUERY_1,QUERY_2' matchtype:'none' %}
Request matches filter: QUERY_2.
{% endpersonalize %}
";
            var expectedOutput = @"";

            AssertOutputForPersonAndRequest( input,
                expectedOutput,
                inputUrl: "http://rock.rocksolidchurchdemo.com?parameter2=true" );
        }

        #endregion

        #region PersonalizationItems Filter

        [TestMethod]
        public void PersonalizationItemsFilter_ForCurrentVisitor_ReturnsItemsForCurrentPerson()
        {
            var input = @"
{% assign items = CurrentVisitor | PersonalizationItems:'Segments,RequestFilters' %}
{% for item in items %}
({{ item.Type }}) {{ item.Key }}
{% endfor %}
";
            var expectedOutput = @"
(Segment) ALL_MEN
(Segment) IN_SMALL_GROUP
(Request Filter) QUERY_1
(Request Filter) QUERY_2
";

            AssertOutputForPersonAndRequest( input,
                expectedOutput,
                personGuid: TestGuids.TestPeople.TedDecker,
                inputUrl: "http://rock.rocksolidchurchdemo.com?parameter1=true&parameter2=true"
                );
        }

        [TestMethod]
        public void PersonalizationItemsFilter_ForEmptyInput_ReturnsRequestFiltersOnly()
        {
            var input = @"
{% assign items = '' | PersonalizationItems:'Segments,RequestFilters' %}
{% for item in items %}
({{ item.Type }}) {{ item.Key }}
{% endfor %}
";
            var expectedOutput = @"
(Request Filter) QUERY_1
(Request Filter) QUERY_2
";

            AssertOutputForPersonAndRequest( input,
                expectedOutput,
                inputUrl: "http://rock.rocksolidchurchdemo.com?parameter1=true&parameter2=true" );
        }

        [TestMethod]
        public void PersonalizationItemsFilter_ForPersonAndRequest_ReturnsAllSegmentsAndFilters()
        {
            var input = @"
{% assign items = '<personGuid>' | PersonalizationItems:'Segments,RequestFilters' %}
{% for item in items %}
({{ item.Type }}) {{ item.Key }}
{% endfor %}
";
            input = input.Replace( "<personGuid>", TestGuids.TestPeople.TedDecker );
            var expectedOutput = @"
(Segment) ALL_MEN
(Segment) IN_SMALL_GROUP
(Request Filter) QUERY_1
(Request Filter) QUERY_2
";

            AssertOutputForPersonAndRequest( input,
                expectedOutput,
                inputUrl: "http://rock.rocksolidchurchdemo.com?parameter1=true&parameter2=true" );
        }

        [TestMethod]
        public void PersonalizationItemsFilter_WithNoActiveRequest_ReturnsSegmentsOnly()
        {
            var input = @"
{% assign items = '<personGuid>' | PersonalizationItems:'Segments,RequestFilters' %}
{% for item in items %}
({{ item.Type }}) {{ item.Key }}
{% endfor %}
";
            input = input.Replace( "<personGuid>", TestGuids.TestPeople.TedDecker );
            var expectedOutput = @"
(Segment) ALL_MEN
(Segment) IN_SMALL_GROUP
";

            AssertOutputForPersonAndRequest( input,
                expectedOutput );
        }

        [TestMethod]
        public void PersonalizationItemsFilter_WithDefaultOptions_ReturnsAllSegmentsAndFilters()
        {
            var input = @"
{% assign items = '<personGuid>' | PersonalizationItems %}
{% for item in items %}
({{ item.Type }}) {{ item.Key }}
{% endfor %}
";
            input = input.Replace( "<personGuid>", TestGuids.TestPeople.TedDecker );
            var expectedOutput = @"
(Segment) ALL_MEN
(Segment) IN_SMALL_GROUP
(Request Filter) QUERY_1
(Request Filter) QUERY_2
";

            AssertOutputForPersonAndRequest( input,
                expectedOutput,
                inputUrl: "http://rock.rocksolidchurchdemo.com?parameter1=true&parameter2=true" );
        }

        [TestMethod]
        public void PersonalizationItemsFilter_DocumentationExample1_IsValid()
        {
            // Example Input: http://rock.rocksolidchurchdemo.com?parameter1=true&parameter2=true
            var input = @"
{% assign items = CurrentVisitor | PersonalizationItems:'Segments,RequestFilters' %}
{% for item in items %}
({{ item.Type }}) {{ item.Key }}
{% endfor %}
";
            var expectedOutput = @"
(Segment) ALL_MEN
(Segment) IN_SMALL_GROUP
(Request Filter) QUERY_1
(Request Filter) QUERY_2
";

            AssertOutputForPersonAndRequest( input,
                expectedOutput,
                personGuid: TestGuids.TestPeople.TedDecker,
                inputUrl: "http://rock.rocksolidchurchdemo.com?parameter1=true&parameter2=true"
                );
        }
        #endregion

        private void AssertOutputForPersonAndRequest( string inputTemplate, string expectedOutput, string personGuid = "", string inputUrl = "" )
        {
            var mergeValues = new LavaDataDictionary();

            if ( !string.IsNullOrWhiteSpace( personGuid ) )
            {
                var rockContext = new RockContext();
                var personService = new PersonService( rockContext );
                var person = personService.GetByGuids( new List<Guid> { personGuid.AsGuid() } ).FirstOrDefault();

                mergeValues["CurrentVisitor"] = person.PrimaryAlias;
                mergeValues["CurrentPerson"] = person;
            }

            var options = new LavaTestRenderOptions() { MergeFields = mergeValues };

            if ( !string.IsNullOrWhiteSpace( inputUrl ) )
            {
                var simulator = new HttpSimulator();
                using ( var request = simulator.SimulateRequest( new Uri( inputUrl ) ) )
                {
                    TestHelper.AssertTemplateOutput( expectedOutput, inputTemplate, options );
                }
            }
            else
            {
                TestHelper.AssertTemplateOutput( expectedOutput, inputTemplate, options );
            }
        }
    }
}
