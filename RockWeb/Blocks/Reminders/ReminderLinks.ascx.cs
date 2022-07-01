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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;

using Rock.Web.UI;

namespace RockWeb.Blocks.Reminders
{
    [DisplayName( "Reminder Links" )]
    [Category( "Reminders" )]
    [Description( "This block is used to show reminder links." )]

    #region Block Attributes

    [LinkedPage(
        "View Reminders Page",
        Description = "The page where a person can view their reminders.",
        DefaultValue = Rock.SystemGuid.Page.REMINDER_LIST,
        Order = 0,
        Key = AttributeKey.ViewRemindersPage )]

    [LinkedPage(
        "Add Reminder Page",
        Description = "The page where a person can add a reminder.",
        DefaultValue = Rock.SystemGuid.Page.REMINDER_EDIT,
        Order = 1,
        Key = AttributeKey.AddReminderPage )]

    #endregion Block Attributes

    [Rock.SystemGuid.BlockTypeGuid( Rock.SystemGuid.BlockType.REMINDER_LINKS )]
    public partial class ReminderLinks : Rock.Web.UI.RockBlock
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string ViewRemindersPage = "ViewRemindersPage";
            public const string AddReminderPage = "AddReminderPage";
        }

        #endregion Attribute Keys

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

            // This event gets fired after block settings are updated. It's nice to repaint the screen if these settings would alter it.
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlReminders );
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
                if ( CurrentPersonAliasId.HasValue )
                {
                    lbReminders.Visible = true;

                    int reminderCount = CurrentPerson?.ReminderCount ?? 0;
                    if ( reminderCount > 0 )
                    {
                        lbReminders.CssClass = lbReminders.CssClass + " badge-" + CurrentPerson.ReminderCount.Value.ToString();
                    }

                    hfContextEntityTypeId.Value = GetContextEntityTypeId().ToString();
                }
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

        /// <summary>
        /// Handles the Click event of the lbAddReminder control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAddReminder_Click( object sender, EventArgs e )
        {
            var contextEntity = GetFirstContextEntity();
            if ( contextEntity == null )
            {
                // This shouldn't be possible, since the button is only visible when the page has a context entity.
                return;
            }

            var queryParams = new Dictionary<string, string>
            {
                { PageParameterKey.EntityTypeId, contextEntity.TypeId.ToString() },
                { PageParameterKey.EntityId, contextEntity.Id.ToString() }
            };

            NavigateToLinkedPage( AttributeKey.AddReminderPage, queryParams );
        }

        #endregion Events

        #region Methods

        /// <summary>
        /// Gets the first context entity for the page.
        /// </summary>
        /// <returns></returns>
        private IEntity GetFirstContextEntity()
        {
            foreach ( var contextEntityType in RockPage.GetContextEntityTypes() )
            {
                var contextEntity = RockPage.GetCurrentContext( contextEntityType );

                if ( contextEntity != null )
                {
                    return contextEntity;
                }
            }

            return null;
        }

        /// <summary>
        /// Checks to see if the page has a context entity.
        /// </summary>
        /// <returns></returns>
        private int GetContextEntityTypeId()
        {
            var entity = GetFirstContextEntity();
            if ( entity == null )
            {
                return 0;
            }

            return entity.TypeId;
        }

        #endregion Methods
    }
}