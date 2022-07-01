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
using System.ComponentModel;
using System.IO;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using Rock.Web.UI;

namespace RockWeb.Blocks.Reminders
{
    [DisplayName( "Reminder List" )]
    [Category( "Reminders" )]
    [Description( "Block to show a list of reminders." )]
    [Rock.SystemGuid.BlockTypeGuid( Rock.SystemGuid.BlockType.REMINDER_LIST )]
    public partial class ReminderList : RockBlock, ICustomGridColumns
    {
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

            if ( !Page.IsPostBack )
            {
                RecalculateReminders();
            }
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
                BindList();
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
            BindList();
        }

        #endregion Events

        #region Methods

        /// <summary>
        /// Recalculates the reminder count for the current person.
        /// </summary>
        private void RecalculateReminders()
        {
            if ( !CurrentPersonId.HasValue )
            {
                return;
            }

            using ( var rockContext = new RockContext() )
            {
                var reminderService = new ReminderService( rockContext );
                reminderService.RecalculateReminderCount( CurrentPersonId.Value );
            }
        }

        /// <summary>
        /// Binds the list.
        /// </summary>
        private void BindList()
        {
            rptReminders.DataSource = GetReminders();
            rptReminders.DataBind();
        }

        /// <summary>
        /// Gets the reminders.
        /// </summary>
        /// <returns></returns>
        private List<Reminder> GetReminders()
        {
            var selectedEntityTypeId = 0;
            var selectedEntityId = 0;
            var selectedTagId = 0;

            return new List<Reminder>();
        }

        #endregion Methods
    }
}