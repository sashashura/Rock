// <copyright>
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
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Http.TestLibrary;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Data;
using Rock.Model;
using Rock.Rest.Controllers;
using Rock.Tests.Integration.TestData;
using Rock.Tests.Shared;

namespace Rock.Tests.Integration.Rest.ControllersTests
{
    [TestClass]
    public class PersonSearchControllerTests
    {
        private static string TedDeckerAlias2GuidString = "CCB555D6-E2C8-4A2C-8564-34083F598465";

        [ClassInitialize]
        public static void ClassInitialize( TestContext context )
        {
            var rockContext = new RockContext();
            var personTedDecker = TestDataHelper.GetTestPerson( TestGuids.TestPeople.TedDecker );

            // Create an alternate PersonAlias for Ted.
            var personAliasService = new PersonAliasService( rockContext );
            var alias1 = personAliasService.Get( TedDeckerAlias2GuidString.AsGuid() );
            if ( alias1 == null )
            {
                alias1 = new PersonAlias();
                alias1.Guid = TedDeckerAlias2GuidString.AsGuid();
                personAliasService.Add( alias1 );
            }
            alias1.PersonId = personTedDecker.Id;
            alias1.AliasPersonId = 9999;

            rockContext.SaveChanges();
        }

        [TestMethod]
        public void SearchForPersonWithMultiplePersonAliases_ReturnsPrimaryAlias()
        {
            var rockContext = new RockContext();
            var personTedDecker = TestDataHelper.GetTestPerson( TestGuids.TestPeople.TedDecker );
            var primaryAliasGuid = personTedDecker.PrimaryAliasId;

            var results = SearchWithRestController( "name", "Decker, Ted" );

            var result = results.FirstOrDefault( r => r.Guid == TestGuids.TestPeople.TedDecker.AsGuid()
                && r.PrimaryAliasGuid == personTedDecker.PrimaryAlias.Guid );

            Assert.That.IsNotNull( result, "Expected primary PersonAlias not found." );
        }

        [TestMethod]
        public void SearchByUniqueLastNameFirstName_ReturnsSingleMatch()
        {
            var results = SearchWithRestController( "name", "Lowe, Maddie" );
            Assert.That.IsTrue( results.Count == 1, "Single result expected, multiple results returned." );

            // Confirm that the result contains a single record for Maddie Lowe.
            var result = results.First( r => r.Guid == TestGuids.TestPeople.MaddieLowe.AsGuid() );
            Assert.That.IsNotNull( result, "Unexpected search result." );
        }

        [TestMethod]
        public void SearchByUniqueAddress_ReturnsExpectedMatches()
        {
            // Search for an address that is unique to the Jones family: Ben and Brian.
            var results = SearchWithRestController( "address", "10601 N 32nd Dr" );

            Assert.That.IsTrue( results.Count == 2 );
            Assert.That.IsNotNull( results.FirstOrDefault( r => r.Guid == TestGuids.TestPeople.BenJones.AsGuid() ) );
            Assert.That.IsNotNull( results.FirstOrDefault( r => r.Guid == TestGuids.TestPeople.BrianJones.AsGuid() ) );
        }

        [TestMethod]
        public void SearchWithIncludeDetails_ReturnsPopulatedDetailFields()
        {
            var results = SearchWithRestController( "name", "Decker, Ted", includeDetails: true );

            Assert.That.IsTrue( results.Count == 1 );
            var result = results.First();
            Assert.That.AreEqual( "11624 N 31st Dr\r\nPhoenix, AZ 85029-3202", result.Address );
        }

        [TestMethod]
        public void SearchWithExcludeDetails_ReturnsEmptyDetailFields()
        {
            var results = SearchWithRestController( "name", "Decker, Ted", includeDetails: false );

            Assert.That.IsTrue( results.Count == 1 );
            var result = results.First();
            Assert.That.IsNull( result.Address, "Address field should be empty if details not requested." );
        }

        public List<PersonSearchResult> SearchWithRestController( string searchField, string searchText, bool includeDetails = true )
        {
            var resultItems = new List<PersonSearchResult>();
            var nameSearchText = string.Empty;
            var addressSearchText = string.Empty;

            if ( searchField == "name" )
            {
                nameSearchText = searchText;
            }
            else if ( searchField == "address" )
            {
                addressSearchText = searchText;
            }
            else
            {
                throw new Exception( "Unknown search field." );
            }

            using ( var simulator = GetHttpSimulator() )
            {
                using ( var request = simulator.SimulateRequest( new Uri( "http://www.rocksolidchurch.com/" ) ) )
                {
                    var controller = new PeopleController();
                    var result = controller.Search( name: nameSearchText,
                        includeDetails: includeDetails,
                        address: addressSearchText );

                    resultItems = result.ToList();

                    // Verify that we have at least 1 match.
                    if ( resultItems.Count == 0 )
                    {
                        Debug.WriteLine( $"WARNING: Search \"{searchText}\" returned no items." );
                    }
                }
            }

            return resultItems;
        }

        private HttpSimulator _simulator = null;
        private string _webContentFolder = null;

        private string GetWebContentFolder()
        {
            if ( _webContentFolder == null )
            {
                var codeBaseUrl = new Uri( System.Reflection.Assembly.GetExecutingAssembly().CodeBase );
                var codeBasePath = Uri.UnescapeDataString( codeBaseUrl.AbsolutePath );
                var dirPath = System.IO.Path.GetDirectoryName( codeBasePath );
                _webContentFolder = System.IO.Path.Combine( dirPath, "Content" );
            }
            return _webContentFolder;
        }

        private HttpSimulator GetHttpSimulator()
        {
            _simulator = new HttpSimulator( "/", GetWebContentFolder() );
            _simulator.DebugWriter = TextWriter.Null;
            return _simulator;
        }
    }
}
