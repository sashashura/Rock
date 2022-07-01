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
using System.ComponentModel;
using System.Data.Entity;
using System.Web.UI;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock;

namespace RockWeb.Blocks.Reminders
{
    [DisplayName( "Reminder Edit" )]
    [Category( "Reminders" )]
    [Description( "Block for editing reminders." )]

    [Rock.SystemGuid.BlockTypeGuid( Rock.SystemGuid.BlockType.REMINDER_EDIT )]
    public partial class ReminderEdit : Rock.Web.UI.RockBlock
    {
        #region Page Parameter Keys

        private static class PageParameterKey
        {
            public const string EntityTypeId = "EntityTypeId";
            public const string EntityId = "EntityId";
        }

        #endregion Page Parameter Keys

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                InitializeBlock();
            }
        }

        #endregion Base Control Methods

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            NavigateToCurrentPageReference();
        }

        private void InitializeBlock()
        {
            var entityTypeId = PageParameter( PageParameterKey.EntityTypeId ).AsInteger();
            var entityId = PageParameter( PageParameterKey.EntityId ).AsInteger();
            if ( entityTypeId == 0 || entityId == 0 )
            {
                NavigateToParentPage();
            }

            using ( var rockContext = new RockContext() )
            {
                IEntity entity = new EntityTypeService( rockContext ).GetEntity( entityTypeId, entityId );
                lEntity.Text = entity.ToString();

                var reminderTypeService = new ReminderTypeService( rockContext );
                var reminderTypes = reminderTypeService.GetReminderTypesForEntityType( entityTypeId );
                rddlReminderType.DataSource = reminderTypes;
                rddlReminderType.DataTextField = "Name";
                rddlReminderType.DataValueField = "Id";
                rddlReminderType.DataBind();
            }

            rppPerson.SetValue( CurrentPerson );
        }

        #endregion Events

        #region Methods

        #endregion Methods
    }
}